using UnityEngine;

/// <summary>
/// Controls the kettle pouring mechanic, including visual rotation, particle effects, 
/// UI instructions, and tracking the pour progress.
/// </summary>
public class KettlePour : MonoBehaviour
{
    [Header("Visuals")]
    public Transform kettleBody;
    public float pourAngle = -90f;
    public float rotateSpeed = 5f;
    public GameObject pourEffect;

    [Header("UI")]
    public GameObject popupCanvas; // "Drag me" instruction popup

    [Header("Pour Settings")]
    public float pourRate = 0.5f; // Progress per second

    [System.NonSerialized]
    public bool isPouring = false;

    private Quaternion originalRot;
    private float currentPourProgress = 0f;

    /// <summary>
    /// Initializes the kettle's state, storing the original rotation and setting initial UI visibility.
    /// </summary>
    void Start()
    {
        originalRot = kettleBody.localRotation;

        // Show instruction popup at the start
        if (popupCanvas != null)
            popupCanvas.SetActive(true);

        // Make sure pour effect is off initially
        if (pourEffect != null)
            pourEffect.SetActive(false);
    }

    /// <summary>
    /// Begins the pouring action. Updates state flags and toggles relevant UI/effects.
    /// </summary>
    public void StartPouring()
    {
        isPouring = true;

        // Hide instruction popup when pouring starts
        if (popupCanvas != null)
            popupCanvas.SetActive(false);

        // Show pour effect
        if (pourEffect != null)
            pourEffect.SetActive(true);
    }

    /// <summary>
    /// Stops the pouring action and disables visual effects.
    /// </summary>
    public void StopPouring()
    {
        isPouring = false;

        // Hide pour effect
        if (pourEffect != null)
            pourEffect.SetActive(false);
    }

    /// <summary>
    /// Handles the per-frame logic for rotating the kettle and accumulating pour progress.
    /// </summary>
    void Update()
    {
        if (isPouring)
        {
            // Rotate kettle smoothly
            Quaternion target = Quaternion.Euler(pourAngle, 0, 0);
            kettleBody.localRotation = Quaternion.Lerp(kettleBody.localRotation, target, Time.deltaTime * rotateSpeed);

            // Update pour progress
            currentPourProgress += pourRate * Time.deltaTime;
            currentPourProgress = Mathf.Clamp01(currentPourProgress);
        }
        else
        {
            // Return kettle to original rotation when not pouring
            kettleBody.localRotation = Quaternion.Lerp(kettleBody.localRotation, originalRot, Time.deltaTime * rotateSpeed);
        }
    }

    /// <summary>
    /// Retrieves the current progress of the pour.
    /// </summary>
    /// <returns>A float value between 0.0 (empty) and 1.0 (full).</returns>
    public float GetPourPercentage()
    {
        return currentPourProgress;
    }

    /// <summary>
    /// Resets the pouring progress, visual rotation, and UI state to their initial values.
    /// </summary>
    public void ResetPour()
    {
        currentPourProgress = 0f;

        // Reset kettle rotation
        kettleBody.localRotation = originalRot;

        // Show instruction popup again
        if (popupCanvas != null)
            popupCanvas.SetActive(true);

        // Hide pour effect
        if (pourEffect != null)
            pourEffect.SetActive(false);

        // Reset pouring state
        isPouring = false;
    }
}