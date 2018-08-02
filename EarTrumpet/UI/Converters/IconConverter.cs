﻿using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.UI.Converters
{
    public class IconConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageSource ret = null;
            var iconInfo = (IconLoadInfo)value;

            if (iconInfo.CachedValue != null)
            {
                return iconInfo.CachedValue;
            }

            try
            {
                if (iconInfo.IsDesktopApp)
                {
                    if (!string.IsNullOrWhiteSpace(iconInfo.IconPath))
                    {
                        var iconPath = new StringBuilder(iconInfo.IconPath);
                        int iconIndex = Shlwapi.PathParseIconLocationW(iconPath);
                        if (iconIndex != 0)
                        {
                            ret = GetIconFromFile(iconPath.ToString(), iconIndex);
                        }
                        else
                        {
                            ret = System.Drawing.Icon.ExtractAssociatedIcon(iconInfo.IconPath).ToImageSource();
                        }
                    }
                }
                else
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(iconInfo.IconPath);
                    bitmap.EndInit();
                    ret = bitmap;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to load icon: {ex}");
            }

            iconInfo.CachedValue = ret;
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private ImageSource GetIconFromFile(string path, int iconIndex = 0)
        {
            IntPtr iconHandle = IntPtr.Zero;
            try
            {
                iconHandle = Shell32.ExtractIcon(Process.GetCurrentProcess().Handle, path, iconIndex);
                return Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                User32.DestroyIcon(iconHandle);
            }
        }
    }
}