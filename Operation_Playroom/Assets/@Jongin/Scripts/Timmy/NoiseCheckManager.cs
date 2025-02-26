using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NoiseCheckManager : NetworkBehaviour
{
    public Image sleepGage;
    public Image noiseGage;

    private NetworkVariable<float> sleep = new NetworkVariable<float>(100f); // 초기 수면 게이지
    private float noise; // 개별 클라이언트의 소음 값

    private static float totalNoise = 0f; // 서버에서 관리하는 총 소음 값

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    AddNoiseGage(2);
        //}

        if (noise > 0)
        {
            noise -= Time.deltaTime;
        }

        noiseGage.fillAmount = noise / 30f; // 개별 클라이언트의 UI 업데이트
        sleepGage.fillAmount = sleep.Value / 100f; // 서버의 sleep 값 기반으로 UI 업데이트
    }

    void AddNoiseGage(float value)
    {
        noise += value;
        noise = Mathf.Clamp(noise, 0, 30);
        if (IsClient) // 클라이언트에서 서버에 요청
        {
            SubmitNoiseToServerRpc(value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitNoiseToServerRpc(float noiseValue)
    {
        totalNoise += noiseValue; // 서버에서 전체 noise 값 관리
    }

    void FixedUpdate()
    {
        if (IsServer) // 서버에서만 sleep 값 조정
        {
            DecreaseSleepGage();
        }
    }

    void DecreaseSleepGage()
    {
        float change = 0f;

        if (totalNoise >= 0 && totalNoise < 5)
        {
            change = 1f * Time.deltaTime; // 초당 1 증가
        }
        else if (totalNoise >= 10 && totalNoise < 20)
        {
            change = -1f * Time.deltaTime; // 초당 1 감소
        }
        else if (totalNoise >= 20 && totalNoise <= 30)
        {
            change = -2f * Time.deltaTime; // 초당 2 감소
        }

        sleep.Value = Mathf.Clamp(sleep.Value + change, 0, 100); // sleep 값 범위 제한
        totalNoise = Mathf.Max(0, totalNoise - Time.deltaTime); // 점진적으로 noise 감소
    }
}
