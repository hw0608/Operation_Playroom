using UnityEngine;

public interface IEnemyTarget
{
    Transform GetTransform();  // 적의 Transform 가져오기
    int GetTeam();             // 적의 팀 정보 가져오기
}
