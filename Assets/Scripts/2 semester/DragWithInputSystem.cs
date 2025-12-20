using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkGrab))]
public class DragWithInputSystem : NetworkBehaviour
{
    /*public InputAction positionAction;
    public InputAction pressAction;
    public InputActionAsset inputActionsAsset;*/
    private InputAction pressAction;
    private InputAction positionAction;

    private Camera cam;
    private NetworkGrab grab;
    private bool dragging;

    private Vector3 offset;
    private float zCoord;

    void Awake()
    {
        cam = Camera.main;
        grab = GetComponent<NetworkGrab>();
        pressAction = GameManager.pressAction;
        positionAction = GameManager.positionAction;

    }

    void OnEnable()
    {
        //positionAction.Enable();
        //pressAction.Enable();

        pressAction.started += OnPress;
        pressAction.canceled += OnRelease;
    }

    void OnDisable()
    {
        pressAction.started -= OnPress;
        pressAction.canceled -= OnRelease;

        //positionAction.Disable();
        //pressAction.Disable();
    }

    void Update()
    {
        if (!dragging)
            return;

        if (!Object.HasStateAuthority)
            return;

        Vector2 screenPos = positionAction.ReadValue<Vector2>();
        Vector3 worldPos = ScreenToWorld(screenPos);
        transform.position = worldPos + offset;
    }

    private void OnPress(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = positionAction.ReadValue<Vector2>();

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out var hit))
            return;

        if (hit.transform != transform)
            return;

        if (!grab.IsFree && !grab.IsHeldBy(Runner.LocalPlayer))
            return;

        grab.RPC_RequestGrab(Runner.LocalPlayer);

        zCoord = cam.WorldToScreenPoint(transform.position).z;
        offset = transform.position - ScreenToWorld(screenPos);

        dragging = true;
    }

    private void OnRelease(InputAction.CallbackContext ctx)
    {
        if (!dragging)
            return;

        dragging = false;

        if (grab.IsHeldBy(Runner.LocalPlayer))
        {
            grab.RPC_ReleaseGrab(Runner.LocalPlayer);
        }
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        return cam.ScreenToWorldPoint(new Vector3(
            screenPos.x,
            screenPos.y,
            zCoord
        ));
    }
}
