# Changelog
All notable changes to this project will be documented in this file using the standards as defined at [Keep a Changelog](https://keepachangelog.com/en/1.0.0/). This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0).

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