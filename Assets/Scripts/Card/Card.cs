using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : GridItem
{
    [Header("Card Data")]
    [SerializeField] CardData cardData;
    [SerializeField] CardUI cardUI;

    [Header("Attributes")]
    [SerializeField] int steps;
    [SerializeField] int attack;

    [Header("Status")]
    [SerializeField] bool placed;
    [SerializeField] bool teleportFatigue;

    [Header("Grid")]
    [SerializeField] int horizontalPosition;
    [SerializeField] int verticalPosition;

    [Header("Selection Settings")]
    [SerializeField] Vector3 defaultScale = new Vector3(0.65f, 1, 0.02f);
    [SerializeField] Vector3 smallScale = new Vector3(0.65f / 1.25f, 1 / 1.25f, 0.02f / 1.25f);

    Coroutine scaleCoroutine;

    public CardData Data { get { return cardData; } set { cardData = value; } }

    public bool Placed { get { return placed; } set { placed = value; } }

    void ApplyStats()
    {
        steps = cardData.Move;
        attack = cardData.Attack;

        cardUI.ModifyUI(CardUI.UIType.Move, steps.ToString());
        cardUI.ModifyUI(CardUI.UIType.Attack, attack.ToString());
    }

    void ApplyRush()
    {
        steps *= cardData.RushMultiplier;
        attack *= cardData.RushMultiplier;

        cardUI.ModifyUI(CardUI.UIType.Move, steps.ToString());
        cardUI.ModifyUI(CardUI.UIType.Attack, attack.ToString());
    }

    public void MoveCard(GridSelect select, GridBlock block, bool noSteps = false)
    {
        int deltaX = Math.Abs(block.X - horizontalPosition);
        int deltaY = Math.Abs(block.Y - verticalPosition);
        int totalDistance = Math.Max(deltaX, deltaY);

        if (noSteps)
        {
            SubtractSteps(0, block.Y, block.X);
            return;
        }

        if (totalDistance > steps) return;

        SubtractSteps(totalDistance, block.Y, block.X);
    }

    void SubtractSteps(int amount, int newYPosition, int newHPosition)
    {
        verticalPosition = newYPosition;
        horizontalPosition = newHPosition;

        if (steps <= 0) return;

        steps = Math.Max(0, steps - amount);
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

    public override GridItem Select(GridBlock block, GridSelect select)
    {
        Scale();

        if (currentBlock != null && currentBlock.LastHeldItem != null)
        {
            if (currentBlock.LastHeldItem.TryGetComponent<Portal>(out _) && !teleportFatigue)
            {
                select.SelectAllPortals();
                return this;
            }
        }

        List<bool> directions = cardData.Directions;
        bool elevated = cardData.WallTraversal;

        int forward = directions[0] ? steps : 0;
        int back = directions[1] ? steps : 0;
        int left = directions[2] ? steps : 0;
        int right = directions[3] ? steps : 0;
        int forwardLeft = directions[4] ? steps : 0;
        int forwardRight = directions[5] ? steps : 0;
        int backLeft = directions[6] ? steps : 0;
        int backRight = directions[7] ? steps : 0;

        select.SelectBlocks(block.Position, fSteps: forward, bSteps: back, lSteps: left, rSteps: right,
            flSteps: forwardLeft, frSteps: forwardRight, blSteps: backLeft, brSteps: backRight, elevated);

        return this;
    }

    public override GridItem GetItem()
    {
        return this;
    }

    public override GridItem CreateClone(Transform parent)
    {
        var clonedCard = Instantiate(transform, parent);
        clonedCard.localPosition = Vector3.zero;
        clonedCard.localRotation = Quaternion.Euler(90, 0, 0);
        clonedCard.gameObject.layer = 8;
        clonedCard.name = $"{cardData.CardName} [Card]";

        return clonedCard.GetComponent<Card>();
    }

    public override GridItem TryPlaceItem(GridBlock block, GridItem selectedItem, GridSelect select)
    {
        if (!selectedItem.GetComponent<Card>().Placed && block.Available) // Placed a card from hand
        {
            selectedItem = block.PlaceObject(ref selectedItem);

            Card placedCard = selectedItem.GetComponent<Card>();

            placedCard.Placed = true;
            placedCard.ApplyStats();
            placedCard.MoveCard(select, block, true);
        }
        else if (selectedItem.GetComponent<Card>().Placed && block.Available) // Placed a card previously placed
        {
            selectedItem = block.PlaceObject(ref selectedItem);

            Card placedCard = selectedItem.GetComponent<Card>();

            bool winCondition = block.LastHeldItem != null ? block.LastHeldItem.TryGetComponent<WinCondition>(out _) : false;
            bool portal = currentBlock.LastHeldItem != null ? currentBlock.LastHeldItem.TryGetComponent<Portal>(out _) : false;
            bool rush = block.LastHeldItem != null ? block.LastHeldItem.TryGetComponent<Rush>(out _) : false;
            bool noSteps = currentBlock.LastHeldItem != null ? !currentBlock.LastHeldItem.takeSteps && !teleportFatigue : false;

            placedCard.teleportFatigue = portal;
            placedCard.MoveCard(select, block, noSteps);

            if (rush) placedCard.ApplyRush();
            if (winCondition) DeckBuilder.Instance.AddRandomCard(CardData.CardTypes.Monster, 1, 2);
        }

        return selectedItem;
    }

    public override bool BlockPath(bool elevated = false)
    {
        return true;
    }

    public override bool CanStack()
    {
        return true;
    }
}
