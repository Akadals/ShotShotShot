using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "sTitle")
        {
            SceneTransition.EnsureInstance();
            EnsurePopupManager();
            EnsureEventSystem();
            GameObject background = GameObject.Find("Background");
            if (background != null && background.GetComponent<TitleBackgroundMotion>() == null)
                background.AddComponent<TitleBackgroundMotion>();
            GameObject buttons = GameObject.Find("Buttons");
            if (buttons != null && buttons.GetComponent<TitleSceneController>() == null)
                buttons.AddComponent<TitleSceneController>();
        }
        else if (scene.name == "sGame")
        {
            EnsureCamera();
            EnsureEventSystem();
            if (GameManager.Instance != null) return;
            GameObject managerObject = new GameObject("GlobalGameManager");
            GameManager manager = managerObject.AddComponent<GameManager>();
            manager.StartGame();
        }
    }

    private static void EnsurePopupManager()
    {
        if (PopupManager.Instance != null) return;
        GameObject managerObject = new GameObject("GlobalPopupManager");
        managerObject.AddComponent<PopupManager>();
    }

    private static void EnsureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.backgroundColor = new Color(0.015f, 0.025f, 0.08f);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }
}
