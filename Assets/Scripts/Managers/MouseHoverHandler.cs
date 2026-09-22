using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseHoverHandler : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] float mouseSelectionDistance;
    [SerializeField] LayerMask itemLayerMask;
    [SerializeField] LayerMask gridLayerMask;

    [Header("Selected Grid Item")]
    [SerializeField] GridItem selectedItem;

    [Header("Card Groups")]
    [SerializeField] CardLayoutGroup mCardLayoutGroup;
    [SerializeField] CardLayoutGroup sCardLayoutGroup;

    [Header("Grid")]
    GridBlock previousGridBlock;
    [SerializeField] GridSelect gridSelect;

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
        GridItem item = null;

        bool itemHovered = Physics.Raycast(ray, out RaycastHit hit, mouseSelectionDistance, itemLayerMask);
        bool gridHovered = Physics.Raycast(ray, out RaycastHit _, mouseSelectionDistance, gridLayerMask);

        if (itemHovered) // Grid item is hovered
        {
            hit.transform.TryGetComponent(out item);

            if (controls.Player.Attack.WasPressedThisFrame()) // Grid item is clicked, select item
            {
                gridSelect.DeselectBlocks();

                if (selectedItem != null && selectedItem.CompareTag("Card"))
                    selectedItem.GetComponent<Card>().Scale(false);

                if (item.CompareTag("Card")) // Item is a card
                {
                    selectedItem = item.GetComponent<Card>();

                    mCardLayoutGroup.SelectCard(selectedItem.GetComponent<Card>());
                    sCardLayoutGroup.SelectCard(selectedItem.GetComponent<Card>());
                }

                if (item.CompareTag("Wall")) // Item is a wall
                {
                    selectedItem = item.GetComponent<Wall>();
                    DeselectItem(false);
                }

                if (item.CompareTag("Jumppad"))
                {
                    selectedItem = item.GetComponent<Jumppad>();
                    DeselectItem(false);
                }
            }
        }
        else if (!gridHovered)
        {
            if (controls.Player.Attack.WasPressedThisFrame()) // Nothing was clicked, deselect item
            {
                DeselectItem();
                gridSelect.DeselectBlocks();
            }
        }
    }

    void HoverGrid(Ray ray)
    {
        GridBlock currentGridBlock = null;

        bool itemHovered = Physics.Raycast(ray, out RaycastHit _, mouseSelectionDistance, itemLayerMask);
        bool gridHovered = Physics.Raycast(ray, out RaycastHit hit, mouseSelectionDistance, gridLayerMask);

        if (gridHovered && !itemHovered)
        {
            hit.transform.TryGetComponent(out currentGridBlock);

            if (controls.Player.Attack.WasPressedThisFrame()) // Grid block is hovered, player clicked on block
            {
                SelectBlock(currentGridBlock);
            }
        }

        if (ItemIsSpellCard())
            return;

        if (selectedItem != null)
            SelectObject(currentGridBlock, ref previousGridBlock, block => block.PlaceObject(ref selectedItem, true), block => block.RemoveObject(preview: true));
    }

    void SelectBlock(GridBlock block)
    {
        bool hasCard = block.Occupied<Card>(out Card card);
        bool hasWall = block.Occupied<Wall>(out Wall wall);
        bool blocked = hasCard || hasWall;

        if (!blocked) // Block does not have anything on it, place an item
        {
            if (ItemIsSpellCard())
                return;

            block.RemoveObject(preview: true);

            TryPlaceCard(block);
            TryPlaceWall(block);
            TryPlaceJumppad(block);

            gridSelect.DeselectBlocks();
        }
        else if (card != null) // Block is occupied by a card
        {
            if (selectedItem != null && selectedItem.CompareTag("Card"))
            {
                selectedItem.GetComponent<Card>().Scale(false);
            }

            selectedItem = card;
            selectedItem.GetComponent<Card>().Scale();

            mCardLayoutGroup.SelectCard(null);
            sCardLayoutGroup.SelectCard(null);

            mCardLayoutGroup.DisplayCard(selectedItem.GetComponent<Card>());
            sCardLayoutGroup.DisplayCard(selectedItem.GetComponent<Card>());

            List<bool> directions = selectedItem.GetComponent<Card>().Data.Directions;
            int steps = selectedItem.GetComponent<Card>().Steps;
            bool elevated = selectedItem.GetComponent<Card>().Data.WallTraversal;

            int forward = directions[0] ? steps : 0;
            int back = directions[1] ? steps : 0;
            int left = directions[2] ? steps : 0;
            int right = directions[3] ? steps : 0;
            int forwardLeft = directions[4] ? steps : 0;
            int forwardRight = directions[5] ? steps : 0;
            int backLeft = directions[6] ? steps : 0;
            int backRight = directions[7] ? steps : 0;

            gridSelect.SelectBlocks(block.Position, fSteps: forward, bSteps: back, lSteps: left, rSteps: right, 
                flSteps: forwardLeft, frSteps: forwardRight, blSteps: backLeft, brSteps: backRight, elevated);
        }
        else if (wall != null) // Block is occupied by a wall
        {
            if (selectedItem != null && selectedItem.CompareTag("Card") && selectedItem.GetComponent<Card>().Data.WallTraversal)
            {
                if (ItemIsSpellCard())
                    return;

                block.RemoveObject(preview: true);

                TryPlaceCard(block);

                gridSelect.DeselectBlocks();
            }
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

    void DeselectItem(bool nullSelected = true)
    {
        if (selectedItem == null) return;

        if (selectedItem.CompareTag("Card") && selectedItem.GetComponent<Card>() != null)
            selectedItem.GetComponent<Card>().Scale(false);

        if (nullSelected)
            selectedItem = null;

        mCardLayoutGroup.SelectCard(null);
        sCardLayoutGroup.SelectCard(null);
    }

    void TryPlaceCard(GridBlock block)
    {
        if (selectedItem == null || !selectedItem.CompareTag("Card")) return;

        if (!selectedItem.GetComponent<Card>().Placed && block.Available) // Placed a card from hand
        {
            selectedItem = block.PlaceObject(ref selectedItem);

            selectedItem.GetComponent<Card>().ApplyStats();
            selectedItem.GetComponent<Card>().Placed = true;

            MoveCard(block, true);
        }
        else if (selectedItem.GetComponent<Card>().Placed && block.Available) // Placed a card previously placed
        {
            selectedItem = block.PlaceObject(ref selectedItem);

            MoveCard(block);
        }

        selectedItem.GetComponent<Card>().Scale(false);
        selectedItem = null;

        mCardLayoutGroup.SelectCard(null);
        sCardLayoutGroup.SelectCard(null);
    }

    void TryPlaceWall(GridBlock block)
    {
        if (selectedItem == null || !selectedItem.CompareTag("Wall")) return;

        if (block.Available)
            block.PlaceObject(ref selectedItem);
    }

    void TryPlaceJumppad(GridBlock block)
    {
        if (selectedItem == null || !selectedItem.CompareTag("Jumppad")) return;

        if (block.Available)
            block.PlaceObject(ref selectedItem);
    }

    void MoveCard(GridBlock block, bool noSteps = false)
    {
        int relativeHorizontalPosition = (block.Position % gridSelect.GetLength()) + block.Row;
        int relativePosition = block.Row;

        if (noSteps)
        {
            int cardPosition = selectedItem.GetComponent<Card>().HorizontalPosition;
            int steps = Math.Abs(relativeHorizontalPosition - cardPosition);

            selectedItem.GetComponent<Card>().Move(0, relativePosition, relativeHorizontalPosition);
            return;
        }

        if (selectedItem.GetComponent<Card>().Position == block.Row)
        {
            int cardPosition = selectedItem.GetComponent<Card>().HorizontalPosition;
            int steps = Math.Abs(relativeHorizontalPosition - cardPosition);

            selectedItem.GetComponent<Card>().Move(steps, relativePosition, relativeHorizontalPosition);
        }
        else
        {
            int cardPosition = selectedItem.GetComponent<Card>().Position;
            int steps = Math.Abs(relativePosition - cardPosition);

            selectedItem.GetComponent<Card>().Move(steps, relativePosition, relativeHorizontalPosition);
        }
    }

    bool ItemIsSpellCard()
    {
        return selectedItem != null && selectedItem.CompareTag("Card") && selectedItem.GetComponent<Card>().Data.Type == CardData.CardTypes.Spell;
    }
}
