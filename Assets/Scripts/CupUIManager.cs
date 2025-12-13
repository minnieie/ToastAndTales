using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the User Interface for the cup brewing interaction, handling status text updates and history panel visibility.
/// </summary>
public class CupUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statusText;   // Brewing status text
    public Button historyButton;         // Button to toggle history
    public GameObject historyPanel;      // Panel that already contains text

    /// <summary>
    /// Initializes the UI state. Sets the initial prompt text, hides history controls, and assigns button listeners.
    /// </summary>
    private void Start()
    {
        if (statusText != null)
            statusText.text = "Nice, Please scan the kettle to start pouring.";

        // Hide history button and panel at start
        if (historyButton != null)
            historyButton.gameObject.SetActive(false);

        if (historyPanel != null)
            historyPanel.SetActive(false);

        if (historyButton != null)
            historyButton.onClick.AddListener(ToggleHistory);
    }

    /// <summary>
    /// Updates the UI to indicate the brewing task is complete and reveals the history button.
    /// </summary>
    public void ShowCongrats()
    {
        if (statusText != null)
            statusText.text = "Congrats, you have finished brewing!";

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