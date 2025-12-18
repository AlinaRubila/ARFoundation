using Fusion;
using UnityEngine;
using UnityEngine.EventSystems;

public class Plug : NetworkBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [Networked] public bool IsHeld { get; set; }
    [Networked] public Vector3 NetworkPosition { get; set; }

    Camera cam;
    Plane dragPlane;

    public bool SafeIsHeld => Object != null && Object.IsValid && IsHeld;
    public override void Spawned()
    {
        cam = Camera.main;
        dragPlane = new Plane(Vector3.up, transform.position);
        NetworkPosition = transform.position;
    }
    public override void Render()
    {
        transform.position = NetworkPosition;
    }
    void Drag(PointerEventData eventData)
    {
        Ray ray = cam.ScreenPointToRay(eventData.position);
        if (dragPlane.Raycast(ray, out float enter))
        {
            //transform.position = ray.GetPoint(enter);
            Vector3 pos = ray.GetPoint(enter);
            if (Object.HasStateAuthority)
                NetworkPosition = pos;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Object.HasInputAuthority) return;
        if (IsHeld) return;
        dragPlane = new Plane(Vector3.up, transform.position);
        RPC_SetHeld(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!Object.HasInputAuthority || !IsHeld) return;
        Drag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!Object.HasInputAuthority) return;
        RPC_SetHeld(false);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetHeld(bool state)
    {
        IsHeld = state;
    }
}
