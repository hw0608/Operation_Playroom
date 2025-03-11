using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NoiseCheckManager : NetworkBehaviour
{
    public Image sleepGauge;
    public Image noiseGauge;

    private NetworkVariable<float> sleep = new NetworkVariable<float>(100f); // 초기 수면 게이지
    private float noise; // 개별 클라이언트의 소음 값

    private NetworkVariable<float> totalNoise = new NetworkVariable<float>(0); // 서버에서 관리하는 총 소음 값

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            AddNoiseGage(2);
        }

        if (IsClient)
        {
            noiseGauge.fillAmount = totalNoise.Value / 30f; // 개별 클라이언트의 UI 업데이트
            sleepGauge.fillAmount = sleep.Value / 100f; // 서버의 sleep 값 기반으로 UI 업데이트
        }
    }

    void AddNoiseGage(float value)
    {
        if (IsClient) // 클라이언트에서 서버에 요청
        {
            SubmitNoiseToServerRpc(value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitNoiseToServerRpc(float noiseValue)
    {
        totalNoise.Value += noiseValue; // 서버에서 전체 noise 값 관리
        totalNoise.Value = Mathf.Clamp(totalNoise.Value, 0, 30);
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

        if (totalNoise.Value >= 0 && totalNoise.Value < 5)
        {
            change = 1f * Time.deltaTime; // 초당 1 증가
        }
        else if (totalNoise.Value >= 10 && totalNoise.Value < 20)
        {
            change = -1f * Time.deltaTime; // 초당 1 감소
        }
        else if (totalNoise.Value >= 20 && totalNoise.Value <= 30)
        {
            change = -2f * Time.deltaTime; // 초당 2 감소
        }

        sleep.Value = Mathf.Clamp(sleep.Value + change, 0, 100); // sleep 값 범위 제한
        totalNoise.Value = Mathf.Max(0, totalNoise.Value - Time.deltaTime); // 점진적으로 noise 감소
    }
}
