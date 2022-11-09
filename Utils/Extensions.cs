using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal static class Extensions
{
    public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> input)
    {
        var i = 0;
        foreach (var t in input)
            yield return (i++, t);
    }
    
    public static float Map(float x, float inMin, float inMax, float outMin, float outMax, bool clamp = true)
    {
        if (clamp) x = Math.Max(inMin, Math.Min(x, inMax));
        return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }
    public static void Map(float[] map, float min, float max, float newMin, float newMax)
    {
        for (var i = 0; i < map.Length; i++)
        {
            map[i] = Map(map[i], min, max, newMin, newMax);
        }
    }
    
    public static void NormalizeMap(float[] map, float newMin, float newMax)
    {
        var max = Mathf.Max(map);
        var min = Mathf.Min(map);
        for (var i = 0; i < map.Length; i++)
        {
            map[i] = Map(map[i], min, max, newMin, newMax);
        }
    }
}