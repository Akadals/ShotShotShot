using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    private AsyncOperationHandle m_initializationHandle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateBootstrap()
    {
        if (FindFirstObjectByType<GameBootstrap>() != null) return;
        new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        m_initializationHandle = Addressables.InitializeAsync();
        yield return m_initializationHandle;
        CreateGlobalManagers();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateSceneMusic(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
    {
        if (_mode == LoadSceneMode.Single) UpdateSceneMusic(_scene);
    }

    private static void UpdateSceneMusic(Scene _scene)
    {
        if (_scene.name == "sTitle")
            AudioManager.Instance.PlayMusic("TitleMusic");
        else
            AudioManager.Instance.StopMusic();
    }

    private void CreateGlobalManagers()
    {
        CreateManager<AudioManager>("AudioManager");
        CreateManager<SceneManager>("SceneManager");
        CreateManager<PopupManager>("PopupManager");
    }

    private static void CreateManager<T>(string _name) where T : MonoBehaviour
    {
        if (FindFirstObjectByType<T>() == null)
            new GameObject(_name).AddComponent<T>();
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        if (m_initializationHandle.IsValid()) Addressables.Release(m_initializationHandle);
    }
}
