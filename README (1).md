# Two Player Cycle Race Game (3D)

A 3D two-player cycle racing game built in Unity where two players race on a street/sports track. The first player to reach the finish distance wins!

---

## Unity Version

- **Unity 2021 LTS** or **Unity 2022 LTS**
- TextMeshPro package required

---

## How to Run

1. Open the project in Unity 2021/2022 LTS
2. Ensure all scenes are added to Build Settings:
   - `HomeScene` (index 0)
   - `GameScene` (index 1)
   - `WinScene` (index 2)
3. Open `HomeScene` and press **Play**

---

## Controls

| Action | Player 1 | Player 2 |
|--------|----------|----------|
| Forward | W | ↑ (Up Arrow) |
| Backward | S | ↓ (Down Arrow) |
| Turn Left | A | ← (Left Arrow) |
| Turn Right | D | → (Right Arrow) |
| Boost | Q | Number 0 |

**Note:** Controls are customizable in the Inspector.

---

## Game Flow

### 1. Home Screen (Panel 1)
- Enter Player 1 and Player 2 names (if name entry is enabled)
- Click **Start** to proceed to cycle selection
- Click **Settings** to configure game options

### 2. Cycle Selection (Panel 2)
- **Player 1** selects a cycle → clicks **Confirm**
- **Player 2** selects a cycle → clicks **Ready to Race**
- Game loads after both players confirm

### 3. Game Screen
- Traffic light countdown: Red → Green (3 lights)
- Race begins when all lights turn green
- First player to reach the winning distance wins
- Distance and speed displayed in real-time
- Energy bar for boost (refills when not boosting)

### 4. Win Screen
- Displays winner name and distance
- **History** button shows last 5 race winners
- **Play Again** returns to game
- **Main Menu** returns to home screen

---

## Settings

All settings are saved using PlayerPrefs and persist between sessions.

| Setting | Description | Range |
|---------|-------------|-------|
| **Winning Meter Length** | Race distance to win | 50m - 1000m |
| **Home Screen UI Theme** | Changes background of all panels | Theme A / B / C |
| **Logo Position** | Logo placement on game screen | Left / Center / Right |
| **Name Entry Toggle** | Enable/disable player name input | ON / OFF |

### How Settings Work:
1. Open Settings from Home Screen
2. Adjust values using sliders, buttons, and toggles
3. Click **Save** to apply changes
4. Click **Back** to discard changes

---

## Features

### Core Features
- ✅ Two-player simultaneous gameplay
- ✅ Independent controls (no input conflicts)
- ✅ Distance-based winner detection
- ✅ Settings persistence with PlayerPrefs
- ✅ 3-panel home screen flow
- ✅ Cycle selection for both players

### Bonus Features
- ✅ Smooth acceleration/deceleration (not instant max speed)
- ✅ Boost system with energy management
- ✅ Traffic light countdown (Red → Green)
- ✅ Character animations (Idle / Normal / Speed)
- ✅ Audio SFX (countdown beeps, winner sound)
- ✅ Visual lean when turning
- ✅ Anti-wheelie physics stabilization
- ✅ Track boundary detection
- ✅ Race history (last 5 winners)
- ✅ Theme customization

---

## Scripts Overview

| Script | Purpose |
|--------|---------|
| `HomeScreenManager.cs` | Handles home screen, cycle selection, and settings panels |
| `CycleRaceGameController.cs` | Main game logic: controls, physics, countdown, winner detection |
| `WinSceneManager.cs` | Win screen display and race history |

---

## Inspector Setup Guide

### CycleRaceGameController

1. **Game Configuration**
   - Set winning distance, speeds, energy values
   - Assign ground layer for physics

2. **Player 1 & Player 2**
   - Assign cycle Transform, Rigidbody, Collider
   - Assign Animator for character
   - Set animation parameter names (Idel, Normal, Speed)
   - Customize controls if needed

3. **UI References**
   - Assign countdown light GameObjects (3 lights with Image component)
   - Assign red and green sprites
   - Assign player UI elements (name, distance, speed, energy slider)
   - Assign winner panel

4. **Audio**
   - Assign AudioSource component
   - Assign countdown AudioClip
   - Assign winner AudioClip

5. **Logo**
   - Create 3 logo GameObjects (Left, Center, Right positions)
   - Assign to inspector fields

### HomeScreenManager

1. **Panels**
   - Assign Home, Cycle Selection, Settings panels

2. **Panel 1 (Home)**
   - Assign name input fields and buttons

3. **Panel 2 (Cycle Selection)**
   - Assign cycle buttons for both players (4 each)
   - Assign Confirm and Ready buttons

4. **Panel 3 (Settings)**
   - Assign winning meter slider and value text
   - Assign theme buttons (A, B, C) and sprites
   - Assign panel background images
   - Assign logo position buttons
   - Assign name entry toggle
   - Assign Save and Back buttons

### WinSceneManager

1. **Panels**
   - Assign Winner and History panels

2. **Winner Panel**
   - Assign winner name and distance texts
   - Assign History, Play Again, Main Menu buttons

3. **History Panel**
   - Create ScrollView with Content (Vertical Layout)
   - Create AttemptRowPrefab with: NumberText, PlayerNameText, DistanceText
   - Assign scroll content and prefab

---

## Known Limitations

1. **3 Screen** - split-screen camera implementation (both players share the same view)

4. **Animation Setup** - Animator must have Bool parameters named exactly: "Idel", "Normal", "Speed"

5. **Scene Names** - Scene names must match exactly in Build Settings and Inspector fields

6. **Audio** - Only two audio clips supported (countdown and winner). Background music included.

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Players fall over | Check Rigidbody constraints, ensure Ground Layer is set |
| Input not working | Verify controls in Inspector, check for input conflicts |
| Settings not saving | Ensure Save button is clicked, check PlayerPrefs |
| Animations not playing | Verify Animator is assigned, check parameter names match |

---