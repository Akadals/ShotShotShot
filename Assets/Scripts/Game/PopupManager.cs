using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PopupManager : Singleton<PopupManager>
{
    private AsyncOperationHandle<GameObject> m_popupHandle;
    private SettingsPopup m_settingsPopup;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this) StartCoroutine(LoadSettingsPopup());
    }

    public bool TryOpenSettings(Action _onResume = null, Action _onTitle = null)
    {
        if (m_settingsPopup == null) return false;
        m_settingsPopup.Show(_onResume, _onTitle);
        return true;
    }

    public void Close()
    {
        if (m_settingsPopup != null) m_settingsPopup.Close();
    }

    private IEnumerator LoadSettingsPopup()
    {
        m_popupHandle = Addressables.LoadAssetAsync<GameObject>("SettingsPopup");
        yield return m_popupHandle;
        if (m_popupHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("SettingsPopup address was not found in Addressables.");
            yield break;
        }

        GameObject popup = Instantiate(m_popupHandle.Result, transform);
        m_settingsPopup = popup.GetComponent<SettingsPopup>();
        if (m_settingsPopup == null)
        {
            Destroy(popup);
            Debug.LogError("SettingsPopup prefab is missing its SettingsPopup component.");
        }
    }

    protected override void OnDestroy()
    {
        bool isOwner = Instance == this;
        base.OnDestroy();
        if (isOwner && m_popupHandle.IsValid()) Addressables.Release(m_popupHandle);
    }
}
