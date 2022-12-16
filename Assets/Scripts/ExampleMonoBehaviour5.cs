using Enderlook.Unity.Toolset.Attributes;

using UnityEngine;

public sealed class ExampleMonoBehaviour5 : MonoBehaviour
{
    [SerializeField]
    [Label("Nuevo Nombre")] // Cambia el nombre del campo en el inspector.
    private int a;

    [SerializeField]
    [Label("Nuevo Nombre", "Nueva Descripci�n")] // Cambia el nombre y descripci�n del campo en el inspector.
    private int b;

    [SerializeField]
    [Label(nameof(e), LabelMode.ByReference)] // Cambia el nombre del campo en el inspector usando el valor del campo `e`.
                                              // Tambi�n funciona con propiedades y m�todos de instancia y est�ticos.
                                              // Soporta parametros opcionales y param.
    private int c;

    [SerializeField]
    [Label(nameof(e), LabelMode.ByReference, nameof(F), LabelMode.ByReference)] // Cambia el nombre del campo en el inspector usando el valor del campo `d` y la descripcic�n por el valor de la propiedad `F`.
    private int d;

    [SerializeField]
    private string e;

    private string F => $"Descripci�n: {e}";
}