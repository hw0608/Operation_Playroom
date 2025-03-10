using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : NetworkBehaviour
{
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RectTransform minimapRect;
    [SerializeField] private Image[] playerIcons; // 플레이어 UI이미지 배열 

    private Character[] characters; // 플레이어 캐릭터 
    private Vector2 worldSize = new Vector2(3f, 2f); // 월드 크기

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

            // 팀에 따라 색상 설정
            if (character.IsLocalPlayer)
            {
                icon.color = Color.green;
            }
            else
            {
                icon.color = character.team.Value == 0 ? Color.red : Color.blue; // 레드팀/블루팀
            }

            iconIndex++;
        }

        // 남은 아이콘 비활성화
        for (int i = iconIndex; i < playerIcons.Length; i++)
        {
            playerIcons[i].gameObject.SetActive(false);
        }
    }
    
    // 월드 좌표를 화면 좌표로 변환
    private Vector2 WorldToScreenPosition(Vector3 worldPos)
    {
        return minimapCamera.WorldToScreenPoint(worldPos);
    }

    // 화면 좌표를 미니맵 UI 좌표로 변환
    private Vector2 ScreenToMinimapPosition(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect, screenPos, null, out Vector2 localPoint);
        return localPoint;
    }

}
