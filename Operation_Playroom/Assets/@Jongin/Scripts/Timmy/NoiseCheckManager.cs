using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NoiseCheckManager : NetworkBehaviour
{
    public Image sleepGauge;
    public Image noiseGauge;

    private NetworkVariable<float> sleep = new NetworkVariable<float>(100f); // �ʱ� ���� ������
    private float noise; // ���� Ŭ���̾�Ʈ�� ���� ��

    private NetworkVariable<float> totalNoise = new NetworkVariable<float>(0); // �������� �����ϴ� �� ���� ��

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            AddNoiseGage(2);
        }

        if (IsClient)
        {
            noiseGauge.fillAmount = totalNoise.Value / 30f; // ���� Ŭ���̾�Ʈ�� UI ������Ʈ
            sleepGauge.fillAmount = sleep.Value / 100f; // ������ sleep �� ������� UI ������Ʈ
        }
    }

    void AddNoiseGage(float value)
    {
        if (IsClient) // Ŭ���̾�Ʈ���� ������ ��û
        {
            SubmitNoiseToServerRpc(value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitNoiseToServerRpc(float noiseValue)
    {
        totalNoise.Value += noiseValue; // �������� ��ü noise �� ����
        totalNoise.Value = Mathf.Clamp(totalNoise.Value, 0, 30);
    }

    void FixedUpdate()
    {
        if (IsServer) // ���������� sleep �� ����
        {
            DecreaseSleepGage();
        }
    }

    void DecreaseSleepGage()
    {
        float change = 0f;

        if (totalNoise.Value >= 0 && totalNoise.Value < 5)
        {
            change = 1f * Time.deltaTime; // �ʴ� 1 ����
        }
        else if (totalNoise.Value >= 10 && totalNoise.Value < 20)
        {
            change = -1f * Time.deltaTime; // �ʴ� 1 ����
        }
        else if (totalNoise.Value >= 20 && totalNoise.Value <= 30)
        {
            change = -2f * Time.deltaTime; // �ʴ� 2 ����
        }

        sleep.Value = Mathf.Clamp(sleep.Value + change, 0, 100); // sleep �� ���� ����
        totalNoise.Value = Mathf.Max(0, totalNoise.Value - Time.deltaTime); // ���������� noise ����
    }
}
