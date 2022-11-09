using System;
using UnityEngine;

namespace World
{
    public static class Events
    {
        public static Action<GameObject> onMouseEnterChunk;
        public static Action<GameObject> onMouseExitChunk;
    }
}