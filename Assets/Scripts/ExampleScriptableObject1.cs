using UnityEngine;

[CreateAssetMenu(fileName = "ExampleScriptableObject1", menuName = "Example/1")]
public sealed class ExampleScriptableObject1 : ScriptableObject, IExample1, IExample2
{
    [SerializeField]
    private int A;
}
