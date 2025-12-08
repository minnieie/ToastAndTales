using UnityEngine;

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

    public void StopPouring()
    {
        isPouring = false;

        // Hide pour effect
        if (pourEffect != null)
            pourEffect.SetActive(false);
    }

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

    // Returns pour percentage (0-1)
    public float GetPourPercentage()
    {
        return currentPourProgress;
    }

    // Reset pour progress (if needed)
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
