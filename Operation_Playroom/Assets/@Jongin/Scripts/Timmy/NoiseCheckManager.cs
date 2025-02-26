using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NoiseCheckManager : NetworkBehaviour
{
    public Image sleepGage;
    public Image noiseGage;

    private NetworkVariable<float> sleep = new NetworkVariable<float>(100f); // �ʱ� ���� ������
    private float noise; // ���� Ŭ���̾�Ʈ�� ���� ��

    private static float totalNoise = 0f; // �������� �����ϴ� �� ���� ��

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

        noiseGage.fillAmount = noise / 30f; // ���� Ŭ���̾�Ʈ�� UI ������Ʈ
        sleepGage.fillAmount = sleep.Value / 100f; // ������ sleep �� ������� UI ������Ʈ
    }

    void AddNoiseGage(float value)
    {
        noise += value;
        noise = Mathf.Clamp(noise, 0, 30);
        if (IsClient) // Ŭ���̾�Ʈ���� ������ ��û
        {
            SubmitNoiseToServerRpc(value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitNoiseToServerRpc(float noiseValue)
    {
        totalNoise += noiseValue; // �������� ��ü noise �� ����
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

        if (totalNoise >= 0 && totalNoise < 5)
        {
            change = 1f * Time.deltaTime; // �ʴ� 1 ����
        }
        else if (totalNoise >= 10 && totalNoise < 20)
        {
            change = -1f * Time.deltaTime; // �ʴ� 1 ����
        }
        else if (totalNoise >= 20 && totalNoise <= 30)
        {
            change = -2f * Time.deltaTime; // �ʴ� 2 ����
        }

        sleep.Value = Mathf.Clamp(sleep.Value + change, 0, 100); // sleep �� ���� ����
        totalNoise = Mathf.Max(0, totalNoise - Time.deltaTime); // ���������� noise ����
    }
}
