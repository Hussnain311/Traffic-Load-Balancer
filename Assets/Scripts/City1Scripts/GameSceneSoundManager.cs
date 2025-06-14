using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource buttonAudioSource;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip dialogueWhistleSound;
    [SerializeField] private AudioClip backgroundMusic;

    [SerializeField] private Slider musicSound;
    [SerializeField] private Slider vfxSound;

    void Awake()
    {
        // Button AudioSource setup
        if (buttonAudioSource == null)
        {
            buttonAudioSource = gameObject.AddComponent<AudioSource>();
            buttonAudioSource.playOnAwake = false;
            buttonAudioSource.volume = PlayerPrefs.GetFloat("ButtonVolume", 0.7f);
        }

        // Music AudioSource setup
        if (musicAudioSource == null)
        {
            musicAudioSource = gameObject.AddComponent<AudioSource>();
            musicAudioSource.playOnAwake = false;
            musicAudioSource.loop = true; // Explicitly set looping
            musicAudioSource.volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        }

        // Start music if in GameScene
        if (SceneManager.GetActiveScene().name == "GameScene" && backgroundMusic != null && musicAudioSource != null)
        {
            musicAudioSource.clip = backgroundMusic;
            musicAudioSource.loop = true; // Reinforce looping just before playing
            musicAudioSource.Play();
            Debug.Log("GameScene background music started in Awake");
        }
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.SetFloat("MusicVolume", 0.5f);
        }
        if (!PlayerPrefs.HasKey("ButtonVolume"))
        {
            PlayerPrefs.SetFloat("ButtonVolume", 0.7f);
        }

        Load();

        if (musicAudioSource != null && musicSound != null)
        {
            musicAudioSource.volume = musicSound.value;
        }
        if (buttonAudioSource != null && vfxSound != null)
        {
            buttonAudioSource.volume = vfxSound.value;
        }

        if (musicSound != null)
        {
            musicSound.onValueChanged.AddListener(ChangeMusicVolume);
        }
        if (vfxSound != null)
        {
            vfxSound.onValueChanged.AddListener(ChangeButtonVolume);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu" && musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            Debug.Log("GameScene background music stopped in MainMenu");
        }
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null && buttonAudioSource != null)
        {
            buttonAudioSource.PlayOneShot(buttonClickSound);
            Debug.Log("GameScene button click sound played");
        }
        else
        {
            Debug.LogWarning("GameScene button sound or AudioSource not assigned!");
        }
    }

    public void PlayDialogueSound()
    {
        if (dialogueWhistleSound != null && buttonAudioSource != null)
        {
            buttonAudioSource.PlayOneShot(dialogueWhistleSound);
            Debug.Log("Dialogue whistle sound played");
        }
        else
        {
            Debug.LogWarning("Dialogue sound or AudioSource not assigned!");
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            Debug.Log("GameScene background music stopped manually");
        }
    }

    public void ChangeMusicVolume(float volume)
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = volume;
            PlayerPrefs.SetFloat("MusicVolume", volume);
            PlayerPrefs.Save();
        }
    }

    public void ChangeButtonVolume(float volume)
    {
        if (buttonAudioSource != null)
        {
            buttonAudioSource.volume = volume;
            PlayerPrefs.SetFloat("ButtonVolume", volume);
            PlayerPrefs.Save();
            Debug.Log($"GameScene button volume set to {volume}");
        }
    }

    private void Load()
    {
        if (musicSound != null)
        {
            musicSound.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        }
        if (vfxSound != null)
        {
            vfxSound.value = PlayerPrefs.GetFloat("ButtonVolume", 0.7f);
        }
    }
}