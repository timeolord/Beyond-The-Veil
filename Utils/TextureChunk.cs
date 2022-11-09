using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using World;

namespace Utils
{
    public class TextureChunk : MonoBehaviour
    {
        public float[] textureData;

        private Texture2D _noiseTex;
        private Color[] _pix;
        private Renderer _rend;

        private void Awake()
        {
            _rend = GetComponent<Renderer>();
        }

        public void InitTexture()
        {
            var length = textureData.Length;
            var sqrtLength = (int) math.sqrt(length);
            _noiseTex = new Texture2D(sqrtLength, sqrtLength);
            _pix = new Color[length];
            _rend.material.mainTexture = _noiseTex;
        }

        public void SetTexture()
        {
            for (var i = 0; i < textureData.Length; i++)
            {
                _pix[i] = new Color(textureData[i], textureData[i], textureData[i], 1.0f);
            }
            _noiseTex.SetPixels(_pix);
            _noiseTex.Apply();
        }
    }
}