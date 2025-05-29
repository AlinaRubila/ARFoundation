using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ImageTracking : MonoBehaviour
{
    [SerializeField] private GameObject[] _placeablePrefabs;
    [SerializeField] private ARTrackedImageManager _imageManager;
    [SerializeField] private AttachManagement _attachManager;
    private Dictionary<string, GameObject> _accessiblePrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string , GameObject> _spawnedObjects = new Dictionary<string , GameObject>();

    private void Awake()
    {
        _imageManager = FindFirstObjectByType<ARTrackedImageManager>();
        foreach (GameObject prefab in _placeablePrefabs) 
        { 
            _accessiblePrefabs.Add(prefab.name, prefab);
        }
    }
    private void OnEnable()
    {
        _imageManager.trackedImagesChanged += ArTrackedImageManagerOnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        _imageManager.trackedImagesChanged -= ArTrackedImageManagerOnTrackedImagesChanged;
    }
    private void ArTrackedImageManagerOnTrackedImagesChanged(ARTrackedImagesChangedEventArgs obj)
    {
        foreach (var imageAdded in obj.added)
        {
            UpdateImage(imageAdded);
        }
        foreach (var imageUpdated in obj.updated)
        {
            UpdateImage(imageUpdated);
        }
    }
    void UpdateImage(ARTrackedImage image)
    {
        string name = image.referenceImage.name;
        if (!_spawnedObjects.ContainsKey(name))
        {
            GameObject newPrefab = Instantiate(_accessiblePrefabs[name], image.transform.position, image.transform.rotation);
            _spawnedObjects.Add(name, newPrefab);
        }
        var tracking = _spawnedObjects[name].GetComponent<TrackingAttach>();
        if (tracking == null) return;
        if (tracking.attachment) 
        {
            _spawnedObjects[name].transform.position = image.transform.position;
            _spawnedObjects[name].transform.rotation = image.transform.rotation;
        }
        if (tracking.isSelected) _attachManager.ObjectSelection(_spawnedObjects[name]);
    }
}
