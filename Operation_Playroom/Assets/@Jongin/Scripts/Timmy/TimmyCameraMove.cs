using DG.Tweening;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TimmyCameraMove : NetworkBehaviour
{
    public Image fadeImage;
    private Color imageColor;

    public CinemachineCamera[] cameras;
    private int activeCameraIndex = 0;

    public GameObject sleepTimmy;
    public GameObject moveTimmy;

    public Vector3 startPos;
    public Quaternion startRot;
    void Start()
    {
        imageColor = fadeImage.color;
        imageColor.a = 0; // ���� �� ����
        fadeImage.color = imageColor;
        startPos = moveTimmy.transform.position;
        startRot = moveTimmy.transform.rotation;
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (!IsServer) return;
            StartTimmyClientRpc();
        }
    }

    //[ClientRpc]
    //void ScreenFadeClientRpc(TweenCallback callback)
    //{
    //    Sequence screenSequence = DOTween.Sequence();
    //    screenSequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 1f, 0.5f));
    //    screenSequence.AppendCallback(callback);
    //    screenSequence.AppendInterval(1f);
    //    screenSequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 0f, 0.5f));
    //}

    [ClientRpc]
    public void StartTimmyClientRpc()
    {
        Sequence timmySequence = DOTween.Sequence();

        //Ƽ�� �Ͼ�� ����
        timmySequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            //ȭ�� ��ȯ
            ActiveCamera(1);
        });
        timmySequence.AppendInterval(1f);
        timmySequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 0f, 0.5f));



        timmySequence.AppendCallback(() =>
        {
            if (IsServer)
            {
                sleepTimmy.GetComponent<Animator>().SetTrigger("WakeUp");
            }
        });
        timmySequence.AppendInterval(3f);

        //�ڴ� Ƽ�̿� �����̴� Ƽ�� ��ü
        timmySequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            //�ڴ� Ƽ�� �ʱ�ȭ
            if (IsServer)
            {
                sleepTimmy.GetComponent<Animator>().SetTrigger("Sleep");
            }
            sleepTimmy.SetActive(false);
            moveTimmy.SetActive(true);
            ActiveCamera(2);
        });
        timmySequence.AppendInterval(1f);

        timmySequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 0f, 0.5f));

        //Ƽ�� �����̱�
        timmySequence.AppendCallback(() =>
        {
            if (IsServer)
            {
                moveTimmy.GetComponent<MoveTimmy>().CallTimmy(FinishTimmyClientRpc);
            }
        });

    }

    [ClientRpc]
    public void FinishTimmyClientRpc()
    {
        Sequence timmySequence = DOTween.Sequence();

        //���� ȭ�� ���� �� Ƽ�� �ʱ�ȭ
        timmySequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            ActiveCamera(0);
            sleepTimmy.SetActive(true);
            moveTimmy.SetActive(false);
            moveTimmy.transform.position = startPos;
            moveTimmy.transform.rotation = startRot;
        });
        timmySequence.AppendInterval(1f);

        // ȭ�� ����� (alpha: 1 �� 0, 0.5��)
        timmySequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 0f, 0.5f));
    }

    private void SetAlpha(float alpha)
    {
        imageColor.a = alpha;
        fadeImage.color = imageColor;
    }

    public void ActiveCamera(int cameraIndex)
    {
        foreach (var camera in cameras)
        {
            camera.Priority = 0;
        }

        cameras[cameraIndex].Priority = 10;
        activeCameraIndex = cameraIndex;
    }
}
