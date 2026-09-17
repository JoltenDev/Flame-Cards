using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VHierarchy.Libs;

public class CardEditor : EditorWindow
{
    [SerializeField] VisualTreeAsset visualTree;

    [SerializeField] CardData blankCardData;
    List<CardData> cardDatas = new List<CardData>();

    [SerializeField] Camera previewCamera;
    [SerializeField] GameObject previewMonsterCard;
    [SerializeField] GameObject previewSpellCard;

    Camera clonedCamera;
    GameObject clonedCard;

    [MenuItem("Tools/Card Editor")]
    public static void ShowWindow()
    {
        CardEditor window = GetWindow<CardEditor>();
        window.titleContent = new GUIContent("Card Editor");

        window.maxSize = new Vector2(850, 600);
        window.minSize = new Vector2(550, 420);
    }

    void CreateGUI()
    {
        VisualElement root = visualTree.Instantiate();
        rootVisualElement.Add(root);

        Button createCardButton = rootVisualElement.Q<Button>("create-card-button");
        Button clearCardButton = rootVisualElement.Q<Button>("clear-card-button");
        Button refreshCardButton = rootVisualElement.Q<Button>("refresh-card-button");
        Button deleteCardButton = rootVisualElement.Q<Button>("delete-card-button");
        Button cancelCardDropdownButton = rootVisualElement.Q<Button>("card-dropdown-cancel-button");

        Toggle uniqueMoveToggle = rootVisualElement.Q<Toggle>("unique-move-toggle");

        DropdownField typeField = rootVisualElement.Q<DropdownField>("type-dropdown");
        DropdownField cardField = rootVisualElement.Q<DropdownField>("card-dropdown");

        if (createCardButton != null) createCardButton.clicked += OnCreateCardClicked;
        if (clearCardButton != null) clearCardButton.clicked += OnClearCardClicked;
        if (refreshCardButton != null) refreshCardButton.clicked += OnRefreshCardClicked;
        if (deleteCardButton != null) deleteCardButton.clicked += OnDeleteCardClicked;
        if (cancelCardDropdownButton != null) cancelCardDropdownButton.clicked += OnClearCardClicked;

        if (uniqueMoveToggle != null)
            uniqueMoveToggle.RegisterValueChangedCallback(OnUniqueMoveToggle);

        if (cardField != null)
            cardField.RegisterValueChangedCallback(OnCardDropdown);

        clonedCamera = Instantiate(previewCamera, new Vector3(0, 0, 0), Quaternion.identity);
        clonedCard = Instantiate(previewSpellCard, new Vector3(0, 0, 0.89f), Quaternion.identity, clonedCamera.transform);

        if (typeField != null)
        {
            typeField.RegisterValueChangedCallback(OnTypeDropdown);
            typeField.index = (int)blankCardData.Type - 1;
        }

        OnClearCardClicked();
        OnRefreshCardClicked();

        BindCardDataToUI();
    }

    void OnDisable()
    {
        OnClearCardClicked();
        OnRefreshCardClicked();

        Button createCardButton = rootVisualElement.Q<Button>("create-card-button");
        Button clearCardButton = rootVisualElement.Q<Button>("clear-card-button");
        Button refreshCardButton = rootVisualElement.Q<Button>("refresh-card-button");
        Button deleteCardButton = rootVisualElement.Q<Button>("delete-card-button");
        Button cancelCardDropdownButton = rootVisualElement.Q<Button>("card-dropdown-cancel-button");

        Toggle uniqueMoveToggle = rootVisualElement.Q<Toggle>("unique-move-toggle");

        DropdownField typeField = rootVisualElement.Q<DropdownField>("type-dropdown");
        DropdownField cardField = rootVisualElement.Q<DropdownField>("card-dropdown");

        if (createCardButton != null) createCardButton.clicked -= OnCreateCardClicked;
        if (clearCardButton != null) clearCardButton.clicked -= OnClearCardClicked;
        if (refreshCardButton != null) refreshCardButton.clicked -= OnRefreshCardClicked;
        if (deleteCardButton != null) deleteCardButton.clicked -= OnDeleteCardClicked;
        if (cancelCardDropdownButton != null) cancelCardDropdownButton.clicked -= OnClearCardClicked;

        if (uniqueMoveToggle != null) uniqueMoveToggle.UnregisterValueChangedCallback(OnUniqueMoveToggle);
        if (typeField != null) typeField.UnregisterValueChangedCallback(OnTypeDropdown);
        if (cardField != null) cardField.UnregisterValueChangedCallback(OnCardDropdown);

        if (clonedCamera != null)
        {
            DestroyImmediate(clonedCamera.gameObject);
            clonedCamera = null;
        }

        if (clonedCard != null)
        {
            DestroyImmediate(clonedCard);
            clonedCard = null;
        }
    }

    void OnCreateCardClicked()
    {
        if (blankCardData == null)
        {
            Debug.LogError("No blankCardData assigned to the Card Editor!");
            return;
        }

        string dataTargetPath = GetTargetPath();
        ObjectUtilities.CloneAsset(blankCardData, dataTargetPath);

        FillCardDropdown();

        OnClearCardClicked();
    }

    void OnDeleteCardClicked()
    {
        string dataTargetPath = GetTargetPath();

        AssetDatabase.DeleteAsset(dataTargetPath);

        DropdownField typeField = rootVisualElement.Q<DropdownField>("type-dropdown");
        typeField.index = 1;

        OnClearCardClicked();
        OnRefreshCardClicked();
    }

