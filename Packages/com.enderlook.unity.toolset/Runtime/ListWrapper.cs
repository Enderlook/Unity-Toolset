#if UNITY_2020_1_OR_NEWER
using Enderlook.Unity.Toolset.Attributes;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// A wrapper around and <see cref="List{T}"/> used to apply property drawers on it.
/// </summary>
/// <typeparam name="T">Type of element in the list.</typeparam>
[RedirectTo(nameof(List))]
[Serializable]
public struct ListWrapper<T>
{
    /// <summary>
    /// Wrapped <see cref="List{T}"/>.
    /// </summary>
    public List<T> List;

    /// <summary>
    /// Element in <see cref="List"/> at the <paramref name="index"/> index.
    /// </summary>
    /// <param name="index">Index of element to retrive or set from <see cref="List"/>.</param>
    /// <returns>Element in <see cref="List"/> at the <paramref name="index"/> index.</returns>
    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => List[index];
        set => List[index] = value;
    }

    /// <summary>
    /// Count of <see cref="List"/>.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => List.Count;
    }

    /// <summary>
    /// Wrap an <see cref="List{T}"/>.
    /// </summary>
    /// <param name="list"><see cref="List{T}"/> to wrap.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ListWrapper(List<T> list) => List = list;


    /// <summary>
    /// Extract a wrapped <see cref="List{T}"/>.
    /// </summary>
    /// <param name="source"><see cref="List{T}"/> to extract.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator List<T>(ListWrapper<T> source) => source.List;

    /// <summary>
    /// Wrap an <see cref="List{T}"/>.
    /// </summary>
    /// <param name="source"><see cref="List{T}"/> to wrap.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ListWrapper<T>(List<T> source) => new ListWrapper<T>(source);
}
#endif