using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkImageResizer
{
    public class SKImageProcess
    {
        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImages(string sourcePath, string destPath, double scale)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            var allFiles = FindImages(sourcePath);
            foreach (var filePath in allFiles)
            {
                var bitmap = SKBitmap.Decode(filePath);
                var imgPhoto = SKImage.FromBitmap(bitmap);
                var imgName = Path.GetFileNameWithoutExtension(filePath);

                var sourceWidth = imgPhoto.Width;
                var sourceHeight = imgPhoto.Height;

                var destinationWidth = (int)(sourceWidth * scale);
                var destinationHeight = (int)(sourceHeight * scale);

                using var scaledBitmap = bitmap.Resize(
                    new SKImageInfo(destinationWidth, destinationHeight),
                    SKFilterQuality.High);
                using var scaledImage = SKImage.FromBitmap(scaledBitmap);
                using var data = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);
                using var s = File.OpenWrite(Path.Combine(destPath, imgName + ".jpg"));
                data.SaveTo(s);
            }
        }

        public async Task ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            await Task.Yield();

            var allFiles = FindImages(sourcePath);
            List<Task> tasks = new List<Task>();
            foreach (var filePath in allFiles)
            {
                tasks.Add(Task.Run(() =>
                {
                    var bitmap = SKBitmap.Decode(filePath);
                    var imgPhoto = SKImage.FromBitmap(bitmap);
                    var imgName = Path.GetFileNameWithoutExtension(filePath);

                    var sourceWidth = imgPhoto.Width;
                    var sourceHeight = imgPhoto.Height;

                    var destinationWidth = (int)(sourceWidth * scale);
                    var destinationHeight = (int)(sourceHeight * scale);

                    using var scaledBitmap = bitmap.Resize(
                        new SKImageInfo(destinationWidth, destinationHeight),
                        SKFilterQuality.High);
                    using var scaledImage = SKImage.FromBitmap(scaledBitmap);
                    using var data = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);
                    using var s = File.OpenWrite(Path.Combine(destPath, imgName + ".jpg"));
                    data.SaveTo(s);
                }));
            }

            await Task.WhenAll(tasks);
        }

        private void work(string filePath, string destPath, double scale)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            var bitmap = SKBitmap.Decode(filePath);
            var imgPhoto = SKImage.FromBitmap(bitmap);
            var imgName = Path.GetFileNameWithoutExtension(filePath);

            var sourceWidth = imgPhoto.Width;
            var sourceHeight = imgPhoto.Height;

            var destinationWidth = (int)(sourceWidth * scale);
            var destinationHeight = (int)(sourceHeight * scale);
            //Console.WriteLine("a" + Thread.CurrentThread.ManagedThreadId + "_" + sw.ElapsedMilliseconds);

            using var scaledBitmap = bitmap.Resize(
            new SKImageInfo(destinationWidth, destinationHeight),
            SKFilterQuality.High);
            //Console.WriteLine("b" + Thread.CurrentThread.ManagedThreadId + "_" + sw.ElapsedMilliseconds);
            using var scaledImage = SKImage.FromBitmap(scaledBitmap);
            //Console.WriteLine("c" + Thread.CurrentThread.ManagedThreadId + "_" + sw.ElapsedMilliseconds);
            using var data = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);
            //Console.WriteLine("d" + Thread.CurrentThread.ManagedThreadId + "_" + sw.ElapsedMilliseconds);
            using var s = File.OpenWrite(Path.Combine(destPath, imgName + ".jpg"));
            //Console.WriteLine("e" + Thread.CurrentThread.ManagedThreadId + "_" + sw.ElapsedMilliseconds);
            data.SaveTo(s);
            //a6_35
            //b6_723
            //c6_734
            //d6_945
            //e6_948

        }

        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }
    }
}
