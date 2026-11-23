using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Meneruskan event hover pointer ke callback (dipakai untuk tautan).</summary>
public class HoverRelay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action Enter;
    public Action Exit;

    public void OnPointerEnter(PointerEventData eventData) => Enter?.Invoke();
    public void OnPointerExit(PointerEventData eventData) => Exit?.Invoke();
}
