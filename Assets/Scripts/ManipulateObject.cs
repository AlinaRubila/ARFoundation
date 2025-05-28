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
                Vector2 touchDifference = (screenTouch2.position + screenTouch1.position) / 2;
                if (screenTouch1.phase == TouchPhase.Moved || screenTouch2.phase == TouchPhase.Moved) 
                    transform.Rotate(new Vector3(touchDifference.y, touchDifference.x, 0f) * 100f * Time.deltaTime);
            }
            else if (Input.GetMouseButton(0))
            {
                float rotationX = Input.GetAxis("Mouse X");
                float rotationY = Input.GetAxis("Mouse Y");
                transform.Rotate(new Vector3(rotationY, rotationX, 0) * 100f * Time.deltaTime);
            }
        }
    }

    /*void Movement()
    {
        if (selected)
        {
            if (Input.touchCount == 1)
            {
                Touch screenTouch = Input.GetTouch(0);
                if (screenTouch.phase == TouchPhase.Moved) 
                {
                    Vector3 move = -transform.right * screenTouch.deltaPosition.y + transform.forward *  screenTouch.deltaPosition.x;
                    transform.Translate(move * 1 * Time.deltaTime);
                }
            }
            else if (Input.GetMouseButton(0))
            {
                float movementX = Input.GetAxis("Horizontal");
                float movementZ = Input.GetAxis("Vertical");
                Vector3 move = -transform.right * movementZ + transform.forward * movementX;
                transform.Translate(move * 1 * Time.deltaTime);
            }
        }
    }*/
    void Update()
    {
        Rotation();
        //Movement();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");
        clicked = true;
        selected = true;
        selection.GetComponent<MeshRenderer>().enabled = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!clicked) return;
        Debug.Log("OnPointerUp");
        selected = false;
        selection.GetComponent<MeshRenderer>().enabled = false;
        clicked = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log((Vector3)eventData.position);
        transform.localPosition += new Vector3(eventData.delta.y / 1000, 0, -eventData.delta.x / 1000) ;
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
