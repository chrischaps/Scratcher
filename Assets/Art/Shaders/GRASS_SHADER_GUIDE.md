# Animated Grass 2D Shader - Setup Guide

## Overview
This shader adds wind wave animation and interactive player displacement to 2D grass sprites/tiles for your isometric game.

## Quick Setup

### 1. Create a Material
1. Right-click in Project window → Create → Material
2. Name it "GrassMaterial"
3. In Inspector, change Shader dropdown to: **Custom/AnimatedGrass2D**
4. Assign your grass sprite texture to "Sprite Texture"

### 2. Apply to Grass Tiles
**For Tilemaps:**
1. Select your Grass Tilemap in Hierarchy
2. Find the Tilemap Renderer component
3. Drag your new GrassMaterial to the "Material" slot

**For Sprite Objects:**
1. Select grass sprite object
2. In Sprite Renderer component
3. Drag GrassMaterial to "Material" slot

### 3. Setup Interactive Grass
1. Select your Player GameObject
2. Add Component → Scripts → **Grass Interaction Controller**
3. Leave "Auto Find Materials" checked (it will find grass materials automatically)
4. **Or** manually drag grass materials to the "Grass Materials" array

### 4. Adjust Settings (Optional)
In the Material Inspector, tweak these settings:

**Wind Wave Animation** (Enabled by default)
- Wind Speed: 1.0 (how fast wind blows)
- Wind Frequency: 2.0 (size of wind waves)
- Wind Amplitude: 0.02 (how much grass moves)
- Wind Direction: (1, 0.3) (direction wind blows)

**Interactive Grass** (Enabled by default)
- Interaction Radius: 2.0 (how far player affects grass)
- Interaction Strength: 0.05 (how much grass bends)
- Recovery Speed: 3.0 (how fast grass springs back)

**Optional Effects** (Disabled by default)
- Enable Color Variation: Adds subtle color shifts to break up tiling
- Enable Shimmer: Adds animated highlights for sun shimmer effect

## Performance Tips
- Wind and Interaction have minimal performance impact
- Update Interval on GrassInteractionController can be increased (0.05-0.1) for better performance
- Color Variation and Shimmer are cheap fragment operations
- All effects can be toggled on/off via material checkboxes

## Advanced Usage

### Multiple Interaction Points
Create multiple GrassInteractionController scripts for:
- Multiple players in multiplayer
- NPCs that affect grass
- Animals/creatures

### Custom Wind Patterns
Animate the Wind Direction vector in code for:
- Changing wind direction over time
- Gusts and wind storms
- Regional wind variations

### Shader Keywords
The shader uses keywords for performance:
- WIND_ENABLED
- INTERACTION_ENABLED
- COLOR_VARIATION_ENABLED
- SHIMMER_ENABLED

Toggle these via material checkboxes - disabled features have zero cost!

## Troubleshooting

**Grass isn't moving:**
- Check that "Enable Wind" is checked in material
- Increase Wind Amplitude to 0.05 for more visible movement
- Verify material is applied to grass tiles/sprites

**Player interaction not working:**
- Ensure GrassInteractionController is attached to player
- Check that grass materials are listed in the component
- Verify Interaction Radius is large enough (try 3.0)
- Make sure "Enable Interaction" is checked in material

**Performance issues:**
- Increase Update Interval on GrassInteractionController to 0.05+
- Disable Color Variation and Shimmer if not needed
- Use shared materials for all grass tiles (don't create instances)

**Grass looks wrong:**
- Ensure you're using URP (Universal Render Pipeline)
- Check that sprite texture is assigned
- Verify blend mode is set to SrcAlpha/OneMinusSrcAlpha

## Example Settings by Use Case

### Subtle Breeze
- Wind Speed: 0.5
- Wind Amplitude: 0.015
- Interaction Strength: 0.03

### Strong Wind
- Wind Speed: 2.0
- Wind Amplitude: 0.05
- Wind Direction: (1, 0)

### Calm Scene with Strong Player Interaction
- Wind Speed: 0.2
- Wind Amplitude: 0.01
- Interaction Radius: 3.0
- Interaction Strength: 0.08

### Magical Shimmering Grass
- Enable Shimmer: On
- Shimmer Speed: 1.5
- Shimmer Intensity: 0.4
- Enable Color Variation: On
