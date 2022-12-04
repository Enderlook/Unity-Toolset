using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

public sealed class ExampleMonoBehaviour2 : MonoBehaviour
{
    [SerializeField]
    [RestrictType(typeof(IExample1))] // Solo permite asignar valores que implementen `IExample1`.
    private ScriptableObject a;

    [SerializeField]
    [RestrictType(typeof(IExample1), typeof(IExample2))] // Solo permite asignar valores que implementen `IExample1` y `IExample2`.
    private ScriptableObject b;

    [SerializeField]
    [Expandable] // Permite expandir cualquier objeto que derive de `UnityEngine.Object` en el editor.
    private MonoBehaviour c;

    [SerializeField]
    [Inline] // Muestra los campos del objeto des-anidados en el editor.
             // Dado que hereda de `UnityEngine.Object`, también muestra el campo de assignación.
    private MonoBehaviour d;

#if UNITY_2020_1_OR_NEWER
    [SerializeField]
    [Inline] // Muestra los campos del objeto des-anidados en el editor.
    private A<int> e;
#endif

    [SerializeField]
    [InitOnly] // Solo permite editar el campo mientras el editor no esta jugando.
    private int f;

    [SerializeField]
    [ReadOnly] // No permite edtar el campo.
    private int g;

    [field: SerializeField]
    [field: IsProperty] // Normaliza el nombre de la propiedad en el inspector para que no se vea mal.
    private int H { get; set; }

    [SerializeField]
    [Layer] // Reemplaza el campo por una lista de las layers.
            // Soporta `int`, `float`, `string` y `LayerMask`.
    private string i;

#if UNITY_2020_1_OR_NEWER
    [Serializable]
    public class A<T>
    {
        [SerializeField]
        private int x;

        [SerializeField]
        private int y;

        [SerializeField]
        private T z;
    }
#endif
}