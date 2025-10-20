# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 3D third-person shooter developed for a game jam (GameJam-25). The project uses Unity 2022.3.62f2 and features a robot character with complete movement system, Mixamo animations with pistol, shooting mechanics, and jumping.

**Current Status:**
- Fully functional third-person movement with 8-directional controls
- 11 Mixamo animations integrated (pistol idle, walk/run in all directions, strafe, jump, shoot)
- Unlimited shooting system with physical bullets
- Jump system with ground detection
- Crosshair UI
- Audio system for shooting

**Pending/Future Features:**
- Reload system
- Enemy AI
- Health/damage system
- Particle effects (muzzle flash, impacts)
- Complete UI (health, ammo counters)
- Additional sounds (footsteps, ambient)

## Development Commands

### Opening and Running the Project
- Open the project in Unity Hub by selecting the `GameJam` folder
- Unity version: 2022.3.62f2
- Main scene: `Assets/00_Scenes/SampleScene.unity`
- Play the game in Unity Editor by pressing the Play button or `Ctrl+P` (Windows) / `Cmd+P` (Mac)

### Building the Project
- Build via Unity Editor: `File > Build Settings > Build`
- The C# solution file is `GameJam.sln` (can be opened in Visual Studio or Rider)

### Testing
- Unity uses the Unity Test Framework (included as a package)
- Run tests in Unity Editor via `Window > General > Test Runner`

## Project Structure

### Asset Organization
The project follows a numbered folder convention for organization:
- `00_Scenes/` - Unity scenes
- `01_Scripts/` - C# game scripts
- `02_Materials/` and `06_Materials/` - Material assets (appears to have duplicates)
- `02_Prefabs/` - Prefab game objects (player gun, bullets, rooms, walls)
- `03_Models/` - 3D models and animations (robot character with various animations)
- `04_Animations/` - Animation files
- `05_Audios/` - Audio files
- `07_Textures/` - Texture files

### Core Scripts Architecture

The project has three main gameplay scripts:

1. **MovementThirdPerson.cs** (Primary player controller)
   - Implements third-person movement with camera-relative controls
   - Handles jumping with ground detection via raycasts
   - Shooting system with bullet instantiation
   - Animator integration for character animations (walking, running, jumping, shooting, strafing)
   - Camera orbit system with mouse controls
   - Uses Rigidbody-based physics movement

2. **Movement.cs** (Alternative/older first-person controller)
   - First-person movement and camera rotation
   - Simpler animation system (just walking boolean)
   - Direct character rotation based on mouse X axis

3. **Bullet.cs**
   - Projectile behavior with trigger-based collision detection
   - Damage system placeholder (commented out Health component integration)
   - Impact effects support

### Animation System

The robot character uses Unity's Animator with 11 Mixamo animations (all with pistol):

**Animation Clips (from Mixamo):**
- `Pistol Idle` - Idle state with pistol
- `Pistol Walk` / `Pistol Run` - Forward movement (walk/run)
- `Left Strafe Walking` / `Pistol Strafe Left` - Left movement (walk/run)
- `Right Strafe Walk` / `Pistol Strafe Right` - Right movement (walk/run)
- `Pistol Walk Backward` / `Pistol Run Backward` - Backward movement (walk/run)
- `Shooting` - Shoot animation (trigger-based)
- `Pistol Jump` - Jump animation

**Mixamo Import Settings:**
- Rig: Humanoid
- Avatar Definition: Copy From Other Avatar (from base character)
- Loop Time: ON for movement animations, OFF for Shooting and Jump

**Animator Parameters:**
- `Horizontal` (Float) - Lateral movement (-1 left, +1 right)
- `Vertical` (Float) - Forward/backward movement (-1 back, +1 forward)
- `IsRunning` (Bool) - Sprint state (Shift held)
- `IsGrounded` (Bool) - Ground detection for jump
- `Jump` (Trigger) - Activate jump animation
- `Shoot` (Trigger) - Activate shoot animation

**Animator State Machine Structure:**

Entry → Pistol Idle (default state)

**Movement States:**
- Walk states (Horizontal/Vertical input, !IsRunning):
  - Pistol Walk (Vertical > 0.1)
  - Left Strafe Walking (Horizontal < -0.1)
  - Right Strafe Walk (Horizontal > 0.1)
  - Pistol Walk Backward (Vertical < -0.1)

- Run states (Horizontal/Vertical input, IsRunning):
  - Pistol Run (Vertical > 0.1)
  - Pistol Strafe Left (Horizontal < -0.1)
  - Pistol Strafe Right (Horizontal > 0.1)
  - Pistol Run Backward (Vertical < -0.1)

**Action States:**
- Shooting: `Any State → Shooting` (Trigger: Shoot, Has Exit Time: OFF, Can Transition To Self: OFF)
  - Returns to Idle with Has Exit Time: ON (Exit Time: 0.9)
- Pistol Jump: `Any State → Pistol Jump` (Trigger: Jump, Has Exit Time: OFF)
  - Returns to Idle when IsGrounded + Has Exit Time: ON (Exit Time: 0.8)

**Transition Settings:**
- Movement transitions: Has Exit Time OFF, Duration 0.15s, Interruption: Current State Then Next State
- Walk ↔ Run pairs: Toggle based on IsRunning boolean
- Action exits: Has Exit Time ON, Duration 0.1s

### Key Gameplay Mechanics

**Input Handling:**
- WASD/Arrow keys for movement
- Mouse for camera rotation
- Left Shift for sprinting
- Space for jumping
- Left Mouse Button (Fire1) for shooting
- Cursor is locked and hidden during gameplay

