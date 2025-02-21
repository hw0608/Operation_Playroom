using UnityEngine;

public class GameManagerYJ : MonoBehaviour
{
    void Start()
    {
        ResourceManagerYJ.Instance.AddResource("Red Team", 7);
        ResourceManagerYJ.Instance.AddResource("Blue Team", 7);
    }
}
