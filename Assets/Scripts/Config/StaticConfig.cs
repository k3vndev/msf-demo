using System;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "StaticConfig", menuName = "Scriptable Objects/StaticConfig")]
public class StaticConfig : ScriptableObject
{
    [field: SerializeField] public UIColors_ UIColors { get; private set; }
    [field: Space]
    [field: SerializeField] public Audio_ Audio { get; private set; }
    [field: Space]
    [field: SerializeField] public Materials_ Materials { get; private set; }

    [Serializable]
    public class UIColors_
    {
        [field: SerializeField] public Color Selected { get; private set; }
        [field: SerializeField] public Color SelectedLighter { get; private set; }
        [field: Space]
        [field: SerializeField] public Color Default { get; private set; }
        [field: SerializeField] public Color DefaultLighter { get; private set; }
        [field: SerializeField] public Color DefaultLightest { get; private set; }
    }

    [Serializable]
    public class Audio_
    {
        [field: SerializeField] public Groups_ Groups { get; private set; }
        [field: SerializeField] public Clips_ Clips { get; private set; }

        [Serializable]
        public class Groups_
        {
            [field: SerializeField] public AudioMixerGroup Music { get; private set; }
            [field: SerializeField] public AudioMixerGroup SFX { get; private set; }
        }

        [Serializable]
        public class Clips_
        {
            [field: SerializeField] public AudioClip FallOnSnow { get; private set; }
            [field: SerializeField] public AudioClip SlideOnIce { get; private set; }
        }
    }

    [Serializable]
    public class Materials_
    {
        [field: SerializeField] public Material Cat { get; private set; }
    }
}
