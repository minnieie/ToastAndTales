using UnityEngine;

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

    void Start()
    {
        originalRot = knifeBody.localRotation;
        targetRot = originalRot;

        // Show instruction popup at the start
        if (popupCanvas != null)
            popupCanvas.SetActive(true);
    }

    public void StartSpreading()
    {
        isSpreading = true;

        // Set target rotation relative to original rotation
        targetRot = originalRot * Quaternion.Euler(spreadAngle, 0, 0);

        // Hide instruction popup when spreading starts
        if (popupCanvas != null)
            popupCanvas.SetActive(false);
    }

    public void StopSpreading()
    {
        isSpreading = false;
        targetRot = originalRot; // rotate back smoothly
    }

    void Update()
    {
        // Smoothly rotate knife towards target rotation
        knifeBody.localRotation = Quaternion.Lerp(knifeBody.localRotation, targetRot, Time.deltaTime * rotateSpeed);
    }

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
