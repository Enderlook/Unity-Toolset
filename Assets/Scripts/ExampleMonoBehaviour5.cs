using Enderlook.Unity.Toolset.Attributes;

using UnityEngine;

public sealed class ExampleMonoBehaviour5 : MonoBehaviour
{
    [SerializeField]
    [Label("Nuevo Nombre")] // Cambia el nombre del campo en el inspector.
    private int a;

    [SerializeField]
    [Label("Nuevo Nombre", "Nueva Descripción")] // Cambia el nombre y descripción del campo en el inspector.
    private int b;

    [SerializeField]
    [Label(nameof(e), LabelMode.ByReference)] // Cambia el nombre del campo en el inspector usando el valor del campo `e`.
                                              // También funciona con propiedades y métodos de instancia y estáticos.
                                              // Soporta parametros opcionales y param.
    private int c;

    [SerializeField]
    [Label(nameof(e), LabelMode.ByReference, nameof(F), LabelMode.ByReference)] // Cambia el nombre del campo en el inspector usando el valor del campo `d` y la descripcicón por el valor de la propiedad `F`.
    private int d;

    [SerializeField]
    private string e;

    private string F => $"Descripción: {e}";
}