using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI lobbyTitleTmp;
    [SerializeField] TextMeshProUGUI lobbyPlayersTmp;

    LobbyList lobbyList;
    Lobby lobby;

    public void SetItem(LobbyList lobbyList, Lobby lobby)
    {
        this.lobbyList = lobbyList;
        this.lobby = lobby;

        lobbyTitleTmp.text = lobby.Name;
        lobbyPlayersTmp.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
    }

    public void JoinPressed()
    {
        lobbyList.JoinAsync(lobby);
    }
}
