using Fusion;
using UnityEngine;
public class DuelHealth : NetworkBehaviour
{
    [Networked]
    public int Health { get; set; }
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            Health = 100;
        }
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReceiveDamage(RpcInfo info = default)
    {
        if (!HasStateAuthority)
            return;
        DuelMatch match = DuelMatch.Instance;
        if (match == null || !match.CanPlay)
            return;
        if (Health <= 0)
            return;
        if (info.Source == Object.StateAuthority)
            return;
        Health = Mathf.Max(0, Health - 20);
        Debug.Log(
        "Jogador recebeu dano. Vida: " + Health
        );
        if (Health == 0)
        {
            match.RPC_ReportDefeat(
            Object.StateAuthority
            );
        }
    }
}