using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ParticlePlayground;

public class WarpZone : MonoBehaviour {
	[SerializeField] GameObject cameraObj;
	[SerializeField] GameObject parentObj;
	[SerializeField] GameObject particleObj = null;
	[SerializeField] GameObject viralSpiral = null;
	[SerializeField] float afterglowTime = 1f;
	[SerializeField] float warpDelayTime = 3f;
	[SerializeField] bool isOldEffect = true;
	PlaygroundParticlesC ppc;
	WarpWindManager particleM;
	ICamera iCamera;
	ICharactorController iCC = null;
	float warpTime;
	
	void Start(){
		if(isOldEffect) particleM = particleObj.GetComponent<WarpWindManager>();
		iCamera = cameraObj.GetComponent(typeof(ICamera)) as ICamera;
		if(viralSpiral) ppc = viralSpiral.GetComponent<PlaygroundParticlesC>();
	}
	void WarpToParent(GameObject obj){
		obj.transform.position = parentObj.transform.position + parentObj.transform.up * 0.001f;
		obj.transform.rotation = parentObj.transform.rotation;
		obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
		iCC = obj.GetComponent(typeof(ICharactorController)) as ICharactorController;
		if(iCC!=null) iCC.Warp();
		iCamera.Warp();
	}
	
	void OnTriggerEnter(Collider collider){
		warpTime = Time.time + warpDelayTime;
		iCamera.WarpIn();
		if(isOldEffect) particleM.ParticlePlay();
		else{
			viralSpiral.transform.position = transform.position;
			viralSpiral.transform.rotation = transform.rotation;
			viralSpiral.transform.Rotate(new Vector3(-90f,0f,0f));
			ppc.emit = true;
		}
	}
	void OnTriggerStay(Collider collider){
		if(warpTime < Time.time) WarpToParent(collider.gameObject);
	}
	void OnTriggerExit(Collider collider){
		iCamera.WarpOut();
		if(isOldEffect) particleM.StopLoop(afterglowTime);
		else ppc.emit = false;
	}
}
