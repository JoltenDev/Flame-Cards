using TMPro;
using UnityEngine;

public class ItemUI : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] GridItem item;

    [Header("UI")]
    public TMP_Text amountUI;

    Camera mainCamera;

    void Start()
    {
        item = GetComponent<GridItem>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera != null && amountUI != null)
        {
            Vector3 direction = amountUI.transform.position - mainCamera.transform.position;
            amountUI.transform.rotation = Quaternion.LookRotation(direction);
        }

        if (item != null && amountUI != null)
        {
            amountUI.text = item.amount.ToString();
        }
    }
}
