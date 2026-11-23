using UnityEngine;

public class TitleSceneController : MonoBehaviour
{
    public void StartGame()
    {
        PopupManager.Instance?.Close();
        SceneManager.Instance.LoadScene("sGame");
    }

    public void OpenSettings()
    {
        PopupManager.Instance?.TryOpenSettings();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
