using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using NUnit.Framework;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;

public class DeleteHandler : MonoBehaviour
{
    bool isAllowed = false;
    GameObject choosenObject;
    List<GameObject> placedObjects = new List<GameObject>();
    public bool allowance { 
        get { return isAllowed; }
        private set { }
    }

    public void PlaceNew(GameObject obj)
    {
        placedObjects.Add(obj);
    }
    public void ManageAllowment(bool allow)
    {
        isAllowed = allow;
    }

    void ObjectSelection()
    {
        Ray ray;
        RaycastHit hit;
        if (EventSystem.current.IsPointerOverGameObject()) 
            return;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
                ray = Camera.main.ScreenPointToRay(touch.position);
        }
        else ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (placedObjects.Contains(hit.collider.gameObject))
            {
                choosenObject = hit.collider.gameObject;
                var manipulation = hit.collider.GetComponent<ManipulateObject>();
                manipulation.EnableManipulation();
            }
        }
        if (isAllowed) { DeleteObject(); }
    }

    public void DeleteObject()
    {
        if (choosenObject != null)
        {
            Destroy(choosenObject);
            choosenObject = null;
            placedObjects.Remove(choosenObject);
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0) ObjectSelection();
    }
}
