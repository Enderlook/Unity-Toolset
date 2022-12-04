using Enderlook.Unity.Pathfinding;
using Enderlook.Unity.Pathfinding.Steerings;

using System;

using UnityEngine;

public class ExampleMonoBehaviour7 : MonoBehaviour
{
    private void Awake() => GetComponent<NavigationAgentRigidbody>().SetSteeringBehaviour(new ManagedSteering(), 3);

    [Serializable]
    private sealed class ManagedSteering : ISteeringBehaviour
    {
        public void DrawGizmos() { }

        public Vector3 GetDirection() => default;

        public override string ToString() => typeof(ManagedSteering).ToString();
    }
}
