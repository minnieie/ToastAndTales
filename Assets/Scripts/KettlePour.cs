using UnityEngine;

public class KettlePour : MonoBehaviour
{
    public Transform kettleBody;
    public float pourAngle = -90f;
    public float rotateSpeed = 5f;
    public GameObject pourEffect;

    private bool isPouring = false;
    private Quaternion originalRot;

    void Start()
    {
        originalRot = kettleBody.localRotation;
    }

    public void StartPouring()
    {
        isPouring = true;
        if (pourEffect != null)
            pourEffect.SetActive(true);
    }

    public void StopPouring()
    {
        isPouring = false;
        if (pourEffect != null)
            pourEffect.SetActive(false);
    }

    void Update()
    {
        if (isPouring)
        {
            Quaternion target = Quaternion.Euler(pourAngle, 0, 0);
            kettleBody.localRotation = Quaternion.Lerp(kettleBody.localRotation, target, Time.deltaTime * rotateSpeed);
        }
        else
        {
            kettleBody.localRotation = Quaternion.Lerp(kettleBody.localRotation, originalRot, Time.deltaTime * rotateSpeed);
        }
    }
}
