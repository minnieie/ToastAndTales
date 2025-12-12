using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.XR.Management;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// The central manager for the AR Gameplay Scene.
/// Handles UI updates (Progress, Intro, Feedback), Scene Navigation, and AR Session lifecycle.
/// Acts as a Singleton to ensure easy access from other scripts like Triggers.
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [Tooltip("The panel shown at the start of the scene (instructions/intro).")]
    public GameObject introPanel;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackText; 

    [Header("Progress Tracking")]
    public TextMeshProUGUI progressText; 
    
    // Internal state tracking
    private int currentStep = 0;
    private readonly int totalSteps = 3; 
    private float lastStepTime = 0f;

    [Header("Home Button")]
    public Button homeButton; 

    public string homeSceneName = "Home";

    /// <summary>
    /// Sets up the Singleton instance.
    /// Destroys duplicates to ensure only one UIManager exists.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            Debug.LogWarning("Duplicate UIManager detected. Destroying new instance.");
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Uncomment if you want it to persist across scenes
        }
    }

    /// <summary>
    /// Initializes UI, finds buttons dynamically, and syncs progress with Firebase.
    /// </summary>
    private void Start()
    {
        // Assign buttons dynamically in case scene objects are new
        AssignButtons();

        // Initialize UI
        InitializeUI();

        // Fetch progress from Firebase
        TryFetchFirebaseProgress();
    }

    /// <summary>
    /// Plays the button click sound effect via AudioManager.
    /// </summary>
    private void PlayUISound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    /// <summary>
    /// Locates the Home Button in the scene if not assigned manually.
    /// </summary>
    private void AssignButtons()
    {
        // If homeButton not assigned in Inspector, try finding it dynamically
        if (homeButton == null)
        {
            GameObject btnObj = GameObject.Find("HomeButton");
            if (btnObj != null)
            {
                homeButton = btnObj.GetComponent<Button>();
            }
        }

        // Setup listener
        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(PlayUISound);
            homeButton.onClick.AddListener(GoToHomeScene);
            Debug.Log("Home button listener assigned.");
        }
        else
        {
            Debug.LogWarning("Home button not found in scene!");
        }
    }

    /// <summary>
    /// Sets initial UI states.
    /// </summary>
    private void InitializeUI()
    {
        // Initialize progress text
        UpdateProgressText();

        // Show intro panel
        ShowIntroPanel();
    }

    /// <summary>
    /// Attempts to get the saved progress from Firebase to sync the local UI.
    /// </summary>
    private async void TryFetchFirebaseProgress()
    {
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.user != null)
        {
            try
            {
                await FirebaseManager.Instance.FetchUserProgress();

                // Update progress if different from local default
                if (FirebaseManager.Instance.CurrentProgress != currentStep)
                {
                    currentStep = Mathf.Clamp(FirebaseManager.Instance.CurrentProgress, 0, totalSteps);
                    UpdateProgressText();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Couldn't fetch Firebase progress: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Increments the current step count, updates the UI, and saves to Firebase.
    /// Called by interaction scripts (e.g., CupPourTrigger, TraySwapTrigger).
    /// </summary>
    public void CompleteStep()
        {
            // 1. SECURITY CHECK: Has it been less than 1 second since the last step?
            if (Time.time < lastStepTime + 1.0f) 
            {
                Debug.LogWarning( "Blocked a double-step call! (Debounce active)");
                return; 
            }

            // 2. Update the timestamp
            lastStepTime = Time.time;

            // 3. Increment step and update UI
            currentStep = Mathf.Clamp(currentStep + 1, 0, totalSteps);
            UpdateProgressText();
            Debug.Log($"Step Accepted. New Progress: {currentStep}/{totalSteps}");

            // Save progress to Firebase
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.UpdateUserProgress(currentStep); 
            }

            if (currentStep == totalSteps)
            {
                Debug.Log("All tasks complete!");
            }
        }

    /// <summary>
    /// Updates the text display (e.g., "1/3").
    /// </summary>
    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"{currentStep}/{totalSteps}";
        }
    }

    /// <summary>
    /// Shows the intro panel and auto-hides it after 3 seconds.
    /// </summary>
    public void ShowIntroPanel()
    {
        if (introPanel != null)
        {
            introPanel.SetActive(true);
            Invoke(nameof(HideIntroPanel), 3f); // hide after 3 seconds
        }
    }

    private void HideIntroPanel()
    {
        if (introPanel != null)
        {
            introPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Displays a temporary message to the user for feedback (used by ImageTracker).
    /// Message disappears after 3 seconds.
    /// </summary>
    /// <param name="message">The string to display.</param>
    public void ShowMessage(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            CancelInvoke(nameof(ClearMessage));
            Invoke(nameof(ClearMessage), 3f);
        }
        Debug.Log($"[UI Feedback] {message}");
    }

    private void ClearMessage()
    {
        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }
    }

    /// <summary>
    /// Stops AR subsystems (Camera, Plane Detection) before leaving the scene.
    /// Prevents crashes on some mobile devices when switching scenes.
    /// </summary>
    private void StopARSession()
    {
        var xrManager = XRGeneralSettings.Instance.Manager;

        if (xrManager.isInitializationComplete)
        {
            xrManager.StopSubsystems(); 
            xrManager.DeinitializeLoader(); 
            Debug.Log("AR session stopped and deinitialized.");
        }
    }

    /// <summary>
    /// Cleanly exits the AR scene and loads the Home Menu.
    /// </summary>
    private void GoToHomeScene()
    {
        Debug.Log($"Going to home scene: {homeSceneName}");
        CancelInvoke(); 

        // Stop AR session before leaving scene
        StopARSession();

        if (!string.IsNullOrEmpty(homeSceneName))
        {
            SceneManager.LoadScene(homeSceneName);
        }
        else
        {
            Debug.LogError("Home scene name is not set in UIManager!");
        }
    }

    /// <summary>
    /// Returns the current progress step.
    /// </summary>
    public int GetCurrentStep()
    {
        return currentStep;
    }

    /// <summary>
    /// Manually sets the progress step (useful for debugging or loading saves).
    /// </summary>
    /// <param name="step">The step number to set.</param>
    public void SetProgress(int step)
    {
        currentStep = Mathf.Clamp(step, 0, totalSteps);
        UpdateProgressText();
    }
}