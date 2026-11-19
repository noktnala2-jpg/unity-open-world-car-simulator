# Open Road Sim — Unity Prototype

A staged Unity C# vehicle-simulation project. **Stage 1 is a source-level playable prototype**: a generated test pad and road intersection, a four-wheel `WheelCollider` vehicle, keyboard/gamepad-axis driving, automatic gears, braking, suspension/anti-roll, fuel and impact-condition telemetry, chase camera, and HUD. It is a foundation, not a finished open-world or multiplayer game.

## Requirements

- Unity 2022.3 LTS or newer (Built-in Render Pipeline supported).
- No third-party packages are required for Stage 1.
- The current input component uses Unity's legacy `Input.GetAxis` API. In **Project Settings → Player → Active Input Handling**, select **Input Manager (Old)** or **Both**. Restart the editor if prompted.

## Quick start

1. Clone/open this repository in Unity. Allow Unity to import the `Assets/OpenRoadSim` scripts.
2. Create a scene, save it as `Assets/OpenRoadSim/Scenes/Prototype.unity`.
3. Create an empty GameObject named `PrototypeBootstrap` and add the `OpenRoadSim.PrototypeBootstrap` component.
4. Optionally create a vehicle tuning asset via **Assets → Create → Open Road Sim → Vehicle Configuration**, then assign it to `Vehicle Config` on the bootstrap component. Without an asset, documented fallback tuning is used.
5. Press Play. The bootstrap creates the test environment and car at runtime. Use **W/S** or **Up/Down** to accelerate/reverse, **A/D** or **Left/Right** to steer, and **Space** for the handbrake. Click the Game view to lock the mouse; press **Esc** to release it.

The generated scene is runtime-only; it is not automatically saved into a Unity scene asset. The bootstrap avoids duplicate vehicle generation if the Play button is entered again without domain reload.

## Vehicle setup and tuning

For a reusable vehicle prefab, create a root with a `Rigidbody`, chassis collider, `VehicleInput`, and `VehicleController`; add four child `WheelCollider`s and corresponding visual wheel transforms. Assign them with `VehicleController.SetWheels` (or extend the inspector wiring). Place the wheel collider centers at the suspension positions and tune radius, suspension spring/damper and friction for the model. Create a `VehicleConfig` asset to tune mass, torque, ratios, steering, braking, top speed, fuel tank, consumption and anti-roll stiffness. Test on a flat collider surface before adjusting suspension.

The included generator builds a simple visual car from Unity primitives so the project runs without imported assets. Its shape/materials are intentionally prototype quality. Replace the visuals with licensed vehicle assets for production.

## Current implementation and limits

- Implemented in code: four-wheel collider drive, configurable torque distribution, automatic gear selection, steering fade at speed, braking and handbrake, suspension configuration, anti-roll forces, fuel consumption/refuel API, collision-based condition loss/repair API, chase camera and speed/RPM/gear/fuel/condition HUD.
- Not implemented in this stage: saved project scene/prefabs, real vehicle/audio assets, tire temperature/wear, damage visuals, customization UI, save profiles, world streaming, weather, traffic, missions, economy, LAN discovery, internet matchmaking or multiplayer synchronization. These are explicitly later milestones, not hidden placeholders.
- WheelCollider behavior and driveability have **not** been run or tested in Unity in this environment. Verify in the target Unity editor and tune per Unity version/platform before treating the prototype as validated.

## Staged roadmap

1. **Driving prototype (current):** vehicle physics skeleton and generated practice scene. Verify in Unity; improve calibration and add edit-time prefab/scene assets.
2. **World and vehicle data:** modular streamed terrain/road tiles, lighting/weather, authored ScriptableObject vehicle lineup and quality LOD-ready art pipeline.
3. **Gameplay and persistence:** missions, traffic/navigation, economy, garage customization and versioned save profiles.
4. **Networking:** select NGO + Unity Transport (or another maintained stack), implement authoritative host simulation, vehicle state replication/interpolation, lobby, explicit LAN UDP discovery and direct-IP joining, connection/reconnect flows, and separate internet matchmaking integration. Test on multiple devices/networks before claiming support.
5. **Optimization and release:** profiling, pooling, LOD/occlusion, graphics presets, Android input and performance, audio/visual polish, build and regression testing.

## Verification checklist for Stage 1

- Confirm scripts compile with the project's Unity version and input setting.
- Enter Play and check the car falls onto the road/ground, responds to steering/throttle, stops with Space, and camera follows.
- Test reverse, automatic up/downshifts, fuel decrement under throttle, and impact-condition loss at safe low speed.
- Check wheel grounding and handling on both road directions. Adjust spring/damper/friction and center of mass if unstable.
- Test a Windows player build; Android/controller support has not yet been validated.

## Repository layout

```text
Assets/OpenRoadSim/Scripts/
  PrototypeBootstrap.cs
  Vehicle/
    VehicleConfig.cs
    VehicleController.cs
    VehicleFollowCamera.cs
    VehicleHud.cs
    VehicleInput.cs
```
