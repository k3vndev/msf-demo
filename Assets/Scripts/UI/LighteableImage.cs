using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LighteableImage : MonoBehaviour
{
    public Image Image { get; private set; }
    readonly float ANIM_TIME = 0.125f;

    [field: SerializeField] public Overrides LighteableOverrides { get; private set; }

    [Serializable]
    public class Overrides
    {
        [field: SerializeField] public Color Full { get; private set; } = new(1, 1, 1, 1);
        [field: SerializeField] public Color High { get; private set; } = new(1, 1, 1, 0.45f);
        [field: SerializeField] public Color Low { get; private set; } = new(1, 1, 1, 0.15f);
        [field: SerializeField] public Color None { get; private set; } = new(1, 1, 1, 0);
    }

    protected virtual void Awake()
        => Image = GetComponent<Image>();

    public void BlendFull() => Blend(LighteableOverrides.Full);
    public void BlendHigh() => Blend(LighteableOverrides.High);
    public void BlendLow() => Blend(LighteableOverrides.Low);
    public void BlendNone() => Blend(LighteableOverrides.None);

    public void BlendFull(float time) => Blend(time, LighteableOverrides.Full);
    public void BlendHigh(float time) => Blend(time, LighteableOverrides.High);
    public void BlendLow(float time) => Blend(time, LighteableOverrides.Low);
    public void BlendNone(float time) => Blend(time, LighteableOverrides.None);

    public void Blend(Color target)
        => Blend(ANIM_TIME, target);

    public void Blend(float time, Color target)
    {
        Image.DOKill();
        Image
            .DOColor(target, time)
            .SetEase(Ease.OutSine)
            .SetUpdate(UpdateType.Normal, true);
    }
}