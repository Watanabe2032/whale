using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;

public class WhaleController : MonoBehaviour {
	
	[SerializeField] Transform lightT;
	[SerializeField] Renderer whaleRenderer;
	[SerializeField] GameObject manualObj;
	[SerializeField] CourseManager courseM;
	[SerializeField] FunctionDisplayManager fdm;
	
	WhaleModelManager		wmm;
	WhaleScreenManager		wsm;
	WhaleParticleManager	wpm;
	WhaleMovementManager	movementM;
	
	
	public ReactiveProperty<bool> onModel; // Model Draw
	public ReactiveProperty<bool> onShadow; // Shadow Draw
	public ReactiveProperty<bool> onParticle; // Particle Draw
	public ReactiveProperty<bool> isWhaleWatching;
	
	void Awake(){
		wmm = GetComponent<WhaleModelManager>();
		wsm = GetComponent<WhaleScreenManager>();
		wpm = GetComponent<WhaleParticleManager>();
		movementM = GetComponent<WhaleMovementManager>();
		
	}
	
	void Start(){
		SetupStream();
		SetModel(1);
		SetGrid(0);
	}
	
	
	void SetLightDirection(){
		lightT.LookAt(transform);
	}
	
	void SetModelDraw(){
		onModel.Value = !onModel.Value;
	}
	
	void HideModel(){
		onModel.Value = false;
	}
	
	void SetModel(int index){
		onShadow.Value = (index == 0)? false : true;
		wmm.SetShadowMode(index);
	}
	void SetGrid(int index){
		if(!onShadow.Value){
			onShadow.Value = true;
			SetModel(1);
		}
		wsm.SetGridMode(index);
	}
	void SetParticle(int index){
		onParticle.Value = (index == 0)? false : true;
		wpm.SetParticleMode(index);
	}
	
	void SetupStream(){
		
		onModel = new ReactiveProperty<bool>(false);
		onShadow = new ReactiveProperty<bool>(false);
		onParticle = new ReactiveProperty<bool>(false);
		
		onModel
			.DistinctUntilChanged()
			.Subscribe(_ => {
				if(onModel.Value){
					SetModel(0);
					SetParticle(0);
					whaleRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					movementM.onSetScale = false;
				}
				else{
					movementM.onSetScale = true;
				}
			});
			
		onShadow
			.DistinctUntilChanged()
			.Subscribe(_ => {
				if(onShadow.Value){
					HideModel();
					SetParticle(0);
					whaleRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
				}
				else{
				}
			});
		
		onParticle
			.DistinctUntilChanged()
			.Subscribe(_ => {
				if(onParticle.Value){
					HideModel();
					SetModel(0);
					whaleRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;				
				}
				else{
				}
			});
		
		isWhaleWatching = new ReactiveProperty<bool>(false);
		isWhaleWatching
			.DistinctUntilChanged()
			.Subscribe(_ => {
				if(isWhaleWatching.Value){
					courseM.WatchingZoneEnter();
					fdm.HideText();
				}
				else{
					courseM.WatchingZoneExit();
					fdm.HideText();
				}
			});
		
		var setParticleColorStream = this.UpdateAsObservable();
		setParticleColorStream
			.Where(_ => onParticle.Value)
			.Subscribe(_ => wpm.SetHSVColor(wsm.shadowHSV.h));
		
		var setModelColorStream = this.UpdateAsObservable();
		setModelColorStream
			.Where(_ => onModel.Value)
			.Subscribe(_ => wmm.SetModelColor(wsm.gridHSV.GetHSV()));
		
		
		var lightSetStream = this.UpdateAsObservable();
		lightSetStream
			.Subscribe(_ => SetLightDirection());
		
		var inputKeyStream = this.UpdateAsObservable();
		inputKeyStream
			.Subscribe(_ => {
		bool onDrawManual = false;
		if(Input.GetKey(KeyCode.Alpha1))	  SetModel(1);
		else if(Input.GetKey(KeyCode.Alpha2)) SetModel(2);
		else if(Input.GetKey(KeyCode.Alpha3)) SetModel(3);
		else if(Input.GetKey(KeyCode.Alpha4)) SetModel(4);
		else if(Input.GetKey(KeyCode.Alpha5)) SetModel(5);
		else if(Input.GetKey(KeyCode.Alpha6)) SetModel(6);
		else if(Input.GetKey(KeyCode.Alpha7)) SetModel(7);
		else if(Input.GetKey(KeyCode.Alpha8)) SetModel(8);
		else if(Input.GetKey(KeyCode.Alpha9)) SetModel(9);
		else if(Input.GetKey(KeyCode.Alpha0)) SetModel(10);
		
		else if(Input.GetKeyDown(KeyCode.Q)) SetModelDraw();
		else if(Input.GetKeyDown(KeyCode.E)) SetGrid(0);
		else if(Input.GetKeyDown(KeyCode.R)) SetGrid(1);
		else if(Input.GetKeyDown(KeyCode.T)) SetGrid(2);
		else if(Input.GetKeyDown(KeyCode.Y)) SetGrid(3);
		else if(Input.GetKeyDown(KeyCode.U)) SetGrid(4);
		else if(Input.GetKeyDown(KeyCode.I)) SetGrid(5);
		else if(Input.GetKeyDown(KeyCode.O)) SetGrid(6);
		else if(Input.GetKeyDown(KeyCode.P)) SetGrid(7);
		
		else if(Input.GetKeyDown(KeyCode.F)) SetParticle(1);
		else if(Input.GetKeyDown(KeyCode.G)) SetParticle(2);
		else if(Input.GetKeyDown(KeyCode.H)) SetParticle(3);
		else if(Input.GetKeyDown(KeyCode.J)) SetParticle(4);
		else if(Input.GetKeyDown(KeyCode.K)) wmm.SwicthBatchRenderer();
		else if(Input.GetKeyDown(KeyCode.L)) {}
		
		
		else if(Input.GetKey(KeyCode.Z))	 onDrawManual = true;
		else if(Input.GetKeyDown(KeyCode.X)) movementM.ChangeSpeedScale();
		else if(Input.GetKeyDown(KeyCode.C)) wmm.SwitchCulling();
		else if(Input.GetKeyDown(KeyCode.V)) courseM.ChangeScanMode();
		else if(Input.GetKeyDown(KeyCode.B)) wsm.SetGridAllScreen();
		else if(Input.GetKey(KeyCode.N))	 wsm.ChangeScreenColor();
		else if(Input.GetKey(KeyCode.M))	 wsm.ChangeScreenSVC();
		else if(Input.GetKey(KeyCode.Comma)) wsm.ChangeGridColor();
		else if(Input.GetKey(KeyCode.Period))wsm.ChangeGridSVC();
		else if(Input.GetKey(KeyCode.Slash)) {}
		else if(Input.GetKey(KeyCode.Backslash)) {}
		
		manualObj.SetActive(onDrawManual);
			});
	}
	
	public void WatchingZoneEnter(){
		isWhaleWatching.Value = true;
	}
	
	public void WatchingZoneExit(){
		isWhaleWatching.Value = false;
	}
	
}
