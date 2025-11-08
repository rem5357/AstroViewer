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

    public MainWindow()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
        Loaded += MainWindow_Loaded;
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
            RenderStars();

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
    private void RenderStars()
    {
        StarsModelGroup.Children.Clear();

        if (_displayedStars.Count == 0)
            return;

        // Camera position for distance calculations
        var cameraPosition = new Point3D(0, 0, 0);

        foreach (var star in _displayedStars)
        {
            // Star position
            var position = new Point3D(star.SystemX, star.SystemY, star.SystemZ);

            // Calculate distance from camera/origin
            double distance = star.DistanceFromOrigin;

            // Skip stars beyond render distance
            if (distance > 50.0)
                continue;

            // Get star color
            var color = StarColorMapper.GetStarColor(star);

            // Calculate render size
            double size = StarColorMapper.GetStarRenderSize(star, distance, 50.0);

            // Create a sphere for the star
            var sphereVisual = new SphereVisual3D
            {
                Center = position,
                Radius = size * 0.2, // Scale down for better visibility
                Material = new DiffuseMaterial(new SolidColorBrush(color))
            };

            // Add to the scene
            var modelVisual = new ModelVisual3D();
            modelVisual.Content = sphereVisual.Content;
            StarsModelGroup.Children.Add(modelVisual.Content);

            // Add label if enabled and within range
            if (_showLabels && StarColorMapper.ShouldShowLabel(star, 25.0))
            {
                AddStarLabel(star, position);
            }
        }

        // Zoom to fit all stars
        Viewport3D.ZoomExtents();
    }

    /// <summary>
    /// Adds a text label for a star
    /// </summary>
    private void AddStarLabel(Star star, Point3D position)
    {
        var displayName = star.IsMultiStar ? star.SystemName : star.Name;
        if (string.IsNullOrEmpty(displayName))
            return;

        var textVisual = new BillboardTextVisual3D
        {
            Text = displayName,
            Position = new Point3D(position.X, position.Y + 0.5, position.Z),
            Foreground = Brushes.White,
            FontSize = 10,
            FontWeight = FontWeights.Normal
        };

        var modelVisual = new ModelVisual3D();
        modelVisual.Children.Add(textVisual);
        StarsModelGroup.Children.Add(modelVisual.Content);
    }

    /// <summary>
    /// Handles label checkbox changes
    /// </summary>
    private void ChkShowLabels_Changed(object sender, RoutedEventArgs e)
    {
        _showLabels = ChkShowLabels.IsChecked ?? false;

        if (_displayedStars.Count > 0)
        {
            RenderStars();
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
        _dbService?.Dispose();
        base.OnClosed(e);
    }
}