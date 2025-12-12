using UnityEngine;
using UnityEngine.UI; 

/// <summary>
/// Handles the "Game Over" or "Victory" state for a specific scene.
/// It listens to global progress updates from FirebaseManager and displays a Victory UI
/// when the player reaches the target number of completed steps.
/// </summary>
public class GameCompletion : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI Panel to display when the user wins (e.g., 'You Win!' popup).")]
    public GameObject victoryPanel;

    [Tooltip("The button used to close or dismiss the victory panel.")]
    public Button closeButton; 

    [Header("Game Settings")]
    [Tooltip("The number of steps required to trigger the win condition (e.g., 3 dishes).")]
    public int targetSteps = 3; 

    /// <summary>
    /// Initializes the UI state and subscribes to progress events.
    /// </summary>
    private void Start()
    {
        // 1. Hide victory panel at start so it doesn't block the view
        if (victoryPanel != null) 
            victoryPanel.SetActive(false);

        // 2. Setup Close Button listener programmatically
        if (closeButton != null)
        {
            // Clear any existing listeners to avoid duplicates, then add ours
            closeButton.onClick.RemoveAllListeners(); 
            closeButton.onClick.AddListener(ClosePanel);
        }

        // 3. Listen to Firebase Manager updates for real-time progress tracking
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnProgressUpdated += CheckWinCondition;
        }
    }

    /// <summary>
    /// Unsubscribes from events when the object is destroyed to prevent memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnProgressUpdated -= CheckWinCondition;
        }
    }

    /// <summary>
    /// Callback method triggered whenever user progress changes.
    /// Checks if the current progress meets the target required to win.
    /// </summary>
    /// <param name="currentProgress">The user's current completed steps.</param>
    /// <param name="totalDishes">The total available steps (unused here, but part of the event signature).</param>
    private void CheckWinCondition(int currentProgress, int totalDishes)
    {
        // Check if we hit the target
        if (currentProgress >= targetSteps)
        {
            Debug.Log("Target Reached! Showing Victory Screen.");

            // Play Victory Sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVictorySound();
            }

            // Add a 1-second delay so the UI doesn't pop up instantly, feeling more natural
            Invoke(nameof(ShowPanel), 1.0f);
        }
    }

    /// <summary>
    /// Activates the Victory Panel GameObject.
    /// </summary>
    private void ShowPanel()
    {
        if (victoryPanel != null) 
            victoryPanel.SetActive(true);
    }

    /// <summary>
    /// Deactivates the Victory Panel GameObject. 
    /// Can be called by the Close Button or other external scripts.
    /// </summary>
    public void ClosePanel()
    {
        if (victoryPanel != null) 
            victoryPanel.SetActive(false);
    }
}