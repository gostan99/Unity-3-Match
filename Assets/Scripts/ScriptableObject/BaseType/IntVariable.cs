using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;

[CreateAssetMenu(menuName = "ScriptableObject/Int Variable")]
public class IntVariable : ScriptableObject
{
    [SerializeField] private GameEvent OnValueChange;

    [SerializeField] private int _value;

    public int Value
    {
        get { return _value; }
        set
        {
            _value = value;

            if (OnValueChange != null) OnValueChange.Raise();
        }
    }
}