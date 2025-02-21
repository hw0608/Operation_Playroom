using System;
using System.Collections;
using TMPro;
using Unity.Android.Gradle.Manifest;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerController : NetworkBehaviour
{

    public static Action<PlayerController> OnPlayerSpawn;
    public static Action<PlayerController> OnPlayerDespawn;

    Vector3 velocity;
    bool isGrounded;
    bool isPlayable;

    Rigidbody rb;
    Character character;
    CinemachineCamera cam;

    void Start()
    {
        character = GetComponent<Character>();
        rb = GetComponent<Rigidbody>();

        if (IsOwner)
        {
            StartCoroutine(CamRoutine());
        }

        character.SetHP();
    }
    void FixedUpdate()
    {
        if (!IsOwner) return;

        // 플레이어가 아니거나 플레이 가능상황이 아니면 리턴
        if (!IsOwner || !isPlayable) return;

        character.Move(cam, rb); // 캐릭터 이동

    }

    private void Update()
    {
        if (!IsOwner) return;

        character.HandleInput(); // 캐릭터 입력처리
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            OnPlayerSpawn?.Invoke(this);
        }

        if (!IsOwner) return;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawn?.Invoke(this);
        }
    }
    void AssignCamera()
    {
        cam = FindFirstObjectByType<CinemachineCamera>();

        if (cam != null)
        {
            cam.transform.position = transform.position;
            cam.Follow = transform;
            cam.LookAt = transform;
        }
        else
        {
            Debug.LogError("Cinemachine Camera를 찾을 수 없습니다.");
        }
    }

    IEnumerator CamRoutine()
    {
        yield return new WaitUntil(() => FindFirstObjectByType<CinemachineCamera>() != null);
        isPlayable = true;
        AssignCamera();
    }

}
