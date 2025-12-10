using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject introPanel;

    [Header("Progress Tracking")]
    public TextMeshProUGUI progressText; // Shows "1/3", "2/3", etc.
    private int currentStep = 0;
    private int totalSteps = 3;

    [Header("Home Button")]
    public Button homeButton; // Optional: can assign in Inspector
    public string homeSceneName = "HomeScene";

    private void Awake()
    {
        // REMOVE DontDestroyOnLoad - UIManager should be scene-specific
        // Unless you have a specific reason to persist it across scenes
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
            string sceneName = SceneManager.GetActiveScene().name;
            FirebaseManager.Instance.MarkDishComplete(sceneName);
        }

        if (currentStep == totalSteps)
        {
            Debug.Log("All tasks complete!");
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

    private void GoToHomeScene()
    {
        Debug.Log($"Going to home scene: {homeSceneName}");
        CancelInvoke(); // Cancel any pending invokes

        if (!string.IsNullOrEmpty(homeSceneName))
        {
            SceneManager.LoadScene(homeSceneName);
        }
        else
        {
            Debug.LogError("Home scene name is not set in UIManager!");
        }
    }

    public void SetProgress(int step)
    {
        currentStep = Mathf.Clamp(step, 0, totalSteps);
        UpdateProgressText();
    }
}