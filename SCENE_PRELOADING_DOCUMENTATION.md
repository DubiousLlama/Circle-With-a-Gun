# Scene Preloading Implementation

## Overview
This implementation adds background scene preloading to improve the user experience when returning to the main menu after a game. The target scene (either main menu or high scores) is preloaded in the background as soon as the game over screen appears, making the transition instant when the user presses the "Return to Main Menu" button.

## Components

### 1. ScenePreloader.cs (New)
A singleton manager that handles background scene loading using Unity's `LoadSceneAsync` with `allowSceneActivation = false`.

**Key Methods:**
- `PreloadScene(int buildIndex)` - Starts loading a scene in the background
- `ActivatePreloaded(int buildIndex)` - Switches to the preloaded scene instantly
- `GetProgress(int buildIndex)` - Returns loading progress (0-1)
- `IsReadyToActivate(int buildIndex)` - Returns true when scene is ready

**Usage:**
1. Create an empty GameObject in your scene
2. Add the ScenePreloader component to it
3. The component will persist across scenes (DontDestroyOnLoad)

### 2. ReturnMainMenu.cs (Modified)
Enhanced to check high scores and preload scenes as soon as the game over screen appears.

**New Features:**
- `OnGameOverScreenShown()` - Called when game over screen activates
  - Checks if there's a new high score
  - Determines target scene (high scores or main menu)
  - Starts preloading the target scene
- `highScoreCheckCompleted` flag - Prevents duplicate high score checks
- `targetSceneIndex` - Stores which scene to load

**Flow:**
1. Player dies ? Game over screen shows
2. `OnGameOverScreenShown()` is called
3. High score check runs (waits for Steam leaderboards)
4. Target scene is determined and preloading starts
5. When user clicks "Return to Main Menu", scene activates instantly

### 3. PlayerHealth.cs (Modified)
Updated to trigger the preloading process when the game over screen appears.

**Changes:**
- Finds `ReturnMainMenu` component when player dies
- Calls `OnGameOverScreenShown()` to initiate high score check and preloading

## Benefits

1. **Instant Scene Transitions**: Scene is already loaded when button is pressed
2. **Better User Experience**: No waiting after clicking return button
3. **Handles Both Paths**: Works for both main menu and high scores scene
4. **Pause Menu Safe**: High score check only happens once, even if accessed from pause menu
5. **Fallback Support**: Falls back to normal loading if preloader isn't available

## Setup Instructions

### In Unity Editor:
1. Create a new empty GameObject in your game scene (e.g., "ScenePreloader")
2. Add the `ScenePreloader` component to it
3. Make sure your `ReturnMainMenu` and `PlayerHealth` components have the necessary references:
   - `ReturnMainMenu`: Needs `roster` reference
   - `PlayerHealth`: Needs `gameOverScreen` and `scoreTracker` references

### Scene Build Index Requirements:
The implementation assumes:
- Main Menu: Current scene index - 1
- High Scores: Current scene index + 1

Verify in **File ? Build Settings** that your scene order is correct.

## Technical Details

### Loading Process:
```
Player Dies
    ?
Game Over Screen Activates
    ?
OnGameOverScreenShown() Called
    ?
CheckAndSaveNewHighScoreCoroutine() Runs
    ?
Wait for Steam Leaderboards (max 1 second)
    ?
Determine New High Score Status
    ?
Calculate Target Scene Index
    ?
ScenePreloader.PreloadScene() Called
    ?
Scene Loads in Background (progress ? 0.9)
    ?
Scene Ready (waiting for activation)
    ?
User Clicks Button
    ?
ActivatePreloaded() Called
    ?
Instant Scene Transition!
```

### Memory Considerations:
- Only one scene is preloaded at a time
- Preloaded scene is held in memory at ~90% completion
- Memory is released when scene activates or preload is cancelled
- If user returns from pause menu, the same preload is reused

## Error Handling

The implementation includes several safety measures:
- Falls back to normal scene loading if preloader isn't available
- Handles missing components gracefully with warnings
- Prevents duplicate high score checks
- Uses timeouts for Steam leaderboard waits
- Validates scene indices before loading

## Performance Impact

- **Memory**: One additional scene loaded at ~90% (typically 5-20 MB depending on scene)
- **CPU**: Minimal - background loading uses Unity's async system
- **Loading Time**: Moved from button press to game over (better perceived performance)

## Future Enhancements

Possible improvements:
1. Add progress bar during initial preload
2. Preload both scenes and choose later
3. Cache commonly accessed scenes in main menu
4. Add configurable timeout for Steam leaderboard check
5. Support for additive scene loading mode
