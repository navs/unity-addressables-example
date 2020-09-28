using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.Video;

public class AddressablesSceneHandler : MonoBehaviour
{
    List<string> logs;
    Vector2 logPos;
    RawImage mainImage;
    RenderTexture rt;

    string Log
    {
        set
        {
            if (logs == null)
            {
                logs = new List<string>();
            }
            logs.Insert(0, value + "\n");
            Debug.Log(value);

            if (logs.Count > 20)
            {
                logs.RemoveRange(0, logs.Count - 20);
            }
        }
    }
    // Start is called before the first frame update

    long cacheSize;
    Vector2 locPos;
    
    void Start()
    {
        mainImage = FindObjectOfType<RawImage>();
        Log = "Started";
        RefershCacheSize(false);
    }
    private void OnGUI()
    {
        if (GUILayout.Button("Initialize"))
        {
            Addressables.InitializeAsync().Completed += (handler) =>
            {
                Log = $"Initialized ... Result : {handler.Status}";
            };
        }
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Load Prefab"))
            {
                Addressables.LoadAssetAsync<GameObject>("Assets/Art/Characters/Prefabs/Base/High Quality/MaleFree1.prefab").Completed += (h) =>
                {
                    if (h.IsDone && h.Status == AsyncOperationStatus.Succeeded)
                    {
                        GameObject instance = Instantiate<GameObject>(h.Result);
                        instance.transform.position = Vector3.zero;
                        RotateAndDispear rotator = instance.AddComponent<RotateAndDispear>();
                        rotator.InitParameters();

#if UNITY_EDITOR
                    foreach (var smr in rotator.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            for (int i = 0; i < smr.sharedMaterials.Length; i++)
                            {
                                smr.sharedMaterials[i].shader = Shader.Find(smr.sharedMaterials[i].shader.name);
                            }
                        }
#endif
                }
                    else
                    {
                        Log = "Something's wrong...";
                    }
                };
            }
            if (GUILayout.Button("Load Texture"))
            {
                Addressables.LoadAssetAsync<Texture>("Assets/Art/Textures/moamoadragon.jpg").Completed += (h) =>
                {
                    if (h.IsDone && h.Status == AsyncOperationStatus.Succeeded)
                    {
                        mainImage.texture = h.Result;
                    }
                    else
                    {
                        Log = "Something's wrong...";
                    }
                };
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label($"CacheSize:{SizeString(cacheSize)}");
            if (GUILayout.Button("Refresh"))
            {
                RefershCacheSize(false);
            }
            if (GUILayout.Button("Clean Cache"))
            {
                RefershCacheSize(true);
            }
        }
        GUILayout.EndHorizontal();

        locPos = GUILayout.BeginScrollView(locPos, GUILayout.Height(300));
        GUILayout.BeginVertical();
        {
            //var bundles = new List<string>();
            foreach(var locator in Addressables.ResourceLocators)
            {
                GUILayout.Label($"{locator.LocatorId}");
                foreach (var key in locator.Keys)
                {
                    if (key.ToString().EndsWith("bundle"))
                    {
                        GUILayout.Label($"[{key.GetType().Name}] {key}");
                        //bundles.Add(key.ToString());
                    }
                }
            }
            //if (bundles.Count > 0 && GUILayout.Button("GetDownloadSize"))
            //{
            //    Addressables.GetDownloadSizeAsync(bundles).Completed += (h) =>
            //    {
            //        if (h.IsDone && h.Status == AsyncOperationStatus.Succeeded)
            //        {
            //            long downloadSize = h.Result;
            //            Log = $"Download Size: {SizeString(downloadSize)}({downloadSize})";
            //        }
            //    };
            //}
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        //if (GUILayout.Button("Check Update Catalog"))
        //{
        //    Addressables.CheckForCatalogUpdates(false).Completed += (h) =>
        //    {
        //        if (h.IsDone && h.Status == AsyncOperationStatus.Succeeded)
        //        {
        //            Log = $"Num Catalogs for update: {h.Result.Count}";
        //            foreach (string cat in h.Result)
        //            {
        //                Log = $"UpdateCatalog:{cat}";
        //            }
        //        }
        //    };
        //}
        if (GUILayout.Button("Get Download Size"))
        {
            var keys = new List<string>() { "Characters", "Textures" };
            Addressables.GetDownloadSizeAsync(keys).Completed += (h) =>
            {
                if (h.IsDone && h.Status == AsyncOperationStatus.Succeeded)
                {
                    long downloadSize = h.Result;
                    Log = $"Download Size: {SizeString(downloadSize)}({downloadSize})";
                }
            };
        }

        if (GUILayout.Button("UpdateCatalog"))
        {
            Addressables.UpdateCatalogs(null, false).Completed += (h) =>
            {
                if (h.IsDone && h.Status == AsyncOperationStatus.Succeeded)
                {
                    foreach (var locator in h.Result)
                    {
                        Log = $"UpdateCatalog:{locator.LocatorId}";
                    }
                }

            };
        }

        if (GUILayout.Button("Movie"))
        {
            LoadMovie();
        }

        GUILayout.FlexibleSpace();

        logPos = GUILayout.BeginScrollView(logPos, "box", GUILayout.Height(200));
        GUILayout.BeginVertical();
        {
            foreach (var log in logs)
            {
                GUILayout.Label(log);
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    private void RefershCacheSize(bool cleanCache)
    {
        var cachePaths = new List<string>();
        Caching.GetAllCachePaths(cachePaths);

        cacheSize = 0;
        foreach (var cachePath in cachePaths)
        {
            var cache = Caching.GetCacheByPath(cachePath);
            cacheSize += cache.spaceOccupied;

            Log = $"cache[{cachePath}] = {cache.spaceOccupied}";
            if (cleanCache)
            {
                cache.ClearCache();
            }
        }
    }

    private static string SizeString(long size)
    {
        if (size > 1024 * 1024 * 1024L)
        {
            return $"{size / 1024 / 1024 / 1024}.{((size / 1024 / 1024) % 1024) * 100 / 1024:000}GiB";
        }
        else if (size > 1024 * 1204L)
        {
            return $"{size / 1024 / 1024}.{((size / 1024) % 1024) * 100 / 1024:000}MiB";
        } 
        else if (size > 1024)
        {
            return $"{size / 1024}.{(size % 1024) * 100 / 1024:000}KiB";
        }
        return $"{size}B";
    }

    private void LoadMovie()
    {
        Addressables.LoadAssetAsync<VideoClip>("Assets/Art/Movies/anne_35_38.avi").Completed += (h) =>
        {
            if (h.IsDone && h.Status == AsyncOperationStatus.Succeeded)
            {
                var playerObject = new GameObject("VidioPlayer");
                var player = playerObject.AddComponent<VideoPlayer>();
                var audioSource = playerObject.AddComponent<AudioSource>();
                player.clip = h.Result;
                //player.renderMode = VideoRenderMode.MaterialOverride;
                //player.targetMaterialProperty = "_Texture";
                //player.targetMaterialRenderer = mainImage.GetComponent<Renderer>();

                player.renderMode = VideoRenderMode.RenderTexture;
                if (rt == null)
                {
                    rt = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
                    rt.Create();
                }
                if (mainImage)
                {
                    mainImage.texture = rt;
                }
                player.targetTexture = rt;


                player.audioOutputMode = VideoAudioOutputMode.AudioSource;
                player.SetTargetAudioSource(0, audioSource);
                player.playOnAwake = true;
            }
            else
            {
                Log = $"Error to load a VideoClip";
            }
        };
    }
}
