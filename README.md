# Chartboost Canary Utilities

Simple utilities package for the Chartboost Unity development environment.

# Installation
This package is meant to be a dependency for other Chartboost Packages; however, if you wish to use it by itself, it can be installed through UPM & NuGet as follows:

## Using the public [npm registry](https://www.npmjs.com/search?q=com.chartboost.unity.utilities)
```json
"dependencies": {
    "com.chartboost.unity.utilities": "1.0.1",
    ...
},
"scopedRegistries": [
{
    "name": "NpmJS",
    "url": "https://registry.npmjs.org",
    "scopes": [
    "com.chartboost"
    ]
}
]
```

## Using the public [NuGet package](https://www.nuget.org/packages/Chartboost.CSharp.Utilities.Unity)

To add the Chartboost Core Unity SDK to your project using the NuGet package, you will first need to add the [NugetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) package into your Unity Project.

This can be done by adding the following to your Unity Project's ***manifest.json***

```json
  "dependencies": {
    "com.github-glitchenzo.nugetforunity": "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity",
    ...
  },
```

Once <code>NugetForUnity</code> is installed, search for `Chartboost.CSharp.Utilities.Unity` in the search bar of Nuget Explorer window(Nuget -> Manage Nuget Packages).
You should be able to see the `Chartboost.CSharp.Utilities.Unity` package. Choose the appropriate version and install.

# Usage 

## ApplicationPreferences
Stores local data related to Chartboost SDKs. Data is stored in local registries without encryption. Defaults to `PlayerPrefs` if native implementation does not exist.

Works in a similar fashion as [PlayerPrefs](https://docs.unity3d.com/ScriptReference/PlayerPrefs.html) but encapsulated to the app's Chartboost contexts and SDKs.