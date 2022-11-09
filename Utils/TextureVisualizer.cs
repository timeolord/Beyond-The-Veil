/*using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using World;

namespace Utils
{
    public class TextureVisualizer : MonoBehaviour
    {
        public GameObject prefab;
        public int size;
        public TextureChunk[] chunks;
        public Settings settings;
        public bool seeHeightmap;
        public bool update;
        
        private void Awake()
        {
            using var job = new NativeArray<JobHandle>(1, Allocator.TempJob);
            chunks = new TextureChunk[size * size];
            for (var i = 0; i < size * size; i++)
            {
                var chunkObj = Instantiate(prefab, transform);
                chunkObj.transform.localPosition = new Vector3(-(i % size) * 10, 0, -(i / size) * 10);
                chunks[i] = chunkObj.GetComponent<TextureChunk>();
                var chunk = chunks[i];
                if (seeHeightmap)
                {
                    chunk.textureData =
                        WorldGenerator.ScheduleHeightMaps(new List<int> {0},
                            new[] {new int2(i % size, i / size)}, settings)[0];
                    Extensions.Map(chunk.textureData, 0, 175, 0, 1);
                }
                else
                {
                    /*var noise = WorldGenerator.GenerateChunkMountainNoise(settings.chunkSize, settings.seed * 2,
                        new int2(i % size, i / size), settings.mountainSettings, job);
                    job[0].Complete();
                    chunk.textureData = noise.ToArray();#1#
                }
                chunk.InitTexture();
                chunk.SetTexture();
            }
        }
        private void GetChunkTexture()
        {
            using var job = new NativeArray<JobHandle>(1, Allocator.TempJob);
            for (var i = 0; i < size * size; i++)
            {
                var chunk = chunks[i];
                if (seeHeightmap)
                {
                    chunk.textureData =
                        WorldGenerator.ScheduleHeightMaps(new List<int> {0},
                            new[] {new int2(i % size, i / size)}, settings)[0];
                    Extensions.Map(chunk.textureData, 0, 175, 0, 1);
                }
                else
                {
                    /*var noise = WorldGenerator.GenerateChunkMountainNoise(settings.chunkSize, settings.seed * 2,
                            new int2(i % size, i / size), settings.mountainSettings, job);
                    job[0].Complete();
                    chunk.textureData = noise.ToArray();#1#
                }
                chunk.SetTexture();
            }
        }

        private void Update()
        {
            if (!update) return;
            GetChunkTexture();
        }
    }
}*/