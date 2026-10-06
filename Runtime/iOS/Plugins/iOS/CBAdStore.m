#import "CBAdStore.h"

@interface CBAdStore ()
@property (nonatomic, strong) NSMutableDictionary<NSNumber *, id> *ads;
@end

@implementation CBAdStore

- (instancetype)init {
    self = [super init];
    if (self) {
        _ads = [NSMutableDictionary dictionary];
    }
    return self;
}

// Native ad callbacks and Unity-thread calls can race; guard all access (parity with Android's ConcurrentHashMap).
- (void)trackAd:(id)ad forKey:(long)key {
    if (!ad) return;
    @synchronized (self) { self.ads[@(key)] = ad; }
}

- (nullable id)adForKey:(long)key {
    @synchronized (self) { return self.ads[@(key)]; }
}

- (nullable id)removeKey:(long)key {
    @synchronized (self) {
        id ad = self.ads[@(key)];
        [self.ads removeObjectForKey:@(key)];
        return ad;
    }
}

- (void)clear {
    @synchronized (self) { [self.ads removeAllObjects]; }
}

- (NSUInteger)count {
    @synchronized (self) { return self.ads.count; }
}

@end
