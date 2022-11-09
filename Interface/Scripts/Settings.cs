using UnityEngine;

namespace Interface
{
    [CreateAssetMenu(fileName = "InterfaceSettings", menuName = "ScriptableObjects/InterfaceSettings", order = 1)]
    public class Settings : ScriptableObject
    {
        public Color highlightColor;
        public float highlightHeight;
    }
}