using DG.Tweening;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class GameManager : NetworkBehaviour
{
    private static GameManager instance;

    public static GameManager Instance { get { return instance; } }

    int myTeam;

    public NetworkVariable<float> remainTime = new NetworkVariable<float>();
    public TMP_Text notiText;
    public TMP_Text timerText;
    public Image circleImage;
    public GameObject winPanel;
    public GameObject losePanel;

    public CinemachineCamera[] kingCams;
    EGameState gameState;

    Sequence textSequence;

    PlayerController[] players;

    PlayerRespawnManager respawnManager;
    OccupyManager occupyManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public override void OnNetworkSpawn()
    {
        circleImage.rectTransform.DOSizeDelta(new Vector2(2500, 2500), 1f);
        gameState = EGameState.Ready;
        textSequence = DOTween.Sequence();
        textSequence.Append(notiText.DOFade(1, 1));
        textSequence.AppendInterval(2f);
        textSequence.Append(notiText.DOFade(0, 1))
            .SetAutoKill(false).Pause();

        respawnManager = FindFirstObjectByType<PlayerRespawnManager>();
        occupyManager = FindFirstObjectByType<OccupyManager>();

        if (IsServer)
        {
            remainTime.Value = 600f;
            StartCoroutine(CallReadyMessage());
        }
        else
        {
            myTeam = (int)ClientSingleton.Instance.UserData.userGamePreferences.gameTeam;
            remainTime.OnValueChanged -= OnChangeTimer;
            remainTime.OnValueChanged += OnChangeTimer;
        }

    }

    public override void OnNetworkDespawn()
    {
        remainTime.OnValueChanged -= OnChangeTimer;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (IsServer)
            {
                AllPlayerRespawn();
            }
        }


        if (gameState != EGameState.Play) return;


    }


    private string GetFormattedTime(float time)
    {
        int min = Mathf.FloorToInt(time / 60);
        int sec = Mathf.FloorToInt(time % 60);
        return sec >= 10 ? $"{min} : {sec}" : $"{min} : 0{sec}";
    }

    IEnumerator TimerRoutine()
    {
        while (remainTime.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            remainTime.Value -= 1.0f;
        }
        TimeOverClientRpc();
        yield return null;
    }

    public void OnChangeTimer(float oldVal, float newVal)
    {
        timerText.text = GetFormattedTime(newVal);
    }
    IEnumerator CallReadyMessage()
    {
        float time = 15;
        CallNotiTextClientRpc("곧 게임이 시작됩니다!");
        while (time > 1)
        {
            yield return new WaitForSeconds(1);
            time--;

            if (time <= 10)
            {
                CallNotiTextClientRpc(time.ToString());
            }
            yield return null;
        }
        yield return new WaitForSeconds(1);
        CallNotiTextClientRpc("Start!");
        gameState = EGameState.Play;
        StartCoroutine(TimerRoutine());
    }

    [ClientRpc]
    public void AllPlayerStopClientRpc(bool isStop, bool isSoldierSpawn = false)
    {
        players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].isPlayable = isStop;
            if (!isSoldierSpawn) continue;


            if (players[i].gameObject.GetComponent<KingTest>() != null)
            {
                Debug.Log(players[i]);
                KingTest king = players[i].gameObject.GetComponent<KingTest>();
                StartCoroutine(SoldierSpawnDelay(king));
            }

        }
    }

    IEnumerator SoldierSpawnDelay(KingTest king)
    {
        yield return new WaitForSeconds(1);
        king.CommandSoldierToWarp();
    }

    public void AllPlayerRespawn()
    {
        AllPlayerStopClientRpc(false, true);
        //리스폰
        respawnManager.ReTransformPostion();

    }

    public void OnKingDead(Health health)
    {
        KingDeadRoutineClientRpc(health.GetComponent<Character>().team.Value);
    }

    [ClientRpc]
    void KingDeadRoutineClientRpc(int team)
    {
        Sequence kingDeadSeq = DOTween.Sequence();
        kingDeadSeq.AppendCallback(() =>
        {
            kingCams[team].Priority = 2;
            Time.timeScale = 0.5f;
        })
        .AppendInterval(2f)
        .AppendCallback(() =>
        {
            AllPlayerStopClientRpc(false);
            Time.timeScale = 1f;
        })
        .AppendInterval(1f)
        .AppendCallback(() =>
        {
            kingCams[team].Priority = 0;
            if (myTeam == team)
            {
                losePanel.SetActive(true);
            }
            else
            {
                winPanel.SetActive(true);
            }
        })
        .AppendInterval(3f)
        .AppendCallback(() =>
        {
            circleImage.rectTransform.DOSizeDelta(new Vector2(0, 0), 1f);
        })
        .AppendInterval(2f)
        .AppendCallback(() =>
         {
             NetworkManager.Singleton.Shutdown();
         });
    }

    [ClientRpc]
    void TimeOverClientRpc()
    {
        Sequence gameOverSeq = DOTween.Sequence();
        gameOverSeq.AppendCallback(() =>
        {
            AllPlayerStopClientRpc(false);
        })
        .AppendInterval(1f)
        .AppendCallback(() =>
        {
            int redPoint = occupyManager.redTeamOccupyCount.Value;
            int bluePoint = occupyManager.blueTeamOccupyCount.Value;

            int winner = redPoint > bluePoint ? 1 : 0;

            if (myTeam == winner)
            {
                winPanel.SetActive(true);
            }
            else
            {
                losePanel.SetActive(true);
            }
        })
        .AppendInterval(3f)
        .AppendCallback(() =>
        {
            circleImage.rectTransform.DOSizeDelta(new Vector2(0, 0), 1f);
        })
        .AppendInterval(2f)
        .AppendCallback(() =>
        {
            NetworkManager.Singleton.Shutdown();
        });
    }

    [ClientRpc]
    public void CallNotiTextClientRpc(string text)
    {
        notiText.text = text;
        textSequence.Restart();
    }
}
