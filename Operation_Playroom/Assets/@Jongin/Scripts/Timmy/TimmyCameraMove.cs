using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class TimmyCameraMove : MonoBehaviour
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
        imageColor.a = 0; // 시작 시 투명
        fadeImage.color = imageColor;
        startPos = moveTimmy.transform.position;
        startRot = moveTimmy.transform.rotation;
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.V))
        {
            StartTimmy();
        }
    }
    public void StartTimmy()
    {
        Sequence timmySequence = DOTween.Sequence();

        //티미 일어나는 연출
        timmySequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            ActiveCamera(1);
        });
        timmySequence.AppendInterval(1f);

        timmySequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 0f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            sleepTimmy.GetComponent<Animator>().SetTrigger("WakeUp");
        });
        timmySequence.AppendInterval(3f);

        //자는 티미와 움직이는 티미 교체
        timmySequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            //자는 티미 초기화
            sleepTimmy.GetComponent<Animator>().SetTrigger("Sleep");
            sleepTimmy.SetActive(false);
            moveTimmy.SetActive(true);
            ActiveCamera(2);
        });
        timmySequence.AppendInterval(1f);

        timmySequence.Append(DOTween.To(() => imageColor.a, x => SetAlpha(x), 0f, 0.5f));

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

        // 화면 밝아짐 (alpha: 1 → 0, 0.5초)
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
