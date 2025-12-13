using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the User Interface for the toast spreading application.
/// Handles the display of status text and the visibility of the history panel.
/// </summary>
public class ToastUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statusText;   // Brewing status text
    public Button historyButton;         // Button to toggle history
    public GameObject historyPanel;      // Panel that already contains text

    /// <summary>
    /// Initializes the UI state. Sets the initial instruction text, hides the history elements, 
    /// and assigns the click listener to the history button.
    /// </summary>
    private void Start()
    {
        if (statusText != null)
            statusText.text = "Nice, Please scan the knife to start spreading.";

        // Hide history button and panel at start
        if (historyButton != null)
            historyButton.gameObject.SetActive(false);

        if (historyPanel != null)
            historyPanel.SetActive(false);

        if (historyButton != null)
            historyButton.onClick.AddListener(ToggleHistory);
    }

    /// <summary>
    /// Updates the UI to indicate the task is finished.
    /// Changes the status text to a congratulatory message and reveals the history button.
    /// </summary>
    public void ShowCongrats()
    {
        if (statusText != null)
            statusText.text = "Congrats, you have finished spreading!";

        // Show history button only now
        if (historyButton != null)
            historyButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Toggles the active state (visibility) of the history panel.
    /// </summary>
    public void ToggleHistory()
    {
        if (historyPanel != null)
        {
            bool isActive = historyPanel.activeSelf;
            historyPanel.SetActive(!isActive); // flip open/close
        }
    }
}