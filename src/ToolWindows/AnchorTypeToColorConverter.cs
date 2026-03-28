using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Community.VisualStudio.Toolkit;

namespace CommentsVS.ToolWindows
{
    /// <summary>
    /// Converts an AnchorType to its corresponding color from the classification format map.
    /// </summary>
    public class AnchorTypeToColorConverter : IValueConverter
    {
        private static AnchorColorService _colorService;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AnchorType anchorType)
            {
                // Get the MEF service on first use
                if (_colorService == null)
                {
                    _colorService = VS.GetMefService<AnchorColorService>();
                }

                if (_colorService != null)
                {
                    var color = _colorService.GetColor(anchorType);
                    return new SolidColorBrush(color);
                }

                // Fallback if service not available
                var fallbackColor = GetFallbackColor(anchorType);
                return new SolidColorBrush(fallbackColor);
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static Color GetFallbackColor(AnchorType anchorType)
        {
            return anchorType switch
            {
                AnchorType.Todo => Colors.Orange,
                AnchorType.Hack => Colors.Crimson,
                AnchorType.Note => Colors.LimeGreen,
                AnchorType.Bug => Colors.Red,
                AnchorType.Fixme => Colors.OrangeRed,
                AnchorType.Undone => Colors.MediumPurple,
                AnchorType.Review => Colors.DodgerBlue,
                AnchorType.Anchor => Colors.Teal,
                AnchorType.Custom => Colors.Goldenrod,
                _ => Colors.Gray
            };
        }
    }
}