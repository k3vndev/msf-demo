using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PadDefaultCollidersSetter : MonoBehaviour
{
	[ContextMenu(nameof(SetColliders))]
	void SetColliders()
	{
		List<BoxCollider> colliders = GetComponents<BoxCollider>().ToList();

		while (colliders.Count != 2)
		{
			if (colliders.Count < 2)
			{
				BoxCollider collider = gameObject.AddComponent<BoxCollider>();
				colliders.Add(collider);
			}
			else colliders.RemoveAt(colliders.Count - 1);
		}

		Vector3 colliderCenter = new(0, -0.45f, 0);
		Vector3 colliderSize = new(0.75f, 0.075f, 0.75f);

		colliders[0].center = colliderCenter;
		colliders[0].size = colliderSize;
		colliders[0].isTrigger = false;

		Vector3 triggerCenter = Vector3.zero;
		Vector3 triggerSize = Vector3.one * 0.5f;

		colliders[1].center = triggerCenter;
		colliders[1].size = triggerSize;
		colliders[1].isTrigger = true;
	}
}
