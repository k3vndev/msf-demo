
public class PlayButtonLevelSetter : TranslatedText
{
    protected override void Awake()
    {
        base.Awake();
        ParamGetter = () => GameManager.I.Global.CurrentLevel.ToString();
    }
}
