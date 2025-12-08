using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject introPanel;         // Only intro popup

    [Header("Progress Tracking")]
    public TextMeshProUGUI progressText;  // Shows "1/3", "2/3", etc.
    private int currentStep = 0;
    private int totalSteps = 3;           

    private void Start()
    {
        // Initialize progress
        if (progressText != null)
            progressText.text = $"0/{totalSteps}";

        ShowIntroPanel();
    }

    // Call this when a task (scan or action) is completed
    public void CompleteStep()
    {
        currentStep++;

        // Clamp
        if (currentStep > totalSteps)
            currentStep = totalSteps;

        // Update progress text
        if (progressText != null)
            progressText.text = $"{currentStep}/{totalSteps}";

        // If all tasks done
        if (currentStep == totalSteps)
        {
            Debug.Log("All tasks complete!");
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
}
