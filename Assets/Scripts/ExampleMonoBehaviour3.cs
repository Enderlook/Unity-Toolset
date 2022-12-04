using Enderlook.Unity.Toolset.Attributes;

using UnityEngine;

public sealed class ExampleMonoBehaviour3 : MonoBehaviour
{
    [SerializeField]
    [DrawTexture] // Dibuja el sprite junto al campo.
    private Sprite a;

    [SerializeField]
    [DrawTexture(DrawTextureMode.NewLineLeft)] // Dibuja la textura debajo del campo a la izquierda.
    private Texture2D b;

    [SerializeField]
    [DrawTexture(DrawTextureMode.NewLineRight)] // Dibuja la textura or sprite encontrado en la carpeta Resources debajo del campo a la derecha.
    private string c;

    [SerializeField]
    [DrawTexture(DrawTextureMode.NewLineCenter)] // Dibuja el sprite debajo del campo en el centro.
    private Sprite d;

    [SerializeField]
    [DrawTexture(10)] // Dibuja el sprite junto al campo especificando un tamaño.
    private Sprite e;

    [SerializeField]
    [DrawTexture(100)] // Dibuja el sprite junto al campo especificando un tamaño.
                       // Reescala la textura si tiene permiso de lectura configurados durante la importación de la textura.
    private Sprite f;

    [SerializeField]
    [DrawTexture(20, DrawTextureMode.NewLineCenter)] // Dibuja el sprite debajo del campo al centro especificando un tamaño.
    private Sprite h;
}