    void OnClearCardClicked()
    {
        Button deleteCardButton = rootVisualElement.Q<Button>("delete-card-button");
        deleteCardButton.style.display = DisplayStyle.None;

        Button cancelCardDropdownButton = rootVisualElement.Q<Button>("card-dropdown-cancel-button");
        cancelCardDropdownButton.style.display = DisplayStyle.None;

        DropdownField cardField = rootVisualElement.Q<DropdownField>("card-dropdown");
        cardField.index = -1;

        blankCardData.CleanData();
        EditorUtility.SetDirty(blankCardData);
    }

    void OnRefreshCardClicked()
    {
        FillCardDropdown();

        blankCardData.NotifyDataChanged();
        EditorUtility.SetDirty(blankCardData);
    }

    void OnUniqueMoveToggle(ChangeEvent<bool> changeEvent)
    {
        bool toggledOn = changeEvent.newValue;

        VisualElement uniqueMoveGroup = rootVisualElement.Q<VisualElement>("unique-move-group");
        IntegerField moveIntegerField = rootVisualElement.Q<IntegerField>("move-integer-field");

        if (toggledOn)
        {
            uniqueMoveGroup.style.display = DisplayStyle.Flex;
            moveIntegerField.style.display = DisplayStyle.None;

            return;
        }

        uniqueMoveGroup.style.display = DisplayStyle.None;
        moveIntegerField.style.display = DisplayStyle.Flex;
    }

    void OnTypeDropdown(ChangeEvent<string> changeEvent)
    {
        VisualElement monsterGroup = rootVisualElement.Q<VisualElement>("monster-group");
        DropdownField spelltypeField = rootVisualElement.Q<DropdownField>("spell-type-dropdown");
        DropdownField monstertypeField = rootVisualElement.Q<DropdownField>("monster-type-dropdown");

        if (clonedCard != null)
        {
            DestroyImmediate(clonedCard);
            clonedCard = null;
        }

        if (changeEvent.newValue == "Monster")
        {
            monsterGroup.style.display = DisplayStyle.Flex;
            monstertypeField.style.display = DisplayStyle.Flex;
            spelltypeField.style.display = DisplayStyle.None;

            blankCardData.Type = CardData.CardTypes.Monster;

            clonedCard = Instantiate(previewMonsterCard, new Vector3(0, 0, 0.89f), Quaternion.identity, clonedCamera.transform);
        }
        else
        {
            monsterGroup.style.display = DisplayStyle.None;
            monstertypeField.style.display = DisplayStyle.None;
            spelltypeField.style.display = DisplayStyle.Flex;

            blankCardData.Type = CardData.CardTypes.Spell;

            clonedCard = Instantiate(previewSpellCard, new Vector3(0, 0, 0.89f), Quaternion.identity, clonedCamera.transform);
        }
    }

    void OnCardDropdown(ChangeEvent<string> changeEvent)
    {
        if (changeEvent.newValue.IsNullOrEmpty()) return;

        foreach (var data in cardDatas)
        {
            if (changeEvent.newValue == data.name)
            {
                blankCardData.CopyData(data);

                DropdownField typeField = rootVisualElement.Q<DropdownField>("type-dropdown");
                typeField.index = (int)data.Type - 1;

                break;
            }
        }

        Button cancelCardDropdownButton = rootVisualElement.Q<Button>("card-dropdown-cancel-button");
        cancelCardDropdownButton.style.display = DisplayStyle.Flex;

        Button deleteCardButton = rootVisualElement.Q<Button>("delete-card-button");
        deleteCardButton.style.display = DisplayStyle.Flex;
    }

    void FillCardDropdown()
    {
        DropdownField cardField = rootVisualElement.Q<DropdownField>("card-dropdown");
        cardField.choices.Clear();

        cardDatas.Clear();
        GetCards(ref cardDatas, "Monsters");
        GetCards(ref cardDatas, "Spells");

        foreach (var data in cardDatas)
        {
            cardField.choices.Add(data.name);
        }
    }

    void GetCards(ref List<CardData> cardDatas, string cards)
    {
        string path = Path.Combine(Application.dataPath, $"Scripts/Scriptable Objects/Cards/{cards}");

        if (!Directory.Exists(path))
        {
            Debug.LogError($"Directory not found: {path}");
            return;
        }

        string[] cardAssets = Directory.GetFiles(path, "*.asset", SearchOption.TopDirectoryOnly);
        foreach (var asset in cardAssets)
        {
            string relativePath = "Assets" + asset.Substring(Application.dataPath.Length);
            relativePath = relativePath.Replace("\\", "/");

            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(relativePath);

            if (card != null)
            {
                cardDatas.Add(card);
            }
            else
            {
                Debug.LogWarning($"Failed to load asset at: {relativePath}");
            }
        }
    }

    string GetTargetPath()
    {
        string folder = blankCardData.Type == CardData.CardTypes.Monster ? "Monsters" : "Spells";
        return $"Assets/Scripts/Scriptable Objects/Cards/{folder}/{blankCardData.CardName} [CardData].asset";
    }

    void BindCardDataToUI()
    {
        SerializedObject serializedCard = new SerializedObject(blankCardData);

        rootVisualElement.Bind(serializedCard);

        rootVisualElement.TrackSerializedObjectValue(serializedCard, (so) =>
        {
            blankCardData.NotifyDataChanged();
            EditorUtility.SetDirty(blankCardData);
        });
    }
}
