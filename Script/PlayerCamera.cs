using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using UnityStandardAssets.ImageEffects;

public class PlayerCamera : TrackingCamera, ICamera {
	
	[SerializeField] float minFOV = 20f;
	[SerializeField] float warpInSpeed = 0.2f;
	[SerializeField] float warpOutSpeed = 4f;
	bool isWarpIN = false;
	VignetteAndChromaticAberration vaca; 
	Camera thisCamera;
	float defaultFOV; // Camera.fieldOfView
	float angleX = 0f;
	float rateX = 0f;
	bool isWarpIn = false;
	int cameraMode = 0;
	/*
		cameraMode 0 : Tracking Camera
		cameraMode 1 : First Person Camera
	*/
	
	[Header("First Person Settings")]
	[SerializeField] float fpRodateSpeed = 0.05f;
	[SerializeField] float fpCameraUpperRange = 2.2f;
	
	void Awake(){
		vaca = null;
		vaca = GetComponent<VignetteAndChromaticAberration>();
	}
	
	void Start (){
		thisCamera = transform.GetComponent<Camera>();
		defaultFOV = thisCamera.fieldOfView;
		SetUsualPosition();
		
		var clickStream = this.UpdateAsObservable()
			.ObserveEveryValueChanged(_ => Input.GetMouseButton(0));
		clickStream
			.Subscribe(_ => {
				if(_){
					cameraMode = 1;
				}
				else{
					cameraMode = 0;
					SetUsualPosition();
				}
			});
	}
	
	void LateUpdate(){
		if(cameraMode == 0) FollowTarget(Time.deltaTime);
		else if(cameraMode == 1) FPSModeUpdate();
	}
	
	void FPSModeUpdate(){
		transform.position = target.position;
		transform.position += target.up * fpCameraUpperRange - target.forward * 0.05f;
		transform.rotation = target.rotation;
		rateX += Input.GetAxis("Mouse Y") * fpRodateSpeed;
		rateX = Mathf.Clamp(rateX, -0.95f, 0.95f);
		Vector3 toForward = (target.forward*(1f-Mathf.Abs(rateX)) + target.up*rateX).normalized;
		transform.rotation = Quaternion.LookRotation(toForward, target.up);
	}
	
	void MouseViewControl(){
		cameraBase.position = target.position - target.forward * 0.05f;
		Vector3 mousePos = Input.mousePosition;
		float halfWidth = (float)(Screen.width/2);
		float halfHeight = (float)(Screen.height/2);
		float rateX = (mousePos.x - halfWidth) / halfWidth;
		float rateY = (mousePos.y - halfHeight) / halfHeight;
		Vector3 eulerAngle = new Vector3(-rateY*90f,rateX*160f,0f);
		eulerAngle = target.rotation * eulerAngle;
		transform.eulerAngles = eulerAngle + target.eulerAngles;
	}
	
	public void Warp(){
		SetUsualPosition();
	}
	
	public void WarpIn(){
		isWarpIN = true;
		var warpInEffect = this.UpdateAsObservable();
		warpInEffect
			.TakeWhile(_ => isWarpIN && thisCamera.fieldOfView != minFOV)
			.Subscribe(_ => WarpInEffect());
	}
	
	public void WarpOut(){
		isWarpIN = false;
		var warpOutEffect = this.UpdateAsObservable();
		warpOutEffect
			.TakeWhile(_ => !isWarpIN && thisCamera.fieldOfView != defaultFOV)
			.Subscribe(_ => WarpOutEffect());
	}
	
	void WarpInEffect(){
		float currentFOV = thisCamera.fieldOfView;
		float newFOV = Mathf.Lerp(currentFOV,minFOV,warpInSpeed*Time.deltaTime);
		thisCamera.fieldOfView = newFOV;
		Vector3 diffPos = target.position - cameraBase.position;
		Vector3 toForward = Vector3.Lerp(cameraBase.forward,diffPos.normalized,0.2f*Time.deltaTime);
		cameraBase.rotation = Quaternion.LookRotation(toForward, target.up);
		if(vaca){
			float caValue = Mathf.Clamp(vaca.chromaticAberration+warpInSpeed*Time.deltaTime*10f,0f,50f);
			vaca.chromaticAberration = caValue;
		}
	}
	
	void WarpOutEffect(){
		float currentFOV = thisCamera.fieldOfView;
		float newFOV = Mathf.Lerp(currentFOV,defaultFOV,warpOutSpeed*Time.deltaTime);
		if(defaultFOV - newFOV < 0.1f) newFOV = defaultFOV;
		thisCamera.fieldOfView = newFOV;
		if(vaca){
			float caValue = Mathf.Clamp(vaca.chromaticAberration-warpOutSpeed*Time.deltaTime*10f,0f,50f);
			vaca.chromaticAberration = caValue;
		}
	}
	
}