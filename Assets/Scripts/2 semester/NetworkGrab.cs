using Fusion;
using UnityEngine;

public class NetworkGrab : NetworkBehaviour
{
    [Networked] public PlayerRef Holder { get; private set; }

    public bool IsFree => Holder == PlayerRef.None;
    public bool IsHeldBy(PlayerRef player) => Holder == player;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Holder = PlayerRef.None;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestGrab(PlayerRef requester)
    {
        if (!IsFree)
            return;

        Holder = requester;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReleaseGrab(PlayerRef requester)
    {
        if (Holder != requester)
            return;

        Holder = PlayerRef.None;
    }
}
