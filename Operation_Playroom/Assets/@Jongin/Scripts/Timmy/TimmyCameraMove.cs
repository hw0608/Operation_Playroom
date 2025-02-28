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

    public NetworkVariable<float> fadeAlpha = new NetworkVariable<float>(0);
    public NetworkVariable<bool> sleepTimmyActive = new NetworkVariable<bool>(true);
    public NetworkVariable<bool> moveTimmyActive = new NetworkVariable<bool>(false);
    public NetworkVariable<int> cameraIndex = new NetworkVariable<int>(0);
    void Start()
    {
        imageColor = fadeImage.color;
        imageColor.a = 0; // ���� �� ����
        fadeImage.color = imageColor;
        startPos = moveTimmy.transform.position;
        startRot = moveTimmy.transform.rotation;
        sleepTimmyActive.Value = sleepTimmy.activeSelf;
        moveTimmyActive.Value = moveTimmy.activeSelf;

        cameraIndex.OnValueChanged += ((oldValue, newValue) =>
        {
            ActiveCamera(newValue);
        });

        sleepTimmyActive.OnValueChanged += ((oldValue, newValue) =>
        {
            sleepTimmy.SetActive(newValue);
        });

        moveTimmyActive.OnValueChanged += ((oldValue, newValue) =>
        {
            moveTimmy.SetActive(newValue);
        });

        fadeAlpha.OnValueChanged += ((oldValue, newValue) =>
        {
            imageColor.a = newValue;
            fadeImage.color = imageColor;
        });
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (!IsServer) return;
            StartTimmy();
        }
    }

    public void StartTimmy()
    {
        Sequence timmySequence = DOTween.Sequence();

        //Ƽ�� �Ͼ�� ����
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            //ȭ�� ��ȯ
            cameraIndex.Value = 1;
        });
        timmySequence.AppendInterval(1f);
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 0f, 0.5f));


        timmySequence.AppendCallback(() =>
        {
            sleepTimmy.GetComponent<Animator>().SetTrigger("WakeUp");
        });
        timmySequence.AppendInterval(3f);

        //�ڴ� Ƽ�̿� �����̴� Ƽ�� ��ü
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            //�ڴ� Ƽ�� �ʱ�ȭ �� ����
            sleepTimmy.GetComponent<Animator>().SetTrigger("Sleep");
            sleepTimmyActive.Value = false;
            moveTimmyActive.Value = true;
            cameraIndex.Value = 2;
        });
        timmySequence.AppendInterval(1f);

        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 0f, 0.5f));

        //Ƽ�� �����̱�
        timmySequence.AppendCallback(() =>
        {
            moveTimmy.GetComponent<MoveTimmy>().CallTimmy(FinishTimmy);
        });

    }
    public void FinishTimmy()
    {
        Sequence timmySequence = DOTween.Sequence();

        //���� ȭ�� ���� �� Ƽ�� �ʱ�ȭ
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            cameraIndex.Value = 0;
            sleepTimmyActive.Value=true;
            moveTimmyActive.Value = false;
            moveTimmy.transform.position = startPos;
            moveTimmy.transform.rotation = startRot;
        });
        timmySequence.AppendInterval(1f);

        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 0f, 0.5f));
    }

    private void SetAlpha(float alpha)
    {
        fadeAlpha.Value = alpha;
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
