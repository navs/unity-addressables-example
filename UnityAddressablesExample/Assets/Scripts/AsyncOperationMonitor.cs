using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AsyncOperationMonitor : MonoBehaviour
{
    public delegate void Callback(bool finished, float percent);

    private bool running = false;
    private Callback callback;
    private AsyncOperationHandle handle;
    private bool selfDestroy;

    private void Update()
    {
        if (running)
        {
            if (handle.IsDone)
            {
                callback(true, 1.0f);
                running = false;

                if (selfDestroy)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                callback(false, handle.PercentComplete);
            }
        }
    }

    public void Begin(AsyncOperationHandle handle, Callback callback, bool selfDestroy = true)
    {
        this.handle = handle;
        this.callback = callback;
        this.selfDestroy = selfDestroy;
        running = true;
    }

    public static AsyncOperationMonitor Create(AsyncOperationHandle handle, Callback callback, bool selfDestroy = true)
    {
        GameObject go = new GameObject("AsyncOperationMonitor");
        AsyncOperationMonitor monitor = go.AddComponent<AsyncOperationMonitor>();
        monitor.Begin(handle, callback, selfDestroy);
        return monitor;
    }
}
