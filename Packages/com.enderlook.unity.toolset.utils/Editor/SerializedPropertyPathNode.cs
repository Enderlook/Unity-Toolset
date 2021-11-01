using System.Reflection;

namespace Enderlook.Unity.Toolset.Utils
{
    internal struct SerializedPropertyPathNode
    {
        public object Object;
        public MemberInfo MemberInfo;
        public int Index;
        public string path;
    }
}