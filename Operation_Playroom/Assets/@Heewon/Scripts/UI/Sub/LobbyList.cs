using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public enum VisibilityToggleType
{
    PublicToggle,
    PrivateToggle
}

public class LobbyList : MonoBehaviour
{
    [SerializeField] Transform lobbyItemParent;
    [SerializeField] LobbyItem lobbyItemPrefab;

    [Header("Create Lobby")]
    [SerializeField] TMP_InputField lobbyNameInputField;
    [SerializeField] ToggleGroup visibilityToggleGroup;
    [SerializeField] TMP_InputField createPasswordInputField;

    [Header("Join Lobby")]
    [SerializeField] GameObject joinPasswordPanel;
    [SerializeField] Button joinPasswordButton;
    [SerializeField] TMP_InputField joinPasswordInputField;

    bool isRefreshing;
    bool isJoining;

    private void OnEnable()
    {
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

            foreach (Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach (Lobby lobby in lobbies.Results)
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
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
                joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
                {
                    Password = await InputPassword()
                });
            else
                joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);

            string joinCode = joiningLobby.Data["JoinCode"].Value;
            await ClientSingleton.Instance.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.IncorrectPassword || e.Reason == LobbyExceptionReason.ValidationError)
            {
                Debug.Log("Invalid Password");
                MessagePopup popup = Managers.Resource.Instantiate("MessagePopup").GetComponent<MessagePopup>();
                if (popup != null)
                {
                    popup.SetText("Invalid Password");
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

    async Task<string> InputPassword()
    {
        bool waiting = true;
        joinPasswordPanel.SetActive(true);

        while (waiting)
        {
            joinPasswordButton.onClick.AddListener(() => waiting = false);
            await Task.Yield();
        }

        joinPasswordPanel.SetActive(false);
        return joinPasswordInputField.text;
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
            MessagePopup popup = Managers.Resource.Instantiate("MessagePopup").GetComponent<MessagePopup>();
            popup.SetText("password should be at least 8 chars long");
            popup.Show();
            return;
        }

        options.IsPrivate = isPrivate;

        await HostSingleton.Instance.StartHostAsync(options, lobbyNameInputField.text);
    }
}
