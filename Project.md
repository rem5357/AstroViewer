# AstroViewer Project

## Project Overview
AstroViewer is a C# WPF application for visualizing and exploring Astrosynthesis 3.0 stellar cartography data. It provides an interactive 3D viewer for star systems with advanced camera controls and filtering options.

## Version Information
- **Current Version:** 0.01
- **Current Build:** 1
- **Last Updated:** 2025-11-07

### Version History
- **v0.01 Build 1** (2025-11-07) - Initial project setup

## Project Goals
Create a 3D stellar viewer that:
- Reads Astrosynthesis 3.0 .AstroDB files (SQLite databases)
- Displays stars in 3D space with accurate positioning
- Provides intuitive camera controls for navigation
- Renders stars with colors and sizes based on stellar classification
- Shows system names for nearby stars (configurable)
- Handles both single-star and multi-star systems

## Technology Stack

### Core Framework
- **.NET 8.0** - Modern .NET platform
- **C# 12** - Latest C# language features
- **WPF (Windows Presentation Foundation)** - UI framework

### Key Libraries
- **Microsoft.Data.Sqlite** - SQLite database access for .AstroDB files
- **Helix Toolkit WPF** - 3D graphics rendering and visualization
- **System.Data.SQLite** (if needed) - Alternative SQLite provider

