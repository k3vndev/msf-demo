using System;

[Serializable]
public class Settings
{
    public float MusicVolume = 75f; // From 0 to 100
    public float SFXVolume = 75f; // From 0 to 100
    public int Quality = 2; // 0 = Low, 1 = Medium, 2 = High

    // 0 = English, 1 = Spanish
    // Default is set to -1 for auto-detecting
    public int Language = -1;

    public bool Fullscreen = true;
}
