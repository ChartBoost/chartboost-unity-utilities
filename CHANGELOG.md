# Changelog
All notable changes to this project will be documented in this file using the standards as defined at [Keep a Changelog](https://keepachangelog.com/en/1.0.0/). This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0).

### Version 2.0.0 *(2026-10-06)*

### Added
- Generic `Chartboost.Caching.AdCache<T>` — a thread-safe weak-reference cache for native-backed ad objects, keyed by Java `hashCode()` (Android) / native pointer (iOS). Each closed generic (e.g. `AdCache<IAd>`) owns an isolated store.
- Shared native `AdStore` primitive — a thread-safe, dependency-free map that retains native ad objects by integer identity (Android `com.chartboost.unity.utilities.utils.AdStore<T>`, shipped in `ChartboostUtilitiesUnity.jar`; iOS `CBAdStore`). The native analog of `AdCache<T>`, consumed by the Mediation and Monetization Unity wrappers.
- Test-only `Chartboost.Utilities.Testing` assembly (namespace `Chartboost.Testing`) with the helpers shared by the Chartboost SDK, adapter and consent module tests: `DevicePlatform`, `NativeTestFixture` (device-only fixtures tagged `[Category("Device")]`, with on-screen per-test progress), `TestWait.Until`, `AdInventoryAssert`, `CallbackRecorder<TSender, TValue>`, `TestProgressTracker`, `AdapterContract` (the identity, setting and Default-log checks every Mediation adapter and Core consent module shares) and `DebugLogLevelFixture`. It compiles only when tests are included; to run those packages' tests, also list `com.chartboost.unity.utilities` in your project's `testables`.

### Changed
- `StronglyTyped<T>` now has value equality (`Equals`, `GetHashCode`, `IEquatable<StronglyTyped<T>>`): two instances of the same type with equal values are equal, so they also collapse to one key in a `HashSet` or `Dictionary`. `==` still compares references.

### Version 1.0.4 *(2025-11-20)*

### Fixed
- iOS `toMain` function now validates block parameter and checks if already on main thread
- Android `PointFToVector2` now uses constants instead of hardcoded property names

### Added
- `DeserializeNullableObject<T>` method for class types in `JsonTools`
- Renamed `DeserializeNullableObject<T>` to `DeserializeNullableStruct<T>` for struct types

### Changed
- Updated `com.chartboost.unity.logging` dependency from 1.0.2 to 1.1.0

### Version 1.0.3 *(2025-06-11)*
Bug Fixes:

- Fix wrong internal namespaces definitions.
- Fix Editor Unit tests to target correct package definitions.
- Fix `VersionCheck` `AreEqual` assertion.

### Version 1.0.2 *(2024-09-19)*
Added:
- `toObjectFromJson` in `ChartboostUnityUtilities.h` for reusable JSON serialization.

### Version 1.0.1 *(2024-08-01)*
Added:

- `ApplicationPreferences` a data storage registry focused on Chartboost SDKs and their context.

### Version 1.0.0 *(2024-01-26)*

First version of Chartboost Utilities package for Unity.

Added:
- General Chartboost shared constants an extension methods.