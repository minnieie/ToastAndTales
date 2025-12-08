using UnityEngine;

public class CupPourTrigger : MonoBehaviour
{
    [Header("Cup Models")]
    public GameObject emptyCupModel;
    public GameObject filledCupModel;

    [Header("Kettle")]
    public KettlePour kettle;

    [Header("Cup UI Manager")]
    public CupUIManager uiManager;

    [Header("Main UI Manager (Progress UI)")]
    public UIManager progressUI;  

    private bool stepCompleted = false;

    private void Awake()
    {
        if (progressUI == null)
            progressUI = FindObjectOfType<UIManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Kettle") && !stepCompleted)
        {
            stepCompleted = true;  // prevent double counting

            if (kettle != null)
                kettle.StartPouring();

            if (emptyCupModel != null && filledCupModel != null)
            {
                emptyCupModel.SetActive(false);
                filledCupModel.SetActive(true);

                if (uiManager != null)
                    uiManager.ShowCongrats();

                if (progressUI != null)
                    progressUI.CompleteStep();  // now increments only once
            }
        }
}

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Kettle"))
        {
            if (kettle != null)
                kettle.StopPouring();

            if (emptyCupModel != null && filledCupModel != null)
            {
                emptyCupModel.SetActive(true);
                filledCupModel.SetActive(false);
            }
        }
    }
}

