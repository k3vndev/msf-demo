using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen I { get; private set; }

    [field: SerializeField] public Vector2 FadeDurationInOut { get; private set; } = new(0.4f, 0.7f);
    [field: SerializeField, Range(0f, 1f)] public float IconVisibleAlpha { get; private set; } = 0.75f;
    public bool IsLoading { get; private set; } = false;

    [Space]
    [SerializeField] CanvasGroup iconGroup;
    [SerializeField] Canvas canvas;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;

        // Hide the icon group by default.
        iconGroup.alpha = 0f;
        iconGroup.gameObject.SetActive(false);
    }

    /// <summary>Shows the loading icon.</summary>
    public void StartLoading()
    {
        if (IsLoading) return;

        IsLoading = true;
        iconGroup.gameObject.SetActive(true);
        FadeIconGroup(IconVisibleAlpha, FadeDurationInOut.x);
    }

    /// <summary>Hides the loading icon.</summary>
    public void StopLoading()
    {
        if (!IsLoading) return;

        IsLoading = false;
        FadeIconGroup(0f, FadeDurationInOut.y, () =>
            iconGroup.gameObject.SetActive(false));
    }

    /// <summary>Fades the icon group to a target alpha.</summary>
    void FadeIconGroup(float targetAlpha, float duration, Action onComplete = null)
    {
        iconGroup.DOKill();
        iconGroup
            .DOFade(targetAlpha, duration)
            .SetEase(Ease.InQuad)
            .SetUpdate(UpdateType.Normal, true)
            .OnComplete(() => onComplete?.Invoke());
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
            MainCamera mainCamera = Utils.GetComp<MainCamera>("MainCamera");
            mainCamera.AddOverlayCamera(canvas.worldCamera, MainCamera.OverlayCameraPriority.LoadingIcon);
        }
        catch
        {
            Debug.LogWarning($"LoadingScreen: Tried to add loading icon overlay camera to the main camera stack,"
                + $" but it was not found at {SceneManager.GetActiveScene().name}.");
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
}
