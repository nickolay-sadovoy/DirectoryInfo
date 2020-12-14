﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace DirectoryInfo.App.Converters
{
    public class KbSizeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = (long)value;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
