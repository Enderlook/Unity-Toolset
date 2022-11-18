using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Draw in scene a handle of the position represented by this serialized property.
    /// </summary>
    [AttributeUsageRequireDataType(typeof(Vector2), typeof(Vector2Int), typeof(Vector3), typeof(Vector3), typeof(Vector4), typeof(Transform), supportEnumerableFields = true)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DrawVectorRelativeToTransformAttribute : Attribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Whenever it should use <see cref="UnityEditor.Handles.PositionHandle(Vector3, Quaternion)"/> or <see cref="UnityEditor.Handles.FreeMoveHandle(Vector3, Quaternion, float, Vector3, UnityEditor.Handles.CapFunction)"/> to draw the handler.
        /// </summary>
        internal readonly bool usePositionHandler;

        /// <summary>
        /// Icon displayed in scene. If empty no icon will be displayed.
        /// </summary>
        internal readonly string icon;

        /// <summary>
        /// Reference used to show handler. If empty, <see cref="Transform"/> of the <see cref="GameObject"/> will be used.
        /// </summary>
        internal readonly string reference;
#endif

        /// <summary>
        /// Draw in scene a handle of the position represented by this serialized property.
        /// </summary>
        /// <param name="usePositionHandler">Whenever it should use a position handle instead of a free move handle to draw the point.</param>
        /// <param name="reference">Reference used to show handler. If empty, <see cref="Transform"/> of the owner <see cref="GameObject"/> of this property will be used.</param>
        public DrawVectorRelativeToTransformAttribute(bool usePositionHandler = false, string reference = "")
        {
#if UNITY_EDITOR
            this.usePositionHandler = usePositionHandler;
            this.reference = reference;
#endif
        }

        /// <summary>
        /// Draw in scene a handle of the position represented by this serialized property.
        /// </summary>
        /// <param name="usePositionHandler">Whenever it should use a position handle instead of a free move handle to draw the point.</param>
        /// <param name="icon">Icon displayed in scene. If empty no icon will be displayed.</param>
        /// <param name="reference">Reference used to show handler. If empty, <see cref="Transform"/> of the owner <see cref="GameObject"/> of this property will be used.</param>
        public DrawVectorRelativeToTransformAttribute(string icon, bool usePositionHandler = false, string reference = "")
        {
#if UNITY_EDITOR
            this.usePositionHandler = usePositionHandler;
            this.icon = icon;
            this.reference = reference;
#endif
        }
    }
}