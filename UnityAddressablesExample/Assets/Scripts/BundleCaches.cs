using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class BundleCaches 
{
    private static BundleCaches instance;
    public static BundleCaches Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new BundleCaches();
            }
            return instance;
        }
    }
    private BundleCaches()
    {
        bundles = new List<(string name, string hash)>();
    }

    private List<(string name, string hash)> bundles;
    private static readonly string PrefName = "BundleCaches";


    public void Save()
    {
        var data = JsonConvert.SerializeObject(bundles);
        PlayerPrefs.SetString(PrefName, data);
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(PrefName))
        {
            try
            {
                string data = PlayerPrefs.GetString(PrefName);
                bundles = JsonConvert.DeserializeObject<List<(string name, string hash)>>(data);
            }
            catch
            {
                bundles.Clear();
            }
        }
        else
        {
            bundles.Clear();
        }
    }

    public void Add(IEnumerable<(string name, string hash)> bundlesAdd)
    {
        foreach (var bundle in bundlesAdd)
        {
            if (!bundles.Contains(bundle))
            {
                bundles.Add(bundle);
            }
        }
    }
}
