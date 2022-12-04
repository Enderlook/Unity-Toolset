using UnityEngine;

[CreateAssetMenu(fileName = "ExampleScriptableObject3", menuName = "Example/3")]
public sealed class ExampleScriptableObject3 : ScriptableObject
{
    [SerializeField]
    private MonoBehaviour A;
}
