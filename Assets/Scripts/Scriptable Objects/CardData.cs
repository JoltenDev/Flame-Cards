using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/Card Data")]
public class CardData : ScriptableObject
{
    public enum CardTypes { None, Monster, Spell }

    [Header("Basic Fields")]
    [SerializeField] string cardName;
    [SerializeField] string description;
    [SerializeField] CardTypes type;
    [SerializeField] int subType;
    [SerializeField] Sprite sprite;

    [Space(25)]
    [Header("Monster Fields")]

    [Header("Attributes")]
    [SerializeField] int move;
    [SerializeField] int attack;
    [SerializeField] int rushMultiplier;

    [Header("Movement Descriptor")]
    [SerializeField] List<bool> directions = new List<bool>();
    [SerializeField] bool wallTraversal;

    public string CardName { get { return cardName; } }
    public string Description { get { return description; } }
    public CardTypes Type { get { return type; } set { type = value; } }
    public int SubType { get { return subType; } }
    public Sprite Sprite { get { return sprite; } }

    public int Move { get { return move; } }
    public int Attack { get { return attack; } }
    public int RushMultiplier { get { return rushMultiplier; } }

    public List<bool> Directions { get { return directions; } set { directions = value; } }

    public System.Action OnDataChanged;

    public void NotifyDataChanged()
    {
        OnDataChanged?.Invoke();
    }

    public void CleanData()
    {
        cardName = "";
        description = "";
        sprite = null;
        subType = 0;
        move = 0;
        attack = 0;
        rushMultiplier = 0;

        for (int i = 0; i < directions.Count; i++)
            directions[i] = false;
        wallTraversal = false;
    }

    public void CopyData(CardData other)
    {
        cardName = other.cardName;
        description = other.description;
        sprite = other.sprite;
        subType= other.subType;
        type = other.type;
        
        attack = other.attack;
        rushMultiplier = other.rushMultiplier;

        move = other.move;

        for (int i = 0 ;i < directions.Count; i++)
            directions[i] = other.directions[i];
        wallTraversal = other.wallTraversal;
    }
}
