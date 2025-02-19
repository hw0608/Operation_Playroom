using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
class TMPList
{
    public TMP_Text[] texts;
}

public class LobbySceneUI : MonoBehaviour
{
    [SerializeField] TMPList[] playerNameTexts = new TMPList[2];
    [SerializeField] TMPList[] playerReadyTexts = new TMPList[2];


    public void OnBackButtonPressed()
    {
        
    }
}
