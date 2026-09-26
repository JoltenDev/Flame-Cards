using System.Collections.Generic;
using UnityEngine;

public class Jumppad : GridItem
{
    [SerializeField] List<GridItem> connectedEndpoints = new List<GridItem>();
    [SerializeField] int numEndpoints;

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
        clonedWall.name = "Jumppad";

        return clonedWall.GetComponent<Jumppad>();
    }

    /* When a jumppad gets placed, you get the option to place as many endpoints
     * as is specified in numEndpoints, when that jumppad has a card selected on
     * it, it selects all the endpoints on the grid as available */
}
