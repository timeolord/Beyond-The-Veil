using System;
using System.Collections.Generic;
using MeshUtil;
using Unity.Mathematics;
using UnityEngine;

namespace World
{
    public static class Chunks
    {
        public static Vector2Int renderChunk;
        public static Dictionary<int, int> completedChunks;
        public static Dictionary<int, int> scheduledChunks;
        //public static MeshFuture[] meshFutures;
        public static Mesh[] meshes;
        public static MeshFilter[] meshFilters;
        public static int2[] positions;
        public static GameObject[] gameObjects;
    }
}