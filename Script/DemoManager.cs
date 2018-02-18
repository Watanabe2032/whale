using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;
using ParticlePlayground;

public class DemoManager : MonoBehaviour {
	
	[SerializeField] GameObject logoParticle;
	[SerializeField] Image logoImage;
	[SerializeField] float sceneTransitionTime = 16f;
	[HeaderAttribute ("Logo Image Setting")]
	[SerializeField] float logoFadeInStartTime = 2f;
	[SerializeField] float logoFadeInTime = 1f;
	[SerializeField] float logoFadeOutStartTime = 6f;
	[SerializeField] float logoFadeOutTime = 1f;
	[HeaderAttribute ("Logo Particle Setting")]
	[SerializeField] float particleStartTime = 5f;
	
	PlaygroundParticlesC ppc;
	float elapsedTime = 0f;
	Color imageColor;
	
	void Awake(){
		imageColor = new Color(0f,0f,0f,0f);
		ppc = logoParticle.GetComponent<PlaygroundParticlesC>();
		Debug.Log("ppc.emit : "+ppc.emit);
		ppc.emit = false;
		
		var updateStream = Observable
			.EveryUpdate()
			.TakeWhile(_ => elapsedTime < sceneTransitionTime)
			.Finally(() => SceneManager.LoadScene("CubeField"))
			.Subscribe(_ => {elapsedTime += Time.deltaTime;});
		
		// Logo Image Fade In Stream
		Observable
			.Timer(TimeSpan.FromMilliseconds(0), TimeSpan.FromSeconds(logoFadeInStartTime))
			.Skip(1)
			.First()
			.Subscribe(_ => {
				Observable
					.EveryUpdate()
					.TakeWhile(__ => imageColor.a != 1f)
					.Subscribe(__ => {
						float rate = Mathf.Clamp((elapsedTime-logoFadeInStartTime)/logoFadeInTime, 0f, 1f);
						imageColor.a = rate;
						logoImage.color = imageColor;
					});
			});
		
		// Logo Image Fade Out Stream
		Observable
			.Timer(TimeSpan.FromMilliseconds(0), TimeSpan.FromSeconds(logoFadeOutStartTime))
			.Skip(1)
			.First()
			.Subscribe(_ => {
				Observable
					.EveryUpdate()
					.TakeWhile(__ => imageColor.a != 0f)
					.Subscribe(__ => {
						float rate = 1f - Mathf.Clamp((elapsedTime-logoFadeOutStartTime)/logoFadeOutTime, 0f, 1f);
						imageColor.a = rate;
						logoImage.color = imageColor;
					});
			});
	
		// Particle Stream
		Observable
			.Timer(TimeSpan.FromMilliseconds(0), TimeSpan.FromSeconds(particleStartTime))
			.Skip(1)
			.First()
			.Subscribe(_ => {
				ppc.emit = true;
			});
	}

	void Start(){
		imageColor = logoImage.color;
	}
	
}
