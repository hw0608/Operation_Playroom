using TMPro;
using UnityEngine;

public class MessagePopup : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;

    public void SetText(string text)
    {
        messageText.text = text;
    }

    public void OnCloseButtonPressed()
    {
        Destroy(gameObject);
    }
}
