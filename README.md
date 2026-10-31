# Scratcher

A 2D isometric fishing and farming game built with Unity 6000.2.5f1 and Universal Render Pipeline (URP).

**Project page:** [chaps.dev/projects/scratcher](https://chaps.dev/projects/scratcher/)

## 🎮 Game Features

### Core Gameplay Systems
- **🎣 Fishing System**: Complete fishing mechanics with water zones, fish database, and interactive catch system
- **🌾 Terrain Generation**: Procedural terrain generation with grass and water tiles using Unity's Tilemap system
- **🚶 Player Movement**: Multiple movement controllers (grid-based, isometric, terrain-aware)
- **📦 Inventory System**: Item management and storage functionality
- **⏰ Time Management**: Dynamic game time progression
- **📷 Camera System**: Flexible camera controller optimized for 2D isometric gameplay

### Technical Features
- Universal Render Pipeline (URP) for optimized 2D rendering
- Component-based architecture with modular systems
- ScriptableObject-based configuration system
- Comprehensive input system (keyboard, mouse, gamepad, touch, XR)
- Organized codebase following Unity best practices

## 🛠️ Tech Stack

- **Engine**: Unity 6000.2.5f1
- **Rendering**: Universal Render Pipeline (URP) 17.2.0
- **Input**: Unity Input System 1.14.2
- **Testing**: Unity Test Framework 1.5.1
- **Platform Support**: Multi-platform (Desktop, Mobile, Gamepad, XR ready)

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Camera/          # Camera control systems
│   ├── Data/            # Core data management (GameManager, Fish data, etc.)
│   ├── Fishing/         # Fishing gameplay mechanics
│   ├── Player/          # Player movement controllers
│   ├── Terrain/         # Terrain generation and management
│   ├── Tiles/           # Custom tile implementations
│   └── UI/              # User interface components
├── Scenes/              # Unity scene files
├── Sprites/             # Character animations and terrain sprites
├── TerrainTiles/        # Tile assets for terrain generation
├── Configs/             # ScriptableObject configurations
└── Settings/            # Render pipeline and project settings
```

## 🚀 Getting Started

### Prerequisites
- Unity 6000.2.5f1 or later
- Git

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/chrischaps/Scratcher.git
   ```

2. Open the project in Unity:
   - Launch Unity Hub
   - Click "Open" and select the cloned `Scratcher` folder
   - Unity will automatically import and configure the project

3. Open the main scene:
   - Navigate to `Assets/Scenes/`
   - Open `Game.unity` or `SampleScene.unity`

### Controls
- **Movement**: WASD or Arrow Keys
- **Interact**: E key
- **Attack**: Left Mouse Click or Enter
- **Look**: Mouse movement
- **Sprint**: Left Shift
- **Crouch**: C key
- **Jump**: Space bar

Full gamepad and touch support included.

## 🎯 Development

### Building the Project
- **Unity Editor**: File → Build Settings → Build (Ctrl+Shift+B)
- **Command Line**: Use Unity's `-batchmode` with `-buildTarget` parameter

### Testing
- Unity Test Framework is included
- Run tests via Window → General → Test Runner in Unity Editor
- Write tests in `Assets/Tests/` directory

### Code Conventions
- C# scripts organized in `Assets/Scripts/` with appropriate subdirectories
- PascalCase for classes, camelCase for variables
- Follow Unity's component-based architecture patterns
- Use ScriptableObjects for configuration data

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🎨 Assets

- Character sprites and animations included
- Terrain tiles and environmental assets
- UI components and interface elements

## 📞 Contact

Project Link: [https://github.com/chrischaps/Scratcher](https://github.com/chrischaps/Scratcher)

---

*Built with ❤️ using Unity and Universal Render Pipeline*
