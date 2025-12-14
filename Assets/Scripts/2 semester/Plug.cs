using Fusion;
using UnityEngine;

public class Plug : NetworkBehaviour
{
    [Networked] public bool IsHeld { get; set; }

    Camera cam;
    Plane dragPlane;

    public override void Spawned()
    {
        cam = Camera.main;
        dragPlane = new Plane(Vector3.up, Vector3.zero);
    }

    void Update()
    {
        if (!Object.HasInputAuthority) return;

        if (Input.GetMouseButtonDown(0))
            TryTake();

        if (Input.GetMouseButton(0) && IsHeld)
            Drag();

        if (Input.GetMouseButtonUp(0) && IsHeld)
            Release();
    }

    void TryTake()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                RPC_SetHeld(true);
            }
        }
    }

    void Drag()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
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
}
