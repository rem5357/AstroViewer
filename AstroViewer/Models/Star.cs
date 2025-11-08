namespace AstroViewer.Models;

/// <summary>
/// Represents a star extracted from an Astrosynthesis .AstroDB file
/// </summary>
public class Star
{
    /// <summary>
    /// Unique identifier from the database
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Star name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Spectral classification (e.g., "G3V", "M4V", "O1V")
    /// </summary>
    public string SpectralType { get; set; } = string.Empty;

    /// <summary>
    /// Star radius in solar radii
    /// </summary>
    public double RadiusSolar { get; set; }

    /// <summary>
    /// Star mass in solar masses
    /// </summary>
    public double MassSolar { get; set; }

    /// <summary>
    /// Star luminosity in solar luminosities
    /// </summary>
    public double LuminositySolar { get; set; }

    /// <summary>
    /// Surface temperature in Kelvin (may be 0 if not calculated)
    /// </summary>
    public double TemperatureK { get; set; }

    /// <summary>
    /// X coordinate in light-years
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y coordinate in light-years
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Z coordinate in light-years
    /// </summary>
    public double Z { get; set; }

    /// <summary>
    /// System name if this is part of a multi-star system (null for single-star systems)
    /// </summary>
    public string? SystemName { get; set; }

    /// <summary>
    /// System container X coordinate (same as X for single stars)
    /// </summary>
    public double SystemX { get; set; }

    /// <summary>
    /// System container Y coordinate (same as Y for single stars)
    /// </summary>
    public double SystemY { get; set; }

    /// <summary>
    /// System container Z coordinate (same as Z for single stars)
    /// </summary>
    public double SystemZ { get; set; }

    /// <summary>
    /// Indicates if this star is part of a multi-star system
    /// </summary>
    public bool IsMultiStar => !string.IsNullOrEmpty(SystemName);

    /// <summary>
    /// Distance from origin (0,0,0) in light-years
    /// </summary>
    public double DistanceFromOrigin => Math.Sqrt(SystemX * SystemX + SystemY * SystemY + SystemZ * SystemZ);

    /// <summary>
    /// Gets the spectral class letter (O, B, A, F, G, K, M)
    /// </summary>
    public char SpectralClass
    {
        get
        {
            if (string.IsNullOrEmpty(SpectralType))
                return 'M'; // Default to M-class if unknown

            char firstChar = SpectralType[0];
            return char.IsLetter(firstChar) ? char.ToUpper(firstChar) : 'M';
        }
    }

    /// <summary>
    /// Gets the spectral subclass number (0-9)
    /// </summary>
    public int SpectralSubclass
    {
        get
        {
            if (string.IsNullOrEmpty(SpectralType) || SpectralType.Length < 2)
                return 5; // Default to middle of range

            return char.IsDigit(SpectralType[1]) ? int.Parse(SpectralType[1].ToString()) : 5;
        }
    }

    /// <summary>
    /// Gets the luminosity class (I, II, III, IV, V, VI, VII)
    /// </summary>
    public string LuminosityClass
    {
        get
        {
            if (string.IsNullOrEmpty(SpectralType))
                return "V"; // Default to main sequence

            // Look for roman numerals at the end of spectral type
            int vIndex = SpectralType.IndexOf('V');
            if (vIndex >= 0)
                return SpectralType.Substring(vIndex);

            return "V"; // Default to main sequence
        }
    }

    public override string ToString()
    {
        if (IsMultiStar)
            return $"{Name} ({SpectralType}) in {SystemName} system at ({SystemX:F1}, {SystemY:F1}, {SystemZ:F1})";
        else
            return $"{Name} ({SpectralType}) at ({X:F1}, {Y:F1}, {Z:F1})";
    }
}
