using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class JumpTargetIndicator : MonoBehaviour
{
    [Header("Indicators GameObjects")]
    [SerializeField] GameObject smallIndicator;
    [SerializeField] GameObject bigIndicator;
    GameObject activeIndicator = null;

    [Header("Tween Configs")]
    [SerializeField] TweenConfig appearConfig;
    [SerializeField] TweenConfig disappearConfig;

    JumpPad jumpPad;
    ParticleSystem particles;


    void Awake()
    {
        // Find the JumpPad component in the parent hierarchy
        jumpPad = GetComponentInParent<JumpPad>();
        if (jumpPad == null)
        {
            Debug.LogError("XMarkIndicator: No JumpPad found in parent hierarchy.");
            gameObject.SetActive(false);
            return;
        }

        // Start with the indicators hidden and disabled
        GameObject[] indicators = { smallIndicator, bigIndicator };
        foreach (GameObject indicator in indicators)
        {
            Vector3 initialScale = Vector3.zero;
            initialScale.y = 1;
            indicator.transform.localScale = initialScale;
            indicator.SetActive(false);
        }

        particles = GetComponent<ParticleSystem>();
    }

    void OnPushEntityStart(JumpPad.JumpController jump)
    {
        // Determine which indicator to use. 
        // Use the small by default and switch to the big if there's a pressable element at the jump target position

        activeIndicator = smallIndicator;
        Collider[] colliders = Utils.GetCollidersAt(jump.Target, -1, 0.5f);

        foreach (Collider collider in colliders)
        {
            if (
                collider.gameObject.GetComponent<IPlayerPressable>() != null
                || collider.gameObject.GetComponent<ISledPressable>() != null
            )
            {
                activeIndicator = bigIndicator;
                break;
            }
        }

        // Move the indicator to the jump target position and start the appear tween
        transform.position = jump.Target;
        activeIndicator.SetActive(true);
        particles.Play();
        TweenScale(activeIndicator, appearConfig);
    }

    void OnEntityHitGround(JumpPad.JumpController _)
    {
        if (activeIndicator)
            TweenScale(activeIndicator, disappearConfig);
    }

    void TweenScale(GameObject indicator, TweenConfig config)
    {
        DOTween.Kill(indicator.transform);

        Vector3 targetScale = config.TargetScale * Vector3.one;
        targetScale.y = 1;

        indicator.transform
            .DOScale(targetScale, config.Duration)
            .SetEase(config.Easing)
            .OnComplete(() =>
            {
                if (config.TargetScale == 0)
                    indicator.SetActive(false);
            });
    }

    void OnEnable()
    {
        jumpPad.PushEntityStarted += OnPushEntityStart;
        jumpPad.EntityHitGround += OnEntityHitGround;
    }

    void OnDisable()
    {
        jumpPad.PushEntityStarted -= OnPushEntityStart;
        jumpPad.EntityHitGround -= OnEntityHitGround;
    }

    [Serializable]
    class TweenConfig
    {
        [SerializeField] private float targetScale;
        [SerializeField, Range(0f, 1f)] private float duration;
        [SerializeField] private Ease easing;

        public float TargetScale => targetScale;
        public float Duration => duration;
        public Ease Easing => easing;
    }
}