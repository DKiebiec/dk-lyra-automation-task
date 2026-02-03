# Lyra Automation Tests
## Dependencies
- Unreal Engine 5.3.2 with Lyra Starter Game
- AltTester Unreal SDK
- .NET 6.0+
- NUnit
## Map Cycle Loading Test
This test validates that the player can successfully load into and play on a predefined set of functional maps. The list of maps is configurable, allowing the test to be reused as new playable maps are added.
The primary purpose of this test is to quickly detect regressions related to:
- Level loading
- Game initialization
- Player spawning
This validation is performed without requiring manual verification.

## Mobility Pad Tests
These tests verify the behavior of Lyra’s mobility pads. This system was chosen because it represents a gameplay-critical mechanic that is both highly visible and time-consuming to test manually.
The tests validate both forward-launching and upward-launching pads by:
- Programmatically positioning the player
- Asserting the resulting movement behavior
- In addition to validating gameplay behavior, these tests were used to explore how Unreal Engine gameplay data can be manipulated and inspected through AltTester.

## Custom Helper Method: Aiming at a Referenced Object or World Coordinate
A custom helper function was implemented to allow the player character to aim at either:
- a referenced actor, or
- a specific point in 3D world space

Instead of relying on Unreal’s Enhanced Input System, the function directly adjusts the pitch and yaw of the currently controlled pawn. This makes camera alignment deterministic and reliable.
This approach mirrors how AI-controlled gameplay systems (for example, turrets) typically handle aiming: rotation is calculated and applied directly rather than being driven by player input.
Using this method significantly improves consistency and reproducibility in automated tests, particularly for scenarios such as:

- Shooting a specific actor
- Aiming at a precise location in the map

Implementing smooth camera tracking purely through AltTester proved insufficient due to communication delay, resulting in unsatisfactory camera movement. For this reason, a small modification was introduced into the player blueprint along with a custom plugin to handle the aiming calculations natively in Unreal Engine.

### Function Parameters
AimPlayerAtTarget(target, continous);

target (string)
Can be either an actor reference or a world-space coordinate.

continuous (bool)
false — snaps instantly to the target location without tracking
true — continuously tracks the target until it becomes invalid or StopAim() is called
