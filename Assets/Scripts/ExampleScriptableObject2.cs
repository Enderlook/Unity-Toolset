using UnityEngine;

[CreateAssetMenu(fileName = "ExampleScriptableObject2", menuName = "Example/2")]
public sealed class ExampleScriptableObject2 : ScriptableObject, IExample1
{
    [SerializeField]
    private string A;
}
