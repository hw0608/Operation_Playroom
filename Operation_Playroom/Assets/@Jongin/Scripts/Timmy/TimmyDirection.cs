using DG.Tweening;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TimmyDirection : NetworkBehaviour
{
    public Image fadeImage;
    private Color imageColor;

    public CinemachineCamera[] cameras;
    private int activeCameraIndex = 0;

    public GameObject sleepTimmyPrefab;
    public GameObject moveTimmyPrefab;

    SleepTimmy sleepTimmy;
    MoveTimmy moveTimmy;


    public NetworkVariable<float> fadeAlpha = new NetworkVariable<float>(0);
    public NetworkVariable<int> cameraIndex = new NetworkVariable<int>(0);
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GameObject sleepTimmyObject = Instantiate(sleepTimmyPrefab);
            sleepTimmyObject.GetComponent<NetworkObject>().Spawn();
            sleepTimmy = sleepTimmyObject.GetComponent<SleepTimmy>();

            GameObject moveTimmyObject = Instantiate(moveTimmyPrefab);
            moveTimmyObject.GetComponent<NetworkObject>().Spawn();
            moveTimmy = moveTimmyObject.GetComponent<MoveTimmy>();
        }
        if (IsClient)
        {
            imageColor = fadeImage.color;
            imageColor.a = 0; // 시작 시 투명
            fadeImage.color = imageColor;

            cameraIndex.OnValueChanged -= ActiveCamera;
            cameraIndex.OnValueChanged += ActiveCamera;

            fadeAlpha.OnValueChanged -= ChangeImageAlpha;
            fadeAlpha.OnValueChanged += ChangeImageAlpha;
        }
    }

    public override void OnNetworkDespawn()
    {
        cameraIndex.OnValueChanged -= ActiveCamera;
        fadeAlpha.OnValueChanged -= ChangeImageAlpha;
        if (sleepTimmy != null && sleepTimmy.GetComponent<NetworkObject>().IsSpawned)
        {
            sleepTimmy.GetComponent<NetworkObject>().Despawn();
        }
        if (moveTimmy != null && moveTimmy.GetComponent<NetworkObject>().IsSpawned)
        {
            moveTimmy.GetComponent<NetworkObject>().Despawn();
        }
    }


    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    if (IsClient)
        //    {
        //        Debug.Log("client");
        //        imageColor = fadeImage.color;
        //        imageColor.a = 0; // 시작 시 투명
        //        fadeImage.color = imageColor;

        //        cameraIndex.OnValueChanged -= ActiveCamera;
        //        cameraIndex.OnValueChanged += ActiveCamera;

        //        fadeAlpha.OnValueChanged -= ChangeImageAlpha;
        //        fadeAlpha.OnValueChanged += ChangeImageAlpha;
        //    }

        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    if (IsServer)
        //    {
        //        Debug.Log("server");
        //        GameObject sleepTimmyObject = Instantiate(sleepTimmyPrefab);
        //        sleepTimmyObject.GetComponent<NetworkObject>().Spawn();
        //        sleepTimmy = sleepTimmyObject.GetComponent<SleepTimmy>();

        //        GameObject moveTimmyObject = Instantiate(moveTimmyPrefab);
        //        moveTimmyObject.GetComponent<NetworkObject>().Spawn();
        //        moveTimmy = moveTimmyObject.GetComponent<MoveTimmy>();
        //    }
        //}
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    if (IsServer)
        //    {
        //        Debug.Log("start");
        //        StartTimmy();
        //    }
        //}
    }

    public void StartTimmy()
    {
        Sequence timmySequence = DOTween.Sequence();

        //Fade in
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            //화면 전환
            cameraIndex.Value = 1;
        });
        timmySequence.AppendInterval(1f);
        //fade out
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 0f, 0.5f));


        timmySequence.AppendCallback(() =>
        {
            sleepTimmy.animator.SetTrigger("WakeUp");
        });
        timmySequence.AppendInterval(3f);

        //자는 티미와 움직이는 티미 교체
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            //자는 티미 초기화 후 끄기
            sleepTimmy.GetComponent<Animator>().SetTrigger("Sleep");
            sleepTimmy.timmyActive.Value = false;
            moveTimmy.timmyActive.Value = true;
            cameraIndex.Value = 2;
        });

        timmySequence.AppendInterval(1f);

        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 0f, 0.5f));

        //티미 움직이기
        timmySequence.AppendCallback(() =>
        {
            moveTimmy.GetComponent<MoveTimmy>().CallTimmy(FinishTimmy);
        });

    }

    public void FinishTimmy()
    {
        Sequence timmySequence = DOTween.Sequence();

        //원래 화면 복귀 및 티미 초기화
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            cameraIndex.Value = 0;
            sleepTimmy.timmyActive.Value = true;
            moveTimmy.timmyActive.Value = false;
            moveTimmy.ResetTimmy();
        });
        timmySequence.AppendInterval(1f);

        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 0f, 0.5f));
    }

    private void SetAlpha(float alpha)
    {
        fadeAlpha.Value = alpha;
    }

    public void ChangeImageAlpha(float oldValue, float newValue)
    {
        Debug.Log("222222");
        imageColor.a = newValue;
        fadeImage.color = imageColor;
    }

    public void ActiveCamera(int oldIndex, int newIndex)
    {
        foreach (var camera in cameras)
        {
            camera.Priority = 0;
        }
        Debug.Log("111111");
        cameras[newIndex].Priority = 10;
        activeCameraIndex = newIndex;
    }
}
