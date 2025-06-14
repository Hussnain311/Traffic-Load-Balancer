using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Linq;

public class UIAnimator : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject[] buttons;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Road & Crossing References")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private GameObject roadButton;
    [SerializeField] private GameObject lockedCrossing;
    [SerializeField] private GameObject crossing;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private BuildingUpgradeManager buildingUpgradeManager;
    [SerializeField] private GameSceneSoundManager gameSceneSoundManager;
    [SerializeField] private GameObject[] postCrossingPopups; // Added missing reference

    [Header("Animation Settings")]
    [SerializeField] private float offScreenDistance = 500f;
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private Vector3 dialogueTargetScale = new Vector3(1f, 1f, 1f);

    [Header("Crossing Settings")]
    [SerializeField] private int crossingCost = 1000;
    [SerializeField] private GameObject[] objectsToEnableAfterCrossing;
    [SerializeField] private GameObject crossingDialoguePanel;
    [SerializeField] public  GameObject crossingImage;
    [SerializeField] private TextMeshProUGUI crossingDetailsText;
    [SerializeField] public GameObject crossingConfirmButton;
    [SerializeField] private GameObject crossingCloseButton;

    [Header("Dialogue Settings")]
    [SerializeField] private Button nextButton;


    // Add this field to track dialogue state
    private DialogueSequence currentSequence;
    private int currentLine;
    private bool waitingForInput;


    [System.Serializable]
    public class DialogueSequence
    {
        public string[] lines;
        public Action onComplete;
    }

    [System.Serializable]
    public class PopupSequence
    {
        public GameObject[] popups;
        public float fadeDuration = 0.3f;
        public Action onComplete;
    }

    // State
    private Vector2[] startPositions;
    
    private const string RoadUnlockedKey = "RoadUnlocked";
    private const string CrossingUnlockedKey = "CrossingUnlocked";

    public event Action OnUIAnimationComplete;

    void Start()
    {
            InitializeUI();
        StartCoroutine(AnimateUIStart());
       
    }

    void Update()
    {
        HandleInput();
    }

    #region Initialization
    private void InitializeUI()
    {
        // Validate references
        if (buttons == null || dialogueBox == null || dialogueText == null)
        {
            Debug.LogError("Essential UI references missing!");
            enabled = false;
            return;
        }

        nextButton.onClick.AddListener(AdvanceDialogue);
        // Initialize button positions
        startPositions = new Vector2[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            RectTransform rect = buttons[i].GetComponent<RectTransform>();
            startPositions[i] = rect.anchoredPosition;
            rect.anchoredPosition += new Vector2(
                rect.anchoredPosition.x < 0 ? -offScreenDistance : offScreenDistance,
                0
            );
        }

        // Set initial states
        dialogueBox.SetActive(false);
        crossingDialoguePanel.SetActive(false);
        roadPrefab.SetActive(PlayerPrefs.GetInt(RoadUnlockedKey, 0) == 1);
        roadButton.SetActive(false);

        bool crossingUnlocked = PlayerPrefs.GetInt(CrossingUnlockedKey, 0) == 1;
        lockedCrossing.SetActive(!crossingUnlocked);
        crossing.SetActive(crossingUnlocked);
        if (crossingUnlocked) EnableObjectsAfterCrossing();
    }

    private IEnumerator AnimateUIStart()
    {
        // Animate buttons in
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            LeanTween.move(buttons[i].GetComponent<RectTransform>(), startPositions[i], animationDuration)
                     .setEase(LeanTweenType.easeOutBack);
        }

        OnUIAnimationComplete?.Invoke();

        // Show initial dialogue if road isn't unlocked
        if (PlayerPrefs.GetInt(RoadUnlockedKey, 0) == 0)
        {
            ShowDialogueSequence(new DialogueSequence()
            {
                lines = new[]
                {
                    "Hey Chief!",
                    "Weather looks great today.",
                    "Let's build something."
                },
                onComplete = () => roadButton.SetActive(true)
            });
        }

        yield return null; // Ensure the method returns an IEnumerator
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
            {
                if (hit.collider.gameObject == lockedCrossing && lockedCrossing.activeSelf)
                {
                    ShowCrossingDialogue();
                }
            }
        }
    }
    #endregion

    #region Dialogue System
    #region Dialogue System
    private bool isRunningDialogue = false; // Add this flag

    public void ShowDialogueSequence(DialogueSequence sequence)
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Length == 0)
        {
            sequence?.onComplete?.Invoke();
            return;
        }

        gameSceneSoundManager.PlayDialogueSound();

        // Stop any existing dialogue
        if (isRunningDialogue)
        {
            StopAllCoroutines();
            CompleteDialogue();
        }

        dialogueBox.SetActive(true);
        LeanTween.scale(dialogueBox, dialogueTargetScale, animationDuration)
                 .setFrom(Vector3.zero)
                 .setEase(LeanTweenType.easeOutBack);

        StartCoroutine(RunDialogueSequence(sequence));
    }

    private IEnumerator RunDialogueSequence(DialogueSequence sequence)
    {
        isRunningDialogue = true;
        currentSequence = sequence;
        currentLine = 0;
        waitingForInput = true;
        nextButton.gameObject.SetActive(true);

        // Show first line
        dialogueText.text = sequence.lines[currentLine];

        // Wait for first Next click
        yield return new WaitUntil(() => !waitingForInput);

        // Progress through remaining lines
        while (currentLine < sequence.lines.Length - 1)
        {
            currentLine++;
            dialogueText.text = sequence.lines[currentLine];
            waitingForInput = true;
            yield return new WaitUntil(() => !waitingForInput);
        }

        CompleteDialogue();
        isRunningDialogue = false;
    }

    private void AdvanceDialogue()
    {
        if (!waitingForInput) return;

        // Reset immediately to prevent multiple triggers
        waitingForInput = false;

        // Visual feedback
        LeanTween.scale(nextButton.gameObject, Vector3.one * 0.9f, 0.1f)
                 .setEase(LeanTweenType.easeOutQuad)
                 .setLoopPingPong(1);
    }

    private void CompleteDialogue()
    {
        LeanTween.scale(dialogueBox, Vector3.zero, 0.2f)
                 .setEase(LeanTweenType.easeInBack)
                 .setOnComplete(() =>
                 {
                     dialogueBox.SetActive(false);
                     nextButton.gameObject.SetActive(false);
                     currentSequence?.onComplete?.Invoke();
                 });
    }
    #endregion
    #endregion

    #region Popup System
    public void RunPopupSequence(PopupSequence sequence)
    {
        if (sequence == null || sequence.popups == null)
        {
            Debug.LogWarning("Invalid popup sequence!");
            return;
        }

        StartCoroutine(ExecutePopupSequence(sequence));
    }

    private IEnumerator ExecutePopupSequence(PopupSequence sequence)
    {
        // Initialize
        foreach (var popup in sequence.popups.Where(p => p != null))
        {
            var cg = popup.GetComponent<CanvasGroup>() ?? popup.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            popup.SetActive(false);
        }

        // Sequence
        for (int i = 0; i < sequence.popups.Length; i++)
        {
            if (sequence.popups[i] == null) continue;

            GameObject currentPopup = sequence.popups[i];
            currentPopup.SetActive(true);
            CanvasGroup cg = currentPopup.GetComponent<CanvasGroup>();

            // Fade in
            LeanTween.alphaCanvas(cg, 1f, sequence.fadeDuration)
                     .setEase(LeanTweenType.easeOutQuad);

            // Wait for tap
            bool tapped = false;
            while (!tapped)
            {
                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    tapped = true;
                }
                yield return null;
            }

            // Fade out
            LeanTween.alphaCanvas(cg, 0f, sequence.fadeDuration)
                     .setEase(LeanTweenType.easeInQuad);
            yield return new WaitForSeconds(sequence.fadeDuration);
            currentPopup.SetActive(false);
        }

        sequence.onComplete?.Invoke();
    }
    #endregion

    #region Road & Crossing
    public void ShowRoad()
    {
        if (roadPrefab.activeSelf) return;

        roadPrefab.SetActive(true);
        LeanTween.scale(roadButton, Vector3.zero, 0.2f)
                 .setEase(LeanTweenType.easeInBack)
                 .setOnComplete(() => roadButton.SetActive(false));

        PlayerPrefs.SetInt(RoadUnlockedKey, 1);
        PlayerPrefs.Save();

        ShowDialogueSequence(new DialogueSequence()
        {
            lines = new[]
            {
                "Congratulations!",
                "Now let's build an intersection.",
                "You have enough money for that."
            },
            onComplete = () => 
            { 
                MoveCameraToPosition();
            }
        });
    }

    private void ShowCrossingDialogue()
    {
        gameSceneSoundManager.PlayDialogueSound();

        crossingDialoguePanel.SetActive(true);
        crossingImage.SetActive(true);
        buildingUpgradeManager.confirmButton.SetActive(false);
        crossingConfirmButton.SetActive(true);

        crossingDetailsText.text = $"Intersection - Level 1\nType: Traffic Flow\nCost: ${crossingCost}";

        LeanTween.scale(crossingDialoguePanel, Vector3.one, 0.3f)
                 .setFrom(Vector3.zero)
                 .setEase(LeanTweenType.easeOutBack);
    }

    public void ConfirmCrossingUnlock()
    {
        if (!moneyManager.SpendCoins(crossingCost))
        {
            ToastMessageSystem.ShowToast("Not enough coins!", ToastMessageSystem.ToastType.WARNING);
            return;
        }

        lockedCrossing.SetActive(false);
        crossing.SetActive(true);
        crossingDialoguePanel.SetActive(false);
        EnableObjectsAfterCrossing();

        LeanTween.scale(crossingDialoguePanel, Vector3.zero, 0.2f)
                 .setEase(LeanTweenType.easeInBack);

        ToastMessageSystem.ShowToast("Crossing Unlocked!", ToastMessageSystem.ToastType.SUCCESS);

        PlayerPrefs.SetInt(CrossingUnlockedKey, 1);
        PlayerPrefs.Save();

        ShowDialogueSequence(new DialogueSequence()
        {
            lines = new[]
            {
                "Great!",
                "Now you can manage traffic flow.",
                "Let's build some more!"
            },
            onComplete = () => { 
                
                RunPopupSequence(new PopupSequence()
                {
                     popups = postCrossingPopups,
                     onComplete = () => { }
                }); }
        });

       
    }

    private void EnableObjectsAfterCrossing()
    {
        foreach (var obj in objectsToEnableAfterCrossing)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    public void CloseCrossingDialogue()
    {
        LeanTween.cancel(crossingDialoguePanel);
        crossingDialoguePanel.transform.localScale = Vector3.zero;
        crossingDialoguePanel.SetActive(false);
        crossingImage.SetActive(false);
    }
    #endregion

    #region Utility
    private void MoveCameraToPosition()
    {
        var cameraController = FindFirstObjectByType<CameraControllerWithAutoMove>();
        if (cameraController != null)
        {
            cameraController.MoveToPosition(new Vector3(3f, 80f, 135f));
        }
        else
        {
            Debug.LogError("Camera controller not found!");
        }
    }
    #endregion
}