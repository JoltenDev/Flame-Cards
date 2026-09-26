using UnityEngine;

public abstract class GridItem : MonoBehaviour
{
    [Header("Item Settings")]
    public int amount = 0;
    public bool takeSteps = true;
    public GridBlock currentBlock;

    public abstract GridItem GetItem();

    public abstract GridItem CreateClone(Transform parent);

    public virtual GridItem TryPlaceItem(GridBlock block, GridItem selectedItem, GridSelect select = null)
    {
        if (block.Available)
        {
            GridItem placedItem = block.PlaceObject(ref selectedItem);
            placedItem.DestroyUI();
        }

        return selectedItem;
    }

    public virtual GridItem Select(GridBlock block = null, GridSelect select = null)
    {
        return this;
    }

    public virtual bool BlockPath(bool elevated = false)
    {
        return false;
    }

    public virtual bool BlockPathPastItem()
    {
        return false;
    }

    public virtual bool CanStack()
    {
        return false;
    }

    public virtual void DestroyUI()
    {
        if (TryGetComponent<ItemUI>(out ItemUI ui))
            Destroy(ui.amountUI.gameObject);
    }
}
