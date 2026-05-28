using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class mouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
       XY, X, Y
    }

    public RotationAxes mouseRotation = RotationAxes.XY;
    public float sensitivityHor = 9.0f;
    public float sensitivityVert = 9.0f;
    public float maximumvert = 45.0f;
    public float minimumVert = -45.0f;
    private float verticalRot = 0;
    private float horizontalRot = 0;
   
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        if(GameState.IsPaused){
            return;
        }

        switch (mouseRotation)
        {
            case RotationAxes.X:
                //Rotazione orizzontale
                transform.parent.Rotate(0,Input.GetAxis("Mouse X") * sensitivityHor, 0);
                break;
            case RotationAxes.Y:
                //Rotazione verticale
                verticalRot -= sensitivityVert * Input.GetAxis("Mouse Y");
                //Limita l'angolo
                verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumvert);
                // Nuovo vettore
                transform.localEulerAngles = new Vector3(verticalRot, horizontalRot, 0); 
                break;
            case RotationAxes.XY:
                //Entrambi
                verticalRot -= sensitivityVert * Input.GetAxis("Mouse Y");
                verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumvert);
                horizontalRot += Input.GetAxis("Mouse X") * sensitivityHor;
                // Nuovo vettore
                transform.parent.eulerAngles = new Vector3(0, horizontalRot, 0);
                transform.localEulerAngles = new Vector3(verticalRot, 0, 0);
                break;
            default:
                throw new System.ArgumentException("Mouse rotation wrongly specified");

        }
    }
}
