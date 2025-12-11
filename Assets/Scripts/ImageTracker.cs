using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracker : MonoBehaviour
{
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    [SerializeField]
    private GameObject[] placeablePrefabs;

    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<GameObject, GameObject> prefabDefaults = new Dictionary<GameObject, GameObject>();

    private void Start()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.AddListener(OnImageChanged);
            SetupPrefabs();
        }
    }

    private void OnDestroy()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnImageChanged);
        }
    }

    void SetupPrefabs()
    {
        foreach (GameObject prefab in placeablePrefabs)
        {
            GameObject newPrefab = Instantiate(prefab);
            newPrefab.name = prefab.name;     // cup / Toast / Tray
            newPrefab.SetActive(false);

            spawnedPrefabs.Add(prefab.name, newPrefab);
            prefabDefaults.Add(newPrefab, prefab);
        }
    }

    void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Added images
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            if (trackedImage != null)
                UpdateImage(trackedImage);
        }

        // Updated images
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            if (trackedImage != null)
                UpdateImage(trackedImage);
        }

        // Removed images
        foreach (var removedPair in eventArgs.removed)
        {
            ARTrackedImage trackedImage = removedPair.Value;

            if (trackedImage != null && trackedImage.referenceImage != null)
            {
                string imageName = trackedImage.referenceImage.name;

                if (spawnedPrefabs.TryGetValue(imageName, out GameObject prefab))
                {
                    prefab.SetActive(false);
                    prefab.transform.SetParent(null);
                }
            }
        }
    }

    void UpdateImage(ARTrackedImage trackedImage)
    {
        if (trackedImage == null || trackedImage.referenceImage == null)
            return;

        string imageName = trackedImage.referenceImage.name;  // cup / Toast / Tray

        if (!spawnedPrefabs.ContainsKey(imageName))
            return;

        GameObject prefab = spawnedPrefabs[imageName];

        // Safety check
        if (prefab == null)
        {
            spawnedPrefabs.Remove(imageName);
            return;
        }

        // tray should only spawn AFTER cup + Toast (2 steps total)
        if (imageName == "fullset")
        {
            int step = UIManager.Instance.GetCurrentStep();

            if (step < 2)
            {
                prefab.SetActive(false);
                UIManager.Instance.ShowMessage("Finish Kopi and Toast first!");
                return;
            }
        }

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            if (prefab.transform.parent != trackedImage.transform)
            {
                prefab.transform.SetParent(trackedImage.transform);

                if (prefabDefaults.ContainsKey(prefab))
                {
                    prefab.transform.localPosition = prefabDefaults[prefab].transform.localPosition;
                    prefab.transform.localRotation = prefabDefaults[prefab].transform.localRotation;
                }
            }

            prefab.SetActive(true);
        }
        else
        {
            prefab.SetActive(false);
            prefab.transform.SetParent(null);
        }
    }
}
