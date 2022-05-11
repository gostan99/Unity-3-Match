using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "ScriptableObject/Int Variable")]
public class IntVariable : ScriptableObject
{
    [SerializeField] private UnityEvent OnValueChange;

    [SerializeField] private int _value;

    public int Value
    {
        get { return _value; }
        set
        {
            _value = value;

            OnValueChange?.Invoke();
        }
    }
}