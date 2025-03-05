using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneUI : MonoBehaviour
{
    [SerializeField] GameObject lobbyCanvas;
    [SerializeField] GameObject startOptionPanel;
    [SerializeField] TMP_InputField userNameInputField;
    [SerializeField] TMP_InputField joinCodeInputField;

    [SerializeField] TMP_Text findMatchStatusText;
    //[SerializeField] TMP_Text findButtonText;

    bool isMatchmaking;
    bool isCancelling;

    void Init()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        startOptionPanel.SetActive(false);
        lobbyCanvas.SetActive(false);

        if (GameObject.FindFirstObjectByType<NetworkManager>() == null)
        {
            SceneManager.LoadScene("NetConnectScene");
        }
    }

    private void Start()
    {
        Init();
        Managers.Resource.LoadAllAsync<GameObject>("default", null);
    }

    public void OnStartButtonPressed()
    {
        startOptionPanel.SetActive(true);
    }

    public void OnClientButtonPressed()
    {
        ClientSingleton.Instance.StartClient("127.0.0.1", 7777);
    }

    public async void OnFindMatchButtonPressed()
    {
        if (isCancelling) { return; }
        if (isMatchmaking)
        {
            //cancel
            isCancelling = true;
            findMatchStatusText.text = "Cancelling...";

            await ClientSingleton.Instance.CancelMatchmaking();

            isCancelling = false;
            isMatchmaking = false;
            findMatchStatusText.text = "";
            //findButtonText.text = "Find Match";
        }
        else
        {
            //match
            findMatchStatusText.text = "Searching...";
            //findButtonText.text = "Cancel";
            isMatchmaking = true;
            isCancelling = false;

            ClientSingleton.Instance.MatchmakeAsync(OnMatchMade);
        }
    }

    void OnMatchMade(MatchmakerPollingResult result)
    {
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                findMatchStatusText.text = "connecting...";
                break;
            default:
                isMatchmaking = false;
                findMatchStatusText.text = "error" + result;
                //findButtonText.text = "Find Match";
                break;
        }
        isMatchmaking = false;
    }

    public async void OnChangeUserNameButtonPressed()
    {
        await AuthenticationService.Instance.UpdatePlayerNameAsync(userNameInputField.text);
    }

    public async void OnJoinButtonPressed()
    {
        await ClientSingleton.Instance.StartClientAsync(joinCodeInputField.text);
    }
}
