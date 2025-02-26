using DG.Tweening;
using TMPro;
using UnityEngine;

public class MessagePopup : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;
    [SerializeField] CanvasGroup messageArea;

    public void SetText(string text)
    {
        messageText.text = text;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        messageArea.alpha = 0f;

        Sequence seq = DOTween.Sequence();

        seq.Append(messageArea.DOFade(1f, 0.3f)).Join(messageArea.transform.DOScale(1.1f, 0.2f));
        seq.Insert(0.2f, messageArea.transform.DOScale(1f, 0.1f));

        seq.Play();
    }

    public void Close()
    {
        messageArea.alpha = 1f;
        Sequence seq = DOTween.Sequence();

        seq.Append(messageArea.DOFade(0f, 0.3f));
        seq.Append(messageArea.transform.DOScale(1.1f, 0.1f));
        seq.Append(messageArea.transform.DOScale(0.2f, 0.2f));

        seq.Play().OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
