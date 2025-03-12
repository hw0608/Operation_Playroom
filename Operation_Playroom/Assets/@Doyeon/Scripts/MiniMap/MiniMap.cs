using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : NetworkBehaviour
{
    private Renderer iconRenderer;
    private Character character;
    private bool IsLocalPlayer => IsOwner && NetworkManager.Singleton.LocalClientId == NetworkObject.OwnerClientId;

    public override void OnNetworkSpawn()
    {
        StartCoroutine(DelayedInitialization());
    }
    private IEnumerator DelayedInitialization()
    {
        yield return null;
        Transform parent = transform.parent;
        while (parent != null)
        {
            Debug.Log($"Parent: {parent.name}, Has Archer: {parent.GetComponent<Archer>() != null}, Has Character: {parent.GetComponent<Character>() != null}");
            Archer archer = parent.GetComponent<Archer>();
            if (archer != null)
            {
                character = archer as Character;
                if (character == null)
                {
                    Debug.LogError("ĳ���� Ÿ������ ����");
                    yield break;
                }
                break;
            }
            parent = parent.parent;
        }

        if (character == null)
        {
            Debug.LogError("Archer ������Ʈ ����");
            yield break;
        }

        iconRenderer = GetComponent<Renderer>();
        if (iconRenderer == null)
        {
            Debug.LogError("������ ����");
            yield break;
        }

        //UpdateIconColor(character.team.Value);
        //character.team.OnValueChanged += OnTeamValueChanged;
    }
    //public override void OnNetworkDespawn()
    //{
    //    if (character != null)
    //    {
    //        character.team.OnValueChanged -= OnTeamValueChanged;
    //    }
    //}
    //private void OnTeamValueChanged(int previousValue, int newValue)
    //{
    //    UpdateIconColor(newValue);
    //}
    [ClientRpc]
    private void UpdateIconColorClientRpc(int newTeam)
    {
        if (iconRenderer == null)
        {
            iconRenderer = GetComponent<Renderer>();
        }

        if (IsLocalPlayer) 
        {
            iconRenderer.material.color = Color.green;
        }
        else
        {
            switch (newTeam)
            {
                case 0:
                    iconRenderer.material.color = Color.red; // ������
                    break;
                case 1:
                    iconRenderer.material.color = Color.blue; // �����
                    break;
                case -1:
                    iconRenderer.material.color = Color.red; // �� ���Ҵ�
                    break;
            }
        }
    }
}
