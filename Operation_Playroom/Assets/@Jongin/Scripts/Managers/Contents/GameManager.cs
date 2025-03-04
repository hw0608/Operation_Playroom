using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static Define;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<float> remainTime = new NetworkVariable<float>();
    public TMP_Text notiText;
    private Color textColor;
    float textAlpha = 0;
    EGameState gameState;

    Sequence textSequence;

    void Start()
    {
        gameState = EGameState.Ready;
        remainTime.Value = 600f;

        textSequence = DOTween.Sequence();
        textSequence.Append(notiText.DOFade(1, 1));
        textSequence.AppendInterval(2f);
        textSequence.Append(notiText.DOFade(0, 1))
            .SetAutoKill(false).Pause();
    }


    void Update()
    {
        if (!IsServer) return;

        //if (gameState != EGameState.Play) return;

        remainTime.Value -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.V))
        {
            CallNotiTextClientRpc("Your flag has been stolen");
        }
    }


    [ClientRpc]
    public void CallNotiTextClientRpc(string text)
    {
        notiText.text = text;
        textSequence.Restart();
    }
}
