using Enderlook.Unity.Toolset.Attributes;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public Alfa mode;
}

[CreateAssetMenu(menuName = "test")]
public class Beta : ScriptableObject
{
    public Alfa mode;
}


[Serializable]
[PropertyPopup(nameof(mode))]
public class Alfa
{
    public bool mode;
    [PropertyPopupOption(false)]
    public int a;
    [PropertyPopupOption(true)]
    public Vector2 b;
}