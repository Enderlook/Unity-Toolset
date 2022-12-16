using Enderlook.Unity.Toolset.Attributes;

using System;
using UnityEngine;

public sealed class ExampleMonoBehaviour6 : MonoBehaviour
{
    [SerializeField]
    private ExampleStruct1 a;

    [SerializeField]
    private ExampleStruct2 b;
}

[Serializable]
[RedirectTo(nameof(b))] // Al mostrar este objeto en el inspector de Unity, todos los campos ser�n ocultados y en cambio se mostrar� solo el campo `b`.
public struct ExampleStruct1
{
    [SerializeField]
    private int a;

    [SerializeField]
    private int b;
}

[Serializable]
[PropertyPopup(nameof(a))] // Al mostrar este objeto en el inspector de Unity, todos los campos ser�n ocultados y en cambio se mostr�r� solo un campo determinado por el valor del campo `a`.
public struct ExampleStruct2
{
    [SerializeField]
    private ExampleEnumeration a; // El attributo tamb�en soporta propiedades siempre y cuando tengan tanto `get` como `set`.

    [SerializeField]
    [PropertyPopupOption(ExampleEnumeration.Alfa)] // El campo `b` se motrar� si `a == ExampleEnumeration.Alfa`.
    private string b;

    [SerializeField]
    [PropertyPopupOption(ExampleEnumeration.Beta)] // El campo `c` se motrar� si `a == ExampleEnumeration.Beta`.
    [Range(0, 1)]
    private float c;

    [SerializeField]
    [PropertyPopupOption(ExampleEnumeration.Gamma)] // El campo `d` se motrar� si `a == ExampleEnumeration.Gamma`.
    private LayerMask d;

    private enum ExampleEnumeration // El attributo no solo soporta enumerables sino cualquier tipo de dato, usando `EqualityComparar<T>.Default` para compararlos.
    {
        Alfa,
        Beta,
        Gamma
    }
}