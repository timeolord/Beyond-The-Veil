using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{
    [CreateAssetMenu(fileName = "InputSettings", menuName = "ScriptableObjects/InputSettings", order = 1)]
    public class Settings : ScriptableObject
    {
        public float cameraRotationSpeed;
        public float cameraMinDistance;
        public float cameraMaxDistance;
    }

}
