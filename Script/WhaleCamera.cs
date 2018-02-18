using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;

[RequireComponent(typeof(Camera))]

public class WhaleCamera : MonoBehaviour {
	
	[SerializeField] float maxRange = 45f;
	[SerializeField] float zoomSpeed = 10f;
	
	//public ReactiveProperty<float> zoomRate{set; get;}
	
	void Awake(){
		//zoomRate = new ReactiveProperty<float>();
		//zoomRate.Value = (maxRange + transform.localPosition.z) / (maxRange * 2f);
		
		var zPos = this.UpdateAsObservable();
		zPos
			.Select(_ => Input.GetAxis("Mouse ScrollWheel"))
			.Where(_ => _ != 0f)
			.Subscribe(_ => {
				float z = Mathf.Clamp(transform.localPosition.z+_*zoomSpeed,-maxRange,maxRange);
				transform.localPosition = new Vector3(0f,0f,z);
				//zoomRate.Value = Mathf.Clamp((maxRange + z) / (maxRange * 2f), 0f, 1f);
			});
	}
}
