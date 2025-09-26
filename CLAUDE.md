# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6000.2.5f1 project called "Scratcher" configured for 2D development using the Universal Render Pipeline (URP).

## Unity Project Structure

- **Assets/Scripts/**: C# scripts directory (currently empty - new project)
- **Assets/Scenes/**: Unity scene files (contains SampleScene.unity)
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

## Architecture Notes

This is a fresh Unity project configured for 2D development with:
- Universal Render Pipeline for optimized 2D rendering
- Comprehensive input system supporting multiple platforms
- 2D-focused package selection (animation, sprites, tilemaps)
- Multi-platform input scheme (desktop, mobile, gamepad, XR ready)

## File Conventions

- C# scripts should be placed in `Assets/Scripts/` with appropriate subdirectories
- Scene files go in `Assets/Scenes/`
- Use Unity's standard naming conventions (PascalCase for classes, camelCase for variables)
- Follow Unity's component-based architecture patterns