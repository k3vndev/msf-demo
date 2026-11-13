using UnityEngine;
using System;

public class CollisionListener : MonoBehaviour
{
	public Action<Collision> _OnCollisionEnter;
	public Action<Collision> _OnCollisionExit;
	public Action<Collider> _OnTriggerEnter;
	public Action<Collider> _OnTriggerExit;

	private void OnCollisionEnter(Collision collision) => _OnCollisionEnter?.Invoke(collision);
	private void OnCollisionExit(Collision collision) => _OnCollisionExit?.Invoke(collision);
	private void OnTriggerEnter(Collider other) => _OnTriggerEnter?.Invoke(other);
	private void OnTriggerExit(Collider other) => _OnTriggerExit?.Invoke(other);
}
