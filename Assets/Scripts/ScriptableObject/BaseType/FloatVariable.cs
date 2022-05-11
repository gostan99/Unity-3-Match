using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "ScriptableObject/Float Variable")]
public class FloatVariable : ScriptableObject
{
    [SerializeField] private UnityEvent OnValueChange;

    [SerializeField] private float _value;

    public float Value
    {
        get { return _value; }
        set
        {
            _value = value;

            OnValueChange?.Invoke();
        }
    }
}