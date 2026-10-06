using UnityEngine;

namespace Chartboost.Testing
{
    /// <summary>Platform check for tests that only make sense against the native SDKs.</summary>
    public static class DevicePlatform
    {
        /// <summary>True on an Android or iOS device, simulator or emulator; false in the Editor.</summary>
        public static bool IsNative
            => Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
    }
}
