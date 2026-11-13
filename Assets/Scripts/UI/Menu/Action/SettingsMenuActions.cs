using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenuActions : MonoBehaviour
{
    [SerializeField] SliderTarget musicSlider;
    [SerializeField] SliderTarget sfxSlider;
    [SerializeField] SwitchTarget qualitySwitch;
    [SerializeField] SwitchTarget languageSwitch;
    [SerializeField] ToggleTarget fullscreenToggle;

    const string SFX_VOLUME_PARAM = "SFXVolume";
    const string MUSIC_VOLUME_PARAM = "MusicVolume";
    bool settingsWereLoaded = false;

    void Start()
    {
        RefreshValues();
        StartCoroutine(MarkSettingsAsLoaded());
    }

    public void MusicSlider(float value)
    {
        GameManager.I.Global.Settings.MusicVolume = value;
        SetAudioVolume(GameManager.I.StaticConfig.Audio.Groups.Music, MUSIC_VOLUME_PARAM, value);
        HandleSettingsChange();
    }

    public void SFXSlider(float value)
    {
        GameManager.I.Global.Settings.SFXVolume = value;
        SetAudioVolume(GameManager.I.StaticConfig.Audio.Groups.SFX, SFX_VOLUME_PARAM, value);
        HandleSettingsChange();
    }

    // Helper method to convert slider value to decibels and set it on the audio mixer group
    void SetAudioVolume(AudioMixerGroup audioGroup, string volumeParam, float sliderValue)
    {
        float normalized = sliderValue / 100f;
        normalized = Mathf.Max(normalized, 0.0001f);
        float db = Mathf.Log10(normalized) * 20f;
        audioGroup.audioMixer.SetFloat(volumeParam, db);
    }

    public void QualitySwitch(int value)
    {
        GameManager.I.Global.Settings.Quality = value;
        HandleSettingsChange();
    }

    public void LanguageSwitch(int value)
    {
        TranslationSystem.I.ChangeLanguage(value);
        HandleSettingsChange();
    }

    public void FullscreenToggle(bool value)
    {
        ScreenSizing.I.SetFullscreen(value);
        HandleSettingsChange();
    }

    /// <summary>
    /// Saves settings if they were loaded. This prevents saving default values on first load before the user has a chance to change anything.
    /// </summary>
    void HandleSettingsChange()
    {
        if (settingsWereLoaded)
            SaveSystem.I.RequestSave();
    }

    public void OkayButtonMain()
        => MainMenuManager.I.FromSettingsToMain();

    public void OkayButtonPause()
        => PauseMenuManager.I.FromSettingsToPause();

    /// <summary>Forces all settings to reload values from global state.</summary>
    public void RefreshValues()
    {
        musicSlider.SetValue(GameManager.I.Global.Settings.MusicVolume);
        sfxSlider.SetValue(GameManager.I.Global.Settings.SFXVolume);
        qualitySwitch.SetValue(GameManager.I.Global.Settings.Quality);
        languageSwitch.SetValue(GameManager.I.Global.Settings.Language);
        fullscreenToggle.SetValue(GameManager.I.Global.Settings.Fullscreen);
    }

    IEnumerator MarkSettingsAsLoaded()
    {
        yield return null;
        settingsWereLoaded = true;
    }
}
