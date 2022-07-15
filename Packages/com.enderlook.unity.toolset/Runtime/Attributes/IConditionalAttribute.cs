namespace Enderlook.Unity.Toolset.Attributes
{
#if UNITY_EDITOR
    internal interface IConditionalAttribute
    {
        public string FirstProperty { get; }

        public string SecondProperty { get; }

        public object CompareTo { get; }

        public ComparisonMode Comparison { get; }

        public bool Chain { get; }

        public ConditionalMode Mode { get; }

        internal enum ConditionalMode
        {
            WithObject,
            WithProperty,
            Single,
        }
    }
#endif
}