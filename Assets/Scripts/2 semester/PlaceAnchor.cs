using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceAnchor : PressInputBase
{
    [SerializeField]
    GameObject field;
    ARRaycastManager m_RaycastManager;
    List<GameObject> anchors = new List<GameObject>();
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    public bool isHost = true;
    bool isFixated = false;
    bool m_Pressed;

    protected override void Awake()
    {
        base.Awake();
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }
    private void Update()
    {
        if (Pointer.current == null || m_Pressed == false)
            return;

        var touchPosition = Pointer.current.position.ReadValue();
        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon) && isHost && !isFixated)
        {
            var hitPose = s_Hits[0].pose;
            SpawnAnchor(hitPose);
        }
    }

    void SpawnAnchor(Pose hitPose)
    {
        foreach (var anchor in anchors)
        {
            Destroy(anchor);
        }
        anchors.Clear();
        GameObject anchorGameObject = new GameObject("Anchor");
        anchorGameObject.transform.position = hitPose.position;
        anchorGameObject.AddComponent<ARAnchor>();
        anchors.Add(anchorGameObject);
        GameObject fieldInstance = Instantiate(field, hitPose.position, Quaternion.identity);
        fieldInstance.transform.parent = anchorGameObject.transform;
    }
    protected override void OnPress(Vector3 position) => m_Pressed = true;

    protected override void OnPressCancel() => m_Pressed = false;
    public void ChangeFixation()
    {
        isFixated = !isFixated;
    }
}
