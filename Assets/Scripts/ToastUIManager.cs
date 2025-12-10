using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToastUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statusText;   // Brewing status text
    public Button historyButton;         // Button to toggle history
    public GameObject historyPanel;      // Panel that already contains text

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

    public void ShowCongrats()
    {
        if (statusText != null)
            statusText.text = "Congrats, you have finished spreading!";

        // Show history button only now
        if (historyButton != null)
            historyButton.gameObject.SetActive(true);
    }

    public void ToggleHistory()
    {
        if (historyPanel != null)
        {
            bool isActive = historyPanel.activeSelf;
            historyPanel.SetActive(!isActive); // flip open/close
        }
    }
}
