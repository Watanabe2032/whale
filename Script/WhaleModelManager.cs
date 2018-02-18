using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class WhaleModelManager : MonoBehaviour {
	
	[SerializeField] Renderer whaleRenderer;
	[SerializeField] Material forwardMat;
	[SerializeField] Transform pCameraT;
	[SerializeField] Transform wCameraT;
	[SerializeField] Shader forwardShader;
	[SerializeField] Shader deferredShader;
	[SerializeField] FunctionDisplayManager fdm;
	[SerializeField] GameObject brObj;
	[SerializeField] bool batchRendererEnabled = true;
	// [SerializeField] Material batchRendererMat;
	public ReactiveProperty<bool> isDeferredShader;
	WhaleController wc;
	Material defaultMat;
	//Shader brShader;
	
	float alphaCutoff = Mathf.PI; // rad
	float colorCutoff = Mathf.PI; // rad
	float samplingInterval = 0f;
	float modelAlpha = 0.5f;
	
	int cameraRangeID;
	int shadowModeID;
	int alphaCutoffID;
	int colorCutoffID;
	int modelColorID;
	int samplingIntervalID;
	int sphereIntervalID;
	int cullID;
	
	int shadowMode;
	int forwardModeCount = 7;
	bool onChangeSampling = false;
	bool onCulling = false;
	
	string[] modeName = {
		"",
		"カットオフ",
		"カットオフ（UV.ｘ）",
		"カットオフ（UV.ｙ）",
		"カットオフ（ローカル.ｘ）",
		"カットオフ（ワールド.ｘ）",
		"アウトライン",
		"カットオフ（サイン）",
		"レイマーチ（球）",
		"レイマーチ（箱）",
		""
	};
	
	
	void Awake(){
		wc = GetComponent<WhaleController>();
		defaultMat = whaleRenderer.material;
		//brShader = 
		
		cameraRangeID = Shader.PropertyToID("_CameraRange");
		shadowModeID = Shader.PropertyToID("_ShadowMode");
		alphaCutoffID = Shader.PropertyToID("_AlphaCutoff");
		colorCutoffID = Shader.PropertyToID("_ColorCutoff");
		samplingIntervalID = Shader.PropertyToID("_SI");
		modelColorID = Shader.PropertyToID("_ModelColor");
		sphereIntervalID = Shader.PropertyToID("_SphereInterval");
		cullID = Shader.PropertyToID("_Cull");
		
		shadowMode = -1;
		SetStream();
		
	}
	
	void Start(){
		forwardMat.SetFloat(colorCutoffID, 0f);
		forwardMat.SetFloat(alphaCutoffID, 0f);
		if(batchRendererEnabled) SwicthBatchRenderer();
	}
	
	public void SetShadowMode(int index){
		ChangeColorCutoff();
		if(shadowMode == index || index < 0) return;
		shadowMode = index;
		
		if(index > forwardModeCount){
			isDeferredShader.Value = true;
			defaultMat.SetInt(shadowModeID, index-forwardModeCount);
		}
		else{
			isDeferredShader.Value = false;
			defaultMat.SetInt(shadowModeID, index);
			// if(brObj.activeSelf){
				// batchRendererMat.SetInt(shadowModeID, index-forwardModeCount);
			// }
		}
		
		if(onCulling) defaultMat.SetInt(cullID, 0);
		else defaultMat.SetInt(cullID, 2);
		defaultMat.SetFloat(colorCutoffID, GetAdjustmentCurve(colorCutoff));
		fdm.SetText(modeName[shadowMode]);
	}
	
	public void SwicthBatchRenderer(){
		if(!batchRendererEnabled) return;
		if(brObj.activeSelf){
			brObj.SetActive(false);
			fdm.SetText("Batch Renderer オフ");
		}
		else{
			brObj.SetActive(true);
			fdm.SetText("Batch Renderer オン");
		}
	}
	
	public void SwitchCulling(){
		onCulling = !onCulling;
		if(onCulling){
			defaultMat.SetInt(cullID, 0);
			fdm.SetText("カリング　オフ");
		}
		else{
			defaultMat.SetInt(cullID, 2);
			fdm.SetText("カリング　オン");
		}
	}
	
	public void SetModelColor(Vector3 hsv){
		Color color = Color.HSVToRGB(hsv.x,hsv.y,hsv.z);
		color.a = modelAlpha;
		defaultMat.SetColor(modelColorID, color);
	}
	
	public void ChangeColorCutoff(){
		colorCutoff += 0.1f * Time.deltaTime * 2f * Mathf.PI;
		if(colorCutoff > 2f * Mathf.PI) colorCutoff -= 2f * Mathf.PI;
		defaultMat.SetFloat(colorCutoffID, GetAdjustmentCurve(colorCutoff));
	}
	
	public void ChangeAlphaCutoff(){
		alphaCutoff += 0.1f * Time.deltaTime * 2f * Mathf.PI;
		if(alphaCutoff > 2f * Mathf.PI) alphaCutoff -= 2f * Mathf.PI;
		defaultMat.SetFloat(alphaCutoffID, GetAdjustmentCurve(alphaCutoff));
	}
	
	float GetAdjustmentCurve(float rad){
		float curve = 1f;
		if(rad < 0.5f * Mathf.PI)
			curve -= Mathf.Sin(rad) * 0.5f;
		else if(rad < Mathf.PI)
			curve += Mathf.Sin(rad) * 0.5f - 1f;
		else if(rad < 1.5f * Mathf.PI)
			curve = -Mathf.Sin(rad) * 0.5f;
		else curve += Mathf.Sin(rad) * 0.5f;
		return curve;
	}
	
	void SetStream(){
		
		isDeferredShader = new ReactiveProperty<bool>(false);
		isDeferredShader
			.DistinctUntilChanged()
			.Subscribe(_ => {
				if(isDeferredShader.Value){
					defaultMat.shader = deferredShader;
				}
				else{
					defaultMat.shader = forwardShader;
				}
			});
		
		var changeSamplingStream = this.UpdateAsObservable();
		changeSamplingStream
			.Subscribe(_ => {
				samplingInterval += 0.01f * Time.deltaTime;
				if(samplingInterval > 1f) samplingInterval -= 1f;
				defaultMat.SetFloat(samplingIntervalID, samplingInterval);
			});
		
	}
	
}
