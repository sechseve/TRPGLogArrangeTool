using System;
using System.Globalization;
using System.Windows.Data;
using TRPGLogArrangeTool.resource;

namespace TRPGLogArrangeTool
{
    public class ImageKeyToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string key && !string.IsNullOrEmpty(key))
            {
                // キャッシュを通じてBitmapImageを取得
                return ImageCache.GetByKey(key);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

