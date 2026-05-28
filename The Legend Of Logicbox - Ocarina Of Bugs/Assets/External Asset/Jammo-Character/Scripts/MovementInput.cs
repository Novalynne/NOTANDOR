using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script requires you to have setup your animator with 3 parameters, "InputMagnitude", "InputX", "InputZ"
//With a blend tree to control the inputmagnitude and allow blending between animations.
[RequireComponent(typeof(CharacterController))]
public class MovementInput : MonoBehaviour {

    public float Velocity;
    [Space]

	public float InputX;
	public float InputZ;
	public Vector3 desiredMoveDirection;
	public bool blockRotationPlayer;
	public float desiredRotationSpeed = 0.1f;
	public Animator anim;
	public float Speed;
	public float allowPlayerRotation = 0.1f;
	public Camera cam;
	public CharacterController controller;
	public bool isGrounded;

    [Header("Animation Smoothing")]
    [Range(0, 1f)]
    public float HorizontalAnimSmoothTime = 0.2f;
    [Range(0, 1f)]
    public float VerticalAnimTime = 0.2f;
    [Range(0,1f)]
    public float StartAnimTime = 0.3f;
    [Range(0, 1f)]
    public float StopAnimTime = 0.15f;

    public float verticalVel;
    private Vector3 moveVector;

	[Header("Player Prefs")]
	public float mouseSensitivity;
	public int invertY;
	[SerializeField] private CinemachineFreeLook freeLook;

	// LOAD SETTINGS FROM PLAYER PREFS
	void LoadSettings() {
		//Get Player Prefs
		mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 9.0f);
		invertY = PlayerPrefs.GetInt("InvertY", 0);
	}
	
	// CALL THIS METHOD FROM GAMEPLAY APPLY MENU CONTROLLER METHOD WHEN SETTINGS ARE CHANGED TO UPDATE THE SENSITIVITY AND INVERTY
	public void OnSettingsChanged() {
		LoadSettings ();
		Debug.Log("Settings aggiornati");
	}
	
	// HANDLE CAMERA ROTATION BASED ON MOUSE INPUT
	void HandleCameraRotation()
	{
    	float mouseX = Input.GetAxis("Mouse X");
    	float mouseY = Input.GetAxis("Mouse Y");

    	// Sensibilità 0.1 - 10 trasformata in curva utile
    	float sens = mouseSensitivity * mouseSensitivity * 0.01f;

    	freeLook.m_XAxis.Value += mouseX * sens;

    	float invertMultiplier = (invertY == 0) ? -1f : 1f;
    	freeLook.m_YAxis.Value += mouseY * sens * invertMultiplier;
	}

	// Use this for initialization
	void Start () {

		anim = this.GetComponent<Animator> ();
		cam = Camera.main;
		controller = this.GetComponent<CharacterController> ();

		LoadSettings ();
		StartCoroutine(LockCursorNextFrame());
	}

	// COROUTINE TO LOCK THE CURSOR ON THE NEXT FRAME
	IEnumerator LockCursorNextFrame()
	{
    	yield return null; // aspetta 1 frame

    	Cursor.lockState = CursorLockMode.Locked;
    	Cursor.visible = false;
	}

	// UNLOCK CURSOR WHEN APPLICATION LOSES FOCUS
	void OnApplicationFocus(bool hasFocus)
	{
    	if (hasFocus)
    	{
        	Cursor.lockState = CursorLockMode.Locked;
        	Cursor.visible = false;
    	}
	}
	
	// Update is called once per frame
	void Update () {

		if (GameState.IsPaused){
			return;
		}
    		
		InputMagnitude ();
		HandleCameraRotation();

        isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            verticalVel -= 0;
        }
        else
        {
            verticalVel -= 1;
        }
        moveVector = new Vector3(0, verticalVel * .2f * Time.deltaTime, 0);
        controller.Move(moveVector);
    }

    void PlayerMoveAndRotation() {
		InputX = Input.GetAxis ("Horizontal");
		InputZ = Input.GetAxis ("Vertical");

		var camera = Camera.main;
		var forward = cam.transform.forward;
		var right = cam.transform.right;

		forward.y = 0f;
		right.y = 0f;

		forward.Normalize ();
		right.Normalize ();

		desiredMoveDirection = forward * InputZ + right * InputX;

		if (blockRotationPlayer == false) {
			transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (desiredMoveDirection), desiredRotationSpeed);
            controller.Move(desiredMoveDirection * Time.deltaTime * Velocity);
		}
	}

    public void LookAt(Vector3 pos)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(pos), desiredRotationSpeed);
    }

    public void RotateToCamera(Transform t)
    {

        var camera = Camera.main;
        var forward = cam.transform.forward;
        var right = cam.transform.right;

        desiredMoveDirection = forward;

        t.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed);
    }

	void InputMagnitude() {
		//Calculate Input Vectors
		InputX = Input.GetAxis ("Horizontal");
		InputZ = Input.GetAxis ("Vertical");

		//anim.SetFloat ("InputZ", InputZ, VerticalAnimTime, Time.deltaTime * 2f);
		//anim.SetFloat ("InputX", InputX, HorizontalAnimSmoothTime, Time.deltaTime * 2f);

		//Calculate the Input Magnitude
		Speed = new Vector2(InputX, InputZ).sqrMagnitude;

        //Physically move player

		if (Speed > allowPlayerRotation) {
			anim.SetFloat ("Blend", Speed, StartAnimTime, Time.deltaTime);
			PlayerMoveAndRotation ();
		} else if (Speed < allowPlayerRotation) {
			anim.SetFloat ("Blend", Speed, StopAnimTime, Time.deltaTime);
		}
	}

	public void SetMovementEnabled(bool enabled)
	{
		this.enabled = enabled;
	}
}
