using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventListener : MonoBehaviour,IPointerClickHandler
{
    public delegate void UIEvent(PointerEventData e);
    public event UIEvent onClick;

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(eventData);
    }
}
