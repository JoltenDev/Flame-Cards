using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseHoverHandler : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] float mouseSelectionDistance;
    [SerializeField] LayerMask cardLayerMask;
    [SerializeField] LayerMask gridLayerMask;

    [Header("Cards")]
    [SerializeField] Card selectedHandCard;
    [SerializeField] Card selectedPlacedCard;

    Card previousSelectedHandCard;

    [SerializeField] CardLayoutGroup mCardLayoutGroup;
    [SerializeField] CardLayoutGroup sCardLayoutGroup;

    [Header("Grid")]
    GridBlock previousGridBlock;
    [SerializeField] GridHighlight gridHightlight;

    Camera mainCamera;
    Controls controls;

    void Start()
    {
        mainCamera = Camera.main;

        controls = new Controls();
        controls.Enable();
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        HoverCard(ray);
        HoverGrid(ray);
    }

    void HoverCard(Ray ray)
    {
        Card currentCard = null;

        bool cardHovered = Physics.Raycast(ray, out RaycastHit hit, mouseSelectionDistance, cardLayerMask);
        bool gridHovered = Physics.Raycast(ray, out RaycastHit _, mouseSelectionDistance, gridLayerMask);

        if (cardHovered)
        {
            hit.transform.TryGetComponent(out currentCard);

            if (controls.Player.Attack.WasPressedThisFrame()) // Card in hand is hovered, player clicked on card, selecting that card
            {
                if (selectedPlacedCard != null)
                    selectedPlacedCard.Scale(false);

                selectedHandCard = currentCard;
                selectedPlacedCard = null;

                mCardLayoutGroup.SelectCard(selectedHandCard);
                sCardLayoutGroup.SelectCard(selectedHandCard);

                gridHightlight.MakeBlocksAvailable();
            }
        }
        else if (!gridHovered)
        {
            if (controls.Player.Attack.WasPressedThisFrame()) // Card in hand is not hovered, player clicked on nothing, deselecting selected card
            {
                if (selectedPlacedCard != null)
                    selectedPlacedCard.Scale(false);

                selectedHandCard = null;
                selectedPlacedCard = null;

                mCardLayoutGroup.SelectCard(null);
                sCardLayoutGroup.SelectCard(null);
            }
        }
    }

    void HoverGrid(Ray ray)
    {
        GridBlock currentGridBlock = null;

        bool cardHovered = Physics.Raycast(ray, out RaycastHit _, mouseSelectionDistance, cardLayerMask);
        bool gridHovered = Physics.Raycast(ray, out RaycastHit hit, mouseSelectionDistance, gridLayerMask);

        if (controls.Player.Attack.WasPressedThisFrame())
            gridHightlight.UnHighlightBlocks();

        if (gridHovered && !cardHovered)
        {
            hit.transform.TryGetComponent(out currentGridBlock);

            if (controls.Player.Attack.WasPressedThisFrame()) // Grid block is hovered, player clicked on block
            {
                if (!currentGridBlock.HasCard()) // Block does not have a card, place and select the new card
                {
                    if (selectedHandCard != null && selectedHandCard.Data.Type == CardData.CardTypes.Spell)
                        return;

                    currentGridBlock.RemoveCard(true);

                    if (selectedHandCard != null && currentGridBlock.Available) // Placed a card from hand
                    {
                        selectedPlacedCard = currentGridBlock.PlaceCard(ref selectedHandCard);
                        selectedPlacedCard.ApplyStats();

                        int relativePosition = currentGridBlock.Row;
                        int relativeHorizontalPosition = (currentGridBlock.Position % gridHightlight.GetLength()) + currentGridBlock.Row;
                        selectedPlacedCard.Move(0, relativePosition, relativeHorizontalPosition);
                    }
                    else if (selectedPlacedCard != null && currentGridBlock.Available) // Placed a card previously placed
                    {
                        selectedPlacedCard = currentGridBlock.PlaceCard(ref selectedPlacedCard);

                        MoveCard(currentGridBlock);
                    }

                    if (selectedPlacedCard != null)
                    {
                        selectedPlacedCard.Scale(false);

                        selectedHandCard = null;
                        selectedPlacedCard = null;

                        mCardLayoutGroup.SelectCard(null);
                        sCardLayoutGroup.SelectCard(null);

                        gridHightlight.MakeBlocksAvailable();
                    }
                }
                else // Block has a card
                {
                    if (selectedPlacedCard != null)
                    {
                        selectedPlacedCard.Scale(false);
                    }

                    selectedHandCard = null;
                    selectedPlacedCard = currentGridBlock.GetCard();
                    selectedPlacedCard.Scale();

                    mCardLayoutGroup.SelectCard(null);
                    sCardLayoutGroup.SelectCard(null);

                    mCardLayoutGroup.DisplayCard(selectedPlacedCard);
                    sCardLayoutGroup.DisplayCard(selectedPlacedCard);

                    int steps = selectedPlacedCard.Steps;

                    gridHightlight.MakeBlocksUnavailable();
                    gridHightlight.HighlightBlocks(currentGridBlock.Position, fSteps: steps, bSteps: steps, lSteps: steps, rSteps: steps, flSteps: steps, frSteps: steps, blSteps: steps, brSteps: steps);
                }
            }
        }

        if (selectedHandCard != null && selectedHandCard.Data.Type == CardData.CardTypes.Spell)
            return;

        if (selectedHandCard != null)
            SelectObject(currentGridBlock, ref previousGridBlock, block => block.PlaceCard(ref selectedHandCard, true), block => block.RemoveCard(true));
        if (selectedPlacedCard != null)
            SelectObject(currentGridBlock, ref previousGridBlock, block => block.PlaceCard(ref selectedPlacedCard, true), block => block.RemoveCard(true));
    }

    void MoveCard(GridBlock currentGridBlock)
    {
        int relativeHorizontalPosition = (currentGridBlock.Position % gridHightlight.GetLength()) + currentGridBlock.Row;
        int relativePosition = currentGridBlock.Row;

        if (selectedPlacedCard.Position == currentGridBlock.Row)
        {
            int cardPosition = selectedPlacedCard.HorizontalPosition;
            int steps = Math.Abs(relativeHorizontalPosition - cardPosition);

            selectedPlacedCard.Move(steps, relativePosition, relativeHorizontalPosition);
        }
        else
        {
            int cardPosition = selectedPlacedCard.Position;
            int steps = Math.Abs(relativePosition - cardPosition);

            selectedPlacedCard.Move(steps, relativePosition, relativeHorizontalPosition);
        }
    }

    void SelectObject<T>(T newObj, ref T previousObject, Action<T> selectAction, Action<T> deselectAction) where T : UnityEngine.Object
    {
        if (previousObject == newObj)
            return;

        if (previousObject != null)
        {
            deselectAction.Invoke(previousObject);
        }
        
        if (newObj != null)
        {
            selectAction.Invoke(newObj);
        }
        
        previousObject = newObj;
    }
}
