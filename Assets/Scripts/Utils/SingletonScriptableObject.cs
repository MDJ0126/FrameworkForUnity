using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// ADDRESS에 지정한 Addressables 에셋을 최초 접근 시 동기 로드하여 공유한다.
/// </summary>
public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
{
    // 에셋 로드 전에 임시 인스턴스에서도 읽을 수 있도록 고정 주소를 반환한다.
    public virtual string ADDRESS => "";

    private static T _instance;
    private static AsyncOperationHandle<T> _handle;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                T addressSource = CreateInstance<T>();
                try
                {
                    _instance = addressSource.Load();
                }
                finally
                {
                    if (Application.isPlaying)
                    {
                        Destroy(addressSource);
                    }
                    else
                    {
                        DestroyImmediate(addressSource);
                    }
                }
            }
            return _instance;
        }
    }

    public static bool IsLive => _instance != null;

    /// <summary>
    /// ADDRESS의 에셋을 동기 로드하며 실패하면 핸들을 해제하고 null을 반환한다.
    /// </summary>
    protected T Load()
    {
        // 이전 에셋이 제거된 경우 남아 있는 로드 핸들을 먼저 해제한다.
        Release();
        try
        {
            string address = ADDRESS;
            if (string.IsNullOrWhiteSpace(address))
            {
                Debug.LogError($"{typeof(T).Name}의 ADDRESS가 비어 있습니다.");
                return null;
            }

            _handle = Addressables.LoadAssetAsync<T>(address);
            T loadedAsset = _handle.WaitForCompletion();
            if (_handle.Status == AsyncOperationStatus.Succeeded && loadedAsset != null)
            {
                return loadedAsset;
            }
            Debug.LogError($"Addressables 에셋 로드에 실패했습니다: {address}");
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
        Release();
        return null;
    }

    /// <summary>
    /// 공유 인스턴스 참조와 Addressables 로드 핸들을 해제한다.
    /// </summary>
    public static void Release()
    {
        _instance = null;
        AsyncOperationHandle<T> handle = _handle;
        _handle = default;
        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }
    }

    /// <summary>
    /// 캐싱한 에셋이 제거되면 해당 인스턴스의 참조를 해제한다.
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
