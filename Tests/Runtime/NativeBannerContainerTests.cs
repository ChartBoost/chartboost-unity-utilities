using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Chartboost.Banners;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Chartboost.Tests
{
    public class NativeBannerContainerTests
    {
        private static int _nextId = 1000;

        private FakePlatform _platform;
        private NativeBannerContainer _container;
        private SynchronizationContext _originalContext;

        [SetUp]
        public void SetUp()
        {
            _originalContext = SynchronizationContext.Current;
            BannerContainerRegistry.CaptureMainThread(null);
            _platform = new FakePlatform(Interlocked.Increment(ref _nextId));
            _container = new NativeBannerContainer(_platform);
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
            BannerContainerRegistry.CaptureMainThread(_originalContext);
        }

        [Test]
        public void DefaultsMatchTheNativeContainer()
        {
            Assert.IsTrue(_container.Visible);
            Assert.IsTrue(_container.ClipContent);
            Assert.IsTrue(_container.InteractionEnabled);
            Assert.IsFalse(_container.Draggable);
            Assert.IsFalse(_container.IsAbsolute);
            Assert.AreEqual(BannerAnchor.BottomCenter, _container.Anchor);
            Assert.AreEqual(new Vector2(NativeBannerContainer.Wrap, NativeBannerContainer.Wrap), _container.Size);
            CollectionAssert.IsEmpty(_platform.Calls);
        }

        [Test]
        public void SettersForwardOnceAndSkipUnchangedValues()
        {
            _container.Visible = false;
            _container.Visible = false;
            _container.Visible = true; // default again, still a change from false
            _container.SetSize(320, 50);
            _container.SetSize(320, 50);
            _container.SetAlignment(BannerHorizontalAlignment.Center, BannerVerticalAlignment.Center);
            _container.Draggable = true;

            CollectionAssert.AreEqual(new[] { "SetVisible:False", "SetVisible:True", "SetSize:320x50", "SetDraggable:True" }, _platform.Calls);
        }

        [Test]
        public void AnchorAndAbsoluteSwitchModes()
        {
            _container.SetAbsolute(new Vector2(10, 20), new Vector2(0.5f, 0.5f));
            Assert.IsTrue(_container.IsAbsolute);
            Assert.AreEqual(new Vector2(10, 20), _container.Position);

            _container.SetAnchor(BannerAnchor.BottomCenter);
            Assert.IsFalse(_container.IsAbsolute);
            _container.SetAnchor(BannerAnchor.BottomCenter); // placement is always forwarded

            CollectionAssert.AreEqual(new[] { "SetAbsolute:(10.00, 20.00)/(0.50, 0.50)", "SetAnchor:BottomCenter/True", "SetAnchor:BottomCenter/True" }, _platform.Calls);
        }

        [Test]
        public void SetHostNoneForwardsAnEmptyView()
        {
            _container.SetHost(NativeView.FromPointer(new IntPtr(42)));
            _container.SetHost(NativeView.None);

            CollectionAssert.AreEqual(new[] { "SetHost:42", "SetHost:none" }, _platform.Calls);
            Assert.IsTrue(NativeView.None.IsNone);
        }

        [Test]
        public void DisposeReleasesOnceAndIgnoresLaterCalls()
        {
            var count = BannerContainerRegistry.Count;
            _container.Dispose();
            _container.Dispose();
            _container.Visible = false;
            _container.SetHost(NativeView.None);

            Assert.IsTrue(_container.IsDisposed);
            Assert.AreEqual(count - 1, BannerContainerRegistry.Count);
            CollectionAssert.AreEqual(new[] { "Dispose" }, _platform.Calls);
        }

        [Test]
        public void CallbacksReachTheContainerByIdAndKeepItsState()
        {
            Vector2? ended = null;
            Rect? bounds = null;
            var boundsEvents = 0;
            _container.DragEnded += (_, origin) => ended = origin;
            _container.BoundsChanged += (_, rect) => { bounds = rect; boundsEvents++; };

            BannerContainerRegistry.Dispatch(_platform.Id, c => c.OnDragEnded(new Vector2(30, 60), new Vector2(10, 20)));
            BannerContainerRegistry.Dispatch(_platform.Id, c => c.OnBoundsChanged(new Rect(1, 2, 320, 50)));
            BannerContainerRegistry.Dispatch(_platform.Id, c => c.OnBoundsChanged(new Rect(1, 2, 320, 50)));

            Assert.AreEqual(new Vector2(30, 60), ended);
            Assert.IsTrue(_container.IsAbsolute);
            Assert.AreEqual(new Vector2(10, 20), _container.Position);
            Assert.AreEqual(new Rect(1, 2, 320, 50), bounds);
            Assert.AreEqual(1, boundsEvents, "an unchanged frame is not re-raised");
        }

        [Test]
        public void CallbacksFromAnotherThreadArePostedToTheMainContext()
        {
            var context = new RecordingContext();
            BannerContainerRegistry.CaptureMainThread(context);
            var began = false;
            _container.DragBegan += (_, _) => began = true;

            var worker = new Thread(() => BannerContainerRegistry.Dispatch(_platform.Id, c => c.OnDragBegan(Vector2.zero)));
            worker.Start();
            worker.Join();

            Assert.IsFalse(began, "nothing runs on the native thread");
            context.RunAll();
            Assert.IsTrue(began);
        }

        [Test]
        public void CallbacksForDisposedOrUnknownContainersAreDropped()
        {
            var raised = false;
            _container.BoundsChanged += (_, _) => raised = true;
            _container.Dispose();

            BannerContainerRegistry.Dispatch(_platform.Id, c => c.OnBoundsChanged(new Rect(0, 0, 1, 1)));
            BannerContainerRegistry.Dispatch(-1, c => c.OnBoundsChanged(new Rect(0, 0, 1, 1)));

            Assert.IsFalse(raised);
        }

        [Test]
        public void SubscriberExceptionsAreLoggedNotThrown()
        {
            _container.Dragged += (_, _) => throw new InvalidOperationException("boom");
            LogAssert.Expect(LogType.Exception, new Regex("boom"));

            Assert.DoesNotThrow(() => BannerContainerRegistry.Dispatch(_platform.Id, c => c.OnDragged(Vector2.one)));
        }

        private sealed class FakePlatform : IBannerContainerPlatform
        {
            public FakePlatform(int id) => Id = id;
            public int Id { get; }
            public List<string> Calls { get; } = new();

            public void SetAnchor(BannerAnchor anchor, bool useSafeArea) => Calls.Add($"SetAnchor:{anchor}/{useSafeArea}");
            public void SetAbsolute(Vector2 position, Vector2 pivot) => Calls.Add($"SetAbsolute:{position}/{pivot}");
            public void SetSize(float width, float height) => Calls.Add($"SetSize:{width}x{height}");
            public void SetAlignment(BannerHorizontalAlignment horizontal, BannerVerticalAlignment vertical) => Calls.Add($"SetAlignment:{horizontal}/{vertical}");
            public void SetBackgroundColor(Color color) => Calls.Add($"SetBackgroundColor:{color}");
            public void SetClipContent(bool clip) => Calls.Add($"SetClipContent:{clip}");
            public void SetVisible(bool visible) => Calls.Add($"SetVisible:{visible}");
            public void SetInteractionEnabled(bool enabled) => Calls.Add($"SetInteractionEnabled:{enabled}");
            public void SetDraggable(bool draggable) => Calls.Add($"SetDraggable:{draggable}");
            public void SetHost(NativeView host) => Calls.Add(host.IsNone ? "SetHost:none" : $"SetHost:{host.Pointer}");
            public void Attach() => Calls.Add("Attach");
            public void Detach() => Calls.Add("Detach");
            public void Dispose() => Calls.Add("Dispose");
            public bool CanDisposeFromFinalizer => true;
        }

        // The posting thread is joined before RunAll, so no locking is needed.
        private sealed class RecordingContext : SynchronizationContext
        {
            private readonly List<(SendOrPostCallback callback, object state)> _posted = new();
            public override void Post(SendOrPostCallback d, object state) => _posted.Add((d, state));

            public void RunAll()
            {
                foreach (var (callback, state) in _posted)
                    callback(state);
                _posted.Clear();
            }
        }
    }
}
