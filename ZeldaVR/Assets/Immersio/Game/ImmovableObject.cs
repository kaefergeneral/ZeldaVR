using UnityEngine;

public class ImmovableObject : MonoBehaviour {

	RigidbodyConstraints _origConstraints;
	bool _origColliderIsTrigger;
	float _origMass;

	bool _isImmovable = false;
	public bool IsImmovable {
		get { return _isImmovable; }
	}

	public void MakeImmovable () {
		if(_isImmovable)
			return;
		if(!rigidbody)
			return;

		Collider collider = GetComponent<Collider>();
		if(collider) {
			_origColliderIsTrigger = collider.isTrigger;
			collider.isTrigger = false;
		}

		_origConstraints = rigidbody.constraints;
		rigidbody.constraints = RigidbodyConstraints.FreezeAll;

		//_origMass = rigidbody.mass;
		//rigidbody.mass = 999999.0f;

		_isImmovable = true;
	}
	public void RevertToOriginalState () {
		if(!_isImmovable)
			return;
		if(!rigidbody)
			return;

		Collider collider = GetComponent<Collider>();
		if(collider) {
			collider.isTrigger = _origColliderIsTrigger;
		}

		rigidbody.constraints = _origConstraints;
		//rigidbody.mass = _origMass;
		

		_isImmovable = false;
	}
}
