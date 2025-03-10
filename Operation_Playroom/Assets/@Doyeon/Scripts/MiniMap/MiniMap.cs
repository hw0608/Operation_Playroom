using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : NetworkBehaviour
{
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RectTransform minimapRect;
    [SerializeField] private Image[] playerIcons; // �÷��̾� UI�̹��� �迭 

    private Character[] characters; // �÷��̾� ĳ���� 
    private Vector2 worldSize = new Vector2(3f, 2f); // ���� ũ��

    private void Start()
    {
        foreach (Image icon in playerIcons)
        {
            icon.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        if (!IsSpawned) return;
        characters = FindObjectsOfType<Character>();
        int iconIndex = 0;

        foreach (var character in characters)
        {
            if (iconIndex >= playerIcons.Length) break;

            Vector2 screenPos = WorldToScreenPosition(character.transform.position);
            Vector2 minimapPos = ScreenToMinimapPosition(screenPos);

            Image icon = playerIcons[iconIndex];
            icon.gameObject.SetActive(true);
            icon.rectTransform.anchoredPosition = minimapPos;

            // ���� ���� ���� ����
            if (character.IsLocalPlayer)
            {
                icon.color = Color.green;
            }
            else
            {
                icon.color = character.team.Value == 0 ? Color.red : Color.blue; // ������/�����
            }

            iconIndex++;
        }

        // ���� ������ ��Ȱ��ȭ
        for (int i = iconIndex; i < playerIcons.Length; i++)
        {
            playerIcons[i].gameObject.SetActive(false);
        }
    }
    
    // ���� ��ǥ�� ȭ�� ��ǥ�� ��ȯ
    private Vector2 WorldToScreenPosition(Vector3 worldPos)
    {
        return minimapCamera.WorldToScreenPoint(worldPos);
    }

    // ȭ�� ��ǥ�� �̴ϸ� UI ��ǥ�� ��ȯ
    private Vector2 ScreenToMinimapPosition(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect, screenPos, null, out Vector2 localPoint);
        return localPoint;
    }

}
