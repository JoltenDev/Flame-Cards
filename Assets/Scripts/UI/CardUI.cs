using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] CardData cardData;

    [Header("UI Fields")]
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] TMP_Text typeText;
    [SerializeField] TMP_Text moveText;
    [SerializeField] TMP_Text attackText;
    [SerializeField] TMP_Text rushText;

    [SerializeField] Image sprite;
    [SerializeField] List<GameObject> directionArrows = new List<GameObject>();
    [SerializeField] List<GameObject> subTypes = new List<GameObject>();

    public enum UIType { Name, Description, Move, Attack, Rush }

    public void SetData(CardData cardData)
    {
        this.cardData = cardData;
    }

    public void ApplyMonsterCardUI()
    {
        if (cardData == null) return;

        if (nameText != null) nameText.text = cardData.CardName;
        if (descriptionText != null) descriptionText.text = cardData.Description;
        if (moveText != null) moveText.text = cardData.Move.ToString();
        if (attackText != null) attackText.text = "ATK: " + cardData.Attack.ToString();
        if (rushText != null) rushText.text = "Rush Multiplier: " + cardData.RushMultiplier.ToString();

        if (sprite != null) sprite.sprite = cardData.Sprite;

        for (int i = 0; i < cardData.Directions.Count; i++)
            if (directionArrows[i] != null) directionArrows[i].gameObject.SetActive(cardData.Directions[i]);

        switch (cardData.SubType)
        {
            case 0: // Normal
                if (subTypes[0] != null) subTypes[0].SetActive(false);
                if (subTypes[1] != null) subTypes[1].SetActive(false);
                break;
            case 1: // Sacrifice
                if (subTypes[0] != null) subTypes[0].SetActive(true);
                if (subTypes[1] != null) subTypes[1].SetActive(false);
                break;
            case 2: // Win Condition
                if (subTypes[0] != null) subTypes[0].SetActive(false);
                if (subTypes[1] != null) subTypes[1].SetActive(true);
                break;
        }
    }

    public void ApplySpellCardUI()
    {
        if (cardData == null) return;

        if (nameText != null) nameText.text = cardData.CardName;
        if (descriptionText != null) descriptionText.text = cardData.Description;
        if (typeText != null) typeText.text = cardData.Type == CardData.CardTypes.Monster ? "M" : "S";

        if (sprite != null) sprite.sprite = cardData.Sprite;

        switch (cardData.SubType)
        {
            case 0: // Action
                if (subTypes[0] != null) subTypes[0].SetActive(true);
                if (subTypes[1] != null) subTypes[1].SetActive(false);
                break;
            case 1: // Reaction
                if (subTypes[0] != null) subTypes[0].SetActive(false);
                if (subTypes[1] != null) subTypes[1].SetActive(true);
                break;
            case 2: // Both
                if (subTypes[0] != null) subTypes[0].SetActive(true);
                if (subTypes[1] != null) subTypes[1].SetActive(true);
                break;
        }
    }

    public void ModifyUI(UIType type, string text)
    {
        switch (type)
        {
            case UIType.Move:
                moveText.text = text;
                break;
        }
    }
}