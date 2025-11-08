# AstroViewer

A C# WPF application for visualizing and exploring Astrosynthesis 3.0 stellar cartography data in 3D.

## Overview

AstroViewer is a 3D stellar viewer that reads Astrosynthesis 3.0 .AstroDB files (SQLite databases) and renders star systems in an interactive 3D environment. It provides intuitive camera controls and filtering options for exploring stellar data.

## Features

- **3D Star Visualization**: View stars in true 3D space with accurate positioning from Astrosynthesis data
- **Interactive Camera Controls**:
  - Left Click + Drag: Rotate view
  - Mouse Wheel: Zoom in/out
  - Middle Mouse Button + Drag: Pan view
- **Stellar Classification Colors**: Stars are colored based on their spectral type (O, B, A, F, G, K, M)
- **Perspective-Based Sizing**: Stars render with sizes based on distance (up to 50 light-years)
- **System Labels**: Toggle display of system names for stars within 25 light-years
- **Multi-Star System Support**: Handles both single-star and multi-star systems, displaying the largest star from each system

## Requirements

- Windows 10/11
- .NET 9.0 Runtime
- Astrosynthesis 3.0 .AstroDB database files

## Technology Stack

- **.NET 9.0** with C# 12
- **WPF** (Windows Presentation Foundation)
- **Helix Toolkit WPF** for 3D rendering
- **Microsoft.Data.Sqlite** for database access

## Getting Started

1. Launch AstroViewer
2. Click "Open .AstroDB File..." button
3. Select an Astrosynthesis database file
4. Explore the 3D star field using mouse controls
5. Toggle system name labels using the checkbox

## Database Format

AstroViewer reads Astrosynthesis 3.0 .AstroDB files, which are SQLite databases containing:
- Star positions (X, Y, Z coordinates in light-years)
- Spectral classifications (O, B, A, F, G, K, M types)
- Physical properties (radius, mass, luminosity, temperature)
- Multi-star system relationships

See `AstroSQL.md` for complete database schema documentation.

## Spectral Classification Colors

- **O-class** (Blue): 30,000+ K
- **B-class** (Blue-White): 10,000-30,000 K
- **A-class** (White): 7,500-10,000 K
- **F-class** (Yellow-White): 6,000-7,500 K
- **G-class** (Yellow/Sun-like): 5,200-6,000 K
- **K-class** (Orange): 3,700-5,200 K
- **M-class** (Red): 2,400-3,700 K

## Project Structure

```
AstroViewer/
├── AstroViewer/
│   ├── Models/           # Data models (Star, etc.)
│   ├── Services/         # Business logic (DatabaseService, StarColorMapper)
│   ├── MainWindow.xaml   # Main UI
│   └── App.xaml          # Application entry point
├── AstroSQL.md           # Database schema documentation
├── Project.md            # Project management document
└── README.md             # This file
```

## Version History

- **v0.01 Build 1** (2025-11-07) - Initial release
  - Basic 3D star visualization
  - File loading from .AstroDB files
  - Camera controls (rotation, zoom, pan)
  - Spectral classification coloring
  - System name labels with distance filtering
  - Multi-star system support

## Future Enhancements

- Routes and connections between systems
- Advanced filtering and search capabilities
- 2D map export
- Screenshot capture
- Planet and moon visualization
- Performance optimizations for large datasets

## License

Copyright © 2025 AstroViewer Project

## Links

- [Astrosynthesis by NBOS Software](https://www.nbos.com/products/astrosynthesis)
- [Helix Toolkit](https://github.com/helix-toolkit/helix-toolkit)
- Project Documentation: See `Project.md`

## Support

For issues and feature requests, please use the GitHub issue tracker.
