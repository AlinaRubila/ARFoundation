using System.Net.Mail;
using UnityEngine;

public class TrackingAttach : MonoBehaviour
{
    private bool isAttached = true;
    public GameObject attachSelect;
    public GameObject detachSelect;
    GameObject currentSelection;
    public bool attachment
    {
        get {return isAttached;}
        private set { }
    }
    public void ChangeValue(bool value)
    {
        isAttached = value;
        if (isAttached) { currentSelection = attachSelect; }
        else { currentSelection = detachSelect; }
    }
    public void ManageSelection(bool mode)
    {
        currentSelection.GetComponent<MeshRenderer>().enabled = mode;
    }
}
