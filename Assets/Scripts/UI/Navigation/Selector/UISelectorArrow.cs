using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UISelectorArrow : MonoBehaviour
{
    enum Direction { Left, Right }
    [SerializeField] Direction direction;

    [Header("Motion")]
    [SerializeField] Vector2 timeInOut = new(0.15f, 0.32f);
    [SerializeField] float moveAmount = 15f;

    RectTransform rect;
    Sequence sequence;
    Image image;

    Vector2 startPosition;
    Vector2 targetIn;
    Vector2 targetOut;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        float moveX = (((int)direction * 2f) - 1) * moveAmount;

        startPosition = rect.anchoredPosition;
        targetIn = new(startPosition.x + moveX, startPosition.y);
        targetOut = new(startPosition.x - moveX, startPosition.y);
    }

    public void StartAnimation()
    {
        rect.anchoredPosition = startPosition;
        Color selectedColor = GameManager.I.StaticConfig.UIColors.Selected;
        Color selectedLighterColor = GameManager.I.StaticConfig.UIColors.SelectedLighter;

        image.color = GameManager.I.StaticConfig.UIColors.Selected;

        float halfInTime = timeInOut.x / 2f;

        sequence.Kill();
        sequence = DOTween.Sequence();

        sequence.Append(rect.DOAnchorPos(targetIn, halfInTime).SetEase(Ease.OutCubic));
        sequence.Join(image.DOColor(selectedLighterColor, halfInTime).SetEase(Ease.OutCubic));

        sequence.Append(rect.DOAnchorPos(targetOut, timeInOut.y).SetEase(Ease.InOutSine));
        sequence.Join(image.DOColor(selectedColor, timeInOut.y).SetEase(Ease.InOutSine));

        sequence.Append(rect.DOAnchorPos(startPosition, halfInTime).SetEase(Ease.InCubic));
        sequence.Join(image.DOColor(selectedLighterColor, halfInTime).SetEase(Ease.InCubic));

        sequence
            .SetUpdate(UpdateType.Normal, true)
            .SetLoops(-1, LoopType.Restart);
    }

    public void StopAnimation()
    {
        sequence.Kill();
    }

    void OnValidate()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        euler.z = direction == Direction.Left ? 180 : 0;
        transform.rotation = Quaternion.Euler(euler);

        if (GameManager.GetInstance() is GameManager gameManager)
            GetComponent<Image>().color = gameManager.StaticConfig.UIColors.Selected;
    }
}