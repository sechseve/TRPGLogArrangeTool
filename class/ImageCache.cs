using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TRPGLogArrangeTool.resource
{
    public static class ImageCache
    {
        /// <summary>
        /// 画像情報一覧(画像化)
        /// </summary>
        private static readonly Dictionary<string, BitmapImage> _cache = new Dictionary<string, BitmapImage>();
        /// <summary>
        /// 画像情報一覧(byte化データ)
        /// </summary>
        private static readonly Dictionary<string, byte[]> _rawCache = new Dictionary<string, byte[]>(); // 元データ保持領域

        /// <summary>
        /// 名称ハッシュ化処理
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string ComputeHash(byte[] bytes)
        {
            using (var sha1 = SHA1.Create())
            {
                return BitConverter.ToString(sha1.ComputeHash(bytes)).Replace("-", "");
            }
        }
        /// <summary>
        /// 画像追加処理
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static BitmapImage GetOrAddFromFile(string path, out string key)
        {
            if (!File.Exists(path))
            {
                key = null;
                return null;
            }
            try
            {

                var bytes = File.ReadAllBytes(path);
                key = ComputeHash(bytes);

                if (_cache.TryGetValue(key, out var bmp))
                {
                    return bmp;
                }

                var bitmap = new BitmapImage();
                using (var ms = new MemoryStream(bytes))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }

                _cache[key] = bitmap;
                _rawCache[key] = bytes;
                return bitmap;
            }
            catch
            {
                key = null;
                return null;
            }
        }
        /// <summary>
        /// 画像追加処理(ファイルストリーム処理)
        /// </summary>
        /// <param name="base64"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static BitmapImage GetOrAddFromBase64(string base64, out string key)
        {
            var bytes = Convert.FromBase64String(base64);
            key = ComputeHash(bytes);

            if (_cache.TryGetValue(key, out var bmp))
            {
                return bmp;
            }

            var bitmap = new BitmapImage();
            using (var ms = new MemoryStream(bytes))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            _cache[key] = bitmap;
            _rawCache[key] = bytes;
            return bitmap;
        }

        public static ImageSource GetImageSource(string key)
        {
            if (_rawCache.TryGetValue(key, out var bytes))
            {
                var ms = new MemoryStream(bytes);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            return null;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public static void Clear()
        {
            _cache.Clear();
            _rawCache.Clear();
        }

        /// <summary>
        /// 指定キーのキャッシュを削除
        /// </summary>
        public static void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            _cache.Remove(key);
            _rawCache.Remove(key);
        }
        public static BitmapImage GetByKey(string key)
            => _cache.TryGetValue(key, out var bmp) ? bmp : null;

        public static string GetBase64ByKey(string key)
        {
            if (_rawCache.TryGetValue(key, out var bytes))
            {
                return Convert.ToBase64String(bytes);
            }
            return null;
        }
    }
}
