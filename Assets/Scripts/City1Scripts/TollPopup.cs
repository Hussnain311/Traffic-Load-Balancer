using UnityEngine;
using TMPro;

public class TollPopup : MonoBehaviour
{
    public static TollPopup Instance;

    [Header("UI References")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    [Header("Position Settings")]
    [SerializeField] private Vector3 tollPosition = new Vector3(333f, 170f, 0f);
    [SerializeField] private Vector3 buildingPosition = new Vector3(333f, 170f, 0f);
    [SerializeField] private float fadeDistance = 10f;
    [SerializeField] private float fadeDuration = 1f;

    private LTDescr currentTween;

    private void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup.alpha = 0; // Start hidden
    }

    public void ShowToll(string amount)
    {
        // Immediately cancel any ongoing animation
        LeanTween.cancel(gameObject);

        // Reset and show toll popup
        rectTransform.anchoredPosition = tollPosition;
        text.text = amount;
        canvasGroup.alpha = 1;

        // Start new animation
        currentTween = LeanTween.value(gameObject, tollPosition.y, tollPosition.y + fadeDistance, fadeDuration)
            .setOnUpdate((float y) => {
                rectTransform.anchoredPosition = new Vector2(tollPosition.x, y);
            })
            .setEaseOutCubic()
            .setOnComplete(() => canvasGroup.alpha = 0);
    }

    public void ShowBuildingRevenue(string amount)
    {
        // Immediately cancel any ongoing animation
        LeanTween.cancel(gameObject);

        // Reset and show building popup
        rectTransform.anchoredPosition = buildingPosition;
        text.text = amount;
        canvasGroup.alpha = 1;

        // Start new animation
        currentTween = LeanTween.value(gameObject, buildingPosition.y, buildingPosition.y + fadeDistance, fadeDuration)
            .setOnUpdate((float y) => {
                rectTransform.anchoredPosition = new Vector2(buildingPosition.x, y);
            })
            .setEaseOutCubic()
            .setOnComplete(() => canvasGroup.alpha = 0);
    }

    // Call this to force-hide any visible popup
    public void ForceHide()
    {
        LeanTween.cancel(gameObject);
        canvasGroup.alpha = 0;
    }
}