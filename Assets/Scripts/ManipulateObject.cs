using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ManipulateObject : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public GameObject selection;
    public bool selected { get; private set; } = false;
    bool clicked = false;
    void Rotation()
    {
        if (selected)
        {
            if (Input.touchCount == 2)
            {
                Touch screenTouch1 = Input.GetTouch(0);
                Touch screenTouch2 = Input.GetTouch(1);
                //Vector2 touchDifference = (screenTouch2.position + screenTouch1.position) / 2;
                Vector2 touchDifference = screenTouch2.deltaPosition;
                if (screenTouch1.phase == TouchPhase.Moved || screenTouch2.phase == TouchPhase.Moved)
                    transform.Rotate(new Vector3(touchDifference.x, touchDifference.y,  0f) * 10f * Time.deltaTime);
            }
            else if (Input.GetMouseButton(0))
            {
                float rotationX = Input.GetAxis("Mouse X");
                float rotationY = Input.GetAxis("Mouse Y");
                transform.Rotate(new Vector3(rotationY, rotationX, 0) * Time.deltaTime);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clicked = true;
        selected = true;
        selection.GetComponent<MeshRenderer>().enabled = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!clicked) return;
        selected = false;
        selection.GetComponent<MeshRenderer>().enabled = false;
        clicked = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Input.touchCount == 2) 
        {
            Rotation();
            return; 
        }
        transform.localPosition += new Vector3(eventData.delta.x / 1000, 0, eventData.delta.y / 1000) ;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        selected = true;
        selection.GetComponent<MeshRenderer>().enabled = true;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        selected = false;
        selection.GetComponent<MeshRenderer>().enabled = false;
    }

   
}
