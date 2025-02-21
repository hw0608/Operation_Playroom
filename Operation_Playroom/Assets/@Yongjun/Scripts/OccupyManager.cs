using System.Collections.Generic;
using UnityEngine;

public class OccupyManager : MonoBehaviour
{
    public static OccupyManager instance;

    public List<GameObject> occupyPoints;
    public GameObject occupyPrefab;
    [SerializeField] private List<GameObject> generatedOccupy;
    public enum TeamType { Neutral, Red, Blue }

    void Awake()
    {
        if (instance != null) instance = this;
    }

    void Start()
    {
        generatedOccupy = new List<GameObject>();
        GenerateOccupy();
    }

    void GenerateOccupy()
    {
        foreach (GameObject points in occupyPoints)
        {
            GameObject occupy = CreateOccupy(points.transform.position);
            generatedOccupy.Add(occupy);
        }
    }
   
    GameObject CreateOccupy(Vector3 pos)
    {
        GameObject occupyObject = Instantiate(occupyPrefab, pos, Quaternion.identity);
        
        return occupyObject;
    }
}