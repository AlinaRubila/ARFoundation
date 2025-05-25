using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class AttachManagement : MonoBehaviour
{
    GameObject selectedObj;
    bool attachMode = true;
    public void ChangeMode(bool choice)
    {
        attachMode = choice;
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
            selectedObj = hit.collider.gameObject;
        }
    }

    void Attachment()
    {
        var tracking = selectedObj.GetComponent<TrackingAttach>();
        if (tracking != null)
        {
            tracking.ChangeValue(attachMode);
            tracking.ManageSelection(true);
            if (Input.touchCount > 0)
            {
                Touch screenTouch = Input.GetTouch(0);
                if (screenTouch.phase == TouchPhase.Ended) { tracking.ManageSelection(false); }
            }
            else if (Input.GetMouseButtonUp(0)) {tracking.ManageSelection(false); }
        }
    }
    void Update()
    {
        ObjectSelection();
        Attachment();
    }
}
