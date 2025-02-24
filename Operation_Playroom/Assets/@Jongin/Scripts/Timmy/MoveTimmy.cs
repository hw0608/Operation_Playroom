using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Define;
public class MoveTimmy : MonoBehaviour
{
    public List<Transform> path = new List<Transform>();
    ETimmyState timmyState = ETimmyState.Sleep;

    int pathIndex = 0;
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            MoveToPath(pathIndex);
        }
    }

    void MoveToPath(int index)
    {
        GetComponent<NavMeshAgent>().SetDestination(path[index].position);
        pathIndex++;
    }
}
