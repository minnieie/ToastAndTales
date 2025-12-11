using UnityEngine;

/// <summary>
/// Handles the interaction between a Kettle and a Cup. 
/// Detects when the Kettle enters the trigger zone, swaps the cup visual to "Filled", 
/// triggers pouring effects, and updates the game progress.
/// </summary>
public class CupPourTrigger : MonoBehaviour
{
    [Header("Cup Visuals")]
    [Tooltip("The model representing the empty cup state.")]
    public GameObject emptyCupModel;

    [Tooltip("The model representing the filled cup state (liquid inside).")]
    public GameObject filledCupModel;

    [Header("External References")]
    [Tooltip("Reference to the script controlling the Kettle's pouring animation/particles.")]
    public KettlePour kettle;

    [Tooltip("UI Manager specific to the Cup interaction (local feedback).")]
    public CupUIManager uiManager;

    [Header("Global Progress")]
    [Tooltip("Main UI Manager that tracks the overall game steps (1/3, 2/3, etc.).")]
    public UIManager progressUI;  

    // Internal flag to ensure the score is only counted once per session
    private bool stepCompleted = false;

    /// <summary>
    /// Initializes references. Falls back to finding UIManager if not assigned.
    /// </summary>
    private void Awake()
    {
        // FIX: CS0618 Warning resolved by using FindFirstObjectByType
        if (progressUI == null)
            progressUI = Object.FindFirstObjectByType<UIManager>();
    }

    /// <summary>
    /// Triggered when the Kettle enters the Cup's pouring zone.
    /// Handles visual swapping and progress updates.
    /// </summary>
    /// <param name="other">The collider entering the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is the Kettle and if we haven't already completed this step
        if (other.CompareTag("Kettle") && !stepCompleted)
        {
            stepCompleted = true;  // Lock this step to prevent double counting score

            // 1. Start the pouring effect on the kettle
            if (kettle != null)
                kettle.StartPouring();

            // 2. Swap visuals from Empty -> Filled
            if (emptyCupModel != null && filledCupModel != null)
            {
                emptyCupModel.SetActive(false);
                filledCupModel.SetActive(true);

                // 3. Show local congratulations (floating text/particle)
                if (uiManager != null)
                    uiManager.ShowCongrats();

                // 4. Update global game progress
                if (progressUI != null)
                    progressUI.CompleteStep();  
            }
        }
    }

    /// <summary>
    /// Triggered when the Kettle leaves the Cup's pouring zone.
    /// Resets the visuals and stops the pouring effect.
    /// </summary>
    /// <param name="other">The collider exiting the trigger.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Kettle"))
        {
            // 1. Stop the pouring effect
            if (kettle != null)
                kettle.StopPouring();

            // 2. Reset visuals (Optional: This makes the cup look empty again if you pull away)
            if (emptyCupModel != null && filledCupModel != null)
            {
                emptyCupModel.SetActive(true);
                filledCupModel.SetActive(false);
            }
        }
    }
}