using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using AstroViewer.Models;
using AstroViewer.Services;
using HelixToolkit.Wpf;

namespace AstroViewer;

/// <summary>
/// Main window for the AstroViewer application
/// </summary>
public partial class MainWindow : Window
{
    private readonly DatabaseService _dbService;
    private List<Star> _allStars = new();
    private List<Star> _displayedStars = new();
    private bool _showLabels = true;
    private Point3D _initialCameraPosition;
    private Vector3D _initialCameraLookDirection;
    private Vector3D _initialCameraUpDirection;
    private System.Windows.Threading.DispatcherTimer? _cameraUpdateTimer;

    public MainWindow()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        Loaded += MainWindow_Loaded;

        // Set up timer for dynamic updates when camera moves
        _cameraUpdateTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // Update every 100ms
        };
        _cameraUpdateTimer.Tick += CameraUpdateTimer_Tick;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateStatus("Ready. Open an .AstroDB file to begin.");
    }

    /// <summary>
    /// Opens a file dialog to select an .AstroDB file
    /// </summary>
    private async void BtnOpenFile_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Astrosynthesis Database File",
            Filter = "AstroDB Files (*.AstroDB)|*.AstroDB|All Files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (openFileDialog.ShowDialog() == true)
        {
            await LoadDatabaseAsync(openFileDialog.FileName);
        }
    }

    /// <summary>
    /// Loads a database file and renders the stars
    /// </summary>
    private async Task LoadDatabaseAsync(string filePath)
    {
        try
        {
            UpdateStatus("Opening database...");
            TxtFileName.Text = System.IO.Path.GetFileName(filePath);

            bool success = await _dbService.OpenDatabaseAsync(filePath);
            if (!success)
            {
                MessageBox.Show("Failed to open database file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("Failed to open database.");
                return;
            }

            UpdateStatus("Reading stars...");

            // Get sector name
            var sectorName = await _dbService.GetSectorNameAsync();
            if (!string.IsNullOrEmpty(sectorName))
            {
                Title = $"AstroViewer - {sectorName}";
            }

            // Read all stars
            _allStars = await _dbService.ReadAllStarsAsync();

            // For display, use only largest star from multi-star systems
            _displayedStars = await GetDisplayStarsAsync();

            UpdateStatus($"Loaded {_allStars.Count} stars ({_displayedStars.Count} systems). Rendering...");
            TxtStarCount.Text = $"Stars: {_allStars.Count} ({_displayedStars.Count} systems)";

            // Render the stars
            RenderStars(true); // true = initial load, zoom to extents

            // Store initial camera position for reset
            SaveCameraPosition();

            // Start the camera update timer for dynamic rendering
            _cameraUpdateTimer?.Start();

            UpdateStatus($"Ready. Viewing {_displayedStars.Count} star systems.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            UpdateStatus("Error loading database.");
        }
    }

    /// <summary>
    /// Gets the stars to display (single stars + largest from multi-star systems)
    /// </summary>
    private async Task<List<Star>> GetDisplayStarsAsync()
    {
        var displayStars = new List<Star>();

        // Add all single-star systems
        displayStars.AddRange(_allStars.Where(s => !s.IsMultiStar));

        // Add largest stars from multi-star systems
        var multiStarSystems = await _dbService.GetLargestStarsFromMultiSystemsAsync();
        displayStars.AddRange(multiStarSystems.Values);

        return displayStars;
    }

    /// <summary>
    /// Renders all stars in the 3D viewport
    /// </summary>
    /// <param name="initialLoad">If true, zoom to extents after rendering</param>
    private void RenderStars(bool initialLoad = false)
    {
        try
        {
            // Clear existing stars (keep the DefaultLights)
            var itemsToRemove = Viewport3D.Children
                .OfType<Visual3D>()
                .Where(v => !(v is DefaultLights))
                .ToList();

            foreach (var item in itemsToRemove)
            {
                Viewport3D.Children.Remove(item);
            }

            if (_displayedStars.Count == 0)
                return;

            // Get current camera position for distance calculations
            var camera = Viewport3D.Camera as PerspectiveCamera;
            if (camera == null)
                return;

            var cameraPosition = camera.Position;

            foreach (var star in _displayedStars)
            {
                // Star position
                var position = new Point3D(star.SystemX, star.SystemY, star.SystemZ);

                // Calculate distance from camera
                double distance = CalculateDistance(cameraPosition, position);

                // Get star color
                var color = StarColorMapper.GetStarColor(star);

                // Calculate render size - minimum 1 pixel for distant stars
                double size = CalculateStarSize(star, distance);

                // Create a sphere for the star with emissive glow
                var sphereVisual = new SphereVisual3D
                {
                    Center = position,
                    Radius = size,
                    Material = CreateStarMaterial(color)
                };

                // Add to the viewport
                Viewport3D.Children.Add(sphereVisual);

                // Add label if enabled and within 100 ly from camera
                if (_showLabels && distance <= 100.0)
                {
                    AddStarLabel(star, position, distance);
                }
            }

            // Only zoom to extents on initial load
            if (initialLoad)
            {
                Viewport3D.ZoomExtents();
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error rendering: {ex.Message}");
        }
    }

    /// <summary>
    /// Calculates star size based on distance - minimum 1 pixel equivalent
    /// </summary>
    private double CalculateStarSize(Star star, double distance)
    {
        // Base size from luminosity and radius
        double baseSizeFromLuminosity = Math.Log10(Math.Max(star.LuminositySolar, 0.01)) + 3.0;
        double baseSizeFromRadius = Math.Log10(Math.Max(star.RadiusSolar, 0.1)) + 2.0;
        double baseSize = Math.Max(baseSizeFromLuminosity, baseSizeFromRadius);
        baseSize = Math.Max(baseSize, 2.0);

        // Scale with distance - closer stars are larger
        double scaleFactor = 100.0 / Math.Max(distance, 1.0);

        // Reduced by 50% - multiply by 0.075 instead of 0.15
        double finalSize = baseSize * scaleFactor * 0.075;

        // Minimum size is 0.05 (approximately 1 pixel)
        return Math.Max(finalSize, 0.05);
    }

    /// <summary>
    /// Calculates distance between two 3D points
    /// </summary>
    private double CalculateDistance(Point3D point1, Point3D point2)
    {
        double dx = point2.X - point1.X;
        double dy = point2.Y - point1.Y;
        double dz = point2.Z - point1.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Saves the current camera position for reset
    /// </summary>
    private void SaveCameraPosition()
    {
        if (Viewport3D.Camera is PerspectiveCamera camera)
        {
            _initialCameraPosition = camera.Position;
            _initialCameraLookDirection = camera.LookDirection;
            _initialCameraUpDirection = camera.UpDirection;
        }
    }

    /// <summary>
    /// Creates a glowing material for stars (they are light sources!)
    /// </summary>
    private Material CreateStarMaterial(Color color)
    {
        // Create a material group with both diffuse and emissive components
        var materialGroup = new MaterialGroup();

        // Diffuse component (reflects light) - main color
        materialGroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(color)));

        // Emissive component (glows on its own) - toned down to 40% brightness
        var emissiveColor = Color.FromArgb(
            102, // Alpha at 40% (255 * 0.4 = 102)
            color.R,
            color.G,
            color.B
        );
        materialGroup.Children.Add(new EmissiveMaterial(new SolidColorBrush(emissiveColor)));

        return materialGroup;
    }

    /// <summary>
    /// Adds a text label for a star
    /// </summary>
    /// <param name="distance">Distance from camera (for bold effect)</param>
    private void AddStarLabel(Star star, Point3D position, double distance)
    {
        var displayName = star.IsMultiStar ? star.SystemName : star.Name;
        if (string.IsNullOrEmpty(displayName))
            return;

        // Make labels bold when within 25 ly, normal beyond
        var fontWeight = distance <= 25.0 ? FontWeights.Bold : FontWeights.Normal;

        // Offset label to the RIGHT by approximately 1 EM (1.5 units)
        // Positive X offset moves to the right
        var labelPosition = new Point3D(position.X + 1.5, position.Y, position.Z);

        var textVisual = new BillboardTextVisual3D
        {
            Text = displayName,
            Position = labelPosition,
            Foreground = Brushes.White,
            FontSize = 20,
            FontWeight = fontWeight
        };

        // Add directly to viewport
        Viewport3D.Children.Add(textVisual);
    }

    /// <summary>
    /// Timer tick event to update rendering based on camera changes
    /// </summary>
    private void CameraUpdateTimer_Tick(object? sender, EventArgs e)
    {
        // Only re-render if we have stars loaded
        if (_displayedStars.Count > 0)
        {
            RenderStars(false); // Don't zoom, just update
        }
    }

    /// <summary>
    /// Handles label checkbox changes
    /// </summary>
    private void ChkShowLabels_Changed(object sender, RoutedEventArgs e)
    {
        _showLabels = ChkShowLabels.IsChecked ?? false;

        if (_displayedStars.Count > 0)
        {
            RenderStars(false); // Don't reset view when toggling labels
        }
    }

    /// <summary>
    /// Resets the camera to the initial view
    /// </summary>
    private void BtnResetView_Click(object sender, RoutedEventArgs e)
    {
        if (Viewport3D.Camera is PerspectiveCamera camera)
        {
            // Restore initial camera position
            camera.Position = _initialCameraPosition;
            camera.LookDirection = _initialCameraLookDirection;
            camera.UpDirection = _initialCameraUpDirection;

            UpdateStatus("View reset to initial position.");
        }
    }

    /// <summary>
    /// Updates the status bar text
    /// </summary>
    private void UpdateStatus(string message)
    {
        TxtStatus.Text = message;
    }

    protected override void OnClosed(EventArgs e)
    {
        _cameraUpdateTimer?.Stop();
        _dbService?.Dispose();
        base.OnClosed(e);
    }
}