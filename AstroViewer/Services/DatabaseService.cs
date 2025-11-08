using Microsoft.Data.Sqlite;
using AstroViewer.Models;
using System.IO;

namespace AstroViewer.Services;

/// <summary>
/// Service for reading Astrosynthesis .AstroDB files (SQLite databases)
/// </summary>
public class DatabaseService : IDisposable
{
    private SqliteConnection? _connection;
    private bool _disposed = false;

    /// <summary>
    /// Opens a connection to an .AstroDB file
    /// </summary>
    /// <param name="dbPath">Path to the .AstroDB file</param>
    /// <returns>True if connection successful</returns>
    public async Task<bool> OpenDatabaseAsync(string dbPath)
    {
        try
        {
            if (!File.Exists(dbPath))
                return false;

            // Close existing connection if any
            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadOnly,
                Cache = SqliteCacheMode.Shared
            }.ToString();

            _connection = new SqliteConnection(connectionString);
            await _connection.OpenAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reads all stars from the database (both single-star and multi-star systems)
    /// </summary>
    /// <returns>List of all stars</returns>
    public async Task<List<Star>> ReadAllStarsAsync()
    {
        if (_connection == null)
            throw new InvalidOperationException("Database not opened");

        var stars = new List<Star>();

        // Query 1: Single-star systems
        const string singleStarQuery = @"
            SELECT id, name, spectral, radius, mass, luminosity, temp, x, y, z
            FROM bodies
            WHERE system_id = id AND parent_id = 0
            AND spectral != '' AND spectral IS NOT NULL
            ORDER BY name";

        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = singleStarQuery;
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var star = new Star
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    SpectralType = reader.GetString(2),
                    RadiusSolar = reader.GetDouble(3),
                    MassSolar = reader.GetDouble(4),
                    LuminositySolar = reader.GetDouble(5),
                    TemperatureK = reader.GetDouble(6),
                    X = reader.GetDouble(7),
                    Y = reader.GetDouble(8),
                    Z = reader.GetDouble(9),
                    SystemName = null, // Single-star system
                    SystemX = reader.GetDouble(7), // Same as star position
                    SystemY = reader.GetDouble(8),
                    SystemZ = reader.GetDouble(9)
                };
                stars.Add(star);
            }
        }

        // Query 2: Multi-star component stars
        const string multiStarQuery = @"
            SELECT b.id, b.name, b.spectral, b.radius, b.mass, b.luminosity, b.temp,
                   b.x, b.y, b.z, c.name, c.x, c.y, c.z
            FROM bodies b
            JOIN bodies c ON b.parent_id = c.id
            WHERE c.system_id = c.id AND c.parent_id = 0
            AND (c.spectral = '' OR c.spectral IS NULL)
            AND b.spectral != '' AND b.spectral IS NOT NULL
            ORDER BY c.name, b.name";

        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = multiStarQuery;
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var star = new Star
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    SpectralType = reader.GetString(2),
                    RadiusSolar = reader.GetDouble(3),
                    MassSolar = reader.GetDouble(4),
                    LuminositySolar = reader.GetDouble(5),
                    TemperatureK = reader.GetDouble(6),
                    X = reader.GetDouble(7),
                    Y = reader.GetDouble(8),
                    Z = reader.GetDouble(9),
                    SystemName = reader.GetString(10), // Container name
                    SystemX = reader.GetDouble(11),    // Container position
                    SystemY = reader.GetDouble(12),
                    SystemZ = reader.GetDouble(13)
                };
                stars.Add(star);
            }
        }

        return stars;
    }

    /// <summary>
    /// Gets the count of stars in the database
    /// </summary>
    /// <returns>Total number of stars (including multi-star components)</returns>
    public async Task<int> GetStarCountAsync()
    {
        if (_connection == null)
            throw new InvalidOperationException("Database not opened");

        const string query = @"
            SELECT
                (SELECT COUNT(*) FROM bodies
                 WHERE system_id = id AND parent_id = 0
                 AND spectral != '' AND spectral IS NOT NULL) +
                (SELECT COUNT(*) FROM bodies b
                 JOIN bodies c ON b.parent_id = c.id
                 WHERE c.system_id = c.id AND c.parent_id = 0
                 AND (c.spectral = '' OR c.spectral IS NULL)
                 AND b.spectral != '' AND b.spectral IS NOT NULL) as total";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = query;
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Gets the sector name from the database
    /// </summary>
    /// <returns>Sector name or empty string if not found</returns>
    public async Task<string> GetSectorNameAsync()
    {
        if (_connection == null)
            throw new InvalidOperationException("Database not opened");

        try
        {
            const string query = "SELECT name FROM sector_info LIMIT 1";
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets multi-star systems grouped by system name with the largest star from each
    /// </summary>
    /// <returns>Dictionary mapping system names to their largest component star</returns>
    public async Task<Dictionary<string, Star>> GetLargestStarsFromMultiSystemsAsync()
    {
        if (_connection == null)
            throw new InvalidOperationException("Database not opened");

        var result = new Dictionary<string, Star>();

        const string query = @"
            SELECT b.id, b.name, b.spectral, b.radius, b.mass, b.luminosity, b.temp,
                   b.x, b.y, b.z, c.name, c.x, c.y, c.z
            FROM bodies b
            JOIN bodies c ON b.parent_id = c.id
            WHERE c.system_id = c.id AND c.parent_id = 0
            AND (c.spectral = '' OR c.spectral IS NULL)
            AND b.spectral != '' AND b.spectral IS NOT NULL
            ORDER BY c.name, b.radius DESC, b.luminosity DESC";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = query;
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            string systemName = reader.GetString(10);

            // Only add the first (largest) star for each system
            if (!result.ContainsKey(systemName))
            {
                var star = new Star
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    SpectralType = reader.GetString(2),
                    RadiusSolar = reader.GetDouble(3),
                    MassSolar = reader.GetDouble(4),
                    LuminositySolar = reader.GetDouble(5),
                    TemperatureK = reader.GetDouble(6),
                    X = reader.GetDouble(7),
                    Y = reader.GetDouble(8),
                    Z = reader.GetDouble(9),
                    SystemName = systemName,
                    SystemX = reader.GetDouble(11),
                    SystemY = reader.GetDouble(12),
                    SystemZ = reader.GetDouble(13)
                };
                result[systemName] = star;
            }
        }

        return result;
    }

    /// <summary>
    /// Closes the database connection
    /// </summary>
    public async Task CloseAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
            _connection = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }
}
