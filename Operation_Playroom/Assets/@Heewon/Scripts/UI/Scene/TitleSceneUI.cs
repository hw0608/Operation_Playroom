using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneUI : MonoBehaviour
{
    [SerializeField] GameObject lobbyCanvas;
    [SerializeField] GameObject startOptionPanel;
    [SerializeField] GameObject nicknameSettingPanel;
    [SerializeField] TMP_InputField userNameInputField;
    [SerializeField] TMP_InputField joinCodeInputField;
    [SerializeField] GameObject findMatchPanel;

    [SerializeField] TMP_Text findMatchStatusText;
    [SerializeField] TMP_Text userNameWarningText;
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
        //Managers.Resource.LoadAllAsync<GameObject>("default", null);
    }

    public async void OnStartButtonPressed()
    {
        string name = await AuthenticationService.Instance.GetPlayerNameAsync();

        if (name == null)
        {
            nicknameSettingPanel.SetActive(true);
        }
        else
        {
            startOptionPanel.SetActive(true);
        }
    }

    public void OnClientButtonPressed()
    {
        ClientSingleton.Instance.StartClient("127.0.0.1", 7777);
    }

    public async void GetUserNickname()
    {
        await AuthenticationService.Instance.GetPlayerNameAsync();
    }

    public async void OnFindMatchButtonPressed()
    {
        if (isCancelling || isMatchmaking) { return; }
        //match
        findMatchStatusText.text = "Searching...";

        findMatchPanel.SetActive(true);
        isMatchmaking = true;
        isCancelling = false;

        ClientSingleton.Instance.MatchmakeAsync(OnMatchMade);
    }

    public async void OnCancelMatchmakeButtonPressed()
    {
        findMatchPanel.SetActive(false);
        isCancelling = true;

        await ClientSingleton.Instance.CancelMatchmaking();

        isCancelling = false;
        isMatchmaking = false;
        findMatchStatusText.text = "";
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
        if (userNameInputField.text == "")
        {
            userNameWarningText.text = "아무것도 입력되지 않았습니다.";
            return;
        }
        else if (userNameInputField.text.Contains(" "))
        {
            userNameWarningText.text = "공백은 입력할 수 없습니다.";
            return;
        }

        await AuthenticationService.Instance.UpdatePlayerNameAsync(userNameInputField.text);

        nicknameSettingPanel.SetActive(false);
    }

    public async void OnJoinButtonPressed()
    {
        await ClientSingleton.Instance.StartClientAsync(joinCodeInputField.text);
    }
}
