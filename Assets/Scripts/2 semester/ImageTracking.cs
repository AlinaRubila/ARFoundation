using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

public class ImageTracking : MonoBehaviour
{
    //[SerializeField] private GameObject _placeablePrefab;
    [SerializeField] private NetworkObject planePrefab;
    //private GameObject _spawnedObject;
    private NetworkRunner runner;
    NetworkObject spawned;
    [SerializeField] private ARTrackedImageManager _imageManager;
    bool isSpawned = false;

    private void Awake()
    {
        runner = FindFirstObjectByType<NetworkRunner>();
        _imageManager = FindFirstObjectByType<ARTrackedImageManager>();
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
        if (runner == null || planePrefab == null) return;
        if (!runner.IsSharedModeMasterClient) return;

        if (runner.ActivePlayers.Count() < 2) return;
        Vector3 pos = image.transform.position;
        Quaternion rot = image.transform.rotation;
        if (!isSpawned)
        {
            //GameObject newPrefab = Instantiate(_placeablePrefab, image.transform.position, image.transform.rotation);
            //_spawnedObject = newPrefab;
            var netObj = runner.Spawn(planePrefab, pos, rot);
            Debug.Log("Spawned netObj!");
            if (netObj != null) spawned = netObj;
            isSpawned = true;
        }
        spawned.transform.SetPositionAndRotation(pos, rot);
        //_spawnedObject.transform.position = image.transform.position;
        //_spawnedObject.transform.rotation = image.transform.rotation;
    }
}
