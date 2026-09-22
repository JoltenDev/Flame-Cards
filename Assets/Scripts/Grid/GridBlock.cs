using System.Collections.Generic;
using UnityEngine;

public class GridBlock : MonoBehaviour
{
    [SerializeField] int position;
    [SerializeField] int row;
    [SerializeField] bool available;

    [Header("Object Parent")]
    [SerializeField] Transform placement;

    [Header("Object & Preview")]
    [SerializeField] GridItem heldObject;
    [SerializeField] GridItem heldObjectPreview;
    [SerializeField] List<GridItem> previousHeldObjects = new List<GridItem>();

    public int Position { get { return position; } set { position = value; } }
    public int Row { get { return row; } set { row = value; } }
    public bool Available { get { return available; } set { available = value; } }

    void Update()
    {
        if (heldObject == null && previousHeldObjects.Count > 0)
        {
            int lastIndex = previousHeldObjects.Count - 1;
            GridItem restoredItem = previousHeldObjects[lastIndex];
            previousHeldObjects.RemoveAt(lastIndex);

            if (restoredItem != null)
            {
                heldObject = restoredItem;
                heldObject.gameObject.SetActive(true);
            }
        }
    }

    public T PlaceObject<T>(ref T obj, bool preview = false) where T : GridItem
    {
        if (preview)
        {
            if (heldObject != null || heldObjectPreview != null)
                return null;
        }

        var clonedObject = obj.CreateClone(placement);

        if (clonedObject == null)
        {
            Debug.LogError("Could not clone the object.");
            return null;
        }

        if (preview)
        {
            clonedObject.name += " Preview";
            heldObjectPreview = clonedObject;
        }
        else
        {
            available = false;

            if (heldObject != null)
            {
                previousHeldObjects.Add(heldObject);
            }

            obj.amount -= 1;
            if (obj.amount <= 0)
            {
                Destroy(obj.gameObject);
            }
            
            heldObject = clonedObject;
        }

        return clonedObject as T;
    }

    public void RemoveObject(bool preview = false)
    {
        if (preview)
        {
            if (heldObjectPreview == null) return;

            Destroy(heldObjectPreview.gameObject);
            heldObjectPreview = null;
            return;
        }

        if (heldObject != null)
        {
            Destroy(heldObject.gameObject);
            heldObject = null;
        }

        if (!Occupied<GridItem>(out _))
        {
            available = true;
        }
    }

    public bool Occupied<T>(out T item) where T : class
    {
        if (heldObject != null)
        {
            item = heldObject.GetComponent<T>() ?? heldObject as T;

            if (item != null)
            {
                return true;
            }
        }

        item = null;
        return false;
    }
}
