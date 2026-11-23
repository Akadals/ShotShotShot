using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum VolumeChannel
{
    MASTER,
    BGM,
    VFX,
    UI
}

public class AudioManager : Singleton<AudioManager>
{
    private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> m_clips = new();
    private AsyncOperationHandle<AudioMixer> m_mixerHandle;
    private AudioMixer m_mixer;
    private AudioSource m_uiSource;
    private AudioSource m_effectSource;
    private AudioSource m_musicSource;
    private Coroutine m_musicPlayback;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        m_uiSource = CreateSource();
        m_effectSource = CreateSource();
        m_musicSource = CreateSource();
        m_musicSource.loop = true;
        StartCoroutine(LoadMixer());
    }

    public void PlayUi(string _address)
    {
        StartCoroutine(PlayClip(_address, m_uiSource, 1f));
    }

    public void PlayEffect(string _address, float _volume = 1f)
    {
        StartCoroutine(PlayClip(_address, m_effectSource, _volume));
    }

    public void PlayMusic(string _address)
    {
        StopMusic();
        m_musicPlayback = StartCoroutine(PlayClip(_address, m_musicSource, 1f));
    }

    public void StopMusic()
    {
        if (m_musicPlayback != null)
        {
            StopCoroutine(m_musicPlayback);
            m_musicPlayback = null;
        }

        m_musicSource.Stop();
        m_musicSource.clip = null;
    }

    public float GetVolume(VolumeChannel _channel)
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat("Audio_" + GetMixerParameter(_channel), 1f));
    }

    public void SetVolume(VolumeChannel _channel, float _volume)
    {
        float volume = Mathf.Clamp01(_volume);
        PlayerPrefs.SetFloat("Audio_" + GetMixerParameter(_channel), volume);
        ApplyVolume(_channel, volume);
    }

    public void SaveVolumeSettings()
    {
        PlayerPrefs.Save();
    }

    private AudioSource CreateSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }

    private IEnumerator LoadMixer()
    {
        m_mixerHandle = Addressables.LoadAssetAsync<AudioMixer>("Mixer");
        yield return m_mixerHandle;
        if (m_mixerHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("AudioMixer address was not found. Check the Addressables address 'Mixer'.");
            yield break;
        }

        m_mixer = m_mixerHandle.Result;
        SetOutputGroup(m_uiSource, "UI");
        SetOutputGroup(m_effectSource, "VFX");
        SetOutputGroup(m_musicSource, "BGM");
        foreach (VolumeChannel channel in System.Enum.GetValues(typeof(VolumeChannel)))
            ApplyVolume(channel, GetVolume(channel));
    }

    private IEnumerator PlayClip(string _address, AudioSource _source, float _volume)
    {
        if (!m_clips.TryGetValue(_address, out AsyncOperationHandle<AudioClip> handle))
        {
            handle = Addressables.LoadAssetAsync<AudioClip>(_address);
            m_clips.Add(_address, handle);
        }

        yield return handle;
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("AudioClip address was not found: " + _address);
            yield break;
        }

        if (_source == m_musicSource)
        {
            _source.clip = handle.Result;
            _source.Play();
        }
        else
        {
            _source.PlayOneShot(handle.Result, _volume);
        }
    }

    private void ApplyVolume(VolumeChannel _channel, float _volume)
    {
        if (m_mixer == null) return;
        float decibels = _volume <= 0.001f ? -80f : Mathf.Lerp(-40f, 0f, _volume);
        m_mixer.SetFloat(GetMixerParameter(_channel), decibels);
    }

    private static string GetMixerParameter(VolumeChannel _channel)
    {
        return _channel == VolumeChannel.MASTER ? "Master" : _channel.ToString();
    }

    private void SetOutputGroup(AudioSource _source, string _groupName)
    {
        AudioMixerGroup[] groups = m_mixer.FindMatchingGroups(_groupName);
        if (groups.Length > 0) _source.outputAudioMixerGroup = groups[0];
    }

    protected override void OnDestroy()
    {
        bool isOwner = Instance == this;
        base.OnDestroy();
        if (!isOwner) return;
        if (m_mixerHandle.IsValid()) Addressables.Release(m_mixerHandle);
        foreach (AsyncOperationHandle<AudioClip> handle in m_clips.Values)
            if (handle.IsValid()) Addressables.Release(handle);
    }
}
