using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]

public class UnityChanController : MonoBehaviour, ICharactorController {
	
	[SerializeField] Transform cameraPivot;
	[SerializeField] float dampingRate = 20f;
	[SerializeField] float jogSpeed = 5f;
	[SerializeField] float rotationSpeed = 270f;
	[SerializeField] float turningOnSpotRotationSpeed = 360f;
	[SerializeField] float groundCheckDepth = 0.3f;
	[SerializeField] float groundCheckRadius = 0.14f;
	[SerializeField] float gravityScale = 120f;
	
	Animator animator;
	AnimatorStateInfo stateInfo;
	Rigidbody thisRigidbody;

	Quaternion targetRotation;
	Vector3 gravityDir = Vector3.zero;
	Vector3 movementVelocity = Vector3.zero;
	Vector3 otherVelocity = Vector3.zero;
	Vector2 directionalInput;
	float moveSpeed;
	bool turningOnSpot;
	bool isMoving;
	bool isGrounded;
	bool isJumping;
	int movementMode = 0; // 0:Third Person Mode, 1:First Person Mode
	
	[Header("First Person Settings")]
	[SerializeField] float fpRodateSpeed = 10f;
	[SerializeField] float fpLateralSpeedScale = 0.7f;
	[SerializeField] float fpBackSpeedScale = 0.6f;
	
	[Header("Jump Settings")]
	[SerializeField] float jumpingForce = 360f;
	[SerializeField] float jumpDelayTime = 0.4f;
	[SerializeField] float jumpingRayDepth = 2f;
	[SerializeField] float jumpAnimationStopTime = 1.2f;
	[SerializeField] float addJumpForceTime = 0.3f;
	float defaultAnimatorSpeed;
	int jumpHash;
	public ReactiveProperty<float> jumpingTime;
	
	void Awake(){
		var getInputJump = this.UpdateAsObservable();
		getInputJump
			.Where(_ =>	Input.GetKeyDown(KeyCode.Space))
			.Subscribe(_ => Jump());
		
		jumpingTime = new ReactiveProperty<float>(20f);
		jumpingTime
			.Where(_ =>	isJumping)
			.Subscribe(_ => {
				if(jumpingTime.Value < jumpDelayTime){}
				else if(jumpingTime.Value < jumpDelayTime + addJumpForceTime){
					otherVelocity += transform.up * jumpingForce * Time.deltaTime;
				}
				else if(Physics.Raycast(transform.position, -transform.up, jumpingRayDepth)){
					animator.speed = defaultAnimatorSpeed;
					if(stateInfo.fullPathHash != jumpHash){
						isJumping = false;
					}
				}
				else if(jumpingTime.Value > jumpAnimationStopTime){
					animator.speed = 0;
				}
			});
		
		var jumpingTimeUpdate = this.FixedUpdateAsObservable();
		jumpingTimeUpdate
			.Where(_ =>	jumpingTime.Value < 20f)
			.Subscribe(_ => {
				jumpingTime.Value += Time.fixedDeltaTime;
			});
		
		var clickStream = this.UpdateAsObservable()
			.ObserveEveryValueChanged(_ => Input.GetMouseButton(0));
		clickStream
			.Subscribe(_ => {
				if(_){
					movementMode = 1;
				}
				else{
					movementMode = 0;
				}
			});
	}

	void Start() {
		gravityDir = -transform.up;
		animator = GetComponent<Animator>();
		jumpHash = Animator.StringToHash("Base Layer.Jump");
		thisRigidbody = GetComponent<Rigidbody>();
	}

	void FixedUpdate() {
		SynchronizeGravity(6f*Time.deltaTime);
		stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		CheckGround();
		GetInput();
		UpdateAnimator();
		if(movementMode == 0) UpdateThirdPersonMode();
		else if(movementMode == 1) UpdateFirstPersonMode();
		UpdateRigidbody();
	}
	
	void UpdateThirdPersonMode(){
		RotateCharacter();
		movementVelocity = transform.forward * moveSpeed * jogSpeed;
	}
	
	void UpdateFirstPersonMode(){
		float angleY = Input.GetAxis("Mouse X") * fpRodateSpeed;
		Vector3 toForward = (transform.forward + transform.right * angleY).normalized;
		transform.rotation = Quaternion.LookRotation(toForward, transform.up);
		
		float speed = moveSpeed * jogSpeed;
		float forwardVel = (directionalInput.y == 0f ? 0f : (directionalInput.y > 0f ? 1f : -fpBackSpeedScale));
		float rightVel = (directionalInput.x == 0f ? 0f : (directionalInput.x > 0f ? fpLateralSpeedScale : -fpLateralSpeedScale));
		forwardVel *= speed;
		rightVel *= speed;
		movementVelocity = transform.forward * forwardVel + transform.right * rightVel;
	}
	
