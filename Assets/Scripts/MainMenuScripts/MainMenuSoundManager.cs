using UnityEngine;

public class MainMenuSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource buttonAudioSource;
    [SerializeField] private AudioClip buttonClickSound; // MainMenu button sound

    void Awake()
    {
        if (buttonAudioSource == null)
        {
            buttonAudioSource = gameObject.AddComponent<AudioSource>();
            buttonAudioSource.playOnAwake = false;
            buttonAudioSource.volume = 0.7f;
        }
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null && buttonAudioSource != null)
        {
            buttonAudioSource.PlayOneShot(buttonClickSound);
            Debug.Log("MainMenu button click sound played");
        }
        else
        {
            Debug.LogWarning("MainMenu button sound or AudioSource not assigned!");
        }
    }
}