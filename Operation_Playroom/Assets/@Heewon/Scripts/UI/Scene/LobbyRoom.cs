using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
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
            players.OnListChanged += HandlePlayerNameChanged;
        }

        if (IsServer)
        {
            AddPlayerServerRpc(NetworkManager.Singleton.LocalClientId, "Player " + players.Count);
        }
    }

    [ServerRpc]
    void AddPlayerServerRpc(ulong clientId, string name)
    {
        int team = players.Count % 2;
        bool isLeader = (players.Count == 0);   // 방에 맨 처음으로 들어왔으면 방장

        LobbyRoomPlayerData newPlayer = new LobbyRoomPlayerData
        {
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
    }

    public void OnBackButtonPressed()
    {

    }

    public void HandlePlayerNameChanged(NetworkListEvent<LobbyRoomPlayerData> changeEvent)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        Debug.Log(playerNameTexts.Length);
        Debug.Log(playerNameTexts[0].texts.Length);

        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            for (int j = 0; j < playerNameTexts[i].texts.Length; j++)
            {
                playerNameTexts[i].texts[j].text = "Waiting...";
                playerReadyTexts[i].texts[j].text = "";
            }
        }

        Dictionary<int, int> teamIndex = new Dictionary<int, int> { { 0, 0 }, { 1, 0 } };

        foreach (var player in players)
        {
            int team = player.team;
            int index = teamIndex[team];

            if (index < playerNameTexts[team].texts.Length)
            {
                playerNameTexts[team].texts[index].text = player.userName.ToString();
                playerReadyTexts[team].texts[index].text = !player.isLeader && player.isReady ? "<color=green>Ready</color>" : "";
                teamIndex[team]++;
            }
        }
    }

    public void ToggleReady()
    {
        ToggleReadyServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc]
    private void ToggleReadyServerRpc(ulong clientId)
    {
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
    }

    private bool CheckAllPlayersReady()
    {
        bool allReady = true;
        foreach (var player in players)
        {
            if (!player.isReady)
            {
                allReady = false;
                break;
            }
        }

        return allReady;
    }


}
