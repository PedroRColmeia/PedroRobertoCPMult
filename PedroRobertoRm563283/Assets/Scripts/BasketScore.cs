
using Fusion;
using UnityEngine;

public class BasketScore : NetworkBehaviour
{
    public static BasketScore Instance { get; private set; }

    [Header("Bola")]
    [SerializeField] private NetworkObject basketballPrefab;

    [Header("Arremesso")]
    [SerializeField] private float velocidade = 12f;
    [SerializeField] private float angulo = 55f;
    [SerializeField] private float cooldown = 3f;

    [Networked] public int BluePoints { get; set; }
    [Networked] public int RedPoints { get; set; }

    [Networked] public NetworkBool GameOver { get; set; }
    [Networked] public PlayerRef Winner { get; set; }

    [Networked] private TickTimer CooldownAzul { get; set; }
    [Networked] private TickTimer CooldownVermelho { get; set; }

    private const int PontosPorCesta = 10;
    private const int PontosParaVencer = 100;

    public override void Spawned()
    {
        Instance = this;

        if (HasStateAuthority)
        {
            BluePoints = 0;
            RedPoints = 0;
            GameOver = false;
            Winner = PlayerRef.None;

            CooldownAzul = TickTimer.None;
            CooldownVermelho = TickTimer.None;
        }
    }

    public bool PodeArremessar(PlayerRef jogador)
    {
        DuelMatch match = DuelMatch.Instance;

        if (match == null || !match.CanPlay || GameOver)
            return false;

        if (jogador == match.BluePlayer)
            return CooldownAzul.ExpiredOrNotRunning(Runner);

        foreach (PlayerRef ativo in Runner.ActivePlayers)
        {
            if (ativo == jogador)
                return CooldownVermelho.ExpiredOrNotRunning(Runner);
        }

        return false;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Arremessar(
        Vector3 origem,
        Vector3 direcao,
        RpcInfo info = default)
    {
        if (!HasStateAuthority || basketballPrefab == null)
            return;

        PlayerRef jogador = info.Source;

        if (jogador == PlayerRef.None ||
            !PodeArremessar(jogador))
            return;

        if (!Runner.TryGetPlayerObject(
            jogador, out NetworkObject objetoJogador))
            return;

        Vector3 frente = new Vector3(
            direcao.x, 0, direcao.z
        );

        if (frente.sqrMagnitude < 0.01f)
            return;

        frente.Normalize();

        // Evita usar uma origem distante do jogador
        if (Vector3.Distance(
            origem, objetoJogador.transform.position) > 3.5f)
        {
            origem = objetoJogador.transform.position
                   + Vector3.up * 1.2f
                   + frente * 1f;
        }

        float radianos = angulo * Mathf.Deg2Rad;

        Vector3 velocidadeInicial =
            frente * (Mathf.Cos(radianos) * velocidade)
            + Vector3.up * (Mathf.Sin(radianos) * velocidade);

        NetworkObject objetoBola = Runner.Spawn(
            basketballPrefab,
            origem,
            Quaternion.LookRotation(frente)
        );

        if (objetoBola == null)
            return;

        BasketballBall bola =
            objetoBola.GetComponent<BasketballBall>();

        if (bola != null)
        {
            bola.Inicializar(jogador, velocidadeInicial);
        }

        TickTimer proximoArremesso =
            TickTimer.CreateFromSeconds(Runner, cooldown);

        if (jogador == DuelMatch.Instance.BluePlayer)
            CooldownAzul = proximoArremesso;
        else
            CooldownVermelho = proximoArremesso;
    }

    public void RegistrarCesta(PlayerRef jogador)
    {
        if (!HasStateAuthority || GameOver)
            return;

        DuelMatch match = DuelMatch.Instance;

        if (match == null || !match.CanPlay)
            return;

        bool conectado = false;

        foreach (PlayerRef ativo in Runner.ActivePlayers)
        {
            if (ativo == jogador)
            {
                conectado = true;
                break;
            }
        }

        if (!conectado)
            return;

        if (jogador == match.BluePlayer)
            BluePoints += PontosPorCesta;
        else
            RedPoints += PontosPorCesta;

        int pontos = jogador == match.BluePlayer
            ? BluePoints
            : RedPoints;

        if (pontos >= PontosParaVencer)
        {
            GameOver = true;
            Winner = jogador;
            match.FinalizarPorPontos(jogador);
        }
    }

    public override void Despawned(
        NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }
}
