using System;
using UnityEngine;

public class CardLayoutGroup : MonoBehaviour
{
    [Header("Selected Card")]
    [SerializeField] Card selectedCard;
    [SerializeField] CardData.CardTypes bindType;

    [Header("Displayed Card")]
    [SerializeField] Transform selectedCardSlot;
    Card displayedCard;

    [Header("Layout Settings")]
    [Range(0.01f, 25f)] [SerializeField] float layoutSpeed;
    [Range(-1, 1)] [SerializeField] float horizontalSpacing;
    [Range(-1, 1)] [SerializeField] float verticalSpacing;
    [Range(-1, 1)] [SerializeField] float forwardSpacing;
    [Range(0, 1)] [SerializeField] float selectionOffset;
    [Range(0, 50)] [SerializeField] float stress;

    void Update()
    {
        LayoutCards();
    }

    public void SelectCard(Card card)
    {
        if (card != null)
        {
            if (card.GetData().Type != bindType)
            {
                selectedCard = null;
                DisplayCard(selectedCard);
                return;
            }
        }

        selectedCard = card;
        DisplayCard(selectedCard);
    }

    public void DisplayCard(Card card)
    {
        if (displayedCard != null)
        {
            Destroy(displayedCard.gameObject);
            displayedCard = null;
        }

        if (card == null) return;

        var clonedCard = card.CreateClone(selectedCardSlot, 0);
        displayedCard = clonedCard.GetComponent<Card>();
    }

    void LayoutCards()
    {
        var cards = GetComponentsInChildren<Card>();

        int change = 0;
        float forwardChange = 0;
        float verticalChange = 0;

        float[] baseXPositions = new float[cards.Length];
        int tempChange = 0;
        for (int i = 0; i < cards.Length; i++)
        {
            baseXPositions[i] = tempChange * horizontalSpacing;

            if (i % 2 == 0) tempChange++;
            tempChange *= -1;
        }

        float selectedBaseX = 0;
        if (selectedCard != null)
        {
            int selectedIndex = Array.IndexOf(cards, selectedCard);
            if (selectedIndex >= 0)
            {
                selectedBaseX = baseXPositions[selectedIndex];
            }
        }

        for (int i = 0; i < cards.Length; i++)
        {
            float offset = 0;

            if (selectedCard != null && cards[i] != selectedCard)
            {
                if (baseXPositions[i] < selectedBaseX)
                    offset = -selectionOffset;
                else
                    offset = selectionOffset;
            }

            Vector3 targetPosition = new Vector3(baseXPositions[i] + offset, verticalChange, forwardChange * forwardSpacing);
            cards[i].transform.localPosition = Vector3.Lerp(cards[i].transform.localPosition, targetPosition, layoutSpeed * Time.deltaTime);
            cards[i].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, stress * forwardChange * forwardSpacing));

            if (i % 2 == 0)
            {
                change++;
                forwardChange++;
            }

            change *= -1;
            forwardChange *= -1;

            verticalChange += verticalSpacing;
        }
    }
}
