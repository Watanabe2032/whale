using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ParticlePlayground;

public class WhaleParticleManager : MonoBehaviour {
	
	[SerializeField] GameObject[] particleObj;
	
	PlaygroundParticlesC[] ppc;
	Gradient commonGradient;
	GradientColorKey[] colorKey;
	GradientAlphaKey[] alphaKey;
	[SerializeField, Range(0,1)] float colorDifference = 0.7f;
	
	int particleMode = -1;
	/*
		particleMode 0 : Non Particle Mode
		particleMode 1 : Playground Rainbow Road
		particleMode 2 : Playground Enchanted Orb
		particleMode 3 : Playground Laser (Script)
		particleMode 4 : Burning Robot
	*/
	
	void Awake(){
		ppc = new PlaygroundParticlesC[particleObj.Length];
		for(int i=0;i<particleObj.Length;i++){
			ppc[i] = particleObj[i].GetComponent<PlaygroundParticlesC>();
		}
	}
	
	void Start(){
		SetParticleMode(0);
		InitializeGradient();
		for(int i=1;i<ppc.Length;i++){
			ppc[i].lifetimeColor = commonGradient;
		}
	}

	void InitializeGradient(){
		commonGradient = new Gradient();
		colorKey = new GradientColorKey[2];
        alphaKey = new GradientAlphaKey[2];
		for(int i=0;i<colorKey.Length;i++){
			colorKey[i].time = (float)i;
			colorKey[i].color = Vector4.one;
			alphaKey[i].time = (float)i;
			alphaKey[i].alpha = 1f - (float)i;
		}
		commonGradient.SetKeys(colorKey, alphaKey);
	}
	
	public void SetParticleMode(int mode){
		if(particleMode == mode || mode < 0) return;
		int index = mode - 1;
		for(int i=0;i<particleObj.Length;i++){
			if(i == index) particleObj[i].SetActive(true);
			else particleObj[i].SetActive(false);
		}
		particleMode = mode;
	}
	
	public void SetColor(Color _color){
		if(particleMode == 0) return;
		colorKey[0].color = _color;
		commonGradient.SetKeys(colorKey, alphaKey);
	}
	
	public void SetHSVColor(float _h){
		if(particleMode == 0) return;
		colorKey[0].color = Color.HSVToRGB(_h,1f,1f);
		float secontH = (_h + colorDifference) % 1f;
		colorKey[1].color = Color.HSVToRGB(secontH,1f,1f);
		commonGradient.SetKeys(colorKey, alphaKey);
	}
}
