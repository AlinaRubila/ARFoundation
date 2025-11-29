using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AnimalsTracking : MonoBehaviour
{
    [SerializeField] private GameObject[] _placeablePrefabs;
    [SerializeField] private AudioClip[] _clips;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] private ARTrackedImageManager _imageManager;
    private Dictionary<string, AudioClip> _accessibleClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, GameObject> _accessiblePrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _spawnedObjects = new Dictionary<string, GameObject>();

    private void Awake()
    {
        _imageManager = FindFirstObjectByType<ARTrackedImageManager>();
        foreach (GameObject prefab in _placeablePrefabs)
        {
            _accessiblePrefabs.Add(prefab.name, prefab);
        }
        foreach (AudioClip clip in _clips) _accessibleClips.Add(clip.name, clip);
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
            _audioSource.clip = _accessibleClips[name];
            _audioSource.Play();
            }
        _spawnedObjects[name].transform.position = image.transform.position;
        _spawnedObjects[name].transform.rotation = image.transform.rotation;
    }
}
