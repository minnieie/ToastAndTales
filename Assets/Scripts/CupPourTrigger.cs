using UnityEngine;

/// <summary>
/// Handles the interaction between a Kettle and a Cup. 
/// Detects when the Kettle enters the trigger zone, swaps the cup visual to "Filled", 
/// and updates Firebase progress.
/// </summary>
public class CupPourTrigger : MonoBehaviour
{   
    [Header("Firebase Settings")]
    public string dishIdentifier = "Kopi"; 

    [Header("Cup Visuals")]
    public GameObject emptyCupModel;
    public GameObject filledCupModel;

    [Header("External References")]
    public KettlePour kettle;

    public CupUIManager uiManager;

    [Header("Global Progress")]
    public UIManager progressUI;  

    // Internal flag to ensure the score is only counted once per session
    private bool stepCompleted = false;

    /// <summary>
    /// Initializes references. Falls back to finding UIManager if not assigned.
    /// </summary>
    private void Awake()
    {
        // FIX: Replaced FindObjectOfType with FindFirstObjectByType for newer Unity versions
        if (progressUI == null)
            progressUI = Object.FindFirstObjectByType<UIManager>();
            
        if (uiManager == null)
            uiManager = Object.FindFirstObjectByType<CupUIManager>();
    }

    private void Start()
    {
        // 1. CHECK FIREBASE HISTORY
        // If the user already finished "Kopi" in a previous session (or earlier in this one),
        // we force the cup to look full immediately so they don't have to do it again.
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsSceneCompleted(dishIdentifier))
        {
            stepCompleted = true; // Lock step
            SetVisualsToFilled(); // Visual update
        }
        else
        {
            // Ensure correct starting state
            if (emptyCupModel != null) emptyCupModel.SetActive(true);
            if (filledCupModel != null) filledCupModel.SetActive(false);
        }
    }

    /// <summary>
    /// Triggered when the Kettle enters the Cup's pouring zone.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is the Kettle and if we haven't already completed this step
        if (other.CompareTag("Kettle") && !stepCompleted)
        {
            CompletePouringStep();
        }
    }

    private void CompletePouringStep()
    {
        stepCompleted = true;  // Lock this step to prevent double counting score

        // 1. Play pouring sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCoffeePour();
        }

        // 2. Start the pouring effect on the kettle
        if (kettle != null) kettle.StartPouring();

        // 3. Visuals
        SetVisualsToFilled();

        // 4. UI Updates
        if (uiManager != null) uiManager.ShowCongrats();
        if (progressUI != null) progressUI.CompleteStep();  

        // 5. FIREBASE UPDATE
        if (FirebaseManager.Instance != null)
        {
            // Calculate a rough time or just use Time.time
            float timeTaken = Time.timeSinceLevelLoad;
            FirebaseManager.Instance.MarkDishComplete(dishIdentifier, timeTaken);
            Debug.Log($"Sent {dishIdentifier} completion to Firebase!");
        }
        else
        {
            Debug.LogWarning("Firebase Manager is missing, progress not saved.");
        }
    }

    /// <summary>
    /// Helper to just swap the models
    /// </summary>
    private void SetVisualsToFilled()
    {
        if (emptyCupModel != null) emptyCupModel.SetActive(false);
        if (filledCupModel != null) filledCupModel.SetActive(true);
    }

    /// <summary>
    /// Triggered when the Kettle leaves the Cup's pouring zone.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Kettle"))
        {
            // Stop the pouring effect
            if (kettle != null) kettle.StopPouring();
        }
    }
}