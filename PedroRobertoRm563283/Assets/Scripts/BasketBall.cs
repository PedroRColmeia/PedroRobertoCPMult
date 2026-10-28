
using Fusion;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BasketballBall : NetworkBehaviour
{
    [Networked]
    public PlayerRef Arremessador { get; set; }

    private Rigidbody rb;
    private TickTimer tempoVida;
    private TickTimer janelaCesta;

    private bool jaPontuou;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void Spawned()
    {
        jaPontuou = false;
        janelaCesta = TickTimer.None;

        if (HasStateAuthority)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            tempoVida = TickTimer.CreateFromSeconds(
                Runner, 12f
            );
        }
        else
        {
            // Apenas a autoridade controla a fisica
            rb.isKinematic = true;
        }
    }

    public void Inicializar(
        PlayerRef jogador,
        Vector3 velocidadeInicial)
    {
        if (!HasStateAuthority)
            return;

        Arremessador = jogador;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = velocidadeInicial;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        // Remove a bola apos 12 segundos
        if (tempoVida.Expired(Runner))
        {
            Runner.Despawn(Object);
            return;
        }
    }

    public void PassarSensor(bool entrada)
    {
        if (!HasStateAuthority || jaPontuou)
            return;

        if (rb == null)
            return;

        // A bola precisa estar descendo
        if (rb.linearVelocity.y >= -0.1f)
            return;

        if (entrada)
        {
            // Registra a passagem pelo sensor superior
            janelaCesta = TickTimer.CreateFromSeconds(
                Runner, 1.5f
            );

            return;
        }

        // Verifica se passou pelo sensor superior
        if (janelaCesta.ExpiredOrNotRunning(Runner))
            return;

        if (Arremessador == PlayerRef.None)
            return;

        BasketScore placar = BasketScore.Instance;

        if (placar == null || placar.GameOver)
            return;

        // Impede contabilizar duas vezes
        jaPontuou = true;

        placar.RegistrarCesta(Arremessador);

        Debug.Log("CESTA! +10 pontos");
    }
}
