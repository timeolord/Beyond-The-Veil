using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    public float playerSpeed = 100.0f;

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            var main = Camera.main;
            gameObject.transform.forward = main.transform.forward;
            controller.Move(new Vector3(main.transform.forward.x, 0, main.transform.forward.z)  * (Time.deltaTime * playerSpeed));
        }
        if (Input.GetKey(KeyCode.S))
        {
            var main = Camera.main;
            gameObject.transform.forward = -main.transform.forward;
            controller.Move(new Vector3(-main.transform.forward.x, 0, -main.transform.forward.z) * (Time.deltaTime * playerSpeed));
        }
        if (Input.GetKey(KeyCode.A))
        {
            var main = Camera.main;
            gameObject.transform.forward = -main.transform.right;
            controller.Move( new Vector3(-main.transform.right.x, 0, -main.transform.right.z) * (Time.deltaTime * playerSpeed));
        }
        if (Input.GetKey(KeyCode.D))
        {
            var main = Camera.main;
            gameObject.transform.forward = main.transform.right;
            controller.Move(new Vector3(main.transform.right.x, 0, main.transform.right.z) * (Time.deltaTime * playerSpeed));
        }
        playerVelocity.y = 0f;
        // Changes the height position of the player
        if (Input.GetKey(KeyCode.Space))
        {
            playerVelocity.y += playerSpeed;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            playerVelocity.y -= playerSpeed;
        }
        
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
