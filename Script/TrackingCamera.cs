using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrackingCamera : MonoBehaviour {
	
	[Header("Tracking Camera Settings")]
	public Transform cameraBase;
	public Transform target;
	[SerializeField] float maxRange = 5f;
	[SerializeField] float minRange = 1f;
	[SerializeField] float halfVisibility = 30f;
	[SerializeField] float forwardPosRange = 0.5f;
	[SerializeField] float upperPosRange = 0.5f;
	[SerializeField] float moveSpeed = 3f;
	[SerializeField] float rotateSpeed = 3f;
	
	public Rigidbody rBody;
	bool isTooNear = false;
	bool isTooFar = false;
	bool isCollider = false;
	
	float dTime;

	
	public void FollowTarget(float _dTime){
		dTime = _dTime;
		SynchronizeUppperAxis(2f*rotateSpeed*dTime);
		CheckRange();
		if(!isTooNear) CheckVisibility();
		if(!isTooNear) SetDepression(2f*rotateSpeed*dTime);
		CheckRay();
	}
	
	public void SetUsualPosition(){
		float range = (maxRange-minRange)/2f;
		cameraBase.position = target.position - target.forward * range;
		cameraBase.rotation = Quaternion.LookRotation(target.forward,target.up);
		transform.localPosition = new Vector3(0f,2f,0f);
		transform.localEulerAngles = new Vector3(20f,0f,0f);
	}
	
	void CheckRange(){
		float range = (target.position - cameraBase.position).sqrMagnitude;
		if(range < minRange*minRange){
			TurnAround();
			isTooNear = true;
		}
		else if(range > maxRange*maxRange){
			CloseWithTarget();
			isTooFar = true;
		}
		else{
			isTooNear = false;
			isTooFar = false;
		}
	}
	
	void CheckVisibility(){
		Vector3 diffPos = target.position + target.forward * forwardPosRange - cameraBase.position;
		float dotZ = Mathf.Abs(Vector3.Dot(cameraBase.forward, diffPos.normalized));
		float dotX = Vector3.Dot(cameraBase.right, diffPos.normalized);
		float angleY = Mathf.Atan2(dotZ, dotX)*Mathf.Rad2Deg-90f;
		if(Mathf.Abs(angleY) > halfVisibility || isTooFar){
			float angle = Mathf.Lerp(0f, -angleY, 0.5f*rotateSpeed*dTime);
			cameraBase.RotateAround(cameraBase.position, cameraBase.up, angle);
		}
	}
	
	void SynchronizeUppperAxis(float rate){
		if(Vector3.Dot(cameraBase.up, target.up) > 0.999f) return;
		Vector3 toUp = Vector3.Lerp(cameraBase.up,target.up,rate);
		cameraBase.rotation = Quaternion.LookRotation(cameraBase.forward,toUp);

		// if negative pitching
		float forwardAngle = Vector3.Angle(target.up, cameraBase.forward);
		if(forwardAngle > 89.999f) return;
		float upRange = maxRange * Mathf.Cos(forwardAngle*2f-Mathf.PI*0.5f);
		if(upRange <= 0f) return;
		
		float rotateAngle = Mathf.Lerp(0f, (90f - forwardAngle), rotateSpeed*dTime);
		cameraBase.RotateAround(target.position, cameraBase.right, rotateAngle);
		cameraBase.position += cameraBase.up * upRange * rotateAngle * dTime;	
	}

	void SynchronizeHeight(float rate){
		if(isCollider) return;
		Vector3 diffPos = target.position - cameraBase.position;
		float dot = Vector3.Dot(diffPos.normalized, target.up);
		if(Mathf.Abs(dot) < 0.0001f) return;
		float range = Mathf.Lerp(0f,diffPos.magnitude*dot,rate);
		cameraBase.position += cameraBase.up * range;
	}
	
	void SetDepression(float rate){
		Vector3 diffPos = target.position + target.up * upperPosRange - transform.position;
		float dotY = Vector3.Dot(diffPos,cameraBase.up);
		float dotZ = Vector3.Dot(diffPos,cameraBase.forward);
		float angleX = -Mathf.Atan2(dotY,dotZ)*Mathf.Rad2Deg;
		if(Mathf.Abs(angleX) < 0.001f) return;
		transform.localEulerAngles = new Vector3(angleX,0f,0f);
	}
	
	void CloseWithTarget(){
		Vector3 diffPos = target.position - cameraBase.position;
		Vector3 toPos = target.position - diffPos.normalized * maxRange * 0.99f;
		cameraBase.position = Vector3.Lerp(cameraBase.position,toPos,moveSpeed*dTime);
		if(diffPos.sqrMagnitude < 0.01f) cameraBase.position = toPos;
	}
	
	void TurnAround(){
		Vector3 t2c = (cameraBase.position - target.position).normalized;
		float dot = Vector3.Dot(target.forward, t2c);
		float sign = (dot > 0f)? 1f : -1f;
		cameraBase.RotateAround(target.position, sign * cameraBase.up, 180f);
		cameraBase.position -= cameraBase.forward * moveSpeed*dTime;
	}
	
	void CheckRay(){
		RaycastHit hit;
		Vector3 startPos = target.position + target.up * 0.8f;
		if(Physics.Linecast(startPos, transform.position, out hit)){
			if(hit.transform.tag != "MainCamera"){
				cameraBase.position = Vector3.Lerp(cameraBase.position,target.position,moveSpeed*dTime);
			}
		}
	}
	
	void OnTriggerEnter(Collider collider){
		isCollider = true;
	}
	void OnTriggerStay(Collider collider){
		cameraBase.position = Vector3.Lerp(cameraBase.position,target.position,moveSpeed*dTime);
	}
	void OnTriggerExit(Collider collider){
		isCollider = false;
	}
	
}
