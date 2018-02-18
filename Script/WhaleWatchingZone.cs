using UnityEngine;
using System.Collections;

public class WhaleWatchingZone : MonoBehaviour {

	[SerializeField] Camera trackingCamera;
	[SerializeField] GameObject whaleCameraObj;
	[SerializeField] WhaleController wc;
	
	void Start () {
		whaleCameraObj.SetActive(false);
	}
	void OnTriggerEnter(Collider collider){
		WhaleWatchingEnabled(true);
		wc.WatchingZoneEnter();
	}
	void OnTriggerExit(Collider collider){
		WhaleWatchingEnabled(false);
		wc.WatchingZoneExit();
	}
	void WhaleWatchingEnabled(bool select){
		whaleCameraObj.SetActive(select);
		trackingCamera.enabled = !select;
	}
}
