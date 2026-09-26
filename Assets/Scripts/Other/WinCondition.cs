using UnityEngine;

public class WinCondition : GridItem
{
    public override GridItem GetItem()
    {
        return this;
    }

    public override GridItem CreateClone(Transform parent)
    {
        var clonedWall = Instantiate(transform, parent);
        clonedWall.localPosition = Vector3.zero;
        clonedWall.localScale = new Vector3(0.5f, 0.2f, 0.5f);
        clonedWall.localRotation = Quaternion.Euler(0, 0, 0);
        clonedWall.gameObject.layer = 10;
        clonedWall.name = "Win Condition";

        return clonedWall.GetComponent<WinCondition>();
    }
}
