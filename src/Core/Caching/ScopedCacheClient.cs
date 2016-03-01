﻿using Foundatio.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foundatio.Caching {
    public class ScopedCacheClient : ICacheClient {
        public ScopedCacheClient(ICacheClient client, string scope) {
            UnscopedCache = client ?? new NullCacheClient();
            Scope = scope;
        }

        public ICacheClient UnscopedCache { get; private set; }

        public string Scope { get; private set; }

        protected string GetScopedCacheKey(string key) {
            return String.Concat(Scope, ":", key);
        }

        protected IEnumerable<string> GetScopedCacheKeys(IEnumerable<string> keys) {
            return keys?.Select(GetScopedCacheKey);
        }

        protected string GetUnscopedCacheKey(string scopedKey) {
            int i = scopedKey.IndexOf(':');

            if (-1 < i && i < (scopedKey.Length - 1)) {
                return scopedKey.Substring(i + 1);
            }

            return scopedKey;
        }

        public Task<int> RemoveAllAsync(IEnumerable<string> keys = null) {
            if (keys == null)
                return RemoveByPrefixAsync(String.Empty);

            return UnscopedCache.RemoveAllAsync(GetScopedCacheKeys(keys));
        }

        public Task<int> RemoveByPrefixAsync(string prefix) {
            return UnscopedCache.RemoveByPrefixAsync(GetScopedCacheKey(prefix));
        }

        public Task<CacheValue<T>> GetAsync<T>(string key) {
            return UnscopedCache.GetAsync<T>(GetScopedCacheKey(key));
        }

        public async Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> keys) {
            var scopedValueMap = await UnscopedCache.GetAllAsync<T>(GetScopedCacheKeys(keys)).AnyContext();
            var valueMap = new Dictionary<string, CacheValue<T>>();

            foreach (var kvp in scopedValueMap)
                valueMap[GetUnscopedCacheKey(kvp.Key)] = kvp.Value;

            return valueMap;
        }

        public Task<bool> AddAsync<T>(string key, T value, TimeSpan? expiresIn = null) {
            return UnscopedCache.AddAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiresIn = null) {
            return UnscopedCache.SetAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public Task<int> SetAllAsync<T>(IDictionary<string, T> values, TimeSpan? expiresIn = null) {
            return UnscopedCache.SetAllAsync(values.ToDictionary(kvp => GetScopedCacheKey(kvp.Key), kvp => kvp.Value), expiresIn);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan? expiresIn = null) {
            return UnscopedCache.ReplaceAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public Task<double> IncrementAsync(string key, double amount = 1, TimeSpan? expiresIn = null) {
            return UnscopedCache.IncrementAsync(GetScopedCacheKey(key), amount, expiresIn);
        }
        
        public Task<bool> ExistsAsync(string key) {
            return UnscopedCache.ExistsAsync(GetScopedCacheKey(key));
        }

        public Task<TimeSpan?> GetExpirationAsync(string key) {
            return UnscopedCache.GetExpirationAsync(GetScopedCacheKey(key));
        }

        public Task SetExpirationAsync(string key, TimeSpan expiresIn) {
            return UnscopedCache.SetExpirationAsync(GetScopedCacheKey(key), expiresIn);
        }

        public Task<double> SetIfHigherAsync(string key, double value, TimeSpan? expiresIn = null) {
            return UnscopedCache.SetIfHigherAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public Task<double> SetIfLowerAsync(string key, double value, TimeSpan? expiresIn = null) {
            return UnscopedCache.SetIfLowerAsync(GetScopedCacheKey(key), value, expiresIn);
        }

        public void Dispose() {}
    }
}
