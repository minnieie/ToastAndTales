using UnityEngine;

public class TraySwapTrigger : MonoBehaviour
{
    [Header("Global Progress")]
    public UIManager progressUI;

    [Header("The Perfect Static Models (Child of Tray)")]
    // Assign the "perfect" versions that are already sitting nicely on the tray
    public GameObject staticKopiModel; 
    public GameObject staticToastModel;

    private bool hasKopi = false;
    private bool hasToast = false;
    private bool stepCompleted = false;

    private void Awake()
    {
        // 1. Find UI
        if (progressUI == null)
            progressUI = Object.FindFirstObjectByType<UIManager>();

        // 2. Hide the perfect models at start (so tray looks empty)
        if (staticKopiModel != null) staticKopiModel.SetActive(false);
        if (staticToastModel != null) staticToastModel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (stepCompleted) return;

        // --- KOPI LOGIC ---
        if (other.CompareTag("Kopi") && !hasKopi)
        {
            // 1. Hide the messy physics object the player threw
            other.gameObject.SetActive(false);

            // 2. Show the perfect static model
            if (staticKopiModel != null) staticKopiModel.SetActive(true);

            hasKopi = true;
            Debug.Log("Kopi Plated!");
            CheckWin();
        }
        
        // --- TOAST LOGIC ---
        else if (other.CompareTag("Toast") && !hasToast)
        {
            // 1. Hide the physics object
            other.gameObject.SetActive(false);

            // 2. Show the perfect static model
            if (staticToastModel != null) staticToastModel.SetActive(true);

            hasToast = true;
            Debug.Log("Toast Plated!");
            CheckWin();
        }
    }

    private void CheckWin()
    {
        if (hasKopi && hasToast)
        {
            stepCompleted = true;
            Debug.Log("🎉 Full Set Served!");
            if (progressUI != null) progressUI.CompleteStep();
        }
    }
}