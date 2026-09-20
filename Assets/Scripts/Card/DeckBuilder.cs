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

    [Header("Temp")]
    [SerializeField] Transform playerMonsterHand;
    [SerializeField] Transform playerSpellHand;

    void Start()
    {
        LoadCardData();

        AddRandomCard(monsterCards, playerMonsterHand, 50);
        AddRandomCard(spellCards, playerSpellHand, 50);
    }

    void LoadCardData()
    {
        var monsters = Resources.LoadAll("Cards/Monsters", typeof(CardData));
        var spells = Resources.LoadAll("Cards/Spells", typeof(CardData));

        foreach (CardData data in monsters)
            monsterCards.Add(data);

        foreach (CardData data in spells)
            spellCards.Add(data);
    }

    public void AddCard(Card card)
    {

    }

    public void AddRandomCard(List<CardData> cards, Transform target, int amount = 1)
    {
        int cardAmount = target.GetComponentsInChildren<Card>().Length;
        
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
