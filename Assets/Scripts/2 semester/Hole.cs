using Fusion;
using UnityEngine;

public class Hole : NetworkBehaviour
{
    [Networked] public bool IsClosed { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        if (IsClosed) return;

        var plug = other.GetComponent<Plug>();
        if (plug == null) return;

        // Нельзя затыкать пока игрок держит
        if (plug.IsHeld) return;

        Runner.Despawn(plug.Object);
        Runner.Despawn(Object);

        IsClosed = true;

        GameManager.Instance.RegisterHoleClosed();

    }
}
