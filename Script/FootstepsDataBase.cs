using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[System.Serializable]
public struct Tex {
	public string name;
	public AudioClip[] clip;
	
	public AudioClip GetRandomAudioClip(){
		return clip[Random.Range(1, clip.Length)];
	}
}

public class FootstepsDataBase : MonoBehaviour {

	[SerializeField] Tex[] tex;
	
	public AudioClip GetAudioClip(string texName){
		int texIndex = GetTexIndex(texName);
		return tex[texIndex].GetRandomAudioClip();
	}
	
	int GetTexIndex(string texName){
		for(int i=0;i<tex.Length;i++){
			if(tex[i].name == texName) return i;
		}
		return 0;
	}
}
