using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;

public class SoundManager : MonoBehaviour {
	
	[SerializeField] GameObject speakerObj;
	[SerializeField] GameObject positiveObj;
	[SerializeField] GameObject negativeObj;
	[SerializeField] float maxVolume = 0f;
	[SerializeField] float minVolume = -80f;
	[SerializeField] float defaultVolume = -25f;
	[SerializeField] float throughTime = 200f;
	
	[SerializeField] AudioMixer mixer;
	
	float iconLiveTime = 3f;
	float countDownTime;
	
	float currentVolume;
	float volMinDiff = 2f;
	
	void Awake(){
		HideIcon();
		countDownTime = -10f;

		var volumeDown = this.UpdateAsObservable();
		volumeDown
			.Where(_ =>
				Input.GetKey(KeyCode.KeypadMinus) ||
				Input.GetKey(KeyCode.Minus)
			)
			.ThrottleFirst(TimeSpan.FromMilliseconds(throughTime))
			.Subscribe(_ => VolumeDown());
		
		var volumeUp = this.UpdateAsObservable();
		volumeUp
			.Where(_ =>
				Input.GetKey(KeyCode.KeypadPlus) ||
				Input.GetKey(KeyCode.Plus) ||
				Input.GetKey(KeyCode.Semicolon)
			)
			.ThrottleFirst(TimeSpan.FromMilliseconds(throughTime))
			.Subscribe(_ => VolumeUp());
		
		var countDounTimer = Observable
			.Timer(TimeSpan.FromMilliseconds(0), TimeSpan.FromSeconds(1))
			.Where(_ => countDownTime > -10f);
		countDounTimer
			.Subscribe(_ => {
				if(countDownTime < 0f){ countDownTime = -10f; HideIcon(); }
				else { countDownTime -= 1f; }
			});
	}
	
	void Start(){
		mixer.SetFloat("VolumeMaster", minVolume);
		mixer.GetFloat("VolumeMaster", out currentVolume);
		Observable
			.EveryUpdate()
			.TakeWhile(_ => currentVolume != defaultVolume)
			.Subscribe(_ => {
				float upSpeed = 15f;
				currentVolume += upSpeed * Time.deltaTime;
				currentVolume = Mathf.Clamp(currentVolume, minVolume, defaultVolume);
				mixer.SetFloat("VolumeMaster", currentVolume);
			});
	}
	
	void VolumeUp(){
		currentVolume = Mathf.Clamp(currentVolume+volMinDiff,minVolume,maxVolume);
		mixer.SetFloat("VolumeMaster", currentVolume);
		speakerObj.SetActive(true);
		positiveObj.SetActive(true);
		negativeObj.SetActive(false);
		countDownTime = iconLiveTime;
	}
	
	void VolumeDown(){;
		currentVolume = Mathf.Clamp(currentVolume-volMinDiff,minVolume,maxVolume);
		mixer.SetFloat("VolumeMaster", currentVolume);
		speakerObj.SetActive(true);
		positiveObj.SetActive(false);
		negativeObj.SetActive(true);
		countDownTime = iconLiveTime;
	}
	
	void HideIcon(){
		speakerObj.SetActive(false);
		positiveObj.SetActive(false);
		negativeObj.SetActive(false);
	}
}
