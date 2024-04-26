﻿using System;
using System.Collections.Generic;

namespace RelinkModOrganizer.ThirdParties.DataTools;

public static class DataToolsExtensions
{
    public static int BinarySearch<T>(
        this IList<T>? list,
        T value,
        IComparer<T>? comparer = default)
    {
        ArgumentNullException.ThrowIfNull(list);

        comparer ??= Comparer<T>.Default;

        int lower = 0;
        int upper = list.Count - 1;

        while (lower <= upper)
        {
            int middle = lower + (upper - lower) / 2;
            int comparisonResult = comparer.Compare(value, list[middle]);
            if (comparisonResult == 0)
                return middle;
            else if (comparisonResult < 0)
                upper = middle - 1;
            else
                lower = middle + 1;
        }

        return ~lower;
    }

    public static int AddSorted<T>(this IList<T>? list, T value)
    {
        ArgumentNullException.ThrowIfNull(list);

        int x = list.BinarySearch(value);
        int newIdx = x >= 0 ? x : ~x;
        list.Insert(newIdx, value);

        return newIdx;
    }
}