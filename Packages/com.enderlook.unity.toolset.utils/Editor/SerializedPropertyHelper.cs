using System;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Utils
{
    public class SerializedPropertyHelper
    {
        public readonly SerializedProperty serializedProperty;

        private bool persistentParentError;
        private object parentTargetObject;
        private bool alreadyCalledParentTargetObject;

        private bool persistentTargetError;
        private object targetObject;
        private bool alreadyCalledTargetObject;

        public SerializedPropertyHelper(SerializedProperty serializedProperty) => this.serializedProperty = serializedProperty;

        /// <summary>
        /// Reset cached values, this method should be executed once per Unity Editor update.
        /// </summary>
        public void ResetCycle()
        {
            alreadyCalledParentTargetObject = false;
            alreadyCalledTargetObject = false;
        }

        /// <summary>
        /// Gets the parent target object of <see cref="serializedProperty"/>. It does work for nested serialized properties.<br/>
        /// If it doesn't have parent it will return itself.
        /// </summary>
        /// <returns>Value of the <see cref="SerializedProperty"/> as <see cref="object"/>.</returns>
        public object GetParentTargetObjectOfProperty() => serializedProperty.GetParentTargetObjectOfProperty();

        /// <summary>
        /// Gets the target object of <see cref="serializedProperty"/>. It does work for nested serialized properties.
        /// </summary>
        /// <param name="last">At which depth from last to first should return.</param>
        /// <returns>Value of the <see cref="SerializedProperty"/> as <see cref="object"/>.</returns>
        public object GetTargetObjectOfProperty(int last = 0) => serializedProperty.GetTargetObjectOfProperty(last);

        /// <summary>
        /// Try to execute <see cref="GetParentTargetObjectOfProperty"/> ignoring error produced by property drawer rendering before update of the <see cref="SerializedProperty"/>.<br/>
        /// It does not ignore other types of errors.
        /// Executing this more than one time per Unity Editor frame may wrong result.<br/>
        /// </summary>
        /// <param name="result">Parent target object of property.</param>
        /// <returns>Whenever it was successful or not.</returns>
        private bool TryGetParentTargetObjectOfPropertyUnsafe(out object result)
        {
            /* Sometimes when an array is resized, the property drawer is renderer before the actual array is resized.
             * That is why we may get error from our custom method GetParentTargetObjectOfProperty.*/
            try
            {
                result = GetParentTargetObjectOfProperty();
            }
            catch (IndexOutOfRangeException) when (!persistentParentError)
            {
                persistentParentError = true;
                result = default;
                return false;
            }
            persistentParentError = false;
            return true;
        }

        /// <summary>
        /// Try to execute <see cref="GetParentTargetObjectOfProperty"/> ignoring error produced by property drawer rendering before update of the <see cref="SerializedProperty"/>.<br/>
        /// It does not ignore other types of errors.
        /// This value is cached until <see cref="ResetCycle"/> is <see langword="true"/>;
        /// </summary>
        /// <param name="result">Parent target object of property.</param>
        /// <returns>Whenever it was successful or not.</returns>
        public bool TryGetParentTargetObjectOfProperty(out object result)
        {
            if (alreadyCalledParentTargetObject)
            {
                result = parentTargetObject;
                return result != null;
            }
            else
            {
                persistentParentError = false;
                alreadyCalledParentTargetObject = true;
                if (TryGetParentTargetObjectOfPropertyUnsafe(out parentTargetObject))
                {
                    result = parentTargetObject;
                    return true;
                }
                else
                {
                    result = default;
                    return false;
                }
            }
        }

        /// <summary>
        /// Try to execute <see cref="GetTargetObjectOfProperty"/> ignoring error produced by property drawer rendering before update of the <see cref="SerializedProperty"/>.<br/>
        /// It does not ignore other types of errors.<br/>
        /// Executing this more than one time per Unity Editor frame may produce wrong results.<br/>
        /// </summary>
        /// <param name="result">Parent target object of property.</param>
        /// <returns>Whenever it was successful or not.</returns>
        private bool TryGetTargetObjectOfPropertyUnsafe(out object result)
        {
            /* Sometimes when an array is resized, the property drawer is renderer before the actual array is resized.
             * That is why we may get error from our custom method GetTargetObjectOfProperty.*/
            try
            {
                result = GetTargetObjectOfProperty();
            }
            catch (IndexOutOfRangeException) when (!persistentTargetError)
            {
                persistentTargetError = true;
                result = default;
                return false;
            }
            persistentTargetError = false;
            return true;
        }

        /// <summary>
        /// Try to execute <see cref="GetTargetObjectOfProperty"/> ignoring error produced by property drawer rendering before update of the <see cref="SerializedProperty"/>.<br/>
        /// It does not ignore other types of errors.<br/>
        /// This value is cached until <see cref="ResetCycle"/> is <see langword="true"/>;
        /// </summary>
        /// <param name="result">Parent target object of property.</param>
        /// <returns>Whenever it was successful or not.</returns>
        public bool TryGetTargetObjectOfProperty(out object result)
        {
            if (alreadyCalledTargetObject)
            {
                result = targetObject;
                return result != null;
            }
            else
            {
                alreadyCalledTargetObject = true;
                persistentTargetError = false;
                if (TryGetTargetObjectOfPropertyUnsafe(out targetObject))
                {
                    result = targetObject;
                    return true;
                }
                else
                {
                    result = default;
                    return false;
                }
            }
        }

        /// <summary>
        /// Produce a <see cref="GUIContent"/> with the <see cref="SerializedProperty.displayName"/> as <see cref="GUIContent.text"/> and <see cref="SerializedProperty.tooltip"/> as <see cref="GUIContent.tooltip"/>.
        /// </summary>
        /// <returns><see cref="GUIContent"/> of <see cref="serializedProperty"/>.</returns>
        public GUIContent GetGUIContent() => serializedProperty.GetGUIContent();

        /// <summary>
        /// Get the <see cref="Attribute"/> of type <typeparamref name="T"/> of the field.<br/>
        /// It does include <see langword="private"/> fields of super-classes.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="Attribute"/></typeparam>
        /// <param name="includeInheritedPrivate">Whenever it should also search private fields of supper-classes.</param>
        /// <returns>The <see cref="Attribute"/> of type <typeparamref name="T"/>.</returns>
        public T GetAttributeFromField<T>(bool includeInheritedPrivate = true) where T : Attribute
        {
            // TODO: GetParentTargetObjectOfProperty is called twice
            if (TryGetParentTargetObjectOfProperty(out object _))
                return serializedProperty.GetFieldInfo(includeInheritedPrivate).GetCustomAttribute<T>(true);
            return null;
        }

        /// <summary>
        /// Try get the <see cref="Attribute"/> of type <typeparamref name="T"/> of the field.<br/>
        /// It does include <see langword="private"/> fields of super-classes.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="Attribute"/></typeparam>
        /// <param name="attribute">The <see cref="Attribute"/> of type <typeparamref name="T"/>.</param>
        /// <param name="includeInheritedPrivate">Whenever it should also search private fields of supper-classes.</param>
        /// <returns>Whenever it could be found or not.</returns>
        public bool TryGetAttributeFromField<T>(out T attribute, bool includeInheritedPrivate = true) where T : Attribute
        {
            attribute = GetAttributeFromField<T>(includeInheritedPrivate);
            return attribute != null;
        }
    }
}