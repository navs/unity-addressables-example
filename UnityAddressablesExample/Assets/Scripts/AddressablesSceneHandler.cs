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
}
