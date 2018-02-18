using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

[RequireComponent(typeof (Text))]

public class FPSCounter : MonoBehaviour{
	
	[SerializeField] double bufferTime = 0.5;
	int flameCount = 0;
	Text fpsText;
	
	void Awake(){
		fpsText = GetComponent<Text>();
		
		this.UpdateAsObservable()
			.Subscribe(_ => {flameCount++;});
		
		var checkTimer = Observable
			.Timer(TimeSpan.FromMilliseconds(0), TimeSpan.FromSeconds(bufferTime));
		checkTimer
			.Subscribe(_ => {
				int fps = (int)(flameCount/bufferTime);
				fpsText.text = fps.ToString()+" FPS";
				flameCount = 0;
			});
	}
}
