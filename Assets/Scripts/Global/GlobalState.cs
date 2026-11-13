using System;

[Serializable]
public class GlobalState
{
    public string GameVersion = SaveSystem.GAME_VERSION;
    public int CurrentLevel = 1;
    public Settings Settings = new();

    public ControlsTooltip.TooltipName LastUsedTooltip = ControlsTooltip.TooltipName.None;

    public int WindowedWidth = -1; // Initially, player hasn't set a windowed width yet.
    public int WindowedHeight = -1; // Initially, player hasn't set a windowed height yet.
}
