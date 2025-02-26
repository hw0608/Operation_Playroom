using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static ReturningState;

public interface IFormable
{
    void SoldierInitialize(Transform king, int formationIndex);
}

public class Soldier : NetworkBehaviour, IFormable 
{
    private Character character;
    public ISoldierState currentState; // ���� ����
    // ���� �ൿ ���� ������Ʈ
    private SoldierFormation soldierFormation;
    private SoldierAttack soldierAttack;
    private SoldierAnim soldierAnim;

    // ���� �� HP ����
    public NetworkVariable<int> state = new NetworkVariable<int>();  
    public NetworkVariable<float> maxHp = new NetworkVariable<float>(80f);
    public NetworkVariable<float> currentHp = new NetworkVariable<float>(80f);

    // ������ Ÿ��, �� ����
    public NetworkVariable<NetworkObjectReference> itemTarget = new NetworkVariable<NetworkObjectReference>();
    public NetworkVariable<NetworkObjectReference> enemyTarget = new NetworkVariable<NetworkObjectReference>();
    public NetworkVariable<int> team = new NetworkVariable<int>();

    public Transform king;
    public bool isSoldierDie = false;

    // ���� �ʱ�ȭ
    public void SoldierInitialize(Transform king, int formationIndex)
    {
        soldierFormation = GetComponent<SoldierFormation>(); // ����
        soldierAttack = GetComponent<SoldierAttack>();
        soldierAnim = GetComponent<SoldierAnim>();

        soldierFormation.SoldierFormationInitialize(king, formationIndex); // �ʱ�ȭ
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            // ������ Ÿ�� Ȯ�� �� ����ȭ
            NetworkObject itemNetObj;
            if (itemTarget.Value.TryGet(out itemNetObj) && !object.ReferenceEquals(itemNetObj, null))
            {
                itemTarget.Value = new NetworkObjectReference(itemNetObj);
            }
            // �� ���� ��û
            AssignTeamServerRpc(); 
        }
    }

    private void Start()
    {
        character = GetComponent<Character>();

        if (IsOwner)
        {
            SetState(0);
        }
    }

    private void Update()
    {
        if (isSoldierDie) return;

        if (IsOwner && currentState != null)
        {
            Transform enemyTransform = FindEnemies();
            if (enemyTransform != null)
            {
                SetState(1, enemyTransform.GetComponent<NetworkObject>().NetworkObjectId);
            }
            else
            {
                currentState.Execute(this);
            }
        }
        else if (currentState == null)
        {
            currentState = NSoldierState.GetStateFromInt(state.Value, king, itemTarget.Value, team.Value);
            currentState.EnterState(this);
        }
    }
    // �� �ν� �� ���� ��ȯ
    private Transform FindEnemies()
    {
        float detectionRange = 10f; // �� Ž�� �Ÿ�

        var allEnemies = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IEnemyTarget>();

        foreach (IEnemyTarget potentialEnemy in allEnemies)
        {
            // �ڽ��� �ƴϰ� ��� ���̸�, Ž�� ���� ��
            if (potentialEnemy.GetTransform() != this.transform && potentialEnemy.GetTeam() != team.Value)
            {
                float distance = Vector3.Distance(transform.position, potentialEnemy.GetTransform().position);

                if (distance <= detectionRange)
                {
                    return potentialEnemy.GetTransform();
                }
            } 
        }
        return null;
    }

    // ������ȯ �� ����ȭ
    public void SetState(int stateCode, ulong targetId)
    {
        if (IsServer)
        {
            if (stateCode == 1) // ����
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject enemyObj))
                {
                    // ��Ʈ��ũ ������ ����
                    enemyTarget.Value = enemyObj;
                    // SoldierState�� AttackingState��
                    currentState = new AttackingState(enemyObj.transform);
                    currentState.EnterState(this);

                    // ����ȭ
                    UpdateStateClientRpc(stateCode, team.Value);
                }
            }
            else if (stateCode == 2) // �ڿ�
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject itemObj))
                {
                    itemTarget.Value = itemObj;
                    currentState = new CollectingState(itemObj.transform);
                    currentState.EnterState(this);

                    // ����ȭ
                    UpdateStateClientRpc(stateCode, team.Value);
                }
            }
        }
        else
        {
            UpdateStateServerRpc(stateCode); // �������� ��û
        }
    }
    public void SetState(int stateCode)
    {
        if (IsServer)
        {
            if (stateCode == 0) // �� ���󰡱�
            {
                currentState = new FollowingState();
                currentState.EnterState(this);

                // Ŭ���̾�Ʈ ����ȭ
                UpdateStateClientRpc(stateCode, team.Value);
            }
        }
        else
        {
            // Ŭ���̾�Ʈ���� ������ ���� ��ȯ ��û
            UpdateStateServerRpc(stateCode);
        }
    }


    [ServerRpc(RequireOwnership = false)] // Owner�� �ƴ϶� ȣ�� ����
    private void UpdateStateServerRpc(int newState)
    {
        state.Value = newState; // �������� state.Value�� ������Ʈ 

        currentState = NSoldierState.GetStateFromInt(newState, king, itemTarget.Value, team.Value);
        currentState.EnterState(this);

        UpdateStateClientRpc(newState, team.Value); // ��� Ŭ���̾�Ʈ���� ���°��� �ް� ���¸� �缳�� 
    }
    [ServerRpc]
    private void AssignTeamServerRpc()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count % 2 == 0)
        {
            team.Value = 0; // �����
        }
        else
        {
            team.Value = 1; // ������
        }
    }

    [ClientRpc]
    private void UpdateStateClientRpc(int newState, int myTeam)
    {
        currentState = NSoldierState.GetStateFromInt(newState, king, itemTarget.Value, team.Value);
        currentState.EnterState(this);
    }

    public void Attack()
    {
        if (IsServer)
        {
            soldierAnim.SoldierAttackAnim();
        }
    }

    public void SetHP()
    {
        maxHp.Value = 80;
        currentHp.Value = maxHp.Value;
    }

    public void CollectItem(Transform item)
    {
        NetworkObject itemNetworkObject = item.GetComponent<NetworkObject>();
        itemTarget.Value = new NetworkObjectReference(itemNetworkObject);

        SetState(2, item.GetComponent<NetworkObject>().NetworkObjectId);
    }

    public void ReturnToKing()
    {
        NetworkObject itemNetworkObject;
        if (itemTarget.Value.TryGet(out itemNetworkObject) && itemTarget.Value.TryGet(out itemNetworkObject))
        {
            SetState(3);
        }
    }

    public void SoldierDie()
    {
        if (isSoldierDie) return;

        SetState(4);
    }
}