**Physics Setup:**
- Character uses Rigidbody for physics-based movement
  - Mass: 1, Drag: 2, Angular Drag: 0.05
  - Constraints: Freeze Rotation X, Y, Z (all checked)
  - Capsule Collider: Center (0, 1, 0), Radius 0.5, Height 2
- Ground detection uses Physics.CheckSphere with LayerMask "Ground"
  - GroundCheck GameObject at character feet (Y: 0 or slightly below)
  - Check radius: 0.2 units
- Bullets use Rigidbody with velocity-based movement
  - Mass: 0.1, Drag: 0, Use Gravity: OFF, Collision Detection: Continuous
  - Sphere Collider with Is Trigger: ON, Radius: 0.5
  - Scale: (0.1, 0.1, 0.1)
  - Auto-destroy after 3 seconds
- Player tag is used to prevent bullet self-collision

**Weapon Hierarchy:**
```
robot@idle (1)
  └─ Armature
      └─ ... bones ...
          └─ RightHand (mixamorig:RightHand bone)
              └─ Weapon (pistol prefab)
                  └─ FirePoint (Empty GameObject)
```

**FirePoint Configuration:**
- Position: At gun barrel tip
- Rotation: X=90, Y=0, Z=0 (critical - ensures bullets fire forward)
- The blue arrow (Z axis) must point in shooting direction

**Shooting System:**
- Unlimited ammo (no counter)
- Bullet speed: 20 units/second
- Input: Left Mouse Button (Fire1)
- Creates bullet at FirePoint position with forward velocity
- Triggers "Shoot" animation
- Plays shoot sound via AudioSource (PlayOneShot)

**Jump System:**
- Jump force: 5 units
- Only allowed when IsGrounded = true
- Input: Space bar
- Triggers "Jump" animation

## Unity Packages

Standard Unity packages are used (see `Packages/manifest.json`):
- TextMeshPro (3.0.7)
- Visual Scripting (1.9.4)
- Timeline (1.7.7)
- Unity Collaboration tools

## Git Workflow

- Main branch: `master`
- Current branch: `Joel`
- Recent commits show progression: base project → environment setup → weapon movement

## Camera System

**Third-Person Camera Configuration:**
- Parent: robot@idle (child of player character)
- Distance from character: 3.5 units
- Height offset: 2 units
- Initial position: X=0, Y=2, Z=-3.5
- Initial rotation: X=10, Y=0, Z=0
- Field of View: 60

**Camera Controls:**
- Mouse sensitivity: 2.0
- Smoothing: 5.0 (Lerp factor)
- Vertical angle clamp: -20° to 60°
- Mouse X: Horizontal orbit around character
- Mouse Y: Vertical angle adjustment
- Camera always LookAt character position + height offset

## UI System

**Crosshair:**
Simple centered crosshair with 4 lines:
```
Canvas
  └─ Crosshair (parent)
      ├─ LineTop (2x10 pixels, Pos Y: 15)
      ├─ LineBottom (2x10 pixels, Pos Y: -15)
      ├─ LineLeft (10x2 pixels, Pos X: -15)
      └─ LineRight (10x2 pixels, Pos X: 15)
```
- Anchors: Center (0.5, 0.5)
- Position: (0, 0)
- Color: White with alpha 200-255

## Common Issues and Solutions

### Animation Issues

**Problem: Shooting animation loops continuously**
- Cause: Multiple transitions to Shooting or "Can Transition To Self" enabled
- Solution: Use only `Any State → Shooting`, disable "Can Transition To Self"

**Problem: Animations don't transition smoothly**
- Cause: Incorrect Transition Duration
- Solution: Movement transitions: 0.15s, Action exits: 0.1s

### Physics Issues

**Problem: Bullet doesn't move**
- Cause: Rigidbody misconfigured or FirePoint wrong rotation
- Solution:
  - Rigidbody: Use Gravity OFF, Is Kinematic OFF
  - FirePoint Rotation: X=90, Y=0, Z=0
  - Verify blue arrow (Z) points forward

**Problem: Jump doesn't work**
- Cause: GroundCheck misconfigured or wrong Layer
- Solution:
  - Create Layer "Ground" in project settings
  - Assign Layer to ground plane/terrain
  - GroundCheck at character feet (Y: 0 or slightly below)

**Problem: Character rotates incorrectly during strafe**
- Cause: Code rotates toward movement direction
- Solution: Character should only rotate with camera direction, use `transform.InverseTransformDirection()` for local space

### Audio Issues

**Problem: Shoot sound doesn't play**
- Solution:
  - Verify AudioSource component on character
  - Play On Awake: OFF
  - Assign audio clip in MovementThirdPerson script's "Shoot Sound" field
  - Check audio clip import settings

### Code Issues

**Problem: 'Color' is an ambiguous reference**
- Cause: Conflict between UnityEngine.Color and System.Drawing.Color
- Solution: Use `UnityEngine.Color.red` or remove `using System.Drawing;`

## Important Notes

1. **All Mixamo animations must be downloaded "Without Skin"**
2. **All animations must use same Avatar** - configure as "Copy From Other Avatar" from base character
3. **Has Exit Time OFF** for movement transitions (immediate response)
4. **Has Exit Time ON** for Shooting and Jump (complete animation before returning to Idle)
5. **FirePoint Rotation X=90** is critical for bullets to fire forward
6. **Tag "Player"** required on character to prevent bullet self-collision
7. **Layer "Ground"** required for jump ground detection
8. **Can Transition To Self OFF** on Any State → Shooting to prevent infinite loops
9. **Movement speed values**: Walk: 3 units/s, Run: 6 units/s, Rotation: 10 deg/s
10. **Character rotation**: Only rotates to match camera direction, not movement input direction
