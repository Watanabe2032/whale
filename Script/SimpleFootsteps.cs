using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/*
	Terrain not supported.
	Multiple Material not supported.
*/

public class SimpleFootsteps : MonoBehaviour {

	[SerializeField] FootstepsDataBase fDB;
	[SerializeField] AudioSource audioSource;
	[SerializeField] Transform[] RaycastBase;
	
	ReactiveProperty<bool>[] isGrounded;

	Collider lastHitCollider = null;
	
	[SerializeField] Vector3 raycastDirection = new Vector3(0f,0f,0f);
	Quaternion adjustEuler = Quaternion.Euler(0f, 0f, 90f);
	[SerializeField] float raycastRange = 0.1f;
	
	void Awake(){
		isGrounded = new ReactiveProperty<bool>[2];
		isGrounded[0] = new ReactiveProperty<bool>(true);
		isGrounded[1] = new ReactiveProperty<bool>(true);
		isGrounded[0]
			.DistinctUntilChanged()
			.Where(_ => _)
			.Subscribe(_ => {
				PlayFootsteps();
			});
		isGrounded[1]
			.DistinctUntilChanged()
			.Where(_ => _)
			.Subscribe(_ => {
				PlayFootsteps();
			});
		
	}
	
	void FixedUpdate(){
		if(isGrounded[1].Value) isGrounded[0].Value = CheckGround(0);
		if(isGrounded[0].Value) isGrounded[1].Value = CheckGround(1);
	}
	
	bool CheckGround(int footIndex){
		RaycastHit hit;
		Vector3 rayDir = -RaycastBase[footIndex].right;
		Ray ray = new Ray(RaycastBase[footIndex].position, rayDir);
		if(Physics.Raycast(ray, out hit, raycastRange)){
			lastHitCollider = hit.collider;
			return true;
		}
		return false;
	}
	
	string GetTextureName(Collider collider){
		if(collider == null) return "";
		// string textureName = "";
		// if(collider.GetType() == typeof(TerrainCollider)) return "";
		
		Renderer r = collider.GetComponent<Renderer>();
		MeshCollider mc = collider as MeshCollider;
		if (r == null || r.sharedMaterial == null || r.sharedMaterial.mainTexture == null){
			return "";
		}
		else if(!mc || mc.convex) {
			return r.material.mainTexture.name;
		}
		return "";
	}
	
	void PlayFootsteps(){
		string textureName = "";
		if(lastHitCollider != null){
			textureName = GetTextureName(lastHitCollider);
		}
		audioSource.PlayOneShot(fDB.GetAudioClip(textureName));
	}
	
}
