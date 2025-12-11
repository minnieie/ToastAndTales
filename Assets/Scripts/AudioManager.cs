using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource; // Drag BGM Source here
    public AudioSource sfxSource; // Drag SFX Source here

    [Header("Sound Clips")]
    public AudioClip coffeePourClip;  // Drag 'coffeepouring' here
    public AudioClip traySwapClip;    // Drag 'putting on tray' here
    public AudioClip toastSpreadClip; // Drag 'spreading toast' here

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Auto-start BGM
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    // --- Specific Play Methods ---

    public void PlayCoffeePour()
    {
        PlaySFX(coffeePourClip);
    }

    public void PlayTraySwap()
    {
        PlaySFX(traySwapClip);
    }

    public void PlayToastSpread()
    {
        PlaySFX(toastSpreadClip);
    }

    // Helper method to actually play the sound
    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Missing Audio Source or Clip in AudioManager!");
        }
    }
}