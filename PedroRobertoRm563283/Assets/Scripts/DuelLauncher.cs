using UnityEngine;
using TMPro;
using Fusion;
public class DuelLauncher : MonoBehaviour
{
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private TMP_InputField roomInput;
    [SerializeField] private TMP_Text conectandoStatus;
    [SerializeField] private GameObject menuPanel;
    
        private bool conectando;
    public async void ConectandoSala()
    {
        if (conectando || runner.IsRunning)
            return;
        string roomName = roomInput.text.Trim();
        if (string.IsNullOrEmpty(roomName))
        {
            conectandoStatus.text =
            "Escreve ai poh só o nome da sala!";
            return;
        }
        conectando = true;
        conectandoStatus.text = "Te botando pra dentro!!!";
        runner.ProvideInput = false;
       
        var sceneManager =
        GetComponent<NetworkSceneManagerDefault>();
       
        var result = await runner.StartGame(
       
            new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            PlayerCount = 2,
            SceneManager = sceneManager
        }
        );
        if (result.Ok)
        {
            menuPanel.SetActive(false);
        }
        else
        {
            conectandoStatus.text =
            "Deu ruim (erro): " + result.ShutdownReason;
            Debug.LogError(result.ShutdownReason);
        }

            conectando = false;
    }
}

