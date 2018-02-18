using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class CourseManager : MonoBehaviour {
	[SerializeField] Renderer cubeRenderer;
	[SerializeField] Renderer turnRenderer;
	[SerializeField] FunctionDisplayManager fdm;
	[SerializeField] Camera rtCamera;
	[SerializeField] Texture turnTexture;
	[SerializeField] Texture renderTexture;
	[SerializeField] Shader[] shaders; // 0:non scan, 1:opaque, 2:transparent
	
	public ReactiveProperty<bool> isWhaleWatching;
	
	Material cubeMat;
	Material turnMat;
	int textureID;
	int shaderCount;
	int scanMode = 0;
	string[] scanModeName = {
		"スキャン　オフ",
		"スキャン　オン（影なし）",
		"スキャン　オン（影あり）",
		"スキャン　オフ（Render Texture）"
	};
	
	void Awake(){
		rtCamera.enabled = false;
		cubeMat = cubeRenderer.material;
		turnMat = turnRenderer.material;
		textureID = Shader.PropertyToID("_MainTex");
		shaderCount = scanModeName.Length;
		SetStream();
	}
	
	public void WatchingZoneEnter(){
		isWhaleWatching.Value = true;
	}
	
	public void WatchingZoneExit(){
		isWhaleWatching.Value = false;
	}
	
	public void ChangeScanMode(){
		if(!isWhaleWatching.Value) return;
		scanMode += 1;
		if(scanMode >= shaderCount) scanMode = 0;
		if(scanMode == 0) SetRendererEnabled(false);
		else SetRendererEnabled(true);
		SetShader(scanMode);
		fdm.SetText(scanModeName[scanMode]);
	}
	
	public void SetScanMode(int modeNum){
		if(!isWhaleWatching.Value && modeNum != 0) return;
		if(scanMode == modeNum) scanMode = 0;
		else scanMode = modeNum;
		SetShader(scanMode);
		if(scanMode == 0){
			SetRendererEnabled(false);
			fdm.SetText("スキャン　オフ");
		}
		else{
			SetRendererEnabled(true);
			fdm.SetText("スキャン　オン");
		}
	}
	
	void SetShader(int index){
		if(index == shaderCount - 1){
			RenderTextrueEnabled(true);
			return;
		}
		else{
			RenderTextrueEnabled(false);
		}
		cubeMat.shader = shaders[index];
		turnMat.shader = shaders[index];
	}
	
	void SetRendererEnabled(bool select){
		cubeRenderer.enabled = select;
		turnRenderer.enabled = select;
	}
	
	void RenderTextrueEnabled(bool select){
		cubeMat.shader = shaders[0];
		turnMat.shader = shaders[0];
		rtCamera.enabled = select;
		if(select){
			turnMat.SetTexture(textureID, renderTexture);
			cubeRenderer.enabled = false;
		}
		else{
			turnMat.SetTexture(textureID, turnTexture);
		}
	}
	
	void SetStream(){
		
		isWhaleWatching = new ReactiveProperty<bool>(false);
		isWhaleWatching
			.DistinctUntilChanged()
			.Subscribe(_ => {
				if(isWhaleWatching.Value){
					SetScanMode(scanMode);
				}
				else{
					SetScanMode(0);
					SetRendererEnabled(true);
				}
			});
		
	}
	
	
}
