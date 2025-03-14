using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class BGMSound : MonoBehaviour
{
    public static BGMSound instance;

    public AudioClip backgroundMusic;
    public AudioMixer audioMixer;
    private AudioSource audioSource;

    private string[] scenesMusic = { "LoadingScene", "MainScene", "LobbyScene" };

    void Awake()
    {
        if (instance != null) { return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource가 없어서 자동으로 추가");
        }
        
        audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGM")[0];
        
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        
        CheckScenePlayMusic();
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
        if (audioSource == null) return;
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
}
