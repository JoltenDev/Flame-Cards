using UnityEngine;

public class Jumppad : GridItem
{
    public override GridItem CreateClone(Transform parent)
    {
        var clonedWall = Instantiate(transform, parent);
        clonedWall.localPosition = Vector3.zero;
        clonedWall.localScale = new Vector3(0.5f, 0.2f, 0.5f);
        clonedWall.localRotation = Quaternion.Euler(0, 0, 0);
        clonedWall.gameObject.layer = 9;
        clonedWall.name = "Jumppad";

        return clonedWall.GetComponent<Jumppad>();
    }
}
