using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WarpWindManager : MonoBehaviour {
	
	[SerializeField] ParticleSystem[] ps;
	
	void StopLoop(){
		foreach(ParticleSystem _ps in ps) _ps.loop = false;
	}
	void LoopSetEnable(bool select){
		foreach(ParticleSystem _ps in ps) _ps.loop = select;
	}
	public void StopLoop(float delayTime){
		Invoke("StopLoop", delayTime);
	}
	public void ParticlePlay(){
		foreach(ParticleSystem _ps in ps) _ps.Play();
		LoopSetEnable(true);
	}
	void ParticleStop(){
		foreach(ParticleSystem _ps in ps) _ps.Stop();
	}
}
