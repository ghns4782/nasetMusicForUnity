
using System.Threading;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class betterButton : Button
{
    public UnityEvent Clickdown;
    protected override void Awake()
    {
        base.Awake();
        onClick.AddListener(() => { DoStateTransition(SelectionState.Highlighted, false); });
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (interactable)
        {
            DoStateTransition(SelectionState.Highlighted, true);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (interactable)
        {
            DoStateTransition(SelectionState.Normal, true);
        }
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        Clickdown?.Invoke();
    }
}
