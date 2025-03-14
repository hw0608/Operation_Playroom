using System.Collections;
using Unity.Cinemachine;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using static Define;
public class KingCam : MonoBehaviour
{
    public ETeam team;
    [SerializeField] GameObject target;
    void Start()
    {
        StartCoroutine(CamRoutine());
    }

    // 카메라 할당 루틴
    IEnumerator CamRoutine()
    {
        yield return new WaitUntil(() => FindObjectsByType<KingTest>(FindObjectsSortMode.None).Length >= 1);
        KingTest[] kings = FindObjectsByType<KingTest>(FindObjectsSortMode.None);
        target = kings[0].team.Value == (int)team ? kings[0].gameObject : null;
        if (target != null)
        {
            GetComponent<CinemachineCamera>().Follow = target.transform;
            GetComponent<CinemachineCamera>().LookAt = target.transform;
        }
    }
}
