using System.Diagnostics;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Utils
{
    [DebuggerDisplay("Obj: {Object} Member: {MemberInfo} ({Index}) Path: {path}")]
    internal struct SerializedPropertyPathNode
    {
        public object Object;
        public MemberInfo MemberInfo;
        public int Index;
        public string path;
    }
}