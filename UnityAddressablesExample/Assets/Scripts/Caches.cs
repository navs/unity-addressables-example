using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Caches
{
    public static long GetAllCachedSize()
    {
        long cacheSize = 0;
        var cachePaths = new List<string>();

        Caching.GetAllCachePaths(cachePaths);
        foreach (string cachePath in cachePaths)
        {
            var cache = Caching.GetCacheByPath(cachePath);
            cacheSize += cache.spaceOccupied;
        }
        return cacheSize;
    }

    public static bool Clear()
    {
        return Caching.ClearCache();
    }
}
