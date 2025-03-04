using DG.Tweening;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static Define;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<float> remainTime = new NetworkVariable<float>();
    public TMP_Text notiText;
    public TMP_Text timerText;
    private Color textColor;
    float textAlpha = 0;
    EGameState gameState;

    Sequence textSequence;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            remainTime.Value = 600f;
        }
        gameState = EGameState.Ready;

        textSequence = DOTween.Sequence();
        textSequence.Append(notiText.DOFade(1, 1));
        textSequence.AppendInterval(2f);
        textSequence.Append(notiText.DOFade(0, 1))
            .SetAutoKill(false).Pause();

        StartCoroutine(CallReadyMessage());
    }

    public override void OnNetworkDespawn()
    {

    }
    private float lastSyncTime = 0f;
    private float syncInterval = 1f;

    void Update()
    {
        if (IsServer)
        {
            if (gameState != EGameState.Play) return;

            remainTime.Value -= Time.deltaTime;
            timerText.text = GetFormattedTime(remainTime.Value);
            // 1초마다 동기화
            if (Time.time - lastSyncTime > syncInterval)
            {
                remainTime.SetDirty(false);  // 강제 동기화
                lastSyncTime = Time.time;
            }
        }
        else
        {
            // 클라이언트는 로컬에서 타이머 감소
            timerText.text = GetFormattedTime(remainTime.Value - Time.deltaTime);
        }
    }

    private string GetFormattedTime(float time)
    {
        int min = Mathf.FloorToInt(time / 60);
        int sec = Mathf.FloorToInt(time % 60);
        return sec >= 10 ? $"{min} : {sec}" : $"{min} : 0{sec}";
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
    }


    [ClientRpc]
    public void CallNotiTextClientRpc(string text)
    {
        if (!IsServer) return;
        notiText.text = text;
        textSequence.Restart();
    }
}
