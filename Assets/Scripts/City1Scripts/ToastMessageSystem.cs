using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ToastMessageSystem : MonoBehaviour
{
    [Header("Toast Settings")]
    public float displayDuration = 2.0f;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.5f;

    [Header("Toast UI Components")]
    public GameObject toastPanel;
    public TextMeshProUGUI toastText;
    public Image backgroundImage;

    [Header("Toast Types")]
    public Color successColor = new Color(0.2f, 0.7f, 0.2f, 0.8f);
    public Color errorColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
    public Color infoColor = new Color(0.2f, 0.2f, 0.8f, 0.8f);
    public Color warningColor = new Color(0.9f, 0.6f, 0.1f, 0.8f);

    private Queue<ToastItem> toastQueue = new Queue<ToastItem>();
    private bool isDisplayingToast = false;
    private CanvasGroup canvasGroup;

    public static ToastMessageSystem current;

    private void Awake()
    {
        current = this;

        // Initialize CanvasGroup
        if (toastPanel != null)
        {
            canvasGroup = toastPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = toastPanel.AddComponent<CanvasGroup>();
            }
            toastPanel.SetActive(false);
        }
    }

    public static void ShowToast(string message, ToastType type = ToastType.INFO)
    {
        if (current != null)
        {
            current.AddToastToQueue(message, type);
        }
        else
        {
            Debug.LogWarning("No ToastMessageSystem found in the scene!");
        }
    }

    private void AddToastToQueue(string message, ToastType type)
    {
        toastQueue.Enqueue(new ToastItem(message, type));
        if (!isDisplayingToast)
        {
            StartCoroutine(DisplayNextToast());
        }
    }

    private IEnumerator DisplayNextToast()
    {
        if (toastQueue.Count == 0)
        {
            isDisplayingToast = false;
            yield break;
        }

        isDisplayingToast = true;
        ToastItem toast = toastQueue.Dequeue();

        // Set toast properties
        toastText.text = toast.Message;
        backgroundImage.color = GetToastColor(toast.Type);

        // Activate the toast (disable raycast blocking)
        toastPanel.SetActive(true);
        canvasGroup.blocksRaycasts = false; // Allow touches to pass through

        // Fade in
        canvasGroup.alpha = 0;
        LeanTween.alphaCanvas(canvasGroup, 1f, fadeInDuration).setEase(LeanTweenType.easeOutQuad);

        yield return new WaitForSeconds(displayDuration);

        // Fade out
        LeanTween.alphaCanvas(canvasGroup, 0f, fadeOutDuration)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() =>
            {
                toastPanel.SetActive(false);
                canvasGroup.blocksRaycasts = true; // Re-enable if needed
                StartCoroutine(DisplayNextToast());
            });
    }

    private Color GetToastColor(ToastType type)
    {
        switch (type)
        {
            case ToastType.SUCCESS: return successColor;
            case ToastType.ERROR: return errorColor;
            case ToastType.WARNING: return warningColor;
            default: return infoColor;
        }
    }

    public enum ToastType { SUCCESS, ERROR, INFO, WARNING }

    private class ToastItem
    {
        public string Message { get; }
        public ToastType Type { get; }

        public ToastItem(string message, ToastType type)
        {
            Message = message;
            Type = type;
        }
    }
}