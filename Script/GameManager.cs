using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class GameManager : MonoBehaviour {
	[SerializeField] GameObject RunwayRight;
	[SerializeField] GameObject RunwayLeft;
	[SerializeField] Image CurtainImage;
	[SerializeField] float cartainSpeed = 0.1f;
	
	void Awake(){
		RunwayRight.SetActive(true);
		RunwayLeft.SetActive(true);
		Observable
			.Timer(TimeSpan.FromSeconds(12))
			.Subscribe(_ => {
				Destroy(RunwayRight);
				Destroy(RunwayLeft);
			});
		
		CurtainImage.enabled = true;
		var cartainStream = Observable
			.EveryUpdate()
			.TakeWhile(_ => CurtainImage.color.a != 0f)
			.Finally(() => Destroy(CurtainImage.gameObject))
			.Subscribe(_ => {
				Color temp = CurtainImage.color;
				temp.a -= Time.deltaTime * (0.001f+cartainSpeed/Mathf.Pow(temp.a,2));
				CurtainImage.color = temp;
			});
	}
}
