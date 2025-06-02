# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6 narrative game based on Breaking Bad where players make choices that affect three core stats: Profit, Relationships, and Suspicion. It's a branching dialogue-based game with visual novel elements.

## Development Commands

### Unity Version
- Unity 6000.0.47f1 (Unity 6)

### Building the Game
1. Open the project in Unity 6
2. Go to File > Build Settings
3. Ensure both scenes are included:
   - Scene0 (main game scene)
   - EndScene (game over/completion scene)
4. Build for target platform (PC/Mac/Linux)

### Running and Testing
- Open Scene0.unity in Unity Editor
- Press Play to test the game
- Use arrow keys to navigate choices, Enter/Space/Number keys to select

## Architecture Overview

### Core Game Loop
The game follows a state machine pattern where:
1. **DialogueJsonLoader** loads story content from JSON files on startup
2. **DialogueManager** displays current dialogue node and handles player input
3. **GameManager** (singleton) tracks game state and applies choice consequences
4. Player choices lead to new dialogue nodes until reaching an end state

### Key Systems

**Dialogue System**
- Main story loaded from `Assets/StreamingAssets/breaking_patterns.json`
- Side events loaded from `Assets/StreamingAssets/side_events.json`
- Each dialogue node contains:
  - Text content, background image, character name
  - Up to 3 choice options with stat consequences
  - Next node ID for story progression

**Stat System**
- **Profit**: 0-100, represents money earned
- **Relationships**: 0-100, represents standing with other characters  
- **Suspicion**: 0-100, triggers game over at 100 (DEA catches player)

**Side Event System**
- 35% chance to trigger between main story beats
- Can have conditions (e.g., minimum suspicion level)
- Rare events have only 10% trigger chance

### Data Flow
```
JSON Files → DialogueJsonLoader → DialogueManager ↔ GameManager
                                         ↓
                                    Player Input
                                         ↓
                                   Update Stats → Check Game Over
```

## Important Implementation Details

### Adding New Content
- Story nodes go in `breaking_patterns.json`
- Side events go in `side_events.json`
- Each node needs: id, text, characterName, backgroundImage, options array
- Options need: text, statChanges object, nextNode

### Audio System
- `bonus-point.mp3`: Positive stat changes
- `bad-or-error-choice.mp3`: Negative stat changes
- `default-choice.mp3`: Neutral choices
- `background-music.mp3`: Loops during gameplay

### UI Formatting
- Choice options display stat changes with color coding:
  - Green (+) for increases
  - Red (-) for decreases
  - Format: `[+5 Profit, -10 Relationships]`

### Critical Files to Understand
1. `GameManager.cs` - Game state and stat management
2. `DialogueManager.cs` - Core gameplay loop and UI
3. `DialogueData.cs` - Data structures for dialogue system
4. `breaking_patterns.json` - Main story content
5. `side_events.json` - Random event content