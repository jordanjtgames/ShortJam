using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterLove.StateMachine;
using static Utils.Helpers;
using UnityEngine.InputSystem;

public class Driver
{
	public StateEvent Update;
	public StateEvent<Collision> OnCollisionEnter;
}

public class PlayerLocomotion : MonoBehaviour
{
	public Camera playerCamera;
	public Animator characterAnim, reflectionAnim;
	public CharacterController CC;
	public Transform graphicsParent, reflectionParent, cameraLookPivot, focusTarget;
	
	private PlayerInputs playerInputs;
	private bool isGamepad = false;
	
	private Vector2 smoothedInputDir;
	
	private Vector3 characterExtraVelocity, smoothedMoveDir, currentCameraOffset;
	private float lookX = 0f, lastPlayerY = 0f, currentMoveSpeedBlend = 0f;
	private bool isFocused = false;
	private Quaternion unfocusedQuat;
	private float unfocusedLookX = 0f, focus_t = 0f, reveal_t = 0f, phaseOut_t = 0f;
	private bool phasing = false;
	
	private SkinnedMeshRenderer[] graphicsRends, reflectionRends;
	private Light reflectionLight;
	
	public enum States
	{
		Init,
		Idle,
		Phasing,
		Moving,
		Falling,
		Attacking
	}
	
	StateMachine<States, Driver> fsm;
	
