using Fusion;
using UnityEngine;

public class Hole : NetworkBehaviour
{
    [Networked] public bool IsClosed { get; set; }
    bool IsSpawned = false;
    public override void Spawned()
    {
        base.Spawned();
        IsSpawned = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsSpawned) return;
        if (IsClosed) return;
        if (!Object.HasStateAuthority) return;

        var plug = other.GetComponent<NetworkGrab>();
        if (plug == null) return;
        Debug.Log("OnTriggerEnter!");

        // Нельзя затыкать пока игрок держит
        //if (plug.SafeIsHeld) return;
        IsClosed = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterHoleClosed();
        }
        //GameManager.Instance.RegisterHoleClosed();
        //Runner.Despawn(plug.Object);
        //Runner.Despawn(Object);
        if (plug.Object != null && plug.Object.IsValid)
        {
            Runner.Despawn(plug.Object);
        }

        if (Object != null && Object.IsValid)
        {
            Runner.Despawn(Object);
        }
    }
}
