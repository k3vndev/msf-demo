using System;
using TMPro;
using UnityEngine;

public class TextHandler : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI textComponent;
    public RectTransform RectTransform => textComponent.rectTransform;
    public event Action OnRefresh;

    protected virtual void Awake()
    {
        if (textComponent == null && !TryGetComponent(out textComponent))
        {
            Debug.LogWarning($"TextMeshProUGUI component not found on this GameObject ({name}). Component will be disabled.");
            enabled = false;
            return;
        }
        textComponent.textWrappingMode = TextWrappingModes.NoWrap;
    }

    protected virtual void Start() { }
    protected virtual void Update() { }

    public virtual void Refresh()
    {
        OnRefresh?.Invoke();
    }

    public float GetPreferredWidth()
    {
        textComponent.ForceMeshUpdate();
        return textComponent.preferredWidth;
    }

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
}