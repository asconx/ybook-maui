using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace yBook.Converters
{
    public class HexToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
            {
                try
                {
                    // Handle various hex formats: #fff, #ffffff, fff, ffffff
                    if (hexColor.StartsWith("#"))
                        hexColor = hexColor.Substring(1);

                    // Convert short hex to long hex (e.g., fff → ffffff)
                    if (hexColor.Length == 3)
                    {
                        hexColor = string.Concat(
                            hexColor[0], hexColor[0],
                            hexColor[1], hexColor[1],
                            hexColor[2], hexColor[2]
                        );
                    }

                    // Parse as hex
                    if (int.TryParse(hexColor, System.Globalization.NumberStyles.HexNumber, null, out int colorInt))
                    {
                        // Extract RGB components
                        int r = (colorInt >> 16) & 0xFF;
                        int g = (colorInt >> 8) & 0xFF;
                        int b = colorInt & 0xFF;

                        return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
                    }
                }
                catch
                {
                    // Fall back to default color
                }
            }

            return Colors.White; // Default fallback
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
