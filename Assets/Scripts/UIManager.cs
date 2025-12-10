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
    public Button homeButton;
    public string homeSceneName = "HomeScene";

    private void Start()
    {
        // Initialize UI first (makes it responsive immediately)
        InitializeUI();

        // Then try to fetch Firebase data in background
        TryFetchFirebaseProgress();
    }

    private void InitializeUI()
    {
        // Setup home button if it exists
        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(GoToHomeScene);
        }

        // Initialize progress display
        if (progressText != null)
            progressText.text = $"{currentStep}/{totalSteps}";

        ShowIntroPanel();
    }

    private async void TryFetchFirebaseProgress()
    {
        // Only try to fetch if Firebase is available
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.user != null)
        {
            try
            {
                await FirebaseManager.Instance.FetchUserProgress();

                // Update progress from Firebase
                if (FirebaseManager.Instance.CurrentProgress != currentStep)
                {
                    currentStep = FirebaseManager.Instance.CurrentProgress;
                    currentStep = Mathf.Clamp(currentStep, 0, totalSteps);

                    // Update UI with Firebase data
                    if (progressText != null)
                        progressText.text = $"{currentStep}/{totalSteps}";
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Couldn't fetch Firebase progress: {e.Message}");
                // Continue with local progress
            }
        }
    }

    // Call this when a task (scan or action) is completed
    public void CompleteStep()
    {
        currentStep++;
        currentStep = Mathf.Clamp(currentStep, 0, totalSteps);

        // Update progress text
        if (progressText != null)
            progressText.text = $"{currentStep}/{totalSteps}";

        // Save to Firebase if available
        if (FirebaseManager.Instance != null)
        {
            string dishName = SceneManager.GetActiveScene().name;
            FirebaseManager.Instance.MarkDishComplete(dishName);
        }

        // If all tasks done
        if (currentStep == totalSteps)
        {
            Debug.Log("All tasks complete!");
            // You could add completion celebration here
        }
    }

    public void ShowIntroPanel()
    {
        if (introPanel != null)
        {
            introPanel.SetActive(true);
            Invoke(nameof(HideIntroPanel), 3f); // hides after 3 seconds
        }
    }

    private void HideIntroPanel()
    {
        if (introPanel != null)
            introPanel.SetActive(false);
    }

    private void GoToHomeScene()
    {
        Debug.Log($"Going to home scene: {homeSceneName}");

        // Cancel any pending invokes (like intro panel hide)
        CancelInvoke();

        // Load home scene
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

        if (progressText != null)
            progressText.text = $"{currentStep}/{totalSteps}";
    }

}
