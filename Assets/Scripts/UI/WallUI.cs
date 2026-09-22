using TMPro;
using UnityEngine;

public class WallUI : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] GridItem item;

    [Header("UI")]
    [SerializeField] TMP_Text amountUI;
    
    void Update()
    {
        amountUI.text = item.amount.ToString();
    }
}
