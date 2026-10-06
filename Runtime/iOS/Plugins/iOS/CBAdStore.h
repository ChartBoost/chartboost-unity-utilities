#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

/// Thread-safe map that retains native ad objects so ARC doesn't deallocate them while Unity holds a
/// managed handle, and looks them up by an integer identity (typically the object pointer cast to @c long)
/// for cache/show/destroy calls coming from C#.
///
/// Instantiable so callers can keep isolated stores (e.g. one per ad type). Dependency-free — the native
/// analog of the managed @c Chartboost.Caching.AdCache<T>. Shared by the Chartboost Mediation and
/// Monetization Unity wrappers.
@interface CBAdStore : NSObject

/// Retains @c ad under @c key (typically @c (long)ad). No-op if @c ad is nil.
- (void)trackAd:(id)ad forKey:(long)key;

/// Returns the ad retained under @c key, or nil if none.
- (nullable id)adForKey:(long)key;

/// Releases the ad retained under @c key and returns it (or nil if none), allowing ARC to deallocate it.
- (nullable id)removeKey:(long)key;

/// Releases every retained ad.
- (void)clear;

/// The number of ads currently retained.
@property (nonatomic, readonly) NSUInteger count;

@end

NS_ASSUME_NONNULL_END
