using System.Collections;
using UnityEngine;

public abstract class MenuManager : MonoBehaviour
{
    public bool IsOnSettingsMenu { get; protected set; } = false;

    [SerializeField, Range(0, 1)] float showMenuWait;
    [SerializeField, Range(0, 1)] float enableNavigationWait;


    protected virtual void OnNavigationCancel() { }

    protected IEnumerator SwapMenu(ElementAnimator hideElement,
        ElementAnimator showElement, NavigationTarget selectTarget)
    {
        NavigationSystem.I.DisableNavigation();
        hideElement.Hide();

        yield return new WaitForSecondsRealtime(showMenuWait);

        showElement.HideInstant();
        showElement.Show();

        NavigationSystem.I.Initialize(showElement.gameObject);
        if (NavigationSystem.I.IsUsingPointer) NavigationSystem.I.StopNavigating();
        NavigationSystem.I.Select(selectTarget);

        yield return new WaitForSecondsRealtime(enableNavigationWait);
        NavigationSystem.I.EnableNavigation();
    }

    protected virtual void OnEnable()
        => NavigationSystem.OnCancel += OnNavigationCancel;

    protected virtual void OnDisable()
        => NavigationSystem.OnCancel -= OnNavigationCancel;
}