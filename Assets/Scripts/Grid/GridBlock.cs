using UnityEngine;

public class GridBlock : MonoBehaviour
{
    [SerializeField] int position;
    [SerializeField] int row;
    [SerializeField] bool available;

    [SerializeField] Transform placement;
    [SerializeField] int placedCardLayer;

    [SerializeField] Card heldCard;
    [SerializeField] Card heldCardPreview;

    public int Position { get { return position; } set { position = value; } }
    public int Row { get { return row; } set { row = value; } }
    public bool Available { get { return available; } set { available = value; } }

    public Card PlaceCard(ref Card card, bool preview = false)
    {
        if (preview)
        {
            if (heldCard != null || heldCardPreview != null)
                return null;
        }

        var clonedCard = card.CreateClone(placement, 7);
        if (preview)
            clonedCard.name += " Preview";

        if (clonedCard == null)
        {
            Debug.LogError($"Could not find Card on instantiated object: {clonedCard.name}");
            return null;
        }

        if (preview)
            heldCardPreview = clonedCard.GetComponent<Card>();
        else
        {
            heldCard = clonedCard.GetComponent<Card>();
            Destroy(card.gameObject);
        }

        return clonedCard.GetComponent<Card>();
    }

    public void RemoveCard(bool preview = false)
    {
        if (preview)
        {
            if (heldCardPreview == null)
                return;

            Destroy(heldCardPreview.gameObject);
            heldCardPreview = null;

            return;
        }

        if (heldCard == null)
            return;

        Destroy(heldCard.gameObject);
        heldCard = null;
    }

    public bool HasCard()
    {
        return heldCard != null;
    }

    public Card GetCard()
    {
        return heldCard;
    }
}
