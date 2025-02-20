using TMPro;
using Unity.Services.Authentication;
using UnityEngine;

public class TitleSceneUI : MonoBehaviour
{
    [SerializeField] GameObject lobbyCanvas;
    [SerializeField] GameObject startOptionPanel;
    [SerializeField] TMP_InputField userNameInputField;
    [SerializeField] TMP_InputField joinCodeInputField;

    void Init()
    {
        startOptionPanel.SetActive(false);
        lobbyCanvas.SetActive(false);
    }

    private void Start()
    {
        Init();
    }

    public void OnStartButtonPressed()
    {
        startOptionPanel.SetActive(true);
    }

    public async void OnFindMatchButtonPressed()
    {
        startOptionPanel.SetActive(false);

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
