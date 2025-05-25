using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UIElements;

public class ManipulateObject : MonoBehaviour
{
    bool isAllowed = false;
    public GameObject selection;
    public void EnableManipulation()
    {
        isAllowed = true;
        if (selection != null)
        {
            selection.GetComponent<MeshRenderer>().enabled = true;
        }
    }
    void Rotation()
    {
        if (isAllowed)
        {
            if (Input.touchCount == 2)
            {
                Touch screenTouch1 = Input.GetTouch(0);
                Touch screenTouch2 = Input.GetTouch(1);
                Vector2 touchDifference = screenTouch2.position - screenTouch1.position;
                if (screenTouch1.phase == TouchPhase.Moved || screenTouch2.phase == TouchPhase.Moved) 
                    transform.Rotate(new Vector3(touchDifference.y, touchDifference.x, 0f) * 100f * Time.deltaTime);
                if (screenTouch1.phase == TouchPhase.Ended && screenTouch2.phase == TouchPhase.Ended)
                {
                    isAllowed = false;
                    selection.GetComponent<MeshRenderer>().enabled = false;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                float rotationX = Input.GetAxis("Mouse X");
                float rotationY = Input.GetAxis("Mouse Y");
                transform.Rotate(new Vector3(rotationY, rotationX, 0) * 100f * Time.deltaTime);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isAllowed = false;
                selection.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    void Movement()
    {
        if (isAllowed)
        {
            if (Input.touchCount == 1)
            {
                Touch screenTouch = Input.GetTouch(0);
                if (screenTouch.phase == TouchPhase.Moved) 
                {
                    Vector3 move = -transform.right * screenTouch.deltaPosition.y + transform.forward *  screenTouch.deltaPosition.x;
                    //transform.position += move * 1 * Time.deltaTime;
                    transform.Translate(move * 1 * Time.deltaTime);
                }
                if (screenTouch.phase == TouchPhase.Ended )
                {
                    isAllowed = false;
                    selection.GetComponent<MeshRenderer>().enabled = false;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                float movementX = Input.GetAxis("Horizontal");
                float movementZ = Input.GetAxis("Vertical");
                Vector3 move = -transform.right * movementZ + transform.forward * movementX;
                //transform.position += move * 1 * Time.deltaTime;
                transform.Translate(move * 1 * Time.deltaTime);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isAllowed = false;
                selection.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
    void Update()
    {
        Rotation();
        Movement();
    }
}
