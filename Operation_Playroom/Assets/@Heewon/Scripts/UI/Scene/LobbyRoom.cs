using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
class TMPList
{
    public TMP_Text[] texts;
}

public class LobbyRoom : NetworkBehaviour
{
    [SerializeField] TMPList[] playerNameTexts = new TMPList[2];
    [SerializeField] TMPList[] playerReadyTexts = new TMPList[2];
    [SerializeField] GameObject readyButton;
    [SerializeField] GameObject startButton;

    NetworkList<LobbyRoomPlayerData> players = new NetworkList<LobbyRoomPlayerData>();

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            players.OnListChanged += HandlePlayerStateChanged;

            foreach (var player in players)
            {
                HandlePlayerStateChanged(new NetworkListEvent<LobbyRoomPlayerData>
                {
                    Type = NetworkListEvent<LobbyRoomPlayerData>.EventType.Add,
                    Value = player
                });
            }
        }

        if (IsServer)
        {
            string userName = ServerSingleton.Instance.clientIdToUserData[NetworkManager.Singleton.LocalClientId].userName;
            AddPlayerServerRpc(AuthenticationService.Instance.PlayerId, NetworkManager.Singleton.LocalClientId, userName);
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayerStateChanged;
        }
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        AddPlayerServerRpc(ServerSingleton.Instance.clientIdToUserData[clientId].userAuthId, clientId, ServerSingleton.Instance.clientIdToUserData[clientId].userName);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (players == null) return;

        foreach (var player in players)
        {
            if (player.clientId == clientId)
            {
                players.Remove(player);
                break;
            }
        }
    }

    [ServerRpc]
    void AddPlayerServerRpc(string authId, ulong clientId, string name)
    {
        int team = AssignTeam();
        bool isLeader = (players.Count == 0);   // 방에 맨 처음으로 들어왔으면 방장

        LobbyRoomPlayerData newPlayer = new LobbyRoomPlayerData
        {
            authId = authId,
            clientId = clientId,
            userName = name,
            isReady = false,
            isLeader = isLeader,
            team = team
        };

        players.Add(newPlayer);

        if (isLeader)
        {
            readyButton.SetActive(false);
            startButton.SetActive(true);
        }

        Debug.Log("Player Count: " + players.Count);
    }

    int AssignTeam()
    {
        int blue = 0, red = 0;

        foreach (var player in players) {
            if (player.team == 0) blue++;
            else red++;
        }

        return (blue == red) ? UnityEngine.Random.Range(0, 1) : blue < red ? 0 : 1;
    }

    public async void OnBackButtonPressedAsync()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (players.Count <= 1)     // 방장밖에 없었으면 방 폭파
            {
                HostSingleton.Instance.ShutDown();
            }
            else
            {
                //TODO: 남아 있는 사람한테 방장 위임
                await AssignNewLeaderAsync();
            }
        }
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    async Task AssignNewLeaderAsync()
    {
        Debug.Log("AssignNewLeader");
        int newLeader = 0;
        do
        {
            newLeader = UnityEngine.Random.Range(0, players.Count);
        } while (players[newLeader].clientId == OwnerClientId);

        players[newLeader] = new LobbyRoomPlayerData
        {
            authId = players[newLeader].authId,
            clientId = players[newLeader].clientId,
            userName = players[newLeader].userName,
            isLeader = true,
            team = players[newLeader].team
        };

        try
        {
            await HostSingleton.Instance.UpdateLobbyHost(players[newLeader].authId.ToString());
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    public void HandlePlayerStateChanged(NetworkListEvent<LobbyRoomPlayerData> changeEvent)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            for (int j = 0; j < playerNameTexts[i].texts.Length; j++)
            {
                playerNameTexts[i].texts[j].text = "Waiting...";
                playerReadyTexts[i].texts[j].text = "";
            }
        }

        int[] teamCount = new int[2];
        ulong clientId = NetworkManager.Singleton.LocalClientId;

        foreach (var player in players)
        {
            int team = player.team;
            int index = teamCount[team];

            if (index < playerNameTexts[team].texts.Length)
            {
                playerNameTexts[team].texts[index].text = player.userName.ToString();
                if (player.clientId == clientId)
                {
                    playerNameTexts[team].texts[index].text = $"<b>{player.userName.ToString()}</b>";
                }
                playerReadyTexts[team].texts[index].text = !player.isLeader && player.isReady ? "<color=green>Ready</color>" : "";
                teamCount[team]++;
            }
        }
    }

    public void ToggleReady()
    {
        Debug.Log($"ToggleReady() called by client {NetworkManager.Singleton.LocalClientId}");
        ToggleReadyServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleReadyServerRpc(ulong clientId)
    {
        Debug.Log($"ToggleReadyServerRpc() called by client {NetworkManager.Singleton.LocalClientId}");
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId == clientId)
            {
                var updatedPlayer = players[i];
                updatedPlayer.isReady = !updatedPlayer.isReady;
                players[i] = updatedPlayer;
                break;
            }
        }

        foreach (var player in players)
        {
            Debug.Log($"{player.clientId} : {player.isReady}");
        }
    }

    private bool CheckAllPlayersReady()
    {
        foreach (var player in players)
        {
            if (!player.isLeader && !player.isReady)
            {
                return false;
            }
        }

        return true;
    }

    public void OnStartButtonPressed()
    {
        if (!IsServer) return;

        if (CheckAllPlayersReady())
        {
            StartGameServerRpc();
        }
    }

    [ServerRpc]
    void StartGameServerRpc()
    {
        NetworkManager.SceneManager.LoadScene("TestScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
