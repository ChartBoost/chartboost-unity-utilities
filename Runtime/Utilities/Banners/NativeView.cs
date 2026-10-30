using System;
using UnityEngine;

namespace Chartboost.Banners
{
    /// <summary>
    /// A platform view a banner container can be hosted in: a <c>ViewGroup</c> on Android or a <c>UIView</c>
    /// pointer on iOS. The container never keeps or disposes the passed Java reference.
    /// </summary>
    public readonly struct NativeView
    {
        /// <summary>No host: the container goes back to its stored placement in the Unity view.</summary>
        public static readonly NativeView None = default;

        /// <summary>The Android view, or null.</summary>
        public AndroidJavaObject JavaObject { get; }

        /// <summary>The iOS view pointer, or <see cref="IntPtr.Zero"/>.</summary>
        public IntPtr Pointer { get; }

        private NativeView(AndroidJavaObject javaObject, IntPtr pointer)
        {
            JavaObject = javaObject;
            Pointer = pointer;
        }

        /// <summary>Wraps an Android <c>ViewGroup</c>.</summary>
        public static NativeView FromJavaObject(AndroidJavaObject view) => new(view, IntPtr.Zero);

        /// <summary>Wraps an iOS <c>UIView</c> pointer.</summary>
        public static NativeView FromPointer(IntPtr view) => new(null, view);

        /// <summary>True when this names no view.</summary>
        public bool IsNone => JavaObject == null && Pointer == IntPtr.Zero;
    }
}
