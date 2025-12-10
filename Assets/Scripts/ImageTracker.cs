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
            newPrefab.name = prefab.name;
            newPrefab.SetActive(false);

            spawnedPrefabs.Add(prefab.name, newPrefab);
            prefabDefaults.Add(newPrefab, prefab);
        }
    }

    void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Handle newly detected images
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            if (trackedImage != null)
                UpdateImage(trackedImage);
        }

        // Handle updated tracking state
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            if (trackedImage != null)
                UpdateImage(trackedImage);
        }

        // Handle removed images separately - removed returns KeyValuePairs
        foreach (var removedPair in eventArgs.removed)
        {
            ARTrackedImage trackedImage = removedPair.Value;
            // Don't try to access the destroyed trackedImage
            // Just deactivate prefabs based on their known state
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

        string imageName = trackedImage.referenceImage.name;

        if (!spawnedPrefabs.ContainsKey(imageName))
            return;

        GameObject prefab = spawnedPrefabs[imageName];

        // **Check if prefab was destroyed**
        if (prefab == null)
        {
            // Remove from dictionary so we don't try again
            spawnedPrefabs.Remove(imageName);
            return;
        }

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            // Attach prefab to tracked image transform
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
