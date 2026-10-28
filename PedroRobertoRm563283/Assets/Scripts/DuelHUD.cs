
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DuelHUD : MonoBehaviour
{
    [Header("Paineis")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject resultsPanel;

    [Header("Rede")]
    [SerializeField] private NetworkRunner runner;

    [Header("HUD")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text matchStatus;

    [Header("Resultado")]
    [SerializeField] private TMP_Text resultText;

    private bool voltandoAoMenu = false;

    private void Start()
    {
        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (hudPanel != null)
            hudPanel.SetActive(false);

        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    private void Update()
    {
        if (voltandoAoMenu)
            return;

        bool mostrarHUD = false;

        if (runner != null && runner.IsRunning)
        {
            int jogadores = 0;

            foreach (PlayerRef jogador in runner.ActivePlayers)
            {
                jogadores++;
            }

            mostrarHUD = jogadores >= 2 &&
                         (DuelMatch.Instance == null ||
                          !DuelMatch.Instance.IsOver);
        }

        if (hudPanel != null &&
            hudPanel.activeSelf != mostrarHUD)
        {
            hudPanel.SetActive(mostrarHUD);
        }

        if (runner == null || !runner.IsRunning)
            return;

        DuelMatch match = DuelMatch.Instance;

        if (match == null)
        {
            matchStatus.text =
                "Aguardando estado da partida...";
            return;
        }

        UpdateHealth();

        if (match.IsOver)
        {
            ShowResult(match);
        }
        else
        {
            resultsPanel.SetActive(false);

            matchStatus.text = match.CanPlay
                ? "Duelo em andamento"
                : "Aguardando segundo jogador...";
        }
    }

    private void UpdateHealth()
    {
        BasketScore placar = BasketScore.Instance;

        if (placar == null || healthText == null)
            return;

        healthText.text =
            "AZUL: " + placar.BluePoints +
            "   |   VERMELHO: " + placar.RedPoints;
    }


    private void ShowResult(DuelMatch match)
    {
        resultsPanel.SetActive(true);

        matchStatus.text = "Partida encerrada";

        bool localPlayerWon =
            match.Winner == runner.LocalPlayer;

        string winnerName =
            match.Winner == match.BluePlayer
                ? "AZUL"
                : "VERMELHO";

        string message = localPlayerWon
            ? "VOCÊ VENCEU!"
            : "VOCÊ PERDEU!";

        resultText.text =
            message +
            "\nVencedor: Jogador " + winnerName;
    }

    public async void VoltarAoMenu()
    {
        if (voltandoAoMenu)
            return;

        voltandoAoMenu = true;

        Debug.Log("Voltando ao menu...");

        if (hudPanel != null)
            hudPanel.SetActive(false);

        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (runner != null && runner.IsRunning)
        {
            await runner.Shutdown();
        }

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex,
            LoadSceneMode.Single
        );
    }
}
