using UnityEngine;

public class Wall : GridItem
{
    public override GridItem CreateClone(Transform parent)
    {
        var clonedWall = Instantiate(transform, parent);
        clonedWall.localPosition = Vector3.zero;
        clonedWall.localScale = new Vector3(0.5f, 1f, 0.5f);
        clonedWall.localRotation = Quaternion.Euler(0, 0, 0);
        clonedWall.gameObject.layer = 9;
        clonedWall.name = "Wall";

        return clonedWall.GetComponent<Wall>();
    }
}
