using UnityEngine;

/// <summary>
/// Controls the visual behavior of the knife during the spreading interaction, 
/// handling smooth rotation and UI instruction visibility.
/// </summary>
public class KnifeSpread : MonoBehaviour
{
    [Header("Visuals")]
    public Transform knifeBody;
    public float spreadAngle = 30f; // Angle to rotate relative to original
    public float rotateSpeed = 5f;

    [Header("UI")]
    public GameObject popupCanvas; // "Spread butter" instruction popup

    [System.NonSerialized]
    public bool isSpreading = false;

    private Quaternion originalRot;
    private Quaternion targetRot;

    /// <summary>
    /// Initializes the knife's state by storing the starting rotation and ensuring instructions are visible.
    /// </summary>
    void Start()
    {
        originalRot = knifeBody.localRotation;
        targetRot = originalRot;

        // Show instruction popup at the start
        if (popupCanvas != null)
            popupCanvas.SetActive(true);
    }

    /// <summary>
    /// Begins the spreading animation by setting the target rotation and hiding instructions.
    /// </summary>
    public void StartSpreading()
    {
        isSpreading = true;

        // Set target rotation relative to original rotation
        targetRot = originalRot * Quaternion.Euler(spreadAngle, 0, 0);

        // Hide instruction popup when spreading starts
        if (popupCanvas != null)
            popupCanvas.SetActive(false);
    }

    /// <summary>
    /// Stops the spreading animation and targets the original resting rotation.
    /// </summary>
    public void StopSpreading()
    {
        isSpreading = false;
        targetRot = originalRot; // rotate back smoothly
    }

    /// <summary>
    /// Smoothly interpolates the knife's rotation towards the current target rotation every frame.
    /// </summary>
    void Update()
    {
        // Smoothly rotate knife towards target rotation
        knifeBody.localRotation = Quaternion.Lerp(knifeBody.localRotation, targetRot, Time.deltaTime * rotateSpeed);
    }

    /// <summary>
    /// Resets the knife's rotation and state to the default starting conditions.
    /// </summary>
    // Optional: reset knife to original rotation
    public void ResetKnife()
    {
        knifeBody.localRotation = originalRot;
        targetRot = originalRot;
        isSpreading = false;

        if (popupCanvas != null)
            popupCanvas.SetActive(true);
    }
}