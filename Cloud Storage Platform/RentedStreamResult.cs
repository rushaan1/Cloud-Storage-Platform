using Microsoft.AspNetCore.Mvc;
using System.Buffers;

namespace Cloud_Storage_Platform
{
    public class RentedStreamResult : IActionResult
    {
        private readonly string _path;
        private readonly string _contentType;
        public RentedStreamResult(string path, string contentType)
          => (_path, _contentType) = (path, contentType);

        // This is an attempt to optimize file previewing endpoint (mainly memory usage) as much as possible without going in depth into video streaming
        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = _contentType;
            response.Headers["Accept-Ranges"] = "bytes";
            response.Headers["Connection"] = "close"; 

            var poolBuf = ArrayPool<byte>.Shared.Rent(512 * 1024);

            try
            {
                await using var fs = new FileStream(
                    _path, FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: poolBuf.Length, useAsync: true);

                long total = fs.Length;
                var rangeHeader = context.HttpContext.Request.Headers["Range"].ToString();
                var openEnded = rangeHeader.EndsWith("-", StringComparison.Ordinal);

                var (start, intendedEnd) = ParseRange(context.HttpContext.Request, total);

                var end = openEnded
                    ? Math.Min(start + poolBuf.Length - 1, total - 1)
                    : intendedEnd;

                var length = end - start + 1;
                response.StatusCode = (start == 0 && end == total - 1) ? 200 : 206;
                response.Headers["Content-Range"] = $"bytes {start}-{end}/{total}";
                response.Headers["Content-Length"] = length.ToString();

                fs.Seek(start, SeekOrigin.Begin);
                var ct = context.HttpContext.RequestAborted;
                long remaining = length;

                while (remaining > 0 && !ct.IsCancellationRequested)
                {
                    int toRead = (int)Math.Min(poolBuf.Length, remaining);
                    int read = await fs.ReadAsync(poolBuf, 0, toRead, ct);
                    if (read == 0) break;

                    await response.Body.WriteAsync(poolBuf, 0, read, ct);
                    await response.Body.FlushAsync(ct);
                    remaining -= read;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(poolBuf);
            }
        }


        // This is essentially what FileStreamResult does
        public static (long Start, long End) ParseRange(HttpRequest request, long fileLength)
        {
            var rangeHeader = request.Headers["Range"].ToString();

            if (string.IsNullOrWhiteSpace(rangeHeader) || !rangeHeader.StartsWith("bytes="))
            {
                return (0, fileLength - 1); // No range specified, return full file
            }

            var range = rangeHeader["bytes=".Length..].Split('-');

            if (!long.TryParse(range[0], out var start))
            {
                start = 0;
            }

            long end;
            if (range.Length == 2 && long.TryParse(range[1], out end))
            {
                // Ensursing the requested end is not larger than file
                end = Math.Min(end, fileLength - 1);
            }
            else
            {
                // End is not provided so serving to end of file
                end = fileLength - 1;
            }

            // Sanity check
            if (start > end || start >= fileLength)
            {
                start = 0;
                end = fileLength - 1;
            }

            return (start, end);
        }

    }

}
