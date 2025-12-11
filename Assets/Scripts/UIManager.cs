using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.XR.Management;
using UnityEngine.XR.ARFoundation;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject introPanel;

    // Add a UI element to display messages (optional but recommended for ShowMessage)
    [Header("Feedback")]
    public TextMeshProUGUI feedbackText; 

    [Header("Progress Tracking")]
    public TextMeshProUGUI progressText; // Shows "1/3", "2/3", etc.
    private int currentStep = 0;
    private readonly int totalSteps = 3; // Changed to readonly for consistency

    [Header("Home Button")]
    public Button homeButton; 
    public string homeSceneName = "Home";

    private void Awake()
    {
        // Singleton setup
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

    private void Start()
    {
        // Assign buttons dynamically in case scene objects are new
        AssignButtons();

        // Initialize UI
        InitializeUI();

        // Fetch progress from Firebase
        TryFetchFirebaseProgress();
    }

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
            homeButton.onClick.AddListener(GoToHomeScene);
            Debug.Log("Home button listener assigned.");
        }
        else
        {
            Debug.LogWarning("Home button not found in scene!");
        }
    }

    private void InitializeUI()
    {
        // Initialize progress text
        UpdateProgressText();

        // Show intro panel
        ShowIntroPanel();
    }

    private async void TryFetchFirebaseProgress()
    {
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.user != null)
        {
            try
            {
                await FirebaseManager.Instance.FetchUserProgress();

                // Update progress if different
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

    public void CompleteStep()
    {
        currentStep = Mathf.Clamp(currentStep + 1, 0, totalSteps);
        UpdateProgressText();

        // Save progress to Firebase
        if (FirebaseManager.Instance != null)
        {
            // Assuming your FirebaseManager has a method to update the step count
            FirebaseManager.Instance.UpdateUserProgress(currentStep); 
        }

        if (currentStep == totalSteps)
        {
            Debug.Log("All tasks complete!");
            // TODO: Trigger reward/end game logic
        }
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"{currentStep}/{totalSteps}";
        }
    }

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
    /// </summary>
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
    /// Stop AR subsystems before leaving AR scene
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

    public int GetCurrentStep()
    {
        return currentStep;
    }

    public void SetProgress(int step)
    {
        currentStep = Mathf.Clamp(step, 0, totalSteps);
        UpdateProgressText();
    }
}