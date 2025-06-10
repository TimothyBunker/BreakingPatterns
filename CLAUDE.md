# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6 narrative game based on Breaking Bad where players make choices that affect three core stats: Profit, Relationships, and Suspicion. It's a branching dialogue-based game with visual novel elements.

## Development Commands

### Unity Version
- Unity 6000.0.47f1 (Unity 6)
- Universal Render Pipeline (URP) 17.0.4

### Building the Game
1. Open the project in Unity 6
2. Go to File > Build Settings
3. Ensure both scenes are included:
   - Scene0 (main game scene)
   - EndScene (game over/completion scene)
4. Build for target platform (PC/Mac/Linux Standalone)

### Running and Testing
- Open Scene0.unity in Unity Editor
- Press Play to test the game
- Use arrow keys to navigate choices, Enter/Space/Number keys to select
- No automated test framework - manual testing only

## Architecture Overview

### Core Game Loop
The game follows a state-driven dialogue system:
1. **DialogueJsonLoader** loads story content from JSON files on startup
2. **DialogueManager** displays current dialogue node and handles player input
3. **GameManager** (singleton) tracks game state and applies choice consequences
4. Player choices lead to new dialogue nodes until reaching an end state

### Architecture Pattern
```
JSON Data → Loader → Manager → UI → Feedback
     ↓                   ↓
Side Events      Game State (Singleton)
```

### Key Systems

**Dialogue System**
- Intro sequence loaded from `Assets/StreamingAssets/breaking_patterns_intro.json`
- Main story loaded from `Assets/StreamingAssets/breaking_patterns.json`
- Side events loaded from `Assets/StreamingAssets/side_events.json`
- Each dialogue node contains:
  - Node ID for navigation
  - Text content, background image, character name
  - Up to 3 choice options with stat consequences
  - Next node ID for story progression

**Stat System**
- **Profit**: 0-999, represents money earned
- **Relationships**: 0-100, represents standing with other characters  
- **Suspicion**: 0-100, triggers game over at 100 (DEA catches player)
- High relationships reduce suspicion gain by up to 30%

**Side Event System**
- 35% chance to trigger between main story beats
- Can have conditions (e.g., minimum suspicion level, relationship thresholds)
- Rare events have only 10% trigger chance
- Deck-based system (events removed after showing)

**Dynamic Stat Modifier System**
- RNG variance adds ±33% to base stat values
- Critical success (10% chance): doubles positive gains
- Critical failure (5% chance): doubles negative impacts
- UI shows expected ranges (e.g., "+5~+10")

**Reactive Ending System**
- 6 different endings based on final stats:
  - **Kingpin**: High profit + good relationships
  - **Family Man**: Low profit + high relationships
  - **Fugitive**: High profit + poor relationships
  - **Captured**: Suspicion reaches 100
  - **Betrayed**: Very low relationships
  - **Survivor**: Balanced stats
- Each ending has unique narrative, visuals, and epilogue

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

### UI System
- Choice options display stat changes with color coding:
  - Green (+) for increases
  - Red (-) for decreases
  - Format: `[+5 Profit, -10 Relationships]`
- Layer hierarchy: Background < Dialogue < Stats < Effects < ScreenFlash
- Screen effects: flash for criticals, shake for major changes
- Danger warnings when suspicion > 80

### Key Design Patterns
- **Singleton Pattern**: GameManager, AudioManager use DontDestroyOnLoad
- **Data-Driven Design**: All content in JSON, no hardcoded dialogue
- **Relationship Gates**: Dialogue options can be hidden based on relationship levels
- **Character Portraits**: Left character changes per scene, Walter stays right
- **Input Accessibility**: Keyboard support (arrows, Enter, number keys)

### Critical Files to Understand
1. `GameManager.cs` - Game state and stat management (singleton)
2. `DialogueManager.cs` - Core gameplay loop and UI
3. `DialogueData.cs` - Data structures for dialogue system
4. `breaking_patterns_intro.json` - Immersive intro sequence
5. `breaking_patterns.json` - Main story content
6. `side_events.json` - Random event content
7. `EndSceneController.cs` - Reactive ending system with 6 different endings
8. `StatModifier.cs` - RNG system for dynamic stat changes
9. `FeedbackSystem.cs` - Visual/audio feedback for player actions
10. `UILayerManager.cs` - UI rendering order control