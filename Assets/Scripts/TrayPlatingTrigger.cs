using UnityEngine;

/// <summary>
/// Manages the "Plating" mechanic for the final step.
/// Detects when the player places the interactive "Kopi" and "Toast" onto the tray,
/// hides them, and reveals static/perfectly positioned models to make the tray look neat.
/// </summary>
public class TraySwapTrigger : MonoBehaviour
{
    [Header("Global Progress")]
    [Tooltip("Reference to the main UI Manager. Used to trigger the final 'Step Complete' or Victory logic.")]
    public UIManager progressUI;

    [Header("The Perfect Static Models (Child of Tray)")]
    [Tooltip("The static visual model of the Kopi attached to the Tray. Starts hidden and appears when plated.")]
    public GameObject staticKopiModel; 

    [Tooltip("The static visual model of the Toast attached to the Tray. Starts hidden and appears when plated.")]
    public GameObject staticToastModel;

    // Internal state tracking to prevent double-counting
    private bool hasKopi = false;
    private bool hasToast = false;
    private bool stepCompleted = false;

    /// <summary>
    /// Initializes references and ensures the "perfect" static models are hidden 
    /// so the tray appears empty at the start of the scene.
    /// </summary>
    private void Awake()
    {
        // 1. Find UI if not assigned manually
        if (progressUI == null)
            progressUI = Object.FindFirstObjectByType<UIManager>();

        // 2. Hide the perfect models at start (so tray looks empty)
        if (staticKopiModel != null) staticKopiModel.SetActive(false);
        if (staticToastModel != null) staticToastModel.SetActive(false);
    }

    /// <summary>
    /// Detects collision with "Kopi" or "Toast" objects.
    /// Swaps the interactive physics object for the static model.
    /// </summary>
    /// <param name="other">The collider of the object entering the tray's trigger zone.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Stop processing if the tray is already fully complete
        if (stepCompleted) return;

        // --- KOPI LOGIC ---
        // Check if the object is tagged "Kopi" and we haven't plated it yet
        if (other.CompareTag("Kopi") && !hasKopi)
        {
            // 1. Hide the messy physics object the player threw/placed
            other.gameObject.SetActive(false);

            // 2. Show the perfect static model attached to the tray
            if (staticKopiModel != null) staticKopiModel.SetActive(true);

            hasKopi = true;
            Debug.Log("Kopi Plated!");
            
            // Check if this was the last item needed
            CheckWin();
        }
        
        // --- TOAST LOGIC ---
        // Check if the object is tagged "Toast" and we haven't plated it yet
        else if (other.CompareTag("Toast") && !hasToast)
        {
            // 1. Hide the physics object
            other.gameObject.SetActive(false);

            // 2. Show the perfect static model
            if (staticToastModel != null) staticToastModel.SetActive(true);

            hasToast = true;
            Debug.Log("Toast Plated!");

            // Check if this was the last item needed
            CheckWin();
        }
    }

    /// <summary>
    /// Checks if both the Kopi and Toast have been successfully plated.
    /// If both are present, marks the step as complete and notifies the UIManager.
    /// </summary>
    private void CheckWin()
    {
        if (hasKopi && hasToast)
        {
            stepCompleted = true;
            Debug.Log("Full Set Served!");
            
            // Notify the Global UI Manager that this task (and likely the game) is done
            if (progressUI != null) progressUI.CompleteStep();
        }
    }
}