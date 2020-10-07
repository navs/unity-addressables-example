using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class AddressablesSceneHandler : MonoBehaviour
{
    public Text DownloadingText;

    private void OnGUI()
    {
        GUICache();
        GUIAddressableInit();
        GUIBundles();
        GUIBundleCaches();
    }

    private long cacheSize = 0;
    void GUICache()
    {
        GUILayout.BeginHorizontal("box");
        {
            if (GUILayout.Button("Clear", GUILayout.Height(40)))
            {
                if (!Caches.Clear())
                {
                    Debug.Log("Failed to clear cache.");
                }
            }
            if (GUILayout.Button("Refresh Size", GUILayout.Height(40)))
            {
                cacheSize = Caches.GetAllCachedSize();
            }
            GUILayout.Label($"SIZE: {cacheSize}");
        }
        GUILayout.EndHorizontal();
    }

    void GUIAddressableInit()
    {
        GUILayout.BeginHorizontal("box");
        {
            if (GUILayout.Button("Init Addressables", GUILayout.Height(40)))
            {
                BundleManager.Instance.Init();
            }
            if (GUILayout.Button("Refresh", GUILayout.Height(40)))
            {
                BundleManager.Instance.RefreshBundles();
            }
        }
        GUILayout.EndHorizontal();
    }

    void GUIBundles()
    {
        var bundles = BundleManager.Instance.Bundles;
        if (bundles == null || bundles.Count == 0)
        {
            return;
        }

        GUILayout.BeginVertical("box");

        if (BundleManager.Instance.HasUpdate() && GUILayout.Button("Download ALL"))
        {
            var handle = Addressables.DownloadDependenciesAsync(BundleManager.Instance.GetUpdateKeysAll(), Addressables.MergeMode.Union);
            if (DownloadingText)
            {
                AsyncOperationMonitor.Create(handle, (finish, percent) =>
                {
                    if (finish)
                    {
                        DownloadingText.text = $"Downloaded";
                        BundleManager.Instance.RefreshBundles();
                        cacheSize = Caches.GetAllCachedSize();
                    }
                    else
                    {
                        DownloadingText.text = $"Downloading {percent:F2} %";
                    }
                });
            }
            else
            {
                handle.Completed += (h) =>
                {
                    BundleManager.Instance.RefreshBundles();
                    cacheSize = Caches.GetAllCachedSize();
                };
            }
        }

        foreach (var bundle in bundles)
        {
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button(bundle.name) && bundle.size > 0)
                {
                    Debug.Log($"Start Loading {bundle.key}");
                    var handle = Addressables.DownloadDependenciesAsync(bundle.key);
                    if (DownloadingText)
                    {
                        DownloadingText.text = "Start";
                        AsyncOperationMonitor.Create(handle, (finish, percent) =>
                        {
                            if (finish)
                            {
                                DownloadingText.text = $"{bundle.name} Downloaded";
                                BundleManager.Instance.RefreshBundles();
                                cacheSize = Caches.GetAllCachedSize();

                                // Release _aa resource
                                Addressables.Release(handle.Result);
                            }
                            else
                            {
                                DownloadingText.text = $"{bundle.name} Downloading {percent:F2} %";
                            }
                        });
                    }
                    else
                    {
                        handle.Completed += (h) =>
                        {
                            BundleManager.Instance.RefreshBundles();
                            cacheSize = Caches.GetAllCachedSize();
                        };
                    }
                }

                GUILayout.Label($"[ {bundle.size} ]");

                if (!string.IsNullOrEmpty(bundle.bundleName))
                {
                    GUILayout.Button($"{bundle.bundleName.Substring(0, 6)}");
                    GUILayout.Button($"{bundle.bundleHash.Substring(0, 6)}");
                }
                else
                {
                    GUILayout.Label("no bundle name & hash");
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    void GUIBundleCaches()
    {
        int clearCounter = 0;
        GUILayout.BeginVertical("box");
        {
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("Save", GUILayout.Height(40)))
                {
                    BundleCaches.Instance.Save();
                }
                if (GUILayout.Button("Load", GUILayout.Height(40)))
                {
                    BundleCaches.Instance.Load();
                }
                if (GUILayout.Button("Add current", GUILayout.Height(40)))
                {
                    BundleCaches.Instance.Add(BundleManager.Instance.Bundles.Select(b => (b.bundleName, b.bundleHash)));
                }
                if (GUILayout.Button("Clear Old Bundle Caches", GUILayout.Height(40)))
                {
                    foreach (var bc in BundleCaches.Instance.Bundles)
                    {
                        if (!BundleManager.Instance.HasBundleWithHash(bc.name, bc.hash))
                        {
                            Debug.Log($"Removing {bc.name} : {bc.hash}");
                            Caching.ClearCachedVersion(bc.name, Hash128.Parse(bc.hash));
                            clearCounter++;
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();

            foreach (var bundleCache in BundleCaches.Instance.Bundles)
            {
                GUILayout.BeginHorizontal("box");
                {
                    if (GUILayout.Button("X", GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        clearCounter++;
                        Caching.ClearCachedVersion(bundleCache.name, Hash128.Parse(bundleCache.hash));
                    }
                    GUILayout.Button($"{bundleCache.name.Substring(0, 6)}");
                    GUILayout.Button($"{bundleCache.hash.Substring(0, 6)}");
                    GUILayout.Button($"{BundleManager.Instance.Bundles.Exists(b => b.bundleName == bundleCache.name && b.bundleHash == bundleCache.hash)}");
                }
                GUILayout.EndVertical();
            }
        }
        GUILayout.EndVertical();

        if (clearCounter > 0)
        {
            cacheSize = Caches.GetAllCachedSize();
            BundleManager.Instance.RefreshBundles();
        }

    }
}
