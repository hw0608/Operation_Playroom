using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerRespawnManager : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        StartCoroutine(SpawnPlayerRoutine());
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return; 

        PlayerController.OnPlayerSpawn -= HandlePlayerSpawn;
        PlayerController.OnPlayerDespawn -= HandlePlayerDespawn;
    }

    private void HandlePlayerSpawn(PlayerController player)
    {
        player.GetComponent<Health>().OnDie += HandlePlayerDie;
    }

    private void HandlePlayerDespawn(PlayerController player)
    {
        player.GetComponent<Health>().OnDie -= HandlePlayerDie;
    }

    private void HandlePlayerDie(Health sender)
    {
        PlayerController player = sender.GetComponent<PlayerController>();

        StartCoroutine(RespawnPlayerRoutine(player));
    }

    // 리스폰 루틴
    IEnumerator RespawnPlayerRoutine(PlayerController player)
    {
        float respawnTime = 10f;
        while (respawnTime > 0)
        {
            UpdateTimerTextClientRpc(player.NetworkObject, respawnTime);
            yield return new WaitForSeconds(1f);
            respawnTime -= 1f;
        }

        Character character = player.GetComponent<Character>();
        Health health = player.GetComponent<Health>();

        GameTeam gameTeam = (GameTeam)character.team.Value;
        Vector3 spawnPosition = SpawnPoint.GetSpawnPoint(gameTeam, GameRole.King);

        Debug.Log($"Respawn Position: {spawnPosition}, Team: {gameTeam}");

        // 서버에서 위치 설정
        player.transform.position = spawnPosition;

        // 다시 플레이 가능 설정
        player.isPlayable = true;
        health.InitializeHealth();
        character.InitializeAnimator();

        // 클라이언트에 위치 동기화
        UpdatePlayerStateClientRpc(player.NetworkObject, spawnPosition);
    }

    [ClientRpc]
    void UpdateTimerTextClientRpc(NetworkObjectReference playerRef, float time)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            if (playerObj.IsOwner)
            {
                if (timerText != null)
                {
                    timerText.gameObject.SetActive(true);
                    timerText.text = time.ToString();
                }
            }
        }
    }

    [ClientRpc]
    void UpdatePlayerStateClientRpc(NetworkObjectReference playerRef, Vector3 position)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            PlayerController player = playerObj.GetComponent<PlayerController>();
            Character character = player.GetComponent<Character>();
            Health health = player.GetComponent<Health>();

            // 클라이언트에서 위치, 상태, 애니메이터 초기화
            playerObj.transform.position = position;
            player.isPlayable = true;
            health.InitializeHealth();
            character.InitializeAnimator();

            // 타이머 해제
            if (playerObj.IsOwner)
            {
                if (timerText != null)
                {
                    timerText.gameObject.SetActive(false);
                }
            }

            Debug.Log($"Client {player.OwnerClientId} respawned at {position}");
        }
    }

    // 플레이어 스폰 루틴
    IEnumerator SpawnPlayerRoutine()
    {
        yield return new WaitForSeconds(5f);

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        if (players.Length <= 0)
        {
            Debug.Log("None Players");
        }

        foreach (PlayerController player in players)
        {
            HandlePlayerSpawn(player);
        }

        PlayerController.OnPlayerSpawn += HandlePlayerSpawn;
        PlayerController.OnPlayerDespawn += HandlePlayerDespawn;
    }
}
