using Enderlook.Unity.Toolset.Attributes;

using System;
using System.Runtime.CompilerServices;

/// <summary>
/// A wrapper around and array used to apply property drawers on it.
/// </summary>
/// <typeparam name="T">Type of element in the array.</typeparam>
[RedirectTo(nameof(Array))]
[Serializable]
public struct ArrayWrapper<T>
{
    /// <summary>
    /// Wrapped array.
    /// </summary>
    public T[] Array;

    /// <summary>
    /// Element in <see cref="Array"/> at the <paramref name="index"/> index.
    /// </summary>
    /// <param name="index">Index of element to retrive or set from <see cref="Array"/>.</param>
    /// <returns>Element in <see cref="Array"/> at the <paramref name="index"/> index.</returns>
    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Array[index];
        set => Array[index] = value;
    }

    /// <summary>
    /// Length of <see cref="Array"/>.
    /// </summary>
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Array.Length;
    }

    /// <summary>
    /// Wrap an array.
    /// </summary>
    /// <param name="array">Array to wrap.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ArrayWrapper(T[] array) => Array = array;

    /// <summary>
    /// Extract a wrapped array.
    /// </summary>
    /// <param name="source">Array to extract.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T[](ArrayWrapper<T> source) => source.Array;

    /// <summary>
    /// Wrap an array.
    /// </summary>
    /// <param name="source">Array to wrap.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ArrayWrapper<T>(T[] source) => new ArrayWrapper<T>(source);
}