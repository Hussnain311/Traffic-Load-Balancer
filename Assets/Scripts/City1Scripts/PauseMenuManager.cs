using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanelManager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("LeanTween Animation Settings - Pause Panel")]
    [SerializeField] private float pauseOpenDuration = 0.3f;
    [SerializeField] private float pauseCloseDuration = 0.3f;
    [SerializeField] private Vector2 pauseStartPosition = new Vector2(-1000f, 0f); // Off-screen left
    [SerializeField] private Vector2 pauseEndPosition = Vector2.zero; // Center

    [Header("LeanTween Animation Settings - Settings Panel")]
    [SerializeField] private float settingsOpenDuration = 0.3f;
    [SerializeField] private float settingsCloseDuration = 0.3f;
    [SerializeField] private Vector2 settingsStartPosition = new Vector2(1000f, 0f); // Off-screen right
    [SerializeField] private Vector2 settingsEndPosition = Vector2.zero; // Center

    private bool isPaused = false;
    private RectTransform pausePanelRect;
    private RectTransform settingsPanelRect;

    void Start()
    {
        if (pausePanel != null)
        {
            pausePanelRect = pausePanel.GetComponent<RectTransform>();
            pausePanelRect.anchoredPosition = pauseStartPosition;
            pausePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Pause Panel is not assigned!");
        }

        if (settingsPanel != null)
        {
            settingsPanelRect = settingsPanel.GetComponent<RectTransform>();
            settingsPanelRect.anchoredPosition = settingsStartPosition;
            settingsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Settings Panel is not assigned!");
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void TogglePausePanel()
    {
        if (isPaused)
        {
            ContinueGame();
        }
        else
        {
            isPaused = true;
            OpenPausePanel();
        }
    }

    public void ContinueGame()
    {
        isPaused = false;
        ClosePausePanel();
        Time.timeScale = 1f;
    }

    public void OpenPausePanel()
    {
        if (pausePanel != null && pausePanelRect != null)
        {
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            LeanTween.cancel(pausePanel);
            pausePanelRect.anchoredPosition = pauseStartPosition;
            LeanTween.move(pausePanelRect, pauseEndPosition, pauseOpenDuration)
                     .setEase(LeanTweenType.easeOutQuad)
                     .setIgnoreTimeScale(true)
                     .setOnStart(() => Debug.Log("Opening Pause Panel..."));
        }
    }

    public void ClosePausePanel()
    {
        if (pausePanel != null && pausePanelRect != null)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }

            LeanTween.cancel(pausePanel);
            LeanTween.move(pausePanelRect, pauseStartPosition, pauseCloseDuration)
                     .setEase(LeanTweenType.easeInQuad)
                     .setIgnoreTimeScale(true)
                     .setOnComplete(() =>
                     {
                         pausePanel.SetActive(false);
                         Debug.Log("Pause Panel closed.");
                     });
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null && settingsPanelRect != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
            LeanTween.cancel(settingsPanel);
            settingsPanelRect.anchoredPosition = settingsStartPosition;
            LeanTween.move(settingsPanelRect, settingsEndPosition, settingsOpenDuration)
                     .setEase(LeanTweenType.easeOutQuad)
                     .setIgnoreTimeScale(true)
                     .setOnStart(() => Debug.Log("Opening Settings Panel..."));
            Canvas.ForceUpdateCanvases();
        }
        else
        {
            Debug.LogError("Settings Panel is null or not assigned!");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null && settingsPanelRect != null)
        {
            Debug.Log("Closing Settings Panel...");
            LeanTween.cancel(settingsPanel);
            LeanTween.move(settingsPanelRect, settingsStartPosition, settingsCloseDuration)
                     .setEase(LeanTweenType.easeInQuad)
                     .setIgnoreTimeScale(true)
                     .setOnComplete(() =>
                     {
                         settingsPanel.SetActive(false);
                         Debug.Log("Settings Panel closed.");
                     });
        }
        else
        {
            Debug.LogError("Settings Panel is null or not assigned!");
        }
    }

    public void GoToMainMenu()
    {
        Debug.Log("Attempting to load MainMenu scene...");
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("StartMenu");
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("Game paused.");
    }
}