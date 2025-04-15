using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public class ThumbnailService
    {
        public async Task GenerateImageThumbnail(Guid id, string filePath, bool isGIF)
        {
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
                System.IO.File.WriteAllBytes(@"C:\CloudStoragePlatform\thumbnails\" + id.ToString()+".png", ms.ToArray());
            });
        }

        public async Task GenerateVideoThumbnail(Guid id, string filePath)
        {
            var outputPath = @"C:\CloudStoragePlatform\thumbnails\" + id + ".png";
            var overlayPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "filmframe.png");
            var ffmpeg = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "ffmpeg.exe");

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = $"-i \"{filePath}\" -i \"{overlayPath}\" -ss 00:00:01 -vframes 1 " +
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


        public static byte[]? GetThumbnail(Guid id) 
        {
            if (!File.Exists(@"C:\CloudStoragePlatform\thumbnails\" + id+".png"))
            {
                return null;
            }
            var bytes = File.ReadAllBytes(@"C:\CloudStoragePlatform\thumbnails\" + id + ".png");
            return bytes;
        }

        public void DeleteThumbnail(Guid id) 
        {

        }
    }
}
