using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using static Define;
using Unity.Cinemachine;

public class GameManager : NetworkBehaviour
{
    ETeam myTeam;

    public NetworkVariable<float> remainTime = new NetworkVariable<float>();
    public TMP_Text notiText;
    public TMP_Text timerText;

    public GameObject winPanel;
    public GameObject losePanel;

    public CinemachineCamera[] kingCams; 
    EGameState gameState;

    Sequence textSequence;

    public override void OnNetworkSpawn()
    {
        gameState = EGameState.Ready;

        textSequence = DOTween.Sequence();
        textSequence.Append(notiText.DOFade(1, 1));
        textSequence.AppendInterval(2f);
        textSequence.Append(notiText.DOFade(0, 1))
            .SetAutoKill(false).Pause();


        if (IsServer)
        {
            remainTime.Value = 600f;
            StartCoroutine(CallReadyMessage());
        }
        else
        {
            remainTime.OnValueChanged -= OnChangeTimer;
            remainTime.OnValueChanged += OnChangeTimer;
        }
    } 
    public void SetMyTeam(int team)
    {
        myTeam = (ETeam)team;
    }

    public override void OnNetworkDespawn()
    {
        remainTime.OnValueChanged -= OnChangeTimer;
    }


    void Update()
    {
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
    }

    public void OnChangeTimer(float oldVal, float newVal)
    {
        timerText.text = GetFormattedTime(newVal);
    }
    IEnumerator CallReadyMessage()
    {
        float time = 15;
        CallNotiTextClientRpc("�� ������ ���۵˴ϴ�!");
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

    public void OnKingDead(int team)
    {
        if(myTeam == (ETeam)team)
        {
            
        }
    }

    void KingDeadRoutine(int team)
    {
        //TODO: ��ü �̵� ����

        Sequence kingDeadSeq = DOTween.Sequence();
        kingDeadSeq.AppendCallback(() =>
        {
            kingCams[team].Priority = 2;
        })
        .AppendInterval(3f)
        .AppendCallback(() =>
        {
            kingCams[team].Priority = 0;
            if(myTeam == (ETeam)team)
            {
                winPanel.SetActive(true);
            }
            else
            {
                losePanel.SetActive(true);
            }
        });
       

    }

    [ClientRpc]
    public void CallNotiTextClientRpc(string text)
    {
        notiText.text = text;
        textSequence.Restart();

    }
}
