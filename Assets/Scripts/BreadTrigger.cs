using UnityEngine;

/// <summary>
/// Handles the interaction between a Knife and Bread.
/// Uses a timer in OnTriggerStay to require the player to "spread" for a set duration
/// before switching the model from Plain Bread to Buttered Toast.
/// </summary>
public class BreadTrigger : MonoBehaviour
{
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

        // Ensure correct starting state
        plainBreadModel.SetActive(true);
        butteredToastModel.SetActive(false);

        // FIX: CS0618 - Replaced FindObjectOfType with FindFirstObjectByType
        if (toastUI == null)
            toastUI = Object.FindFirstObjectByType<ToastUIManager>();
        
        if (progressUI == null)
            progressUI = Object.FindFirstObjectByType<UIManager>();
    }

    /// <summary>
    /// Locates the Knife script if not assigned.
    /// </summary>
    private void Start()
    {
        if (knife == null)
        {
            // FIX: CS0618 - Replaced FindObjectOfType with FindFirstObjectByType
            knife = Object.FindFirstObjectByType<KnifeSpread>();
            
            if (knife == null)
                Debug.LogError("KnifeSpread component not found in the scene!");
        }
    }

    /// <summary>
    /// Checks every frame the knife is inside the bread trigger.
    /// Increments a timer; if timer > spreadCompletionTime, completes the step.
    /// </summary>
    /// <param name="other">The collider inside the trigger.</param>
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
                stepCompleted = true;

                // Switch models
                if (plainBreadModel != null) plainBreadModel.SetActive(false);
                if (butteredToastModel != null) butteredToastModel.SetActive(true);

                // Update UI
                toastUI?.ShowCongrats();       // Show local success UI
                progressUI?.CompleteStep();    // Update global progress

                Debug.Log("Bread switched and UI managers updated!");
            }
        }
    }

    /// <summary>
    /// Resets the spreading action if the knife leaves before the timer finishes.
    /// </summary>
    /// <param name="other">The collider exiting the trigger.</param>
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