### Development Tools
- **Visual Studio 2022** (or VS Code with C# Dev Kit)
- **Git** - Version control
- **GitHub** - Remote repository hosting

## Features

### Phase 1 (Current - v0.01)
- [x] Project initialization
- [ ] File picker for .AstroDB files
- [ ] 3D viewport with basic rendering
- [ ] Camera controls:
  - Left click + drag: Rotate view
  - Middle mouse scroll: Zoom in/out
  - Middle mouse button + drag: Pan view
- [ ] Star rendering:
  - Color based on spectral classification (O, B, A, F, G, K, M)
  - Size based on classification and distance
  - Perspective scaling (50 ly max distance, 1 pixel minimum)
- [ ] System name labels (toggle checkbox)
  - Show/hide names for systems within 25 light-years
- [ ] Multi-star system handling
  - Display the larger star from multi-star systems

### Future Phases
- Phase 2: Routes and connections between systems
- Phase 3: Advanced filtering and search
- Phase 4: Export capabilities (2D maps, screenshots)
- Phase 5: Planet and moon visualization

## Database Schema Knowledge

### Key Tables (from AstroSQL.md)
- **bodies** (63 columns) - Primary table containing stars, planets, moons
- **routes** - Interstellar travel routes
- **sector_info** - Sector-level metadata
- **subsectors** - Grid-based spatial divisions

### Star Identification Logic
```sql
-- Single-star systems
WHERE system_id = id AND parent_id = 0
AND spectral != '' AND spectral IS NOT NULL

-- Multi-star containers
WHERE system_id = id AND parent_id = 0
AND (spectral = '' OR spectral IS NULL)

-- Component stars in multi-star systems
WHERE parent_id = container_id AND spectral != ''
```

### Important Fields
- `id` - Unique body ID
- `name` - Display name
- `x, y, z` - 3D coordinates in light-years
- `spectral` - Star classification (e.g., "G3V", "M4V", "O1V")
- `luminosity` - Stellar luminosity in solar units
- `radius` - Body radius in solar radii
- `mass` - Body mass in solar masses
- `system_id` - ID of parent star system
- `parent_id` - ID of body this orbits

## Stellar Classification Color Mapping

### Spectral Classes (Hottest to Coolest)
- **O-class** (Blue) - 30,000+ K - RGB: (155, 176, 255)
- **B-class** (Blue-White) - 10,000-30,000 K - RGB: (170, 191, 255)
- **A-class** (White) - 7,500-10,000 K - RGB: (202, 215, 255)
- **F-class** (Yellow-White) - 6,000-7,500 K - RGB: (248, 247, 255)
- **G-class** (Yellow) - 5,200-6,000 K - RGB: (255, 244, 234) [Sun-like]
- **K-class** (Orange) - 3,700-5,200 K - RGB: (255, 210, 161)
- **M-class** (Red) - 2,400-3,700 K - RGB: (255, 204, 111)

### Size Classes (Luminosity)
- I - Supergiant
- II - Bright giant
- III - Giant
- IV - Subgiant
- V - Main sequence (dwarf)
- VI - Subdwarf
- VII - White dwarf

## Project Structure
```
AstroViewer/
├── AstroViewer.sln
├── AstroViewer/
│   ├── AstroViewer.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── Models/
│   │   ├── Star.cs
│   │   └── StarSystem.cs
│   ├── Services/
│   │   ├── DatabaseService.cs
│   │   └── StarColorMapper.cs
│   ├── ViewModels/
│   │   └── MainViewModel.cs
│   └── Views/
│       └── (Additional views as needed)
├── AstroSQL.md
├── Project.md
├── README.md
└── .gitignore
```

## Camera Controls Reference
- **Left Click + Drag** - Rotate view around center
- **Middle Mouse Scroll** - Zoom in/out
- **Middle Mouse Button + Drag** - Pan view
- **Right Click** (future) - Context menu for selected objects

## Screenshots
Screenshots are stored in: `D:\Dropbox\Screenshots\`

## Lessons Learned

### Build 1 (2025-11-07)
- Astrosynthesis uses SQLite format (.AstroDB) making direct database access straightforward
- Multi-star systems require special handling - container vs. component stars
- Spectral type field is the key discriminator for actual stars vs. system containers
- Need to handle systems with no stars in them (gas giants, stations only)

## Known Issues & Considerations

### Database Schema
- No formal foreign key constraints - relationships maintained via parent_id/system_id
- Temperature field often 0 even with valid spectral types
- Component stars in multi-star systems share container position (no barycentric offset)

### Rendering Challenges
- Perspective scaling for stars at varying distances
- Performance with large datasets (hundreds of stars)
- Label overlap prevention for dense star fields
- Multi-star system representation (single point vs. multiple points)

## Development Guidelines

### Version Management
- Version number increments on user command only
- Build number increments with each compilation
- Version format: `MAJOR.MINOR` (e.g., 0.01, 0.02, 1.00)
- Build format: Sequential integer (1, 2, 3...)

### Code Standards
- Follow C# naming conventions (PascalCase for public members)
- Use async/await for database operations
- MVVM pattern for WPF architecture
- Comprehensive XML documentation comments
- Unit tests for data access layer (future)

### Git Workflow
- Commit after each significant feature completion
- Meaningful commit messages following convention:
  - `feat:` New features
  - `fix:` Bug fixes
  - `docs:` Documentation changes
  - `refactor:` Code refactoring
  - `test:` Test additions/changes
  - `build:` Build system changes

## Testing Strategy

### Test Data
- **TotalSystem.AstroDB** - Reference database with:
  - 482 total stars
  - 246 single-star systems
  - 103 multi-star containers (236 component stars)
  - Spectral types from O to M class

### Test Scenarios
1. Load database with single-star systems only
2. Load database with multi-star systems
3. Verify star colors match spectral classification
4. Test camera controls (rotation, zoom, pan)
5. Test label toggle with distance filtering (25 ly)
6. Performance test with full dataset (482 stars)

## Performance Targets
- Load time: < 2 seconds for 500 stars
- Rendering: 60 FPS with 500 visible stars
- Memory: < 200 MB for typical datasets
- Camera response: < 16ms (60 FPS)

## Resources

### Documentation
- AstroSQL.md - Complete database schema reference
- Astrosynthesis Manual - User guide for data format
- Helix Toolkit Documentation - 3D rendering API
- SQLite Documentation - Database query reference

### External Links
- NBOS Astrosynthesis: https://www.nbos.com/products/astrosynthesis
- Helix Toolkit: https://github.com/helix-toolkit/helix-toolkit
- SQLite: https://www.sqlite.org/

## Contact & Support
- GitHub Issues: (URL will be added after repo creation)
- Project Repository: (URL will be added after repo creation)

---

**Last Modified:** 2025-11-07
**Next Build:** 2 (pending first compilation)
**Next Version:** 0.01 (awaiting user increment command)
