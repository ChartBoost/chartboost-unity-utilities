// swift-tools-version:5.9
// Native unit tests for the iOS utilities sources (no Unity, no pods). Sources are symlinks to
// Runtime/iOS/Plugins/iOS, so the tests build the exact files Unity ships. Run:
//   xcodebuild test -scheme ChartboostUtilitiesNative-Package -destination 'platform=iOS Simulator,name=<device>'
import PackageDescription

let package = Package(
    name: "ChartboostUtilitiesNative",
    platforms: [.iOS(.v13)],
    products: [
        .library(name: "CBUBannerContainer", targets: ["CBUBannerContainer"]),
        .library(name: "ChartboostUnityUtilities", targets: ["ChartboostUnityUtilities"]),
    ],
    targets: [
        .target(name: "CBUBannerContainer", path: "Sources/CBUBannerContainer"),
        .testTarget(name: "CBUBannerContainerTests", dependencies: ["CBUBannerContainer"], path: "Tests/CBUBannerContainerTests"),
        .target(name: "ChartboostUnityUtilities", path: "Sources/ChartboostUnityUtilities"),
        .testTarget(name: "ChartboostUnityUtilitiesTests", dependencies: ["ChartboostUnityUtilities"], path: "Tests/ChartboostUnityUtilitiesTests"),
    ]
)
