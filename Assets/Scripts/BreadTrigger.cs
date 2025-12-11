using UnityEngine;

/// <summary>
/// Handles the interaction between a Knife and Bread.
/// Checks Firebase on Start to see if this step is already done.
/// </summary>
public class BreadTrigger : MonoBehaviour
{
    [Header("Firebase Settings")]
    [Tooltip("The name of this dish as it should appear in the Database (e.g., 'Toast', 'KayaToast').")]
    public string dishName = "Toast"; 

    [Header("Bread Visuals")]
    [Tooltip("The model representing the plain, unbuttered bread.")]
    public GameObject plainBreadModel;

    [Tooltip("The model representing the finished buttered toast.")]
    public GameObject butteredToastModel;

    [Header("Interaction References")]
    [Tooltip("Reference to the Knife script that handles spreading particles/animation.")]
    public KnifeSpread knife;

    [Header("UI Managers")]
    [Tooltip("UI Manager specific to the Toast interaction (shows 'Next' button).")]
    public ToastUIManager toastUI;

    [Tooltip("Main UI Manager that tracks the overall game steps (1/3, 2/3, etc.).")]
    public UIManager progressUI;

    [Header("Configuration")]
    [Tooltip("How many seconds the knife must stay inside the trigger to finish spreading.")]
    public float spreadCompletionTime = 1f;

    // Internal state tracking
    private bool stepCompleted = false;
    private float timer = 0f;

    /// <summary>
    /// Initializes state and locates UI managers if not manually assigned.
    /// </summary>
    private void Awake()
    {
        // Validation check for models
        if (plainBreadModel == null || butteredToastModel == null)
        {
            Debug.LogError("Bread models not assigned in Inspector!");
            return;
        }

        // FIX: Replaced FindObjectOfType with FindFirstObjectByType
        if (toastUI == null)
            toastUI = Object.FindFirstObjectByType<ToastUIManager>();
        
        if (progressUI == null)
            progressUI = Object.FindFirstObjectByType<UIManager>();
    }

    /// <summary>
    /// Locates the Knife script and CHECKS FIREBASE HISTORY.
    /// </summary>
    private void Start()
    {
        // 1. Locate Knife if missing
        if (knife == null)
        {
            knife = Object.FindFirstObjectByType<KnifeSpread>();
            if (knife == null) Debug.LogError("KnifeSpread component not found in the scene!");
        }

        // 2. CHECK FIREBASE HISTORY (The new part!)
        // If Firebase says we already finished "Toast", update visuals immediately.
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsSceneCompleted(dishName))
        {
            stepCompleted = true; // Lock step so we can't do it again
            
            // Force visuals to "Done" state
            if (plainBreadModel != null) plainBreadModel.SetActive(false);
            if (butteredToastModel != null) butteredToastModel.SetActive(true);
            
            Debug.Log($"🍞 Firebase says {dishName} is already done! Loading Toast model.");
        }
        else
        {
            // Ensure correct starting state (Plain Bread)
            if (plainBreadModel != null) plainBreadModel.SetActive(true);
            if (butteredToastModel != null) butteredToastModel.SetActive(false);
        }
    }

    /// <summary>
    /// Checks every frame the knife is inside the bread trigger.
    /// Increments a timer; if timer > spreadCompletionTime, completes the step.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        // Ignore if step is already done or knife is missing
        if (stepCompleted || knife == null) return;

        if (other.CompareTag("Knife"))
        {
            // 1. Activate spreading visuals (particles/trails)
            knife.StartSpreading();

            // 2. Increase timer
            timer += Time.deltaTime;

            // 3. Check if spreading is complete
            if (timer >= spreadCompletionTime)
            {
                CompleteStep();
            }
        }
    }

    /// <summary>
    /// Helper method to handle all completion logic (Visuals, UI, and Firebase).
    /// </summary>
    private void CompleteStep()
    {
        stepCompleted = true;

        // --- 1. Visuals: Switch models ---
        if (plainBreadModel != null) plainBreadModel.SetActive(false);
        if (butteredToastModel != null) butteredToastModel.SetActive(true);

        // --- 2. UI: Update Interfaces ---
        toastUI?.ShowCongrats();       // Show local success UI
        progressUI?.CompleteStep();    // Update global progress

        // --- 3. Firebase: Save Data ---
        if (FirebaseManager.Instance != null)
        {
            // We pass the dishName ("Toast") and the timer duration as 'timeTaken'
            FirebaseManager.Instance.MarkDishComplete(dishName, timer);
        }
        else
        {
            Debug.LogWarning("FirebaseManager Instance not found. Progress not saved to cloud.");
        }

        Debug.Log("Bread switched, UI updated, and Firebase notified!");
    }

    /// <summary>
    /// Resets the spreading action if the knife leaves before the timer finishes.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Knife") && !stepCompleted)
        {
            if (knife != null)
                knife.StopSpreading();
                
            timer = 0f; // Reset progress if they pull away too early
        }
    }
}