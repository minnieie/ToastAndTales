using UnityEngine;
using UnityEngine.UI; 

public class GameCompletion : MonoBehaviour
{
    [Header("UI References")]
    public GameObject victoryPanel;
    public Button closeButton; 

    [Header("Game Settings")]
    public int targetSteps = 3; 

    private void Start()
    {
        // 1. Hide victory panel at start
        if (victoryPanel != null) 
            victoryPanel.SetActive(false);

        // 2. Setup Close Button
        if (closeButton != null)
        {
            // When clicked, run the ClosePanel function
            closeButton.onClick.RemoveAllListeners(); // Good habit to clear old ones
            closeButton.onClick.AddListener(ClosePanel);
        }

        // 3. Listen to Firebase Manager updates
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnProgressUpdated += CheckWinCondition;
        }
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnProgressUpdated -= CheckWinCondition;
        }
    }

    private void CheckWinCondition(int currentProgress, int totalDishes)
    {
        if (currentProgress >= targetSteps)
        {
            Debug.Log("🏆 Target Reached! Showing Victory Screen.");
            Invoke(nameof(ShowPanel), 1.0f);
        }
    }

    private void ShowPanel()
    {
        if (victoryPanel != null) 
            victoryPanel.SetActive(true);
    }
    
    public void ClosePanel()
    {
        if (victoryPanel != null) 
            victoryPanel.SetActive(false);
    }
}