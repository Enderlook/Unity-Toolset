namespace Enderlook.Unity.Toolset.Attributes
{
#if UNITY_EDITOR
    internal interface IConditionalAttribute
    {
        string FirstProperty { get; }

        string SecondProperty { get; }

        object CompareTo { get; }

        ComparisonMode Comparison { get; }

        bool Chain { get; }

        ConditionalMode Mode { get; }
    }

    internal enum ConditionalMode
    {
        WithObject,
        WithProperty,
        Single,
    }
#endif
}