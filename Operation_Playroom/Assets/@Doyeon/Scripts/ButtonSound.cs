using System.Globalization;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    public AudioClip hoverSound; 
    public AudioClip clickSound; 
    private AudioSource audioSource;

    public Button button;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 버튼 이벤트 연결
        button.onClick.AddListener(OnButtonClicked);

        // 호버링 이벤트 설정
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnButtonHovered(); });
        trigger.triggers.Add(entry);
    }

    

    public void OnButtonHovered()
    {
        audioSource.PlayOneShot(hoverSound);
        Debug.Log("호버소리"); 
    }

    
    public void OnButtonClicked()
    {
        audioSource.PlayOneShot(clickSound);
        Debug.Log("클릭소리");   
    }

    
    
}
