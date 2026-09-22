using System.Collections;
using UnityEngine;

public class Card : GridItem
{
    [Header("Card Data")]
    [SerializeField] CardData cardData;
    [SerializeField] CardUI cardUI;

    [Header("Attributes")]
    [SerializeField] int steps;
    [SerializeField] int attack;

    [SerializeField] bool placed;

    [Header("Grid Position")]
    [SerializeField] int position;
    [SerializeField] int horizontalPosition;

    [Header("Selection Settings")]
    [SerializeField] Vector3 defaultScale = new Vector3(0.65f, 1, 0.02f);
    [SerializeField] Vector3 smallScale = new Vector3(0.65f / 1.25f, 1 / 1.25f, 0.02f / 1.25f);

    Coroutine scaleCoroutine;

    public CardData Data { get { return cardData; } set { cardData = value; } }
    public int Steps { get { return steps; } }
    public int Attack { get { return attack; } }
    public bool Placed { get { return placed; } set { placed = value; } }

    public int Position { get { return position; } }
    public int HorizontalPosition { get { return horizontalPosition; } }

    public void ApplyStats()
    {
        steps = cardData.Move;
        attack = cardData.Attack;
    }

    public void Move(int amount, int newPosition, int newHPosition)
    {
        if (steps <= 0)
        {
            steps = 0;
            return;
        }
        
        position = newPosition;
        horizontalPosition = newHPosition;

        steps -= amount;
        cardUI.ModifyUI(CardUI.UIType.Move, steps.ToString());
    }

    public void Scale(bool scale = true)
    {
        if (!placed) return;

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }

        scaleCoroutine = StartCoroutine(Scale(0.25f, scale));
    }

    IEnumerator Scale(float duration, bool scale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 newScale = scale ? smallScale : defaultScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, newScale, t);

            yield return null;
        }

        transform.localScale = newScale;
    }

    public override GridItem CreateClone(Transform parent)
    {
        var clonedCard = Instantiate(transform, parent);
        clonedCard.localPosition = Vector3.zero;
        clonedCard.localRotation = Quaternion.Euler(90, 0, 0);
        clonedCard.gameObject.layer = 8;
        clonedCard.name = "Card";

        return clonedCard.GetComponent<Card>();
    }

    public Card CreateCardDisplay(Transform parent, int layer)
    {
        var clonedCard = Instantiate(transform, parent);
        clonedCard.localPosition = new Vector3(0, 0, 0);
        clonedCard.localRotation = Quaternion.Euler(180, -10, 180);
        clonedCard.localScale = new Vector3(0.65f, 1.1f, 0.03f);
        clonedCard.gameObject.layer = layer;
        clonedCard.name = "Displayed Card";

        return clonedCard.GetComponent<Card>();
    }
}
