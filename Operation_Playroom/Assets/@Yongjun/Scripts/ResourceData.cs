using UnityEngine;

public enum Owner { Red, Blue, Neutral }

public class ResourceData : MonoBehaviour
{
    [SerializeField] Owner currentOwner = Owner.Neutral;
    
    public Owner CurrentOwner
    {
        get { return currentOwner; }
        set { currentOwner = value; }
    }
}
