using UnityEngine;

public class TitleSceneUI : MonoBehaviour
{
    [SerializeField] GameObject LobbyCanvas;
    [SerializeField] GameObject StartOptionPanel;

    void Init()
    {
        StartOptionPanel.SetActive(false);
        LobbyCanvas.SetActive(false);
    }

    private void Start()
    {
        Init();
    }

    public void OnStartButtonPressed()
    {
        StartOptionPanel.SetActive(true);
    }

    public async void OnFindMatchButtonPressed()
    {
        StartOptionPanel.SetActive(false);

    }
}