	void Update()
	{
		fsm.Driver.Update.Invoke();
		
		ListenForControlsSwitch();
		
		if(playerInputs.PlayerMap.Aim.WasPerformedThisFrame()) {
			if(!isFocused) {
				unfocusedQuat = CC.transform.rotation;
				unfocusedLookX = lookX;
				focus_t = 0f;
			}
			isFocused = !isFocused;
		}
		
		bool allowMovement = !phasing;
		bool isSprinting = playerInputs.PlayerMap.Sprint.IsPressed();
		
		Vector2 inputDir = playerInputs.PlayerMap.Move.ReadValue<Vector2>();
		if(inputDir.magnitude < 0.2 || !allowMovement) inputDir = Vector2.zero;
		smoothedInputDir = Vector3.Slerp(smoothedInputDir, inputDir + (Vector2.one*0.001f), Time.deltaTime * 7.3f);
		float angle = Mathf.Atan2(smoothedInputDir.x, smoothedInputDir.y) * Mathf.Rad2Deg;
		int regionID = GetInputAngleRegionID(angle);
		
		float halfAngle = Mathf.Abs(angle / 360f);
		float animMovementAngle = angle < 0 ? 0.5f-halfAngle : 0.5f+halfAngle;
		
		//float moveSpeedBlend = 0.0f; // 0.25f;
		float moveSpeedBlend = isFocused ? 0f : inputDir.magnitude * 0.5f; // 0.25f;
		if(isSprinting) moveSpeedBlend = 1f;
		currentMoveSpeedBlend = Mathf.Lerp(currentMoveSpeedBlend, moveSpeedBlend, Time.deltaTime * 1.8f);
		
		float lookSensitivity = isGamepad ? 0.25f : 0.15f;
		Vector2 lookDelta = playerInputs.PlayerMap.Look.ReadValue<Vector2>() * lookSensitivity;
		if(isFocused)
		{
			focus_t = Mathf.Clamp01(focus_t + (Time.deltaTime * 3f));
			float focusLerp_t = Curves.EaseOutCubic(focus_t);
			Vector3 dirToFocus = new Vector3(focusTarget.position.x,0,focusTarget.position.z) - new Vector3(CC.transform.position.x,0,CC.transform.position.z);
			if(!phasing) CC.transform.rotation = Quaternion.Lerp(unfocusedQuat, Quaternion.LookRotation(dirToFocus,Vector3.up), focusLerp_t);
			
			float hypot = Vector3.Distance(cameraLookPivot.position, focusTarget.position);
			float opp = cameraLookPivot.position.y - focusTarget.position.y;
			float lookAngle = Mathf.Asin(opp / hypot) * Mathf.Rad2Deg;
			lookX = Mathf.Lerp(unfocusedLookX, lookAngle, focusLerp_t);
		}else
		{
			CC.transform.Rotate(Vector3.up * lookDelta.x);
			lookX = Mathf.Clamp(lookX + -lookDelta.y, -89,89);
		}
		cameraLookPivot.localEulerAngles = new Vector3(lookX,0,0);
		
		float moveSpeedMod = Mathf.Lerp(2.75f,9f,Mathf.Clamp01(currentMoveSpeedBlend * 1.15f));
		//moveSpeedMod *= isSprinting ? MovementSpeedDirMod(regionID) : 1f;
		moveSpeedMod *= MovementSpeedDirMod(regionID);
		if(isFocused && !isSprinting) moveSpeedMod *= 0.9f;
		smoothedMoveDir = Vector3.Lerp(smoothedMoveDir, CC.transform.TransformDirection(new Vector3(inputDir.x, 0, inputDir.y)) * moveSpeedMod, Time.deltaTime * 10f);
		
		characterExtraVelocity = Vector3.Lerp(characterExtraVelocity, Vector3.zero, Time.deltaTime * 5);
		Vector3 finalCharMoveVector = (smoothedMoveDir + characterExtraVelocity) * Time.deltaTime;
		if(CC.enabled) CC.Move(finalCharMoveVector);
		
		if(inputDir.magnitude > 0) lastPlayerY = CC.transform.eulerAngles.y;
		//graphicsParent.rotation = Quaternion.Slerp(graphicsParent.rotation, Quaternion.Euler(0,angle,0) * Quaternion.Euler(0,lastPlayerY,0), Time.deltaTime * 15);
		graphicsParent.rotation = Quaternion.Slerp(graphicsParent.rotation, Quaternion.Euler(0,lastPlayerY,0), Time.deltaTime * 15);
		graphicsParent.position = CC.transform.position;
		
		characterAnim.SetFloat("MovementAngle", animMovementAngle);
		characterAnim.SetFloat("MoveSpeedBlend", currentMoveSpeedBlend);
		characterAnim.SetFloat("MoveSpeedMod", Mathf.Lerp(1.25f,1.0f,currentMoveSpeedBlend));
		characterAnim.SetBool("Moving", inputDir.magnitude > 0);
		
		if(focusTarget != null)
		{
			Vector3 reflectionPos = RotatePointAroundPivot(CC.transform.position, focusTarget.transform.position, Vector3.up * 180);
			reflectionPos.y = CC.transform.position.y;
			reflectionParent.position = reflectionPos;
			reflectionParent.rotation = CC.transform.rotation * Quaternion.Euler(0,180,0);
		}
		
		reflectionAnim.SetFloat("MovementAngle", animMovementAngle);
		reflectionAnim.SetFloat("MoveSpeedBlend", currentMoveSpeedBlend);
		reflectionAnim.SetFloat("MoveSpeedMod", Mathf.Lerp(1.25f,1.0f,currentMoveSpeedBlend));
		reflectionAnim.SetBool("Moving", inputDir.magnitude > 0);
		
		currentCameraOffset = Vector3.Lerp(currentCameraOffset, GetTargetCameraOffset(), Time.deltaTime * 8f);
		playerCamera.transform.localPosition = currentCameraOffset;
		
		bool showReflection = isFocused && focus_t >= 0.51f;
		reveal_t = Mathf.Lerp(reveal_t, showReflection ? 1f : 0f, Time.deltaTime * (showReflection ? 3.5f : 7f));
		reflectionLight.intensity = reveal_t * 6.32f;
		foreach(SkinnedMeshRenderer rend in reflectionRends)
		{
			for (int i = 0; i < rend.materials.Length; i++)
			{
				rend.materials[i].SetFloat("_Reveal", reveal_t);
			}
		}
		phaseOut_t = Mathf.Lerp(phaseOut_t, phasing ? 2f : 0f, Time.deltaTime * (phasing ? 12f : 6f));
		foreach(SkinnedMeshRenderer rend in graphicsRends)
		{
			for (int i = 0; i < rend.materials.Length; i++)
			{
				rend.materials[i].SetFloat("_PhaseOut", phaseOut_t);
			}
		}
		if(playerInputs.PlayerMap.Interact.WasPerformedThisFrame())
		{
			if(!phasing && isFocused) fsm.ChangeState(States.Phasing);
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		fsm.Driver.OnCollisionEnter.Invoke(collision);
	}

	// State Drivers ^

	void Awake()
	{
		playerInputs = new PlayerInputs();
		playerInputs.PlayerMap.Enable();
		// playerInputs.bindingMask = new InputBinding {groups = "GamepadScheme"};
		
		fsm = new StateMachine<States, Driver>(this);
		fsm.ChangeState(States.Init);
		
		graphicsRends = graphicsParent.GetComponentsInChildren<SkinnedMeshRenderer>();
		reflectionRends = reflectionParent.GetComponentsInChildren<SkinnedMeshRenderer>();
		reflectionLight = reflectionParent.GetChild(0).GetComponent<Light>();
		currentCameraOffset = GetTargetCameraOffset();
	}

	void Init_Enter()
	{
		fsm.ChangeState(States.Idle);
	}

	void Idle_Enter()
	{
		
	}

	void Idle_Update()
	{
		
	}
	
	private float phasing_t = 0f;
	private bool finishedPhasing = false;
	private Vector3 phaseFromPos, phaseToPos;
	private Quaternion phaseFromRot, phaseToRot;
	private Vector3 pushoutRight;
	
	void Phasing_Enter()
	{
		phasing = true;
		phasing_t = 0f;
		finishedPhasing = false;
		
		phaseFromPos = CC.transform.position;
		phaseFromRot = CC.transform.rotation;
		phaseToPos = reflectionParent.position;
		phaseToRot = reflectionParent.rotation;
		pushoutRight = CC.transform.TransformDirection(Vector3.right) * (Vector3.Distance(phaseFromPos,phaseToPos) * 0.125f);
		CC.enabled = false;
		
		CLOG("Phase Entered");
	}

	void Phasing_Update()
	{
		phasing_t = Mathf.Clamp01(phasing_t + (Time.deltaTime * 2.15f));
		float pushout = Mathf.Sin(Curves.EaseInOutQuint(phasing_t) * 3.1415f);
		
		CC.transform.position = Vector3.Lerp(phaseFromPos, phaseToPos + (pushoutRight * pushout), Curves.EaseInOutQuint(phasing_t));
		CC.transform.rotation = Quaternion.Lerp(phaseFromRot, phaseToRot, Curves.EaseInOutQuint(phasing_t));
		
		if(phasing_t >= 1f && !finishedPhasing)
		{
			fsm.ChangeState(States.Idle);
			finishedPhasing = true;
		}
	}
	
	void Phasing_Exit()
	{
		CC.enabled = true;
		phasing = false;
	}
	
	private string currentMovementAnimState = "";
	void SetMovementAnimation(string newState, float transitionTime)
	{
		if (currentMovementAnimState == newState) return;
		characterAnim.CrossFadeInFixedTime(newState, 0.2f);
		currentMovementAnimState = newState;
	}
	
	public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
   		return Quaternion.Euler(angles) * (point - pivot) + pivot;
 	}
	
