using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static World.TileName;

namespace World
{
    [CreateAssetMenu(fileName = "WorldSettings", menuName = "ScriptableObjects/WorldSettings", order = 1)]
    public class Settings : ScriptableObject
    {
        public int chunkSize;
        public int mapSize;
        public int seed;
        public int maxHeight;
        public NoiseSettings terrainSettings;
        public NoiseSettings mountainSettings;
        public NoiseSettings flatnessSettings;
        /*public NoiseSettings roughnessSettings;*/
        public int scheduleDistance;
        public int renderDistance;
        public Color[] tileColors;

        #if UNITY_EDITOR
        public void CreateTextureArray()
        {
            var length = tileColors.Length;
            var textureArray = new Texture2DArray(1,1, length, TextureFormat.RGBA32, false);
            for (var i = 0; i < length; i++)
            {
                var color = new []{ tileColors[i] };
                textureArray.SetPixels(color, i);
            }
            textureArray.Apply();
            AssetDatabase.CreateAsset(textureArray, "Assets/World/Textures/TileTextures.asset");
        }
        #endif

        /*public void SetTileTextures()
        {
            tileTextures = new List<TileTexture>();
            SetTileTexture(Grass);
            SetTileTexture(Snow);
            SetTileTexture(Stone);
        }
        
        private void SetTileTexture(TileName tileName)
        {
            tileTextures.Add(new TileTexture()
            {
                Name = tileName.ToString(),
                Color = tileColors[(int) tileName],
                ID = (int) tileName
            });
        }*/
    }

    [System.Serializable]
    public class NoiseSettings
    {
        public float frequency;
        public float amplitude;
        public float lacunarity;
        public int octaves;
        public float persistence;
    }
}