using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EnvironmentManager : MonoBehaviour, IPointerClickHandler
{
    public static EnvironmentManager Instance;

    [Header("Environment Settings")]
    public Light directionalLight;
    public Color dullColor = new Color(0.5f, 0.5f, 0.5f);
    public float dullDuration = 60f;

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;
    

    [Header("Dialogue Lines")]
    public string[] pollutionDialogueLines = {
        "The Factory Polluted the Environment",
        "We have to Take Action, Chief",
        "Plant Trees From the Shop"
    };

    [Header("Refferences")]
    [SerializeField] private UIAnimator uiAnimator;
    [SerializeField] private GameSceneSoundManager gameSceneSoundManager;

    // Dialogue state
    private bool isRunningDialogue = false;
    private int currentDialogueLine = 0;
    private bool waitingForInput = false;

    // Environment state
    private Color originalColor;
    private float pollutionProgress = 0f;
    private bool pollutionStarted = false;
    private bool isHealing = false;
    private float healProgress = 0f;
    private int currentTreeLevel = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        InitializeDialogueSystem();
    }

    private void InitializeDialogueSystem()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
            nextButton.onClick.AddListener(AdvanceDialogue);
        }
    }

    public void StartPollutionPhase()
    {
        if (!pollutionStarted && !isRunningDialogue)
        {
            pollutionStarted = true;
            originalColor = directionalLight.color;
            StartCoroutine(DullEnvironment());
        }
    }

    private IEnumerator DullEnvironment()
    {
        float elapsed = 0f;
        while (elapsed < dullDuration)
        {
            elapsed += Time.deltaTime;
            pollutionProgress = Mathf.Clamp01(elapsed / dullDuration);
            directionalLight.color = Color.Lerp(originalColor, dullColor, pollutionProgress);
            yield return null;
        }

        StartDialogue();
    }

    private void StartDialogue()
    {
        if (isRunningDialogue || dialoguePanel == null || dialogueText == null || nextButton == null)
            return;

        gameSceneSoundManager.PlayDialogueSound(); // Play dialogue sound


        currentDialogueLine = 0;
        isRunningDialogue = true;
        dialoguePanel.SetActive(true);
        nextButton.gameObject.SetActive(true);
        ShowNextDialogueLine();
    }

    private void ShowNextDialogueLine()
    {
        if (currentDialogueLine < pollutionDialogueLines.Length)
        {
            dialogueText.text = pollutionDialogueLines[currentDialogueLine];
            waitingForInput = true;
        }
        else
        {
            EndDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        if (!waitingForInput) return;

        waitingForInput = false;
        currentDialogueLine++;

        // Button feedback
        LeanTween.scale(nextButton.gameObject, Vector3.one * 0.9f, 0.1f)
                 .setEase(LeanTweenType.easeOutQuad)
                 .setLoopPingPong(1);

        ShowNextDialogueLine();
    }

    private void EndDialogue()
    {
        isRunningDialogue = false;
        dialoguePanel.SetActive(false);
        nextButton.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Keep for other click interactions
    }

    private void Update()
    {
        if (isHealing)
        {
            healProgress = currentTreeLevel * 0.2f;
            directionalLight.color = Color.Lerp(dullColor, originalColor, healProgress);

            if (currentTreeLevel >= 5)
            {
                isHealing = false;
                pollutionStarted = false;
                directionalLight.color = originalColor;
            }
        }
    }

    public void OnTreePurchased(int treeLevel)
    {
        currentTreeLevel = treeLevel;
        isHealing = true;
    }
}