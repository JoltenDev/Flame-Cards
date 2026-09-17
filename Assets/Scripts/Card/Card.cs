using System.Collections;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] CardData cardData;

    [SerializeField] Vector3 defaultScale = new Vector3(0.65f, 1, 0.02f);
    [SerializeField] Vector3 smallScale = new Vector3(0.65f / 1.25f, 1 / 1.25f, 0.02f / 1.25f);

    Coroutine scaleCoroutine;

    public CardData GetData()
    {
        return cardData;
    }

    public Transform CreateClone(Transform parent, int layer)
    {
        var clonedCard = Instantiate(transform, parent);
        clonedCard.localPosition = new Vector3(0, 0, 0);
        clonedCard.localRotation = Quaternion.Euler(180, 0, 180);
        clonedCard.gameObject.layer = layer;
        clonedCard.name = "Card";

        return clonedCard;
    }

    public void Scale(bool scale = true)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }

        scaleCoroutine = StartCoroutine(Scale(0.25f, scale));
    }

    IEnumerator Scale(float duration, bool scale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 newScale = scale ? smallScale : defaultScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, newScale, t);

            yield return null;
        }

        transform.localScale = newScale;
    }
}
