using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Settings Panel Reference")]
    [SerializeField] private GameObject restartPanel; // Assign this in the Inspector

    [Header("LeanTween Animation Settings")]
    [SerializeField] private float openDuration = 0.3f; // Duration for opening animation
    [SerializeField] private float closeDuration = 0.3f; // Duration for closing animation
    [SerializeField] private Vector3 openScale = Vector3.one; // Scale when dialog is open
    [SerializeField] private Vector3 closeScale = Vector3.zero; // Scale when dialog is closed

    [Header("Buttons")]
    [SerializeField] private GameObject[] buttons; // Array of buttons to animate

    [Header("Button Animation Settings")]
    [SerializeField] private float buttonAnimationDuration = 0.5f; // Duration for button animations
    [SerializeField] private float buttonAnimationDelay = 0.1f; // Delay between button animations
    [SerializeField] private Vector3 buttonStartScale = Vector3.zero; // Initial scale for buttons
    [SerializeField] private Vector3 buttonEndScale = Vector3.one; // Final scale for buttons

    [Header("Volume and Music Controls")]
    public bool isVolumeMuted = false;
    public bool isMusicMuted = false;


    public GameObject loadingPanel;
    public Slider loadingSlider;


    private void Start()
    {
        // Initialize the settings panel to be closed (scaled to zero)
        if (restartPanel != null)
        {
            restartPanel.transform.localScale = closeScale;
            restartPanel.SetActive(false); // Ensure the panel is inactive initially
        }
        else
        {
            Debug.LogError("Settings Panel reference is missing!");
        }

        // Initialize buttons to be hidden (scaled to zero)
        if (buttons != null && buttons.Length > 0)
        {
            foreach (GameObject button in buttons)
            {
                if (button != null)
                {
                    button.transform.localScale = buttonStartScale;
                }
            }
        }
        else
        {
            Debug.LogError("Buttons array is empty or not assigned!");
        }

        // Animate buttons on start
        AnimateButtons();
    }

    #region Scene Management
    public void PlayGame()
    {
        StartCoroutine(LoadGameScene("GameScene"));
    }
    IEnumerator LoadGameScene(string sceneName)
    {
        loadingPanel.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // <-- Important

        float targetProgress = 0f;
        float fillSpeed = 0.5f; // You can adjust for smoother effect

        while (!operation.isDone)
        {
            if (operation.progress < 0.9f)
            {
                targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            }
            else
            {
                targetProgress = 1f;
            }

            loadingSlider.value = Mathf.MoveTowards(loadingSlider.value, targetProgress, fillSpeed * Time.deltaTime);

            if (loadingSlider.value >= 1f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }



public void QuitGame()
    {
        Application.Quit();             
    }
  
    public void OpenRestartPanel()
    {
        if (restartPanel != null)
        {
            restartPanel.SetActive(true); // Activate the panel
            LeanTween.scale(restartPanel, openScale, openDuration)
                     .setEase(LeanTweenType.easeOutBack); // Use a smooth easing type
                    
        }
        else
        {
            Debug.LogError("Settings Panel reference is missing!");
        }
    }
    public void CloseRestartPanel()
    {
        if (restartPanel != null)
        {
            LeanTween.scale(restartPanel, closeScale, closeDuration)
                     .setEase(LeanTweenType.easeInBack); // Use a smooth easing type
                    
        }
    }
    public void RestartGame()
    {
        PlayerPrefs.DeleteAll(); // Deletes all PlayerPrefs data
        PlayerPrefs.Save(); // Save the changes
        SceneManager.LoadScene("GameScene"); // Optional: Reload GameScene
    }

    
    #endregion

    #region Audio Controls
    public void ToggleVolume()
    {
        isVolumeMuted = !isVolumeMuted;
        AudioListener.volume = isVolumeMuted ? 0 : 1; // Example: Mute/unmute all audio
        Debug.Log($"{nameof(ToggleVolume)} clicked: {(isVolumeMuted ? "Volume Muted" : "Volume Unmuted")}");
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        // Example: Mute/unmute background music
        // AudioSource musicSource = GetComponent<AudioSource>();
        // musicSource.mute = isMusicMuted;
        Debug.Log($"{nameof(ToggleMusic)} clicked: {(isMusicMuted ? "Music Muted" : "Music Unmuted")}");
    }
    #endregion

    #region Button Animations
    private void AnimateButtons()
    {
        if (buttons != null && buttons.Length > 0)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    // Check if this is the last button
                    if (i == buttons.Length - 1)
                    {
                        // Apply LeanTween animation but ensure the last button scales to (1,1,1)
                        LeanTween.scale(buttons[i], Vector3.one, buttonAnimationDuration)
                                 .setDelay(i * buttonAnimationDelay)
                                 .setEase(LeanTweenType.easeOutBack);
                    }
                    else
                    {
                        // Animate other buttons with a delay
                        LeanTween.scale(buttons[i], buttonEndScale, buttonAnimationDuration)
                                 .setDelay(i * buttonAnimationDelay) // Add delay based on button index
                                 .setEase(LeanTweenType.easeOutBack); // Use a smooth easing type
                    }
                }
            }
        }
    }
    #endregion
}