using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SwitchTarget : ChangeableNavigationTarget
{
    [Header("Switch")]
    [field: SerializeField] public Option[] Options { get; private set; }
    [SerializeField] UnityEvent<int> onValueChanged;
    [SerializeField] TranslatedText translatedTextLabel;

    [Serializable]
    public class Option
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public SwitchLine SwitchLine { get; private set; }
    }

    [field: SerializeField] public int SelectedIndex { get; private set; } = 0;

    protected override void Start()
    {
        base.Start();
        RefreshValueDisplay();
    }

    public override void ChangeValue(int direction, float _)
    {
        base.ChangeValue(direction, _);

        int maxIndex = Options.Length - 1;
        int newSelected = SelectedIndex + direction;

        int newValue = newSelected > maxIndex
            ? 0
            : newSelected < 0
                ? maxIndex
                : newSelected;

        SetValue(newValue);
    }

    public void SetValue(int value)
    {
        int clampedValue = Mathf.Clamp(value, 0, Options.Length - 1);
        SelectedIndex = clampedValue;
        onValueChanged?.Invoke(SelectedIndex);
        RefreshValueDisplay();
    }

    public override void RefreshValueDisplay()
    {
        base.RefreshValueDisplay();
        translatedTextLabel.RefreshId(Options[SelectedIndex].Id);
        RefreshOptionsDisplay();
    }

    protected override void OnChangeState()
    {
        base.OnChangeState();
        RefreshOptionsDisplay();
    }

    public void RefreshOptionsDisplay()
    {
        for (int i = 0; i < Options.Length; i++)
        {
            SwitchLine swLine = Options[i].SwitchLine;

            if (i == SelectedIndex) swLine.Choose();
            else swLine.UnChoose();
        }
    }
}