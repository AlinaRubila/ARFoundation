using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class AttachManagement : MonoBehaviour
{
    GameObject selectedObj;
    bool attachMode = true;
    public void ChangeMode(bool choice)
    {
        attachMode = choice;
    }

    public void ObjectSelection(GameObject go)
    {
        selectedObj = go;
    }

    void Attachment()
    {
         if (selectedObj == null) return;
        var tracking = selectedObj.GetComponent<TrackingAttach>();
        if (tracking != null)
        {
            tracking.ChangeValue(attachMode);
            tracking.ManageSelection(false);
		selectedObj = null;
        }
    }
    void Update()
    {
            Attachment();
    }
}
