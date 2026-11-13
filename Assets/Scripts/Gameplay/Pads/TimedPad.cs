using UnityEngine;

public abstract class TimedPad : Pad
{
    [field: SerializeField] protected float StopPlayerTime { get; private set; } = 0.3f;
    protected enum SteppingEntiy { Player, Sled, None }
    protected SteppingEntiy SteppingEntity { get; private set; } = SteppingEntiy.None;

    public override void PlayerPress()
    {
        base.PlayerPress();
        SteppingEntity = SteppingEntiy.Player;
    }

    public override void SledPress()
    {
        base.SledPress();
        SteppingEntity = SteppingEntiy.Sled;

        // Refresh temperature animations on the cat when the sled steps on the pad
        CatAnimationsGameplay catAnimations = GameObject
            .FindWithTag("Sled")?
            .GetComponentInChildren<CatAnimationsGameplay>();
        catAnimations?.RefreshTemperatureAnimations();
    }

    public override void PlayerRelease()
    {
        base.PlayerRelease();

        if (SteppingEntity == SteppingEntiy.Player)
            SteppingEntity = SteppingEntiy.None;
    }

    public override void SledRelease()
    {
        base.SledRelease();

        if (SteppingEntity == SteppingEntiy.Sled)
            SteppingEntity = SteppingEntiy.None;
    }

    protected override void SetPadIsActive(bool value)
    {
        base.SetPadIsActive(value);

        if (value)
        {
            if (SteppingEntity == SteppingEntiy.Player) PlayerPress();
            else if (SteppingEntity == SteppingEntiy.Sled) SledPress();
        }
    }
}
