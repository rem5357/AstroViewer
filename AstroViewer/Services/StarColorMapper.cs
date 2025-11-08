using System.Windows.Media;
using AstroViewer.Models;

namespace AstroViewer.Services;

/// <summary>
/// Maps stellar spectral classifications to colors and sizes for rendering
/// </summary>
public static class StarColorMapper
{
    /// <summary>
    /// Gets the color for a star based on its spectral classification
    /// </summary>
    /// <param name="star">The star to get the color for</param>
    /// <returns>A Color representing the star's spectral class</returns>
    public static Color GetStarColor(Star star)
    {
        return GetStarColorBySpectralClass(star.SpectralClass);
    }

    /// <summary>
    /// Gets the color for a given spectral class
    /// </summary>
    /// <param name="spectralClass">The spectral class letter (O, B, A, F, G, K, M)</param>
    /// <returns>A Color representing the spectral class</returns>
    public static Color GetStarColorBySpectralClass(char spectralClass)
    {
        return spectralClass switch
        {
            'O' => Color.FromRgb(155, 176, 255),  // Blue - 30,000+ K
            'B' => Color.FromRgb(170, 191, 255),  // Blue-White - 10,000-30,000 K
            'A' => Color.FromRgb(202, 215, 255),  // White - 7,500-10,000 K
            'F' => Color.FromRgb(248, 247, 255),  // Yellow-White - 6,000-7,500 K
            'G' => Color.FromRgb(255, 244, 234),  // Yellow (Sun-like) - 5,200-6,000 K
            'K' => Color.FromRgb(255, 210, 161),  // Orange - 3,700-5,200 K
            'M' => Color.FromRgb(255, 204, 111),  // Red - 2,400-3,700 K
            _ => Color.FromRgb(255, 255, 255)     // White for unknown
        };
    }

    /// <summary>
    /// Gets the approximate temperature range for a spectral class
    /// </summary>
    /// <param name="spectralClass">The spectral class letter</param>
    /// <returns>A tuple with min and max temperature in Kelvin</returns>
    public static (double min, double max) GetTemperatureRange(char spectralClass)
    {
        return spectralClass switch
        {
            'O' => (30000, 60000),
            'B' => (10000, 30000),
            'A' => (7500, 10000),
            'F' => (6000, 7500),
            'G' => (5200, 6000),
            'K' => (3700, 5200),
            'M' => (2400, 3700),
            _ => (3000, 6000)
        };
    }

    /// <summary>
    /// Gets the base render size for a star based on its properties
    /// </summary>
    /// <param name="star">The star to get the size for</param>
    /// <param name="distanceFromCamera">Distance from camera in light-years</param>
    /// <param name="maxRenderDistance">Maximum render distance (default 50 ly)</param>
    /// <returns>Size in pixels (minimum 1)</returns>
    public static double GetStarRenderSize(Star star, double distanceFromCamera, double maxRenderDistance = 50.0)
    {
        if (distanceFromCamera > maxRenderDistance)
            return 1.0; // Don't render stars beyond max distance

        // Base size calculation using luminosity and radius
        // Brighter and larger stars should be more visible
        double baseSizeFromLuminosity = Math.Log10(Math.Max(star.LuminositySolar, 0.01)) + 3.0;
        double baseSizeFromRadius = Math.Log10(Math.Max(star.RadiusSolar, 0.1)) + 2.0;

        double baseSize = Math.Max(baseSizeFromLuminosity, baseSizeFromRadius);
        baseSize = Math.Max(baseSize, 2.0); // Minimum base size

        // Apply distance scaling
        // Stars get smaller with distance, down to 1 pixel at max distance
        double distanceFactor = 1.0 - (distanceFromCamera / maxRenderDistance);
        distanceFactor = Math.Max(distanceFactor, 0.1); // Minimum 10% size

        double finalSize = baseSize * distanceFactor;

        // Ensure minimum 1 pixel
        return Math.Max(finalSize, 1.0);
    }

    /// <summary>
    /// Determines if a star's label should be shown based on distance
    /// </summary>
    /// <param name="star">The star to check</param>
    /// <param name="maxLabelDistance">Maximum distance to show labels (default 25 ly)</param>
    /// <returns>True if the label should be shown</returns>
    public static bool ShouldShowLabel(Star star, double maxLabelDistance = 25.0)
    {
        return star.DistanceFromOrigin <= maxLabelDistance;
    }

    /// <summary>
    /// Gets a human-readable description of the spectral class
    /// </summary>
    /// <param name="spectralClass">The spectral class letter</param>
    /// <returns>Description of the star type</returns>
    public static string GetSpectralClassDescription(char spectralClass)
    {
        return spectralClass switch
        {
            'O' => "Blue Supergiant",
            'B' => "Blue Star",
            'A' => "White Star",
            'F' => "Yellow-White Star",
            'G' => "Yellow Star (Sun-like)",
            'K' => "Orange Dwarf",
            'M' => "Red Dwarf",
            _ => "Unknown Star Type"
        };
    }

    /// <summary>
    /// Gets the WPF Brush for rendering a star
    /// </summary>
    /// <param name="star">The star to get the brush for</param>
    /// <returns>A SolidColorBrush for rendering</returns>
    public static SolidColorBrush GetStarBrush(Star star)
    {
        var color = GetStarColor(star);
        return new SolidColorBrush(color);
    }
}
