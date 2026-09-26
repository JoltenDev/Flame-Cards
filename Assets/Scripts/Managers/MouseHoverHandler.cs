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
        bool itemHovered = Physics.Raycast(ray, out RaycastHit hit, mouseSelectionDistance, itemLayerMask);
        bool gridHovered = Physics.Raycast(ray, out RaycastHit _, mouseSelectionDistance, gridLayerMask);

        if (itemHovered) // Grid item is hovered
        {
            hit.transform.TryGetComponent(out GridItem item);

            if (controls.Player.Attack.WasPressedThisFrame()) // Grid item is clicked, select item
            {
                gridSelect.DeselectBlocks();

                if (selectedItem != null && selectedItem.CompareTag("Card"))
                    selectedItem.GetComponent<Card>().Scale(false);

                selectedItem = item.GetItem();
                DeselectItem(false);

                if (item.CompareTag("Card")) // Item is a card
                {
                    mCardLayoutGroup.SelectCard(selectedItem.GetComponent<Card>());
                    sCardLayoutGroup.SelectCard(selectedItem.GetComponent<Card>());
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
        bool blocked = block.Occupied<GridItem>(out _);

        if (!blocked) // Block does not have anything on it, place an item
        {
            PlaceItem(block);
        }
        else if (block.Occupied<Card>(out Card card)) // Block is occupied by a card, select that card
        {
            if (selectedItem != null && selectedItem.CompareTag("Card"))
            {
                selectedItem.GetComponent<Card>().Scale(false);
            }

            selectedItem = card.Select(block, gridSelect);

            mCardLayoutGroup.SelectCard(null);
            sCardLayoutGroup.SelectCard(null);

            mCardLayoutGroup.DisplayCard(selectedItem.GetComponent<Card>());
            sCardLayoutGroup.DisplayCard(selectedItem.GetComponent<Card>());
        }
        else // Block is occupied by something
        {
            PlaceItem(block);
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

    void PlaceItem(GridBlock block)
    {
        if (selectedItem == null)
            return;

        if (ItemIsSpellCard())
            return;

        if (block.HeldItem != null && !selectedItem.CanStack())
            return;

        block.RemoveObject(preview: true);

        selectedItem = selectedItem.TryPlaceItem(block, selectedItem, gridSelect);

        gridSelect.DeselectBlocks();

        if (selectedItem.CompareTag("Card"))
        {
            selectedItem.GetComponent<Card>().Scale(false);
            selectedItem = null;

            mCardLayoutGroup.SelectCard(null);
            sCardLayoutGroup.SelectCard(null);
        }
    }

    bool ItemIsSpellCard()
    {
        return selectedItem != null && selectedItem.CompareTag("Card") && selectedItem.GetComponent<Card>().Data.Type == CardData.CardTypes.Spell;
    }
}
