using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsageRequireDataType(typeof(Vector2), typeof(Vector2Int), typeof(Vector3), typeof(Vector3), typeof(Vector4), typeof(Transform), includeEnumerableTypes = true)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DrawVectorRelativeToTransformAttribute : Attribute
    {
        /// <summary>
        /// Whenever it should use <see cref="UnityEditor.Handles.PositionHandle(Vector3, Quaternion)"/> or <see cref="UnityEditor.Handles.FreeMoveHandle(Vector3, Quaternion, float, Vector3, UnityEditor.Handles.CapFunction)"/> to draw the handler.
        /// </summary>
        public readonly bool usePositionHandler;

        /// <summary>
        /// Icon displayed in scene. If empty no icon will be displayed.
        /// </summary>
        public readonly string icon;

        /// <summary>
        /// Reference used to show handler. If empty, <see cref="Transform"/> of the <see cref="GameObject"/> will be used.
        /// </summary>
        public readonly string reference;

        public DrawVectorRelativeToTransformAttribute(bool usePositionHandler = false, string reference = "")
        {
            this.usePositionHandler = usePositionHandler;
            this.reference = reference;
        }

        public DrawVectorRelativeToTransformAttribute(string icon, bool usePositionHandler = false, string reference = "")
        {
            this.usePositionHandler = usePositionHandler;
            this.icon = icon;
            this.reference = reference;
        }
    }
}