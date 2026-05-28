using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Control Script/FPSInput")]

public class FPSInput : MonoBehaviour
{
    private CharacterController characterController;
    public float speed = 6.0f;
    public float gravity = -9.8f;
    public float jumpHeight = 2.0f;
    private float verticalVelocity;
    public PlayerGrab grabSystem;
   
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Se il player sta afferrando qualcosa, non può muoversi
        if (grabSystem.IsGrabbing()){
            return;
        }
        float deltaX = Input.GetAxis("Horizontal")* speed;
        float deltaZ = Input.GetAxis("Vertical") * speed;
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = Vector3.ClampMagnitude(movement,speed);
        movement = transform.TransformDirection(movement);
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -2f;

            // Salto
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        // Gravità
        verticalVelocity += gravity * Time.deltaTime;

        movement.y = verticalVelocity;

        characterController.Move(movement * Time.deltaTime);
    }
}
