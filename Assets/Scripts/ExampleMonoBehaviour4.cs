using Enderlook.Unity.Toolset.Attributes;

using UnityEngine;

public sealed class ExampleMonoBehaviour4 : MonoBehaviour
{
    // DrawVectorRelativeToTransform permite visualizar (y mover) las posiciones relativas de vectores u objetos en la escena.
    // Esto puede ser desactivado o activado en la configuración del menu "Enderlook/Toolset/Draw Vector Relative To Transform/Enable Visualization`.

    // Al mantener la tecla Ctrl, se habilita la capacidad de presionar editar una posicion a travez de un Scene GUI,
    // al realizar click en una de las esferas mientras Ctrl esta activado, muestra una ventana en la escena para editar su posición con mayor facilidad.
    // Esto puede ser desactivado o activado en la configuración del menu "Enderlook/Toolset/Draw Vector Relative To Transform/Enable Scene GUI Editing`.

    [SerializeField]
    [DrawVectorRelativeToTransform] // Dibuja una esfera de posición en la escena que representa la posición del vector relativa al gameobject donde se encuentra este componente.
    private Vector3 a;

    [SerializeField]
    [DrawVectorRelativeToTransform(true)] // Dibuja una gizmos con flechas en la escena que representa la posición del vector relativa al gameobject donde se encuentra este componente.
                                          // El attributo soporta los tipos Vector2, Vector2Int, Vector3, Vector3Int, Vector4, Component (o derivados), Transform y GameObject.
                                          // Para Component y GameObject, usa la posición de su respectivos Transforms.
    private Vector3Int b;

    [SerializeField]
    [DrawVectorRelativeToTransform("HP_Icon")] // Dibuja una esfera de posición y la textura extraida de la carpeta Resources en la escena que representa la posición del vector relativa al gameobject donde se encuentra este componente.
    private Vector2 c;

    [SerializeField]
    [DrawVectorRelativeToTransform("HP_Icon", true)] // Dibuja un gizmos con flechas y la textura extraida de la carpeta Resources en la escena que representa la posición del vector relativa al gameobject donde se encuentra este componente.
    private GameObject d;

    [SerializeField]
    private Component e;

    [SerializeField]
    [DrawVectorRelativeToTransform(reference: nameof(e))] // Dibuja una esfera de posición en la escena que representa la posición del vector relativa a la posición del transform del componente `e`.
                                                          // El attributo soporta para la propiedad `reference` los tipos Vector2, Vector2Int, Vector3, Vector3Int, Vector4, Component (o derivados), Transform y GameObject.
                                                          // Para Component y GameObject, usa la posición de su respectivos Transforms.
    private Vector4 f;
}