// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEngine;

namespace BizSim.GPlay.Games
{
    internal static class JsonArrayParser
    {
        internal static T[] Parse<TWrapper, T>(string json) where TWrapper : IArrayWrapper<T>
        {
            if (string.IsNullOrEmpty(json) || json == "[]" || json == "{}")
                return System.Array.Empty<T>();

            var wrappedJson = "{\"items\":" + json + "}";
            var wrapper = JsonUtility.FromJson<TWrapper>(wrappedJson);
            return wrapper?.Items ?? System.Array.Empty<T>();
        }
    }

    internal interface IArrayWrapper<T>
    {
        T[] Items { get; }
    }
}
