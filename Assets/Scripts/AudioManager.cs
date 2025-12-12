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
    public AudioClip buttonClickClip; // Drag 'button click' here
    public AudioClip victoryClip;     // Drag 'victory' sound here

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float buttonClickVolume = 0.4f;

    [Range(0f, 1f)]
    public float victoryVolume = 0.5f; 
    [Range(0f, 1f)]
    public float traySwapVolume = 1.0f; 
    
    [Range(0f, 1f)]
    public float coffeePourVolume = 0.6f; // New slider for coffee!

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
        // Now uses the custom volume slider
        PlaySFX(coffeePourClip, coffeePourVolume); 
    }

    public void PlayTraySwap()
    {
        PlaySFX(traySwapClip, traySwapVolume);
    }

    public void PlayToastSpread()
    {
        // Assuming toast is fine at default 1.0, otherwise we can add a slider for this too
        PlaySFX(toastSpreadClip); 
    }

    public void PlayVictorySound()
    {
        PlaySFX(victoryClip, victoryVolume);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickClip, buttonClickVolume);
    }

    // Helper method to actually play the sound
    // 'volume = 1.0f' makes the volume optional. It defaults to full volume if not specified.
    private void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning("Missing Audio Source or Clip in AudioManager!");
        }
    }
}