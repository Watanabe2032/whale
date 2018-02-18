using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UniRx;

[RequireComponent(typeof(Button))]

public class ReturnButton : MonoBehaviour {
	[SerializeField] GameObject targetObj;
	[SerializeField] GameObject cameraObj;
	[SerializeField] GameObject homeObj;
	ICamera iCamera;
	ICharactorController iCC = null;
	Button button;
	
	void Start(){
		iCamera = cameraObj.GetComponent(typeof(ICamera)) as ICamera;
		button = GetComponent<Button>();
		button.onClick
			.AsObservable()
			.Subscribe(_ => {WarpToHome();});
	}
	
	public void WarpToHome(){
		targetObj.transform.position = homeObj.transform.position + homeObj.transform.up * 0.001f;
		targetObj.transform.rotation = homeObj.transform.rotation;
		targetObj.GetComponent<Rigidbody>().velocity = Vector3.zero;
		iCC = targetObj.GetComponent(typeof(ICharactorController)) as ICharactorController;
		if(iCC!=null) iCC.Warp();
		iCamera.Warp();
	}
}