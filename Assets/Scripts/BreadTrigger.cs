using UnityEngine;

public class BreadTrigger : MonoBehaviour
{
    [Header("Bread Models")]
    public GameObject plainBreadModel;     
    public GameObject butteredToastModel;  

    [Header("Knife")]
    public KnifeSpread knife;

    [Header("UI Managers")]
    public ToastUIManager toastUI;    // Updates text and shows button
    public UIManager progressUI;      // Updates overall progress steps

    [Header("Timing")]
    public float spreadCompletionTime = 1f; // Time in seconds before bread switches

    private bool stepCompleted = false;
    private float timer = 0f;

    private void Awake()
    {
        if (plainBreadModel == null || butteredToastModel == null)
        {
            Debug.LogError("Bread models not assigned!");
            return;
        }

        plainBreadModel.SetActive(true);
        butteredToastModel.SetActive(false);

        // Find UI managers if not assigned
        if (toastUI == null)
            toastUI = FindObjectOfType<ToastUIManager>();
        if (progressUI == null)
            progressUI = FindObjectOfType<UIManager>();
    }

    private void Start()
    {
        if (knife == null)
        {
            knife = FindObjectOfType<KnifeSpread>();
            if (knife == null)
                Debug.LogError("KnifeSpread component not found!");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (stepCompleted || knife == null) return;

        if (other.CompareTag("Knife"))
        {
            // Activate spreading visuals
            knife.StartSpreading();

            // Increase timer while knife is inside
            timer += Time.deltaTime;

            if (timer >= spreadCompletionTime)
            {
                stepCompleted = true;

                // Switch bread models
                if (plainBreadModel != null) plainBreadModel.SetActive(false);
                if (butteredToastModel != null) butteredToastModel.SetActive(true);

                // Update UI
                toastUI?.ShowCongrats();        // Update toast-specific text/button
                progressUI?.CompleteStep();      // Increment overall progress

                Debug.Log("Bread switched and UI managers updated!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Knife") && !stepCompleted)
        {
            knife.StopSpreading();
            timer = 0f; // reset timer if knife leaves
        }
    }
}
