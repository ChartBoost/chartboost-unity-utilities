using System;
using Chartboost.Caching;
using NUnit.Framework;
// ReSharper disable UnusedMember.Local

namespace Chartboost.Tests
{
    public class AdCacheTests
    {
        private const long PrimaryId = 12345L;
        private const long SecondaryId = 67890L;

        private DummyAd _ad;
        private DummyAd _ad2;

        [SetUp]
        public void SetUp()
        {
            AdCache<DummyAd>.ClearForTesting();
            AdCache<OtherAd>.ClearForTesting();
            _ad = new DummyAd();
            _ad2 = new DummyAd();
        }

        [TearDown]
        public void TearDown()
        {
            AdCache<DummyAd>.ClearForTesting();
            AdCache<OtherAd>.ClearForTesting();
        }

        [Test]
        public void TrackAndGetByLongReturnsSameInstance()
        {
            AdCache<DummyAd>.TrackAd(PrimaryId, _ad);
            Assert.AreSame(_ad, AdCache<DummyAd>.GetAd(PrimaryId));
        }

        [Test]
        public void TrackAndGetByIntPtrReturnsSameInstance()
        {
            var id = new IntPtr(PrimaryId);
            AdCache<DummyAd>.TrackAd(id, _ad);
            Assert.AreSame(_ad, AdCache<DummyAd>.GetAd(id));
        }

        [Test]
        public void GetWithUnknownIdReturnsNull()
        {
            Assert.IsNull(AdCache<DummyAd>.GetAd(PrimaryId));
        }

        [Test]
        public void ReleaseRemovesFromCache()
        {
            AdCache<DummyAd>.TrackAd(PrimaryId, _ad);
            AdCache<DummyAd>.ReleaseAd(PrimaryId);
            Assert.IsNull(AdCache<DummyAd>.GetAd(PrimaryId));
        }

        [Test]
        public void ReleaseWithUnknownIdDoesNotThrow()
        {
            Assert.DoesNotThrow(() => AdCache<DummyAd>.ReleaseAd(PrimaryId));
        }

        [Test]
        public void ReleaseByOwnerRemovesItsOwnEntry()
        {
            AdCache<DummyAd>.TrackAd(PrimaryId, _ad);

            AdCache<DummyAd>.ReleaseAd(PrimaryId, _ad);

            Assert.IsNull(AdCache<DummyAd>.GetAd(PrimaryId));
        }

        // A stale ad disposed (or finalized) after a newer one took over its id must not evict the newer entry.
        [Test]
        public void ReleaseByAStaleOwnerKeepsTheNewerEntry()
        {
            AdCache<DummyAd>.TrackAd(PrimaryId, _ad);
            AdCache<DummyAd>.TrackAd(PrimaryId, _ad2);

            AdCache<DummyAd>.ReleaseAd(PrimaryId, _ad);

            Assert.AreSame(_ad2, AdCache<DummyAd>.GetAd(PrimaryId));
        }

        [Test]
        public void ReleaseByOwnerWithUnknownIdDoesNotThrow()
            => Assert.DoesNotThrow(() => AdCache<DummyAd>.ReleaseAd(PrimaryId, _ad));

        [Test]
        public void ReleaseByOwnerByIntPtrBehavesLikeByLong()
        {
            AdCache<DummyAd>.TrackAd(PrimaryId, _ad);
            AdCache<DummyAd>.TrackAd(SecondaryId, _ad2);

            AdCache<DummyAd>.ReleaseAd(new IntPtr(PrimaryId), _ad2);
            Assert.AreSame(_ad, AdCache<DummyAd>.GetAd(PrimaryId));

            AdCache<DummyAd>.ReleaseAd(new IntPtr(PrimaryId), _ad);
            Assert.IsNull(AdCache<DummyAd>.GetAd(PrimaryId));
        }

        // The finalizer case: a finalizing ad reads as dead, so its dead entry is still its own to remove.
        [Test]
        public void ReleaseByOwnerRemovesAnEntryWhoseAdIsDead()
        {
            AdCache<DummyAd>.TrackEntryForTesting(PrimaryId, new WeakReference<DummyAd>(null));

            AdCache<DummyAd>.ReleaseAd(PrimaryId, _ad);

            Assert.AreEqual(0, AdCache<DummyAd>.Count);
        }

        [Test]
        public void TrackWithSameIdOverwritesPrevious()
        {
            AdCache<DummyAd>.TrackAd(PrimaryId, _ad);
            AdCache<DummyAd>.TrackAd(PrimaryId, _ad2);
            Assert.AreSame(_ad2, AdCache<DummyAd>.GetAd(PrimaryId));
        }

        [Test]
        public void TracksMultipleIdsIndependently()
        {
            AdCache<DummyAd>.TrackAd(PrimaryId, _ad);
            AdCache<DummyAd>.TrackAd(SecondaryId, _ad2);
            Assert.AreSame(_ad, AdCache<DummyAd>.GetAd(PrimaryId));
            Assert.AreSame(_ad2, AdCache<DummyAd>.GetAd(SecondaryId));
        }

        [Test]
        public void ClosedGenericsOwnIsolatedStores()
        {
            var other = new OtherAd();
            AdCache<DummyAd>.TrackAd(PrimaryId, _ad);
            AdCache<OtherAd>.TrackAd(PrimaryId, other);

            // Same key, different closed generic => different store, no collision.
            Assert.AreSame(_ad, AdCache<DummyAd>.GetAd(PrimaryId));
            Assert.AreSame(other, AdCache<OtherAd>.GetAd(PrimaryId));
        }

        [Test]
        public void CountReflectsTrackedAds()
        {
            Assert.AreEqual(0, AdCache<DummyAd>.Count);

            AdCache<DummyAd>.TrackAd(PrimaryId, _ad);
            AdCache<DummyAd>.TrackAd(SecondaryId, _ad2);

            Assert.AreEqual(2, AdCache<DummyAd>.Count);
        }

        [Test]
        public void CacheInfoReportsType()
        {
            var info = AdCache<DummyAd>.CacheInfo();
            Assert.IsTrue(info.Contains("AdCache<DummyAd>:"), $"Unexpected CacheInfo: {info}");
            Assert.IsTrue(info.Contains(" tracked"), $"Unexpected CacheInfo: {info}");
        }

        private class DummyAd { }

        private class OtherAd { }
    }
}
