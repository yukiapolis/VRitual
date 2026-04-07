using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GrabItem : XRGrabInteractable
{
    private Transform parentTra;
    private Vector3 localPosition;
    private Quaternion localQuaternion;
    private Collider _collider;
    private Action _grabAction, _ungrabAction;
    public bool IsAction = true;
    protected override void Awake()
    {
        base.Awake();
        parentTra = transform.parent;
        localPosition = transform.localPosition;
        localQuaternion = transform.localRotation;
        if (IsAction)
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }
    }
    public void Open(Action grabAction,Action ungrabAction)
    {
        if (IsAction)
        {
            _grabAction = grabAction;
            _ungrabAction = ungrabAction;
            _collider.isTrigger = false;
        }
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if(IsAction)
        _grabAction?.Invoke();
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args); 
        transform.localPosition=localPosition;
        transform.localRotation=localQuaternion;
        if (IsAction)
        {
            _collider.isTrigger = false;
            _ungrabAction?.Invoke();
        }
    }
}