	void GetInput(){
		directionalInput.x = Input.GetAxisRaw("Horizontal");
		directionalInput.y = Input.GetAxisRaw("Vertical");
		moveSpeed = Mathf.Clamp01(directionalInput.magnitude);
		moveSpeed += (moveSpeed > 0f ? (Input.GetKey(KeyCode.LeftShift) ? 1f : 0f) : 0f);
	}
	
	void UpdateAnimator() {
		isMoving = Input.GetButton("Horizontal") || Input.GetButton("Vertical") || !isGrounded;
		animator.SetFloat("move_speed", moveSpeed, 0.3f, Time.fixedDeltaTime);
		animator.SetBool("move", isMoving);
	}

	void UpdateRigidbody(){
		thisRigidbody.velocity = movementVelocity;
		if(!isGrounded){
			otherVelocity += gravityScale * gravityDir * Time.fixedDeltaTime;
		}
		if(otherVelocity != Vector3.zero){
			thisRigidbody.velocity += otherVelocity;
			otherVelocity -= otherVelocity * dampingRate * Time.fixedDeltaTime;
			if(otherVelocity.sqrMagnitude < 0.01f) otherVelocity = Vector3.zero;
		}
	}

	void RotateCharacter() {
		Vector3 movementDirection = cameraPivot.right * directionalInput.x + cameraPivot.forward * directionalInput.y;
		bool inIdle = stateInfo.IsName("idle");
		float deltaAngle = 0f;
		float targetRotationSpeed = rotationSpeed;
		if(turningOnSpot) targetRotationSpeed = turningOnSpotRotationSpeed;
		if(inIdle) {
			Vector3 targetDirection = new Vector3(movementDirection.x, 0f, movementDirection.z);
			deltaAngle = Vector3.Angle(targetDirection, transform.forward);
			float angleSign = Mathf.Sign(Vector3.Cross(transform.forward, targetDirection).y);
			deltaAngle *= angleSign;
		}
		turningOnSpot = Mathf.Abs(deltaAngle) > 30f && inIdle;
		if(movementDirection != Vector3.zero) {
			targetRotation = Quaternion.LookRotation(movementDirection, transform.up);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * targetRotationSpeed);
		}
	}
	
	void SynchronizeGravity(float rate){
		if(transform.up != -gravityDir){
			Vector3 newForward = Vector3.ProjectOnPlane(transform.forward, -gravityDir).normalized; 
			Vector3 toUp = Vector3.Lerp(transform.up,-gravityDir,0.5f);
			Vector3 toForward = Vector3.Lerp(transform.forward,newForward,0.5f);
			transform.rotation = Quaternion.LookRotation(toForward,toUp);
		}
	}
	
	void CheckGround(){
		RaycastHit hit;
		Ray ray = new Ray(transform.position+transform.up*0.2f, -transform.up);
		if(Physics.SphereCast(ray, groundCheckRadius, out hit, groundCheckDepth)) {
			isGrounded = true;
			if(hit.transform.gameObject.tag == "ForcedGravityHold"){
				gravityDir = -hit.normal;
			}
			else if(hit.transform.gameObject.tag == "GravityHold"){
				if(gravityDir != -hit.normal){
					float diffAngle = Vector3.Angle(-transform.forward, hit.normal);
					if(diffAngle < 90f) gravityDir = -hit.normal;
					if(!CheckRightAngle()) gravityDir = -hit.normal;
				}
			}
		}
		else {
			isGrounded = false;
		}
	}
	
	bool CheckRightAngle(){
		RaycastHit hit;
		Vector3 startPos = transform.position+transform.forward*3f+transform.up*0.01f;
		Vector3 toDir = (-transform.forward-transform.up*0.1f).normalized;
		Ray ray = new Ray(startPos, toDir);
		if(Physics.Raycast(ray, out hit, 4f)){
			if(hit.transform.gameObject.tag == "GravityHold"){
				float diffAngle = Vector3.Angle(gravityDir, -hit.normal);
				if(diffAngle < 89f) return false;
			}
		}
		return true;
	}
	
	public void Warp(){
		CheckGround();
	}
	
	void Jump(){
		if(!isGrounded || isJumping) return;
		isJumping = true;
		animator.SetTrigger("Jump");
		defaultAnimatorSpeed = animator.speed;
		jumpingTime.Value = 0f;

		Observable
			.Timer(TimeSpan.FromMilliseconds(jumpDelayTime*1000f))
			.Subscribe(_ => {
				otherVelocity += transform.forward * moveSpeed * jogSpeed * 2f;
			});
	}

}