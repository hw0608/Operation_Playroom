using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.UI;

public enum VisibilityToggleType
{
    PublicToggle,
    PrivateToggle
}

public class LobbyList : MonoBehaviour
{
    [SerializeField] Transform roomItemParent;
    [SerializeField] LobbyItem roomItemPrefab;

    [Header("Create Room")]
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] ToggleGroup visibilityToggleGroup;
    [SerializeField] TMP_InputField createPasswordInputField;
    [SerializeField] TMP_Text PasswordSettingWarningText;
    [SerializeField] GameObject creatingProgressPanel;

    [Header("Join Room")]
    [SerializeField] GameObject joinPasswordPanel;
    [SerializeField] Button joinPasswordButton;
    [SerializeField] Button closePasswordPanelButton;
    [SerializeField] TMP_InputField joinPasswordInputField;

    [Header("Join Room By JoinCode")]
    [SerializeField] GameObject joinCodePanel;
    [SerializeField] Button joinByJoinCodeButton;
    [SerializeField] Button closeJoinCodePanelButton;
    [SerializeField] TMP_InputField joinCodeInputField;

    [SerializeField] GameObject joiningProgressPanel;

    bool isRefreshing;
    bool isJoining;

    private void OnEnable()
    {
        joiningProgressPanel.SetActive(false);
        RefreshList();
    }

    public async void RefreshList()
    {
        if (isRefreshing) return;
        isRefreshing = true;

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;
            options.Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT), //available slots이 0보다 크거나 같은 방을 가져온다
                //new QueryFilter(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ) // 0은 false. locked가 아닌 방을 가져옴
            };
            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            foreach (Transform child in roomItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach (Lobby lobby in lobbies.Results)
            {
                LobbyItem lobbyItem = Instantiate(roomItemPrefab, roomItemParent);
                lobbyItem.SetItem(this, lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }

        isRefreshing = false;
    }

    public async void JoinAsync(Lobby lobby, bool needPassword = false)
    {
        if (isJoining) return;
        isJoining = true;
        Lobby joiningLobby;

        try
        {
            if (needPassword)
            {
                string password = await InputPassword();

                if (string.IsNullOrEmpty(password))
                {
                    isJoining = false;
                    return;
                }
                else
                {
                    joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
                    {
                        Password = password
                    });
                }
            }
            else
                joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);

            joiningProgressPanel.SetActive(true);

            string joinCode = joiningLobby.Data["JoinCode"].Value;
            await ClientSingleton.Instance.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.IncorrectPassword || e.Reason == LobbyExceptionReason.ValidationError)
            {
                MessagePopup popup = Managers.Resource.Instantiate("MessagePopup").GetComponent<MessagePopup>();
                if (popup != null)
                {
                    popup.SetText("비밀번호가 틀립니다");
                    popup.Show();
                }
            }
            else
            {
                Debug.LogException(e);
            }
        }
        isJoining = false;
    }

    public async void JoinByJoinCodeAsync()
    {
        joinCodePanel.SetActive(true);

        string joinCode = await InputJoinCode();

        if (string.IsNullOrEmpty(joinCode))
        {
            joinCodePanel.SetActive(false);
        }
        else
        {
            joiningProgressPanel.SetActive(true);
            await ClientSingleton.Instance.StartClientAsync(joinCodeInputField.text);
        }
    }

    async Task<string> InputJoinCode()
    {
        bool waiting = true;
        bool cancel = false;

        joinByJoinCodeButton.onClick.AddListener(() => waiting = false);
        closeJoinCodePanelButton.onClick.AddListener(() => cancel = true);

        while (!cancel && waiting)
        {
            await Task.Yield();
        }

        joinByJoinCodeButton.onClick.RemoveAllListeners();
        closeJoinCodePanelButton.onClick.RemoveAllListeners();

        return cancel ? "" : joinCodeInputField.text;
    }

    async Task<string> InputPassword()
    {
        bool waiting = true;
        bool cancel = false;
        joinPasswordPanel.SetActive(true);

        joinPasswordButton.onClick.AddListener(() => waiting = false);
        closePasswordPanelButton.onClick.AddListener(() => cancel = true);

        while (!cancel && waiting)
        {
            await Task.Yield();
        }

        joinPasswordButton.onClick.RemoveAllListeners();
        closePasswordPanelButton.onClick.RemoveAllListeners();

        joinPasswordPanel.SetActive(false);
        return cancel ? "" : joinPasswordInputField.text;
    }

    public async void OnCreateLobbyButtonPressed()
    {
        CreateLobbyOptions options = new CreateLobbyOptions();

        bool isPrivate = visibilityToggleGroup.ActiveToggles().FirstOrDefault().name.Equals(VisibilityToggleType.PrivateToggle.ToString());
        string password = createPasswordInputField.text;

        if (password.Length >= 8)
        {
            options.Password = password;
        }
        else if (password.Length > 0)
        {
            PasswordSettingWarningText.gameObject.SetActive(true);
            PasswordSettingWarningText.text = "비밀번호는 최소 8자 이상이어야 합니다.";
            return;
        }

        options.IsPrivate = isPrivate;

        creatingProgressPanel.SetActive(true);
        await HostSingleton.Instance.StartHostAsync(options, roomNameInputField.text);

        creatingProgressPanel.SetActive(false);
        PasswordSettingWarningText.text = "";
    }
}
