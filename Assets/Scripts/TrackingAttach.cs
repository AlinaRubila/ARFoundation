using System.Net.Mail;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrackingAttach : MonoBehaviour, IPointerClickHandler, IPointerUpHandler
{
    private bool isAttached = true;
    public GameObject attachSelect;
    public GameObject detachSelect;
    GameObject currentSelection;
    public bool isSelected { get; private set; } = false;
    public bool attachment
    {
        get {return isAttached;}
        private set { }
    }
    public void ChangeValue(bool value)
    {
        isAttached = value;
        if (isAttached) { currentSelection = detachSelect; }
        else { currentSelection = attachSelect; }
    }
    public void ManageSelection(bool mode)
    {
        currentSelection.GetComponent<MeshRenderer>().enabled = mode;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isSelected = true;
        ManageSelection(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isSelected = false;
        ManageSelection(false);
    }
}
