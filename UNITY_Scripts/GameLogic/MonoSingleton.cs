using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[MonoSingleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        Debug.LogError($"[{typeof(T).Name}] instance not found in the scene. Add it to the scene manually.");
                        return null;
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            Initialize();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected abstract void Initialize();

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    private void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}
