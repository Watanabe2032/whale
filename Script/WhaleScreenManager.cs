using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class HSVColorModel {
	[NonSerialized] public float h = 0f;
	[NonSerialized] public float s = 1f;
	[NonSerialized] public float v = 1f;
	[NonSerialized] public float svc = 0f; // s-v circle (h')
	[NonSerialized]	public float rotateSpeed = 0.05f; // h round per sec
	
	public HSVColorModel(Color color, float RotateSpeed){
		Color.RGBToHSV(color, out h, out s, out v);
		rotateSpeed = RotateSpeed;
	}
	
	public void UpdateColor(){
		h = (h + rotateSpeed * Time.deltaTime) % 1f;
	}
	public void FixedUpdateColor(){
		h = (h + rotateSpeed * Time.fixedDeltaTime) % 1f;
	}
	public Color GetColor(){
		return Color.HSVToRGB(h,s,v);
	}
	public Color GetUpdateColor(){
		UpdateColor();
		return GetColor();
	}
	public Color GetFixedUpdateColor(){
		FixedUpdateColor();
		return GetColor();
	}
	public Vector3 GetHSV(){
		return new Vector3(h, s, v);
	}
	
	public void RotateSVC(float add){
		svc = (svc + add) % 1f;
		Vector4 hsv = Color.HSVToRGB(svc,1f,1f);
		s = hsv.x;
		v = (hsv.y + (1f - hsv.z)) / 2f;
	}
}

public class WhaleScreenManager : MonoBehaviour {
	
	[SerializeField] FunctionDisplayManager fdm;
	[SerializeField] Material screenMat;
	
	[NonSerialized] public HSVColorModel gridHSV;
	[NonSerialized] public HSVColorModel shadowHSV;
	[NonSerialized] public HSVColorModel screenHSV;
	
	float threshold = 0.5f;
	float gridSize;
	int shadowColorID;
	int screenColorID;
	int gridColorID;
	int gridSizeID;
	int gridRateID;
	int gridModeID;
	int tileModeID;
	int thresholdID;
	int gridAllScreenID;
	
	int gridMode;
	string[] gridName = {
		"",
		"ドット",
		"チェック",
		"トルシェタイル",
		"クォーターサークル",
		"ランダム",
		"ノイズ",
		"歯車"
	};
	
	
	void Awake(){
		shadowColorID = Shader.PropertyToID("_ShadowColor");
		screenColorID = Shader.PropertyToID("_ScreenColor");
		gridColorID = Shader.PropertyToID("_GridColor");
		gridSizeID = Shader.PropertyToID("_GridSize");
		gridModeID = Shader.PropertyToID("_GridMode");
		tileModeID = Shader.PropertyToID("_TileMode");
		gridAllScreenID = Shader.PropertyToID("_GridAllScreen");
		
		shadowHSV = new HSVColorModel(Color.blue, 0.02f);
		screenHSV = new HSVColorModel(Color.black, 0.1f);
		gridHSV = new HSVColorModel(Color.black, 0.1f);
		screenMat.SetColor(screenColorID, screenHSV.GetColor());
		screenMat.SetColor(gridColorID, gridHSV.GetColor());
		screenMat.SetInt(gridAllScreenID, 0);
		
		SetupStream();
		gridMode = -1;
		SetGridMode(0);
	}
	
	public void SetGridMode(int index){
		if(index < 0) return;
		if(gridMode == 3 && index == 3){
			gridMode = index;
			ChangeTileMode();
		}
		else if(index == gridMode) gridMode = 0;
		else gridMode = index;
		screenMat.SetInt(gridModeID, gridMode);
		if(index == 2) SetGridSizeStream();
		fdm.SetText(gridName[gridMode]);
	}
	
	public void SetGridAllScreen(){
		if(screenMat.GetInt(gridAllScreenID) == 0){
			screenMat.SetInt(gridAllScreenID, 1);
			fdm.SetText("全画面グリッド　オン");
		}
		else{
			screenMat.SetInt(gridAllScreenID, 0);
			fdm.SetText("全画面グリッド　オフ");
		}
	}
	
	public void ChangeGridColor(){
		screenMat.SetColor(gridColorID, gridHSV.GetUpdateColor());
		fdm.SetText("グリッドカラー（色）");
	}
	public void ChangeGridSVC(){
		gridHSV.RotateSVC(0.1f * Time.deltaTime);
		screenMat.SetColor(gridColorID, gridHSV.GetColor());
		fdm.SetText("グリッドカラー（明暗）");
	}
	public void ChangeScreenColor(){
		screenMat.SetColor(screenColorID, screenHSV.GetUpdateColor());
		fdm.SetText("スクリーンカラー（色）");
	}
	public void ChangeScreenSVC(){
		screenHSV.RotateSVC(0.1f * Time.deltaTime);
		screenMat.SetColor(screenColorID, screenHSV.GetColor());
		fdm.SetText("スクリーンカラー（明暗）");
	}
	
	void ChangeTileMode(){
		int current = screenMat.GetInt(tileModeID) + 1;
		if(current > 2) current = 0;
		screenMat.SetInt(tileModeID, current);
	}
	
	void SetGridSizeStream(){
		gridSize = 0f;
		this.UpdateAsObservable()
			.TakeWhile(_ => gridMode == 2)
			.Subscribe(_ => {
				gridSize += Time.deltaTime;
				if(gridSize > 10000f) gridSize = 0f;
				screenMat.SetFloat(gridSizeID, gridSize);
			});
	}
	
	void SetupStream(){
		var changeShadowColorStream = this.UpdateAsObservable();
		changeShadowColorStream
			.Subscribe(_ => {
				screenMat.SetColor(shadowColorID, shadowHSV.GetUpdateColor());
			});
	}
	
}
