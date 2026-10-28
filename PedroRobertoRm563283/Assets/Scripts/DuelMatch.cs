using Fusion;
using UnityEngine;
public class DuelMatch : NetworkBehaviour
{
    public static DuelMatch Instance { get; private set; }
    [Networked]
    public NetworkBool IsOver { get; set; }
    [Networked]
    public PlayerRef Winner { get; set; }
    [Networked]
    public PlayerRef BluePlayer { get; set; }
    public bool CanPlay
    {
        get
        {
            if (IsOver)
                return false;
            int connectedPlayers = 0;
            foreach (PlayerRef player in Runner.ActivePlayers)
            {
                connectedPlayers++;
            }
            return connectedPlayers == 2;
        }
    }
    public override void Spawned()
    {
        Instance = this;
        if (HasStateAuthority)
        {
            IsOver = false;
            BluePlayer = Runner.LocalPlayer;
        }
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReportDefeat(
    PlayerRef defeated,
    RpcInfo info = default)
    {
        if (!HasStateAuthority || IsOver)
            return;
        if (!CanPlay)
            return;
        if (info.Source != defeated)
            return;
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            if (player == defeated)
                continue;
            Winner = player;
            IsOver = true;
            Debug.Log("Partida encerrada!");
            break;
        }
    }
    public override void Despawned(
    NetworkRunner runner,
    bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }
}