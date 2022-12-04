using Enderlook.Unity.Toolset.Attributes;

using UnityEngine;

public sealed class ExampleMonoBehaviour1 : MonoBehaviour
{
    [SerializeField]
    private bool a;

    [SerializeField]
    // Casi todos los attributos creados soportan pueden ser aplicados juntos.
    // (No se sobreescriben como los de Unity sino que se combinan).
    [Indented] // Añade tabulación al campo.
    [ShowIf(nameof(a))] // El campo se mostrará si `a == true`.
    private bool b;

#if UNITY_2020_1_OR_NEWER
    [SerializeField]
    [Indented]
    [ShowIf(nameof(a))]
    private ArrayWrapper<bool> c; // ArrayWrapper<T> y ListWrapper<T> permiten aplicar atributos al campo entero en vez de a cada valor de la colección individualmente.
#endif

    [SerializeField]
    [Indented] // Añade doble tabulación al campo.
    [ShowIf(nameof(b), chain: true)] // `chain` hace que se deba también cumplir la condición de `mostrable`.
    private string d;

    [SerializeField]
    [EnableIf(nameof(a))] // `EnableIfAttribute` soporta todas las mismas carácteristicas que `ShowIfAttribute`.
    [Range(0, 1)]
    private float e;

    [SerializeField]
    [Indented]
    [ShowIf(nameof(e), .3f, ComparisonMode.GreaterOrEqual)] // El campo se mostrara si `e >= 0.3`.
    [Range(0, 1)]
    private float f;

    [SerializeField]
    [Indented(2)]
    [ShowIf(ComparisonMode.LessOrEqual, nameof(e), nameof(f))] // El campo se mostrará si `e <= f`.
    private string g;

    [SerializeField]
    [Indented]
    [ShowIf(nameof(Property))] // El campo se mostrará si la propiedad devuelve `true`. También soporta propiedades estaticas.
    private ScriptableObject h;

    [SerializeField]
    [Indented(2)]
    [ShowIf(nameof(h))] // El campo se mostrará si `h != null`.
    private int j;

    private bool Property => a;

    [SerializeField]
    [ShowIf(nameof(Method))] // El campo se mostrará si el método devuelve `true`.
                             // También soporta metodos de instancia.
                             // Soporta parametros opcionales y param.
    private string i;

    private static bool Method(bool unused = true, params int[] unused2) => !Application.isPlaying;
}
