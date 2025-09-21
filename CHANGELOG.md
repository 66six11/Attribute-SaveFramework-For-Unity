# Changelog

All notable changes to SaveFramework will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-09-21

### Added
- Initial release of SaveFramework
- Core save/load functionality with `[Save]` attribute marking
- Support for Unity common types (Vector3, Quaternion, Color, etc.)
- Support for basic types (int, float, string, bool, etc.)
- Support for arrays and collections
- Support for enum types
- `SaveId` component for unique object identification
- `SaveManager` singleton for centralized save operations
- File-based backend with JSON serialization
- Multiple save slot support
- Custom type converter system with `IValueConverter` interface
- Editor tools for code generation and management
- Comprehensive documentation and examples
- Sample PlayerStats component demonstrating usage
- Cross-platform compatibility
- Performance optimization through code generation

### Code Generation Tools
- **Generate Registration**: Scans project for `[Save]` attributes and generates optimized code
- **Clear Generated Registration**: Removes generated files for cleanup
- **Show Generated File**: Opens generated code location in file manager
- **Pre-generate Converter Registry**: Optimizes custom type converters

### API Features
- `SaveManager.Instance.Save(slotName)` - Save data to slot
- `SaveManager.Instance.Load(slotName)` - Load data from slot
- `SaveManager.Instance.HasSave(slotName)` - Check save existence
- `SaveManager.Instance.DeleteSave(slotName)` - Delete save
- `SaveManager.Instance.GetSaveSlots()` - List all save slots
- `SaveManager.Instance.SetBackend(backend)` - Custom backend support

### SaveAttribute Options
- `[Save]` - Use field name as key
- `[Save("custom_key")]` - Custom key name
- `[Save("key", "alias1", "alias2")]` - Support aliases for data migration
- `[Save(typeof(CustomConverter))]` - Custom type converter
- `[Save("key", typeof(CustomConverter), "alias")]` - Full options

### SaveId Features
- Automatic unique ID generation
- Custom ID support for predictable identifiers
- Validation and utility methods
- Editor integration with context menus

### Backend System
- `ISaveBackend` interface for custom implementations
- Default `FileBackend` with JSON serialization
- Configurable save directories
- Error handling and validation

### Examples Included
- `PlayerStats` - Complete player data management example
- `MoreTest` - Comprehensive type testing component
- `StartupSaveBackend` - Custom backend configuration example

### Documentation
- Comprehensive Chinese user manual (README.md)
- English user manual (README_EN.md)  
- Quick reference guide (QUICKREF.md)
- API documentation with examples
- Best practices and troubleshooting guide
- Performance optimization tips

### Performance Features
- Code generation eliminates runtime reflection
- Efficient serialization of Unity types
- Minimal memory allocation during save/load
- Optimized type conversion system

### Platform Support
- All Unity target platforms
- Persistent data path integration
- Cross-platform file system support

---

## Future Plans

### Planned for v1.1.0
- Encryption support for save files
- Compression options for large save files
- Cloud save backend implementations
- Save file versioning and migration tools
- Additional Unity type support (LayerMask, AnimationCurve, etc.)

### Planned for v1.2.0
- Visual save slot management in Editor
- Save file validation and integrity checking
- Backup and restore functionality
- Performance profiling tools
- Advanced debugging features

---

*Note: This project follows semantic versioning. All breaking changes will be clearly documented and will only occur in major version updates.*