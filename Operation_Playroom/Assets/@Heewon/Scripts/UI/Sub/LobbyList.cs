using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyList : MonoBehaviour
{
    [SerializeField] Transform lobbyItemParent;
    [SerializeField] LobbyItem lobbyItemPrefab;

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
                new QueryFilter(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ) // 0은 false. locked가 아닌 방을 가져옴
            };
            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            foreach(Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach(Lobby lobby in lobbies.Results)
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

    public async void JoinAsync(Lobby lobby)
    {
        if (isJoining) return;
        isJoining = true;

        try
        {
            Lobby joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);

            string joinCode = joiningLobby.Data["JoinCode"].Value;
            await ClientSingleton.Instance.StartClientAsync(joinCode);
        }
        catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
        isJoining = false;
    }
}
