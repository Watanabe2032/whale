using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;

public class WhaleMovementManager : MonoBehaviour {
	
	[SerializeField] FunctionDisplayManager fdm;
	[SerializeField] float minRange = 5f;
	[SerializeField] float maxRange = 35f;
	[SerializeField] float wallRange = 50f;
	[SerializeField, TooltipAttribute("m/sec")]
					 float moveSpeed = 1f;
	[SerializeField] float rotateSpeed = 0.1f;
	[SerializeField] float maxScale = 16f;
	[SerializeField] float minScale = 8f;
	[SerializeField, TooltipAttribute("m/sec")]
					 float prefetchTime = 6f;
					
	[NonSerialized] public float speedScale = 1f;
	[NonSerialized] public bool onSetScale = true;
	
	Vector3 centerPos = new Vector3(0f,0f,0f);
	Vector3 startPos = new Vector3(0f,10f,-30f);
	
	double checkWallInterval = 1f;
	float defaultScale;
	Vector3 newForward;
	//int speedScaleStage = 1;
	
	void Awake(){
		defaultScale = transform.localScale.x;
		
		var checkWallTimer = Observable
			.Timer(TimeSpan.FromMilliseconds(0), TimeSpan.FromSeconds(checkWallInterval));
		checkWallTimer
			.Subscribe(_ => CheckWall());
	}
	
	void Start () {
		transform.position = startPos;
	}

	void FixedUpdate () {
		RotateWhale();
		MoveWhale();
		SetScale();
	}
	
	public void ChangeSpeedScale(){
		if(speedScale < 10f) speedScale += 4f;
		else speedScale = 1f;
		fdm.SetText("× "+speedScale.ToString());
	}
	
	void SetScale(){
		if(!onSetScale){
			transform.localScale = Vector3.one * defaultScale;
			return;
		}
		float range = (transform.position-centerPos).sqrMagnitude;
		float rate = 1f - (range-minRange*minRange)/(maxRange*maxRange-minRange*minRange);
		transform.localScale = Vector3.one*Mathf.Lerp(minScale,maxScale,rate);
	}
	
	void MoveWhale(){
		transform.position += transform.forward * speedScale * moveSpeed * Time.fixedDeltaTime;
	}
	
	void RotateWhale(){
		if(transform.forward != newForward){
			float rate = speedScale * rotateSpeed * Time.fixedDeltaTime;
			Vector3 toForward = Vector3.RotateTowards(transform.forward,newForward,rate,0f);
			transform.rotation = Quaternion.LookRotation(toForward);
		}
	}
	
	void SetNewForward(){
		Vector3 sign;
		sign.x = (transform.forward.x > 0f)? 1f:-1f;
		sign.y = (transform.forward.y > 0f)? 1f:-1f;
		sign.z = (transform.forward.z > 0f)? 1f:-1f;
		Vector3 diffOuter = centerPos + maxRange * sign - transform.position;
		Vector3 diffinner = centerPos + minRange * sign - transform.position;
		sign.x = UnityEngine.Random.Range(diffinner.x, diffOuter.x);
		sign.y = UnityEngine.Random.Range(diffinner.y, diffOuter.y);
		sign.z = UnityEngine.Random.Range(diffinner.z, diffOuter.z);
		newForward = sign.normalized;
	}
	
	void CheckWall(){
		if((transform.position - centerPos).sqrMagnitude > (Vector3.one*wallRange).sqrMagnitude){
			transform.position = startPos;
		}
		
		Vector3 futurePos = transform.position + transform.forward * speedScale * moveSpeed * prefetchTime;
		Vector3 diffPos = futurePos - centerPos;
		
		if(diffPos.sqrMagnitude > (Vector3.one*maxRange).sqrMagnitude){
			SetNewForward();
		}
		else if(diffPos.sqrMagnitude > (Vector3.one*minRange).sqrMagnitude){
			SetNewForward();
		}
	}
}

