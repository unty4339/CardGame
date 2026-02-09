using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Addressablesを管理するシングルトンクラス
/// 他のクラスでAddressablesを直接使わず、このクラスを経由してアセットをロードする
/// </summary>
public class AddressableManager : MonoBehaviour
{
    private static AddressableManager _instance;
    public static AddressableManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AddressableManager");
                _instance = go.AddComponent<AddressableManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// アセットを非同期でロードする
    /// </summary>
    /// <typeparam name="T">ロードするアセットの型</typeparam>
    /// <param name="address">アセットのアドレス</param>
    /// <returns>ロードされたアセット</returns>
    /// <exception cref="AddressableAssetNotFoundException">アセットが見つからない場合</exception>
    public async Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(address))
        {
            string errorMsg = "アセットアドレスが空です";
            Debug.LogError($"[AddressableManager] {errorMsg}");
            throw new ArgumentException(errorMsg, nameof(address));
        }

        try
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            T asset = await handle.Task;

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                string errorMsg = $"アセットのロードに失敗しました: {address}";
                Debug.LogError($"[AddressableManager] {errorMsg}. エラー: {handle.OperationException}");
                Addressables.Release(handle);
                throw new AddressableAssetNotFoundException(address, handle.OperationException?.Message ?? "不明なエラー");
            }

            if (asset == null)
            {
                string errorMsg = $"アセットが見つかりませんでした: {address}";
                Debug.LogError($"[AddressableManager] {errorMsg}");
                Addressables.Release(handle);
                throw new AddressableAssetNotFoundException(address);
            }

            return asset;
        }
        catch (Exception ex)
        {
            if (ex is AddressableAssetNotFoundException)
            {
                throw;
            }

            string errorMsg = $"アセットのロード中にエラーが発生しました: {address}";
            Debug.LogError($"[AddressableManager] {errorMsg}. エラー: {ex.Message}");
            throw new AddressableAssetNotFoundException(address, ex.Message);
        }
    }

    /// <summary>
    /// アセットを同期でロードする
    /// 注意: このメソッドはメインスレッドをブロックする可能性があります
    /// </summary>
    /// <typeparam name="T">ロードするアセットの型</typeparam>
    /// <param name="address">アセットのアドレス</param>
    /// <returns>ロードされたアセット</returns>
    /// <exception cref="AddressableAssetNotFoundException">アセットが見つからない場合</exception>
    public T LoadAsset<T>(string address) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(address))
        {
            string errorMsg = "アセットアドレスが空です";
            Debug.LogError($"[AddressableManager] {errorMsg}");
            throw new ArgumentException(errorMsg, nameof(address));
        }

        AsyncOperationHandle<T> handle = default;
        try
        {
            handle = Addressables.LoadAssetAsync<T>(address);
            T asset = handle.WaitForCompletion();

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                string errorMsg = $"アセットのロードに失敗しました: {address}";
                Debug.LogError($"[AddressableManager] {errorMsg}. エラー: {handle.OperationException}");
                Addressables.Release(handle);
                throw new AddressableAssetNotFoundException(address, handle.OperationException?.Message ?? "不明なエラー");
            }

            if (asset == null)
            {
                string errorMsg = $"アセットが見つかりませんでした: {address}";
                Debug.LogError($"[AddressableManager] {errorMsg}");
                Addressables.Release(handle);
                throw new AddressableAssetNotFoundException(address);
            }

            return asset;
        }
        catch (Exception ex)
        {
            if (ex is AddressableAssetNotFoundException)
            {
                throw;
            }

            // ハンドルが初期化されている場合は解放
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            string errorMsg = $"アセットのロード中にエラーが発生しました: {address}";
            Debug.LogError($"[AddressableManager] {errorMsg}. エラー: {ex.Message}");
            throw new AddressableAssetNotFoundException(address, ex.Message);
        }
    }

    /// <summary>
    /// GameObjectをロードしてインスタンス化する
    /// </summary>
    /// <param name="address">アセットのアドレス</param>
    /// <param name="parent">親Transform（オプション）</param>
    /// <returns>インスタンス化されたGameObject</returns>
    /// <exception cref="AddressableAssetNotFoundException">アセットが見つからない場合</exception>
    public async Task<GameObject> InstantiateAsync(string address, Transform parent = null)
    {
        if (string.IsNullOrEmpty(address))
        {
            string errorMsg = "アセットアドレスが空です";
            Debug.LogError($"[AddressableManager] {errorMsg}");
            throw new ArgumentException(errorMsg, nameof(address));
        }

        try
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, parent);
            GameObject instance = await handle.Task;

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                string errorMsg = $"GameObjectのインスタンス化に失敗しました: {address}";
                Debug.LogError($"[AddressableManager] {errorMsg}. エラー: {handle.OperationException}");
                Addressables.Release(handle);
                throw new AddressableAssetNotFoundException(address, handle.OperationException?.Message ?? "不明なエラー");
            }

            if (instance == null)
            {
                string errorMsg = $"GameObjectが見つかりませんでした: {address}";
                Debug.LogError($"[AddressableManager] {errorMsg}");
                Addressables.Release(handle);
                throw new AddressableAssetNotFoundException(address);
            }

            return instance;
        }
        catch (Exception ex)
        {
            if (ex is AddressableAssetNotFoundException)
            {
                throw;
            }

            string errorMsg = $"GameObjectのインスタンス化中にエラーが発生しました: {address}";
            Debug.LogError($"[AddressableManager] {errorMsg}. エラー: {ex.Message}");
            throw new AddressableAssetNotFoundException(address, ex.Message);
        }
    }

    /// <summary>
    /// GameObjectを同期でロードしてインスタンス化する
    /// 注意: このメソッドはメインスレッドをブロックする可能性があります
    /// </summary>
    /// <param name="address">アセットのアドレス</param>
    /// <param name="parent">親Transform（オプション）</param>
    /// <returns>インスタンス化されたGameObject</returns>
    /// <exception cref="AddressableAssetNotFoundException">アセットが見つからない場合</exception>
    public GameObject Instantiate(string address, Transform parent = null)
    {
        if (string.IsNullOrEmpty(address))
        {
            string errorMsg = "アセットアドレスが空です";
            Debug.LogError($"[AddressableManager] {errorMsg}");
            throw new ArgumentException(errorMsg, nameof(address));
        }

        AsyncOperationHandle<GameObject> handle = default;
        try
        {
            handle = Addressables.InstantiateAsync(address, parent);
            GameObject instance = handle.WaitForCompletion();

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                string errorMsg = $"GameObjectのインスタンス化に失敗しました: {address}";
                Debug.LogError($"[AddressableManager] {errorMsg}. エラー: {handle.OperationException}");
                Addressables.Release(handle);
                throw new AddressableAssetNotFoundException(address, handle.OperationException?.Message ?? "不明なエラー");
            }

            if (instance == null)
            {
                string errorMsg = $"GameObjectが見つかりませんでした: {address}";
                Debug.LogError($"[AddressableManager] {errorMsg}");
                Addressables.Release(handle);
                throw new AddressableAssetNotFoundException(address);
            }

            return instance;
        }
        catch (Exception ex)
        {
            if (ex is AddressableAssetNotFoundException)
            {
                throw;
            }

            // ハンドルが初期化されている場合は解放
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            string errorMsg = $"GameObjectのインスタンス化中にエラーが発生しました: {address}";
            Debug.LogError($"[AddressableManager] {errorMsg}. エラー: {ex.Message}");
            throw new AddressableAssetNotFoundException(address, ex.Message);
        }
    }

    /// <summary>
    /// アセットを解放する
    /// </summary>
    /// <typeparam name="T">アセットの型</typeparam>
    /// <param name="asset">解放するアセット</param>
    public void ReleaseAsset<T>(T asset) where T : UnityEngine.Object
    {
        if (asset == null)
        {
            Debug.LogWarning("[AddressableManager] 解放しようとしたアセットがnullです");
            return;
        }

        try
        {
            Addressables.Release(asset);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AddressableManager] アセットの解放中にエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// GameObjectインスタンスを解放する
    /// </summary>
    /// <param name="instance">解放するGameObjectインスタンス</param>
    public void ReleaseInstance(GameObject instance)
    {
        if (instance == null)
        {
            Debug.LogWarning("[AddressableManager] 解放しようとしたGameObjectインスタンスがnullです");
            return;
        }

        try
        {
            Addressables.ReleaseInstance(instance);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AddressableManager] GameObjectインスタンスの解放中にエラーが発生しました: {ex.Message}");
        }
    }
}