	int GetInputAngleRegionID(float angle)
	{
		int ret = 0;
		float a = Mathf.Abs(angle);
		if(angle > 0)
		{
			if(a > 0 && a <= 22.5) ret = 0;
			if(a > 22.5 && a <= 67.5) ret = 1;
			if(a > 67.5 && a <= 112.5) ret = 2;
			if(a > 112.5 && a <= 157.5) ret = 3;
			if(a > 157.5 && a <= 180.0) ret = 4;
		}
		else
		{
			if(a > 0 && a <= 22.5) ret = 0;
			if(a > 22.5 && a <= 67.5) ret = 7;
			if(a > 67.5 && a <= 112.5) ret = 6;
			if(a > 112.5 && a <= 157.5) ret = 5;
			if(a > 157.5 && a <= 180.0) ret = 4;
		}
		
		return ret;
	}
	
	float MovementSpeedDirMod(int regionID)
	{
		Dictionary<int, float> speedMod = new Dictionary<int, float>(){
			{0,1f},
			{1,1f},
			{2,0.9f},
			{3,0.75f},
			{4,0.75f},
			{5,0.75f},
			{6,0.9f},
			{7,1f},
		};
		return speedMod[regionID];
	}
	
	Vector3 GetTargetCameraOffset()
	{
		Vector3 offset = new Vector3(0.42f, 0.4579999f, -1.932f);
		if(isFocused) offset = new Vector3(0.5f, 0.4579999f, -1.81f);
		return offset;
	}
	
	void ListenForControlsSwitch()
	{
		if(Keyboard.current != null)
		{
			if(Keyboard.current.anyKey.wasPressedThisFrame) isGamepad = false;
		}
		if(Gamepad.current != null)
		{
			if(Gamepad.current.leftStick.ReadValue().magnitude > 0.1f || Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
			{
				isGamepad = true;
			}
		}
	}
}
