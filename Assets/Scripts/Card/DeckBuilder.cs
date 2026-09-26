using System.Collections.Generic;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
    [Header("Card Templates")]
    [SerializeField] GameObject monsterCardTemplate;
    [SerializeField] GameObject spellCardTemplate;

    [Header("Card Data")]
    [SerializeField] List<CardData> monsterCards = new List<CardData>();
    [SerializeField] List<CardData> spellCards = new List<CardData>();
    [SerializeField] List<CardData> winConditionCards = new List<CardData>();

    [Header("Temp")]
    [SerializeField] Transform playerMonsterHand;
    [SerializeField] Transform playerSpellHand;

    public static DeckBuilder Instance;

    void Start()
    {
        Instance = this;

        LoadCardData();

        AddRandomCard(CardData.CardTypes.Monster, 50);
        AddRandomCard(CardData.CardTypes.Spell, 50);
    }

    void LoadCardData()
    {
        var monsters = Resources.LoadAll("Cards/Monsters", typeof(CardData));
        var spells = Resources.LoadAll("Cards/Spells", typeof(CardData));
        var winConditions = Resources.LoadAll("Cards/Win Conditions", typeof(CardData));

        foreach (CardData data in monsters)
            monsterCards.Add(data);

        foreach (CardData data in spells)
            spellCards.Add(data);

        foreach (CardData data in winConditions)
            winConditionCards.Add(data);
    }

    public void AddCard(Card card)
    {

    }

    public void AddRandomCard(CardData.CardTypes type, int amount = 1, int subtype = 0)
    {
        Transform target = null;
        List<CardData> cards = new List<CardData>();

        switch (type)
        {
            case CardData.CardTypes.Monster:
                if (subtype == 2)
                    cards = winConditionCards;
                else
                    cards = monsterCards;

                target = playerMonsterHand;
                break;
            case CardData.CardTypes.Spell:
                cards = spellCards;

                target = playerSpellHand;
                break;
        }

        int cardAmount = target.GetComponentsInChildren<Card>().Length;
        if (cards.Count <= 0)
            return;

        for (int i = 0; i < amount; i++)
        {
            if (cardAmount + i > 4)
                return;

            int index = Random.Range(0, cards.Count);
            CardData cardData = cards[index];

            if (cardData.Type == CardData.CardTypes.Monster)
            {
                var card = Instantiate(monsterCardTemplate, target);
                card.name = cardData.CardName;

                card.GetComponent<Card>().Data = cardData;
                card.GetComponent<CardUI>().SetData(cardData);
                card.GetComponent<CardUI>().ApplyMonsterCardUI();
            }

            if (cardData.Type == CardData.CardTypes.Spell)
            {
                var card = Instantiate(spellCardTemplate, target);
                card.name = cardData.CardName;

                card.GetComponent<Card>().Data = cardData;
                card.GetComponent<CardUI>().SetData(cardData);
                card.GetComponent<CardUI>().ApplySpellCardUI();
            }
        }
    }
}
