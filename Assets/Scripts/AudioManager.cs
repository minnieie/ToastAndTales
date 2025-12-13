using UnityEngine;

/// <summary>
/// Manages all audio playback for the game, including Background Music (BGM) and Sound Effects (SFX).
/// Implements the Singleton pattern to persist across scenes.
/// </summary>
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

    /// <summary>
    /// Initializes the singleton instance and ensures the background music starts automatically.
    /// </summary>
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

    /// <summary>
    /// Plays the coffee pouring sound effect using the custom volume setting.
    /// </summary>
    public void PlayCoffeePour()
    {
        // Now uses the custom volume slider
        PlaySFX(coffeePourClip, coffeePourVolume); 
    }

    /// <summary>
    /// Plays the sound effect for swapping items on the tray.
    /// </summary>
    public void PlayTraySwap()
    {
        PlaySFX(traySwapClip, traySwapVolume);
    }

    /// <summary>
    /// Plays the sound effect for spreading topping on toast.
    /// </summary>
    public void PlayToastSpread()
    {
        // Assuming toast is fine at default 1.0, otherwise we can add a slider for this too
        PlaySFX(toastSpreadClip); 
    }

    /// <summary>
    /// Plays the victory sound effect (e.g., upon completing a level).
    /// </summary>
    public void PlayVictorySound()
    {
        PlaySFX(victoryClip, victoryVolume);
    }

    /// <summary>
    /// Plays the UI button click sound.
    /// </summary>
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickClip, buttonClickVolume);
    }

    /// <summary>
    /// Internal helper method to play a specific audio clip once through the SFX AudioSource.
    /// </summary>
    /// <param name="clip">The AudioClip to play.</param>
    /// <param name="volume">The volume scale (0.0 to 1.0).</param>
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