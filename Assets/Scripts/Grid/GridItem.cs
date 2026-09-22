using UnityEngine;

public abstract class GridItem : MonoBehaviour
{
    [Header("Item Settings")]
    public int amount;

    public abstract GridItem CreateClone(Transform parent);
}
