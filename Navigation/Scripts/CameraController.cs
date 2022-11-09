using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Navigation
{
    public class CameraController : MonoBehaviour
    {
        public Settings settings;
        private Transform _player;

        private float _distanceFromPlayer = 5f;

        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        private void LateUpdate()
        {
            ControlCamera();
        }
        
        private void ControlCamera()
        {
            var cameraTransform = transform;
            var angles = cameraTransform.eulerAngles;
            var x = angles.y;
            var y = angles.x;
        
            if (Input.GetMouseButton(2))  
            { 
                x += Input.GetAxis("Mouse X") * settings.cameraRotationSpeed * _distanceFromPlayer;
                y -= Input.GetAxis("Mouse Y") * settings.cameraRotationSpeed;
            }
            
            var rotation = Quaternion.Euler(y, x, 0);

            _distanceFromPlayer = Mathf.Clamp
                (_distanceFromPlayer - Input.GetAxis("Mouse ScrollWheel") * 5, settings.cameraMinDistance, settings.cameraMaxDistance);
            
            var targetPosition = rotation * new Vector3(0.0f, 0.0f, -_distanceFromPlayer) + _player.position;

            cameraTransform.rotation = rotation;
            cameraTransform.position = targetPosition;
        }
    }

}