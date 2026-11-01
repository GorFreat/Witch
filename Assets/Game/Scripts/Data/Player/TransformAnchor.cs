using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TransformAnchor", menuName = "Scriptable Objects/TransformAnchor")]
public class TransformAnchor : ScriptableObject
{
    //Save transform of the tracked object
    public Transform Value { get; private set; }

    //Check for any object has been registered yet
    public bool HasValue => Value != null;

    //Shoot out event when an object has been registered
    public event Action<Transform> OnTransformSet;

    private void OnEnable()
    {
        Value = null;
    }
    public void Set(Transform target)
    {
        Value = target;
        OnTransformSet?.Invoke(target);
    }

    public void Clear()
    {
        Value = null;
    }
}
