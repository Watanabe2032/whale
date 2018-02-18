using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

[RequireComponent(typeof(Text))]

public class FunctionDisplayManager : MonoBehaviour {
	
	[SerializeField] float displayTime = 3f;
	Text thisText;
	float remainingTime = 0f;
	
	void Awake(){
		thisText = GetComponent<Text>();
		thisText.enabled = false;
		
		var countDownStream = this.FixedUpdateAsObservable();
		countDownStream
			.Where(_ => remainingTime > 0f)
			.Subscribe(_ => {
				remainingTime -= Time.fixedDeltaTime;
				if(remainingTime <= 0f){
					thisText.enabled = false;
					remainingTime = 0f;
				}
			});
		
	}
	
	public void SetText(string text){
		thisText.text = text;
		thisText.enabled = true;
		remainingTime = displayTime;
	}
	
	public void HideText(){
		thisText.text = "";
		remainingTime = 0f;
	}
}
