using UnityEngine;
using UnityEngine.UI;

public class NoiseCheckManager : MonoBehaviour
{

    public Image sleepGage;
    public Image noiseGage;

    float sleep;
    float noise;


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            AddNoiseGage(2);
        }

        if (noise > 0)
        {
            noise -= Time.deltaTime;
        }

        noiseGage.fillAmount = noise / 20f;
    }

    void AddNoiseGage(float addValue)
    {
        noise += addValue;
    }
}
