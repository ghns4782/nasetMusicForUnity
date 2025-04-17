
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
    public UnityEvent OnClickdown;
    public UnityEvent OnClickEnter;
    public UnityEvent OnClickExit;
    public UnityEvent OnClickUp;
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
        OnClickEnter?.Invoke();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (interactable)
        {
            DoStateTransition(SelectionState.Normal, true);
        }
        OnClickExit?.Invoke();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        OnClickdown?.Invoke();
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        OnClickUp?.Invoke();
    }
}
