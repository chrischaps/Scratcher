# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6000.2.5f1 project called "Scratcher" - a 2D isometric fishing/farming game configured for development using the Universal Render Pipeline (URP). The project is now version controlled with Git and hosted on GitHub at: https://github.com/chrischaps/Scratcher

## Game Features

This project implements a 2D isometric game with the following systems:

### Core Systems
- **Terrain Generation**: Procedural terrain with grass and water tiles using Unity's Tilemap system
- **Player Movement**: Multiple controller options including grid-based, isometric, and terrain-aware movement
- **Fishing System**: Complete fishing mechanics with water zones, fish database, and catch mechanics
- **Inventory System**: Item management and storage functionality
- **Time Management**: Game time progression system
- **Camera System**: Flexible camera controller for 2D isometric gameplay

### Implemented Components
- **Data Systems**: Game Manager, Fish Database, Level Configuration, Inventory System
- **Player Controllers**: Grid-based, Isometric, and Terrain-aware movement options
- **Terrain Management**: Terrain generation and layer management
- **Tile System**: Custom tile implementations for different terrain types
- **UI System**: Fishing game user interface components

## Unity Project Structure

- **Assets/Scripts/**: Organized C# scripts with subdirectories:
  - `Camera/`: Camera control systems
  - `Data/`: Core data management (GameManager, Fish data, Inventory, etc.)
  - `Fishing/`: Fishing gameplay mechanics
  - `Player/`: Player movement controllers
  - `Terrain/`: Terrain generation and management
  - `Tiles/`: Custom tile implementations
  - `UI/`: User interface components
- **Assets/Scenes/**: Unity scene files (SampleScene.unity, Game.unity)
- **Assets/Sprites/**: Character animations and terrain sprites
- **Assets/TerrainTiles/**: Tile assets for terrain generation
- **Assets/Configs/**: ScriptableObject configurations
- **Assets/Settings/**: Render pipeline and project settings
- **ProjectSettings/**: Unity project configuration files
- **Packages/**: Unity package dependencies managed via Package Manager

## Input System

The project uses Unity's Input System with comprehensive input action mappings configured in `Assets/InputSystem_Actions.inputactions`:

### Player Actions
- **Move**: WASD/Arrow keys, gamepad left stick, touch gestures
- **Look**: Mouse delta, gamepad right stick
- **Attack**: Mouse left click, gamepad X button, touch tap, Enter key
- **Interact**: E key, gamepad Y button (hold interaction)
- **Crouch**: C key, gamepad B button
- **Jump**: Space bar, gamepad A button
- **Sprint**: Left Shift, gamepad left stick press
- **Previous/Next**: Number keys 1/2, gamepad D-pad

### UI Actions
Standard UI navigation with support for all input methods (keyboard, gamepad, touch, XR).

## Key Unity Packages

- Universal Render Pipeline (URP) 17.2.0
- Unity Input System 1.14.2
- Unity Test Framework 1.5.1
- 2D Animation, Sprites, and Tilemap tools
- Visual Scripting 1.9.7

## Development Commands

### Building the Project
- Open project in Unity Editor
- File → Build Settings → Build (or Ctrl+Shift+B)
- For command line builds: Use Unity's `-batchmode` with `-buildTarget` parameter

### Testing
- Unity Test Framework is included (com.unity.test-framework 1.5.1)
- Run tests via Window → General → Test Runner in Unity Editor
- Write tests in `Assets/Tests/` directory (create if needed)

## Version Control

This project is tracked with Git and hosted on GitHub:
- **Repository**: https://github.com/chrischaps/Scratcher
- **Branch**: master
- **Git Setup**: Includes Unity-specific .gitignore file
- All Unity-generated and temporary files are properly excluded from version control

## Architecture Notes

This Unity project is configured for 2D isometric game development with:
- Universal Render Pipeline for optimized 2D rendering
- Component-based architecture with modular systems
- ScriptableObject-based configuration system
- Comprehensive input system supporting multiple platforms
- 2D-focused package selection (animation, sprites, tilemaps)
- Multi-platform input scheme (desktop, mobile, gamepad, XR ready)
- Organized code structure following Unity best practices

## File Conventions

- C# scripts should be placed in `Assets/Scripts/` with appropriate subdirectories
- Scene files go in `Assets/Scenes/`
- Use Unity's standard naming conventions (PascalCase for classes, camelCase for variables)
- Follow Unity's component-based architecture patterns