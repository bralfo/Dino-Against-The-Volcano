using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameAudioSettings
{
    private const string MusicVolumeKey = "audio.music.volume";
    private const string EffectsVolumeKey = "audio.effects.volume";
    private const string MusicMutedKey = "audio.music.muted";
    private const string EffectsMutedKey = "audio.effects.muted";

    private static readonly Dictionary<AudioSource, float> BaseVolumes = new();

    public static float MusicVolume { get; private set; } = 1f;
    public static float EffectsVolume { get; private set; } = 1f;
    public static bool MusicMuted { get; private set; }
    public static bool EffectsMuted { get; private set; }
    public static float EffectsOutputLevel => EffectsMuted ? 0f : EffectsVolume;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        BaseVolumes.Clear();
        MusicVolume = 1f;
        EffectsVolume = 1f;
        MusicMuted = false;
        EffectsMuted = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
        EffectsVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsVolumeKey, 1f));
        MusicMuted = PlayerPrefs.GetInt(MusicMutedKey, 0) == 1;
        EffectsMuted = PlayerPrefs.GetInt(EffectsMutedKey, 0) == 1;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.Save();
        ApplyToLoadedSources();
    }

    public static void SetEffectsVolume(float volume)
    {
        EffectsVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(EffectsVolumeKey, EffectsVolume);
        PlayerPrefs.Save();
        ApplyToLoadedSources();
    }

    public static void SetMusicMuted(bool muted)
    {
        MusicMuted = muted;
        PlayerPrefs.SetInt(MusicMutedKey, muted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyToLoadedSources();
    }

    public static void SetEffectsMuted(bool muted)
    {
        EffectsMuted = muted;
        PlayerPrefs.SetInt(EffectsMutedKey, muted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyToLoadedSources();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BaseVolumes.Clear();
        ApplyToLoadedSources();
    }

    private static void ApplyToLoadedSources()
    {
        AudioSource[] sources = UnityEngine.Object.FindObjectsByType<AudioSource>(
            FindObjectsInactive.Include);

        foreach (AudioSource source in sources)
        {
            if (!BaseVolumes.TryGetValue(source, out float baseVolume))
            {
                baseVolume = source.volume;
                BaseVolumes[source] = baseVolume;
            }

            bool isMusic = source.loop
                || source.gameObject.name.Contains("Music", StringComparison.OrdinalIgnoreCase);
            float channelVolume = isMusic
                ? (MusicMuted ? 0f : MusicVolume)
                : (EffectsMuted ? 0f : EffectsVolume);

            source.volume = baseVolume * channelVolume;
        }
    }
}
