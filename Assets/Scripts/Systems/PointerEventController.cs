using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Changes the alpha value of a button bg image when the mouse hovers over it
public class PointerEventController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(SetColorAlphaValue(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(SetColorAlphaValue(false));
    }

    IEnumerator SetColorAlphaValue(bool isEnter)
    {
        while ((isEnter && buttonImage.color.a < 1f) || (!isEnter && buttonImage.color.a > 0f))
        {
            Color newColor = buttonImage.color;
            newColor.a += isEnter ? .1f : -.1f;
            buttonImage.color = newColor;
            yield return new WaitForSeconds(.03f);
        }
    }
}