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

        var plug = other.GetComponent<Plug>();
        if (plug == null) return;

        // Нельзя затыкать пока игрок держит
        if (plug.SafeIsHeld) return;
        IsClosed = true;
        GameManager.Instance.RegisterHoleClosed();
        Runner.Despawn(plug.Object);
        Runner.Despawn(Object);
    }
}
