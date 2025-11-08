# AstroViewer Project

## Project Overview
AstroViewer is a C# WPF application for visualizing and exploring Astrosynthesis 3.0 stellar cartography data. It provides an interactive 3D viewer for star systems with advanced camera controls and filtering options.

## Version Information
- **Current Version:** 0.02
- **Current Build:** 3
- **Last Updated:** 2025-11-07

### Version History
- **v0.02 Build 3** (2025-11-07) - Production-ready 3D viewer with dynamic updates
  - Camera-perspective distance calculations
  - Dynamic label updates (100 ly range)
  - Emissive star materials with proper spectral colors
  - Real-time rendering updates (100ms timer)
  - Reset View button
  - All stars visible with minimum 1-pixel size
  - Label positioning and boldness based on proximity

- **v0.01 Build 2** (2025-11-07) - Bug fixes and improvements
  - Fixed star vanishing bug
  - Improved rendering stability
  - Camera-based distance calculations

- **v0.01 Build 1** (2025-11-07) - Initial project setup
  - Basic 3D rendering infrastructure
  - Database connectivity

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
- **.NET 9.0** - Modern .NET platform
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

### Phase 1 (v0.01 - v0.02) - COMPLETED ✅
- [x] Project initialization and setup
- [x] File picker for .AstroDB files
- [x] 3D viewport with Helix Toolkit
- [x] Camera controls (trackball rotation, wheel zoom, middle-button pan)
- [x] Star rendering with emissive materials (stars glow!)
- [x] Spectral classification coloring (O=Blue → M=Red)
- [x] Dynamic camera-based distance calculations
- [x] Perspective-based star sizing (all stars visible, minimum 1 pixel)
- [x] System name labels with toggle
  - Dynamic updates based on camera position
  - Show within 100 light-years
  - Bold when within 25 light-years
  - Positioned to right of stars
- [x] Multi-star system handling (largest star displayed)
- [x] Reset View button
- [x] Real-time updates (100ms refresh timer)
- [x] Stable rendering (fixed star vanishing bugs)

### Phase 2 (Next - v0.03+) - PLANNED 📋
- [ ] Improve label positioning and readability
  - Better offset calculations
  - Collision detection for overlapping labels
  - Distance-based font sizing
- [ ] 2D map export capabilities
  - Orthographic projection options
  - Export to PNG/SVG
  - Configurable view planes (XY, XZ, YZ)
- [ ] Routes and connections visualization
  - Read route data from database
  - Render travel paths between systems
  - Color-coded routes
- [ ] Enhanced UI/UX
  - Better color scheme
  - Info panel for selected stars
  - Search functionality
- [ ] Advanced filtering
  - Filter by spectral class
  - Filter by distance range
  - Filter by luminosity/size
- [ ] Performance optimizations
  - LOD (Level of Detail) system
  - Frustum culling
  - Reduce timer frequency for distant camera positions

### Future Phases (v0.10+)
- Phase 3: Planet and moon visualization
- Phase 4: Subsector grid overlay
- Phase 5: Custom star data editing
- Phase 6: Animation and time-based changes

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

## Lessons Learned

### Build 1 (2025-11-07)
- Helix Toolkit WPF integrates well with .NET 9 despite framework warnings
- SQLite direct access is straightforward with Microsoft.Data.Sqlite
- Multi-star systems require special handling in database queries
- Initial 3D object hierarchy was incorrect - Visual3D objects go directly to Viewport, not Model3DGroup

### Build 2 (2025-11-07)
- Stars vanishing was caused by incorrect Model3DGroup usage
- Camera position must be the reference for all distance calculations, not origin
- User perspective is critical - "viewer is the camera" concept
- ZoomExtents should only be called on initial load, not during updates

### Build 3 (2025-11-07)
- Stars are light sources, not planets - need emissive materials
- EmissiveMaterial with 40% opacity provides good glow without washing out colors
- Spectral colors must be preserved - too much brightness hides them
- Dynamic updates (100ms timer) provide smooth label appearance/disappearance
- Billboard text labels always face camera, simplifying positioning
- Label distance doubled (100 ly) provides better context without clutter
- Size reduction (50%) improves depth perception and scale
- Bold font at close range (25 ly) creates intuitive proximity feedback

### Technical Discoveries
- **3D Rendering:** SphereVisual3D and BillboardTextVisual3D can be added directly to Viewport3D.Children
- **Materials:** MaterialGroup allows combining Diffuse, Emissive, and Specular for realistic star glow
- **Performance:** 100ms timer provides good balance between responsiveness and CPU usage
- **Color Theory:** Emissive materials need careful alpha tuning to avoid washing out spectral colors
- **User Experience:** Labels appearing/disappearing dynamically as you navigate feels very natural

### Design Decisions
1. **All stars visible:** No distance culling ensures complete dataset visibility
2. **Minimum 1-pixel size:** Distant stars stay visible but don't dominate view
3. **Camera-based distances:** Creates proper perspective and depth perception
4. **100 ly label range:** Sweet spot between information and visual clutter
5. **Dynamic rendering:** Real-time updates feel responsive and natural
6. **Emissive stars:** More realistic representation of self-luminous objects
7. **Right-side labels:** Consistent reading direction (left-to-right)

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
- GitHub Issues: https://github.com/rem5357/AstroViewer/issues
- Project Repository: https://github.com/rem5357/AstroViewer

---

**Last Modified:** 2025-11-07
**Next Build:** 4 (pending next compilation)
**Next Version:** 0.03 (awaiting user increment command)
