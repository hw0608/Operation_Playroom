using System.Globalization;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    public AudioClip hoverSound; 
    public AudioClip clickSound;
    public AudioClip backgroundMusic;
    
    private AudioSource audioSource;

    private string[] scenesMusic = {"LoadingScene", "MainScene", "LobbyScene" };

    public Button button;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.clip = backgroundMusic;
        audioSource.loop = true;

        CheckScenePlayMusic();

        // 버튼 이벤트 연결
        button.onClick.AddListener(OnButtonClicked);

        // 호버링 이벤트 설정
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnButtonHovered(); });
        trigger.triggers.Add(entry);
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckScenePlayMusic();
    }
    void CheckScenePlayMusic()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        
        bool isPlayMusic = System.Array.Exists(scenesMusic, sceneName => sceneName == currentScene);

        if (isPlayMusic && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (!isPlayMusic && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    // 호버링 사운드
    public void OnButtonHovered()
    {
        audioSource.PlayOneShot(hoverSound);
    }

    // 클릭 사운드
    public void OnButtonClicked()
    {
        audioSource.PlayOneShot(clickSound);
    }

    
    
}
