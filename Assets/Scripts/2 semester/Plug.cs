using Fusion;
using UnityEngine;
using UnityEngine.EventSystems;

public class Plug : NetworkBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [Networked] public bool IsHeld { get; set; }

    Camera cam;
    Plane dragPlane;

    public override void Spawned()
    {
        cam = Camera.main;
        dragPlane = new Plane(Vector3.up, Vector3.zero);
    }

    /*void Update()
    {
        if (!Object.HasInputAuthority) return;

        if (Input.GetMouseButtonDown(0))
            TryTake();

        if (Input.GetMouseButton(0) && IsHeld)
            Drag();

        if (Input.GetMouseButtonUp(0) && IsHeld)
            Release();
    }*/

    /*void TryTake()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                RPC_SetHeld(true);
            }
        }
    }*/

    void Drag(PointerEventData eventData)
    {
        Ray ray = cam.ScreenPointToRay(eventData.position);
        if (dragPlane.Raycast(ray, out float enter))
        {
            transform.position = ray.GetPoint(enter);
        }
    }

    void Release()
    {
        RPC_SetHeld(false);
    }

    [Rpc]
    void RPC_SetHeld(bool state)
    {
        IsHeld = state;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Object.HasInputAuthority) return;
        RPC_SetHeld(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!Object.HasInputAuthority) return;
        if (!IsHeld || !eventData.pointerCurrentRaycast.isValid) return;
        Drag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!Object.HasInputAuthority) return;
        Release();
    }
}
