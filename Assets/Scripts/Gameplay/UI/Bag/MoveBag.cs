using UnityEngine;
using UnityEngine.EventSystems;

public class MoveBag : MonoBehaviour
{
    public Canvas canvas;

    private RectTransform currentRect;
    // Start is called before the first frame update
    void Awake()
    {
        currentRect = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {

        currentRect.anchoredPosition += eventData.delta;
    }
}
