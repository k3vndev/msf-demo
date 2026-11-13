using UnityEngine;

public class ItemWithTextCenterer : MonoBehaviour
{
    readonly float spacing = 21f;
    [SerializeField] RectTransform pictureTransform;
    [SerializeField] TextHandler textHandler;
    RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void HandleTextHandlerRefresh()
    {
        float center = rectTransform.anchoredPosition.x;
        float textWidth = textHandler.GetPreferredWidth();
        float pictureWidth = pictureTransform.rect.width;
        float totalWidth = textWidth + pictureWidth + spacing;

        float textOffset = (totalWidth / 2f) - (textWidth / 2f);
        float pictureOffset = (totalWidth / 2f) - (pictureWidth / 2f);

        textHandler.RectTransform.anchoredPosition = new Vector2(center + textOffset, textHandler.RectTransform.anchoredPosition.y);
        pictureTransform.anchoredPosition = new Vector2(center - pictureOffset, pictureTransform.anchoredPosition.y);
    }

    void OnEnable()
    {
        textHandler.OnRefresh += HandleTextHandlerRefresh;
    }

    void OnDisable()
    {
        textHandler.OnRefresh -= HandleTextHandlerRefresh;
    }
}