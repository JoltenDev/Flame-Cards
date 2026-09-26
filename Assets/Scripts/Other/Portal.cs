using UnityEngine;

public class Portal : GridItem
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
        clonedWall.gameObject.layer = 11;
        clonedWall.name = "Portal";

        return clonedWall.GetComponent<Portal>();
    }

    public override GridItem TryPlaceItem(GridBlock block, GridItem selectedItem, GridSelect select = null)
    {
        if (block.Available)
        {
            GridItem placedItem = block.PlaceObject(ref selectedItem);
            select.AddGridItem(ref placedItem);
            placedItem.DestroyUI();
        }

        return selectedItem;
    }

    public override bool BlockPathPastItem()
    {
        return true;
    }
}
