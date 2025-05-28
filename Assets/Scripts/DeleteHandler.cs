using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using NUnit.Framework;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARSubsystems;

public class DeleteHandler : MonoBehaviour
{
    bool isAllowed = false;
    GameObject choosenObject;
    List<GameObject> _placedObjects = new List<GameObject>();
    public bool allowance { 
        get { return isAllowed; }
        private set { }
    }
    public void AddObject(GameObject obj)
    {
        _placedObjects.Add(obj);
    }
    public void ManageAllowment(bool allow)
    {
        isAllowed = allow;
    }
    void ObjectSelection()
    {
        foreach (var obj in _placedObjects) 
        { 
            var m = obj.GetComponent<ManipulateObject>();
            if (m.selected) 
            {
                choosenObject = obj;
                break;
            }
        }
        if (choosenObject == null) return;
        if (isAllowed) { DeleteObject(); }
    }

    public void DeleteObject()
    {
        if (choosenObject != null)
        {
            _placedObjects.Remove(choosenObject);
            Destroy(choosenObject);
            choosenObject = null;
        }
    }
    void Update()
    {
        ObjectSelection();
    }
}
