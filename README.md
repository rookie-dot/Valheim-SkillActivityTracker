Skill Activity Tracker

A lightweight and configurable mod that displays your currently active skills in a clean, unobtrusive HUD on the right side of your screen.

FEATURES

Real-Time Skill Tracking
- Automatically displays skills as you use them
- Shows up to 5 most frequently used skills simultaneously
- Each skill displays: localized name, current level, and progress percentage to next level
- Skills are sorted by usage frequency - the more you use a skill, the higher priority it gets

Smart Display Management
- Skills automatically hide after a period of inactivity (configurable)
- Skills at level 100 are automatically hidden from the tracker
- Smooth, non-intrusive interface that doesn't clutter your screen

Full Configuration Support
- Enable or disable tracking for individual skills
- Customize display duration (how long skills remain visible after last use)
- All settings available in BepInEx config file
- Changes can be made without restarting the game

Visual Design
- Elegant gradient background matching Valheim's visual style
- Clear, readable text with outline for visibility
- Positioned on the right side of screen, vertically centered
- Minimal performance impact

CONFIGURATION

After first launch, configuration file is created at:
BepInEx/config/pl.rookie.skillactivitytracker.cfg

Available settings:
- DisplayDurationMinutes: How long skills stay visible (default: 2 minutes)
- Individual skill toggles: Enable/disable tracking for each skill type
- All Valheim skills are included with true/false flags

INSTALLATION

1. Install BepInEx for Valheim
2. Extract SkillActivityTracker.dll to BepInEx/plugins folder
3. Launch game and start playing
4. Skills will appear automatically as you use them

COMPATIBILITY

- Compatible with vanilla Valheim and most other mods
- Uses Harmony for non-destructive patching
- No conflicts expected with other UI mods

REQUIREMENTS

- BepInEx 5.4.x or newer
- Valheim (current version)