using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
class TMPList
{
    public TMP_Text[] texts;
}

[System.Serializable]
class ImageList
{
    public Image[] images;
}

public class LobbyRoom : NetworkBehaviour
{
    [SerializeField] TMPList[] playerNameTexts = new TMPList[2];
    [SerializeField] ImageList[] playerReadyImages = new ImageList[2];
    [SerializeField] ImageList[] playerBGImages = new ImageList[2];
    [SerializeField] Sprite[] BGImagesByTeam;
    [SerializeField] GameObject readyButton;
    [SerializeField] GameObject startButton;
    [SerializeField] TMP_Text joinCodeText;

    NetworkVariable<FixedString32Bytes> joinCode = new NetworkVariable<FixedString32Bytes>();
    NetworkList<LobbyRoomPlayerData> players = new NetworkList<LobbyRoomPlayerData>();
    MatchplayMatchmaker matchmaker = new MatchplayMatchmaker();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            joinCode.Value = HostSingleton.Instance.joinCode;
            string userName = ServerSingleton.Instance.clientIdToUserData[NetworkManager.Singleton.LocalClientId].userName;
            AddPlayerServerRpc(AuthenticationService.Instance.PlayerId, NetworkManager.Singleton.LocalClientId, userName);
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        if (IsClient)
        {
            players.OnListChanged -= HandlePlayerStateChanged;
            players.OnListChanged += HandlePlayerStateChanged;

            foreach (var player in players)
            {
                HandlePlayerStateChanged(new NetworkListEvent<LobbyRoomPlayerData>
                {
                    Type = NetworkListEvent<LobbyRoomPlayerData>.EventType.Add,
                    Value = player
                });
            }

            joinCodeText.text = joinCode.Value.ToString();
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
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (IsServer)
        {
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn();
            }
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

    public void OnBackButtonPressedAsync()
    {
        if (NetworkManager.Singleton.IsHost)
        {
           HostSingleton.Instance.ShutDown();
        }
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.Shutdown();
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
                playerReadyImages[i].images[j].gameObject.SetActive(false);
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
                if (player.isLeader)
                {
                    playerReadyImages[team].images[index].gameObject.SetActive(false);
                    playerBGImages[team].images[index].sprite = BGImagesByTeam[2];
                }
                else
                {
                    playerBGImages[team].images[index].sprite = BGImagesByTeam[3];
                }

                playerNameTexts[team].texts[index].text = player.userName.ToString();
                if (player.clientId == clientId)
                {
                    playerBGImages[team].images[index].sprite = BGImagesByTeam[team];
                }
                if (!player.isLeader && player.isReady)
                    playerReadyImages[team].images[index].gameObject.SetActive(true);
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
            MatchmakeAsync(HandleMatchmakeAssignment);
        }
    }

    void HandleMatchmakeAssignment(MatchmakerPollingResult result)
    {
        if (result != MatchmakerPollingResult.Success)
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName("LoadingScene"));
                MessagePopup popup = Managers.Resource.Instantiate("MessagePopup").GetComponent<MessagePopup>();
                if (popup != null)
                {
                    popup.SetText("서버 연결에 실패했습니다.\n다시 시도해 주세요.");
                    popup.Show();
                }
            }
        }
    }

    async void MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakeResponse = null)
    {
        if (matchmaker.IsMatchmaking)
        {
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);

        MatchmakerPollingResult result = await GetMatchAsync();
        onMatchmakeResponse?.Invoke(result);
    }

    public async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        List<UserData> userDatas = new List<UserData>();

        List<int> availableRoles = Enum.GetValues(typeof(GameRole)).Cast<int>().ToList();
        availableRoles.Remove((int)GameRole.None);
        availableRoles = availableRoles.OrderBy(x => UnityEngine.Random.value).ToList();

        int idx = 0;

        for (int i = 0; i < players.Count; i++)
        {
            var updatedPlayer = players[i];
            updatedPlayer.role = availableRoles[idx];
            players[i] = updatedPlayer;

            idx = (idx + 1) % 3;
        }

        userDatas = ServerSingleton.Instance.authIdToUserData.Values.ToList();

        MatchmakingResult result = await matchmaker.Matchmake(userDatas);

        Debug.Log(result.resultMessage);

        if (result.result == MatchmakerPollingResult.Success)
        {
            if (result.ip == "0.0.0.0")
            {
                return MatchmakerPollingResult.MatchAssignmentError;
            }

            // 클라이언트 시작
            SwitchToDSClientRpc(result.ip, (ushort)result.port);
        }

        return result.result;
    }

    [ClientRpc]
    void SwitchToDSClientRpc(string ip, ushort port)
    {
        Debug.Log($"{NetworkManager.Singleton.LocalClientId} called SwitchToDSClientRpc.");
        Debug.Log($"ip : {ip} , port : {port}");

        foreach (var player in players)
        {
            if (player.clientId != NetworkManager.Singleton.LocalClientId) continue;
            ClientSingleton.Instance.UserData.userGamePreferences.gameRole = (GameRole)player.role;
            ClientSingleton.Instance.UserData.userGamePreferences.gameTeam = (GameTeam)player.team;
            break;
        }

        ClientSingleton.Instance.StartClient(ip, port);
    }
}
