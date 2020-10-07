using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class BundleManager 
{
    private static BundleManager instance;
    public static BundleManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new BundleManager();
            }
            return instance;
        }
    }
    
    private BundleManager()
    {
        bundles = new List<(string key, string name, long size, string bundleName, string bundleHash)>();
    }

    private List<(string key, string name, long size, string bundleName, string bundleHash)> bundles;
    public List<(string key, string name, long size, string bundleName, string bundleHash)> Bundles => bundles;

    public void Init()
    {
        Addressables.InitializeAsync().Completed += (h) =>
        {
            if (h.IsDone && h.Status == AsyncOperationStatus.Succeeded)
            {
                RefreshBundles();
            }
            else
            {
                Debug.Log("Failed to initialize addressables");
            }
        };
    }

    public void RefreshBundles()
    {
        bundles.Clear();
        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var key in locator.Keys)
            {
                if (key is string keyStr && keyStr.EndsWith("_aa.txt"))
                {
                    AddBundle(locator, keyStr);
                }
            }
        }
    }

    void AddBundle(IResourceLocator locator, string key)
    {
        Addressables.GetDownloadSizeAsync(key).Completed += (h) =>
        {
            if (h.IsDone && h.Status == AsyncOperationStatus.Succeeded)
            {
                long size = h.Result;
                string name = Path.GetFileNameWithoutExtension(key);
                name = name.Substring(0, name.Length - 3); // remove "_aa"
                string bundleName = "";
                string bundleHash = "";
                IList<IResourceLocation> locations;
                if (locator.Locate(key, typeof(object), out locations))
                {
                    foreach (var location in locations)
                    {
                        foreach (var dep in location.Dependencies)
                        {
                            if (dep.Data is AssetBundleRequestOptions bundleData)
                            {
                                bundleName = bundleData.BundleName;
                                bundleHash = bundleData.Hash;
                            }
                        }
                    }
                }
                bundles.Add((key, name, size, bundleName, bundleHash));
            }
        };
    }

    public bool HasUpdate(Predicate<(string key, string name, long size, string bundleName, string bundleHash)> match)
    {
        return bundles.Exists(b => b.size > 0 && match(b));
    }

    public bool HasUpdate()
    {
        return bundles.Exists(b => b.size > 0);
    }

    public IEnumerable<string> GetUpdateKeysAll()
    {
        return bundles.Where(b => b.size > 0).Select(b => b.key);
    }

    public bool HasBundleWithHash(string bundleName, string bundleHash)
    {
        return bundles.Exists(b => b.bundleName == bundleName && b.bundleHash == bundleHash);
    }
}
