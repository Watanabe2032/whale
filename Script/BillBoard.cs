using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UniRx;
using UniRx.Triggers;

public class BillBoard : MonoBehaviour {

	[SerializeField] Transform target;
	// Quaternion adjustEuler = Quaternion.Euler (0f, 0f, 0f);

	void Start(){
		var update = this.UpdateAsObservable();
		update
			.Where(_ => target != null)
			.Subscribe(_ => {
				transform.rotation = target.rotation;
				// transform.rotation *= adjustEuler;
			});
	}
}
