using UnityEngine;

public class CupPourTrigger : MonoBehaviour
{
    [Header("Cup Models")]
    public GameObject emptyCupModel;
    public GameObject filledCupModel;

    [Header("Kettle")]
    public KettlePour kettle;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider is the kettle
        if (other.CompareTag("Kettle"))
        {
            // Start the kettle pouring
            if (kettle != null)
                kettle.StartPouring();

            // Swap cup models
            if (emptyCupModel != null && filledCupModel != null)
            {
                emptyCupModel.SetActive(false);
                filledCupModel.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the collider leaving is the kettle
        if (other.CompareTag("Kettle"))
        {
            // Stop the kettle pouring
            if (kettle != null)
                kettle.StopPouring();

            // Swap cup models back
            if (emptyCupModel != null && filledCupModel != null)
            {
                emptyCupModel.SetActive(true);
                filledCupModel.SetActive(false);
            }
        }
    }
}
