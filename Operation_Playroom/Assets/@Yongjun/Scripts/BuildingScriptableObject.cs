using UnityEngine;

[CreateAssetMenu(fileName = "BuildingScriptableObject", menuName = "Create BuildingScriptableObject")]
public class BuildingScriptableObject : ScriptableObject
{
    [Header("Information")]
    public int health;
}
