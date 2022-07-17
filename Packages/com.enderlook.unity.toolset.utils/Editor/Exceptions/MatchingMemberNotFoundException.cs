using System;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// Exception that is thrown when no member in a type is found that matches the specified signature.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "<pendiente>")]
    public class MatchingMemberNotFoundException : Exception
    {
        /// <summary>
        /// Represent that no member named <paramref name="name"/> that was found in <paramref name="type"/> which matches:
        /// <list type="bullet">
        ///     <item><description>If <see cref="FieldInfo"/>, its <see cref="FieldInfo.FieldType"/> must be of type <paramref name="memberType"/>.</description></item>
        ///     <item><description>If <see cref="PropertyInfo"/>, its <see cref="PropertyInfo.PropertyType"/> must be of type <paramref name="memberType"/> and it must have a getter.</description></item>
        ///     <item><description>If <see cref="MethodInfo"/>, its <see cref="MethodInfo.ReturnType"/> must be of type <paramref name="memberType"/> and apart from the <see langword="this"/> parameter, all other parameters (if any) must be optional, has default value or has <see cref="ParamArrayAttribute"/>.</description></item>
        /// </list>
        /// The criteria was not matched by any member looked recursively through the inheritance hierarchy of <paramref name="type"/> regardless of members accessibility.
        /// </summary>
        /// <param name="name">Name of the member whose signature doesn't match.</param>
        /// <param name="type">Type where the member was gotten.</param>
        /// <param name="resultType">Result type expected.</param>
        /// <param name="bindingFlags">Binding flags of member expected.</param>
        public MatchingMemberNotFoundException(string name, Type type, Type resultType, ExhaustiveBindingFlags bindingFlags) : base(GetMessage(name, type, resultType, bindingFlags)) { }
    
        private static string GetMessage(string name, Type type, Type resultType, ExhaustiveBindingFlags bindingFlags)
        {
            string text;
            switch (bindingFlags)
            {
                case ExhaustiveBindingFlags.Instance:
                    text = "instance ";
                    break;
                case ExhaustiveBindingFlags.Static:
                    text = "static ";
                    break;
                default:
                    text = "";
                    break;
            }
            return $"No {text}member named '{name}' was found in type '{type}' of type '{resultType}' if field or property, or whose return type is '{resultType}' and apart from the this parameter (if instance method), all other parameters (if any) are optional, has default value or has {typeof(ParamArrayAttribute)} attribute.";
        }
    }
}
