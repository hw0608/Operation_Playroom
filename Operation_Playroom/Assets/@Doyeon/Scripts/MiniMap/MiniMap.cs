using Unity.Netcode;
using UnityEngine;

public class MiniMap : NetworkBehaviour
{
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private GameObject[] occupyPoints;


}
