using UnityEngine;

public class UISelector : MonoBehaviour
{
    CanvasGroup canvasGroup;
    NavigationTarget parentTarget;
    [SerializeField] UISelectorArrow[] arrows;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        parentTarget = GetComponentInParent<NavigationTarget>();

        transform.position = parentTarget.transform.position;
    }

    void HandleParentSelection(bool isSelected)
    {
        canvasGroup.alpha = isSelected ? 1 : 0;

        // Start/Stop arrows animation on select/unselect
        foreach (UISelectorArrow arrow in arrows)
        {
            if (isSelected) arrow.StartAnimation();
            else arrow.StopAnimation();
        }
    }

    void OnEnable() => parentTarget.OnVisualsChanged += HandleParentSelection;
    void OnDisable() => parentTarget.OnVisualsChanged -= HandleParentSelection;
}
