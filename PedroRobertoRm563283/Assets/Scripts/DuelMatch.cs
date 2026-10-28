
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
            Winner = PlayerRef.None;
            BluePlayer = Runner.LocalPlayer;
        }
    }

    public void FinalizarPorPontos(PlayerRef vencedor)
    {
        if (!HasStateAuthority || IsOver)
            return;

        Winner = vencedor;
        IsOver = true;

        Debug.Log("Partida encerrada por pontos!");
    }

    // Mantido para compatibilidade com o DuelHealth antigo.
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReportDefeat(
        PlayerRef defeated,
        RpcInfo info = default)
    {
        // O basquete não encerra a partida por dano.
    }

    public override void Despawned(
        NetworkRunner runner,
        bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }
}
