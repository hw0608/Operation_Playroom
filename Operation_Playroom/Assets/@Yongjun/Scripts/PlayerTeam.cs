using UnityEngine;

public enum Team { Red, Blue }

public class PlayerTeam : MonoBehaviour
{
    public Team currentTeam;

    public Team CurrentTeam => currentTeam;
}
