using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
public class CardUIBinder : MonoBehaviour
{
    [Header("Data & References")]
    [SerializeField] CardData cardData;
    [SerializeField] CardData.CardTypes bindType;
    [SerializeField] CardUI cardUI;

    CardData subscribedData;
    CardData.CardTypes subscribedBindType;

    void OnEnable()
    {
        Unsubscribe();
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void OnDestroy()
    {
        Unsubscribe();
    }

    void OnValidate()
    {
        if (subscribedData != cardData || subscribedBindType != bindType)
        {
            Unsubscribe();
            Subscribe();
        }

        if (bindType == CardData.CardTypes.Monster)
            UpdateMonsterUI();
        else if (bindType == CardData.CardTypes.Spell)
            UpdateSpellUI();
    }

    void Subscribe()
    {
        if (cardData == null) return;

        if (bindType == CardData.CardTypes.Monster)
        {
            cardData.OnDataChanged -= UpdateMonsterUI;
            cardData.OnDataChanged += UpdateMonsterUI;
        }
        else if (bindType == CardData.CardTypes.Spell)
        {
            cardData.OnDataChanged -= UpdateSpellUI;
            cardData.OnDataChanged += UpdateSpellUI;
        }

        subscribedData = cardData;
        subscribedBindType = bindType;
    }

    void Unsubscribe()
    {
        if (subscribedData == null) return;

        subscribedData.OnDataChanged -= UpdateMonsterUI;
        subscribedData.OnDataChanged -= UpdateSpellUI;

        subscribedData = null;
    }

    public void UpdateMonsterUI()
    {
        if (cardUI == null) return;

        cardUI.ApplyMonsterCardUI();

        Dirty();
    }

    public void UpdateSpellUI()
    {
        if (cardUI == null) return;

        cardUI.ApplySpellCardUI();

        Dirty();
    }

    void Dirty()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (this != null)
                UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}