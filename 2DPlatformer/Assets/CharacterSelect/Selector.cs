using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SelectorType { 
    Ability,
    Button
}
public class Selector : MonoBehaviour
{
    public SelectorType selectorType;
    public CharacterSelector characterSelector;
}
