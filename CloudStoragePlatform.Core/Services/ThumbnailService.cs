using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public class ThumbnailService
    {
        private readonly UserIdentification _userIdentification;
        public ThumbnailService(UserIdentification userIdentification)
        {
            _userIdentification = userIdentification;
        }

        public async Task GenerateImageThumbnail(Guid fileId, string path, bool isGIF)
        {
            var userId = _userIdentification.User.Id;
            var filePath = $@"C:\\CloudStoragePlatform\\{userId}\\{fileId}";
            await Task.Run(() =>
            {
                using var image = Image.Load(filePath);
                if (isGIF)
                {
                    while (image.Frames.Count > 1)
                    {
                        image.Frames.RemoveFrame(1);
                    }
                }
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(200, 200),
                    Mode = ResizeMode.Max
                }));

                using var ms = new MemoryStream();
                image.SaveAsPng(ms);
                System.IO.File.WriteAllBytes($@"C:\\CloudStoragePlatform\\thumbnails\\{fileId}.png", ms.ToArray());
            });
        }

        public async Task GenerateVideoThumbnail(Guid fileId, string filePath)
        {
            var userId = _userIdentification.User.Id;
            var inputPath = $@"C:\\CloudStoragePlatform\\{userId}\\{fileId}";
            var outputPath = $@"C:\\CloudStoragePlatform\\thumbnails\\{fileId}.png";
            var overlayPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "filmframe.png");
            var ffmpeg = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "ffmpeg.exe");

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = $"-i \"{inputPath}\" -i \"{overlayPath}\" -ss 00:00:01 -vframes 1 " +
                            "-filter_complex \"[0:v]scale=200:-1[bg];[bg][1:v]overlay=(main_w-overlay_w)/2:(main_h-overlay_h)/2\" " +
                            $"\"{outputPath}\" -y",
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            await process.WaitForExitAsync();
        }

        public byte[]? GetThumbnail(Guid fileId)
        {
            var thumbPath = $@"C:\\CloudStoragePlatform\\thumbnails\\{fileId}.png";
            if (!File.Exists(thumbPath))
            {
                return null;
            }
            var bytes = File.ReadAllBytes(thumbPath);
            return bytes;
        }

        public void DeleteThumbnail(Guid fileId)
        {
            var thumbPath = $@"C:\\CloudStoragePlatform\\thumbnails\\{fileId}.png";
            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
        }
    }
}
