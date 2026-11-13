using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class Utils
{
	private static readonly Collider[] colliderBuffer = new Collider[16];

	/// <summary>Checks if something that matches the layer mask is colliding at a specific point.</summary>
	public static bool IsColliding(Vector3 position, int layerMask = -1, float boxSize = 0.25f)
	{
		Collider[] colliders = GetCollidersAt(position, layerMask, boxSize);
		return colliders.Length > 0;
	}

	/// <summary>Gets all the colliders colliding in a specific point that also match the layer mask.</summary>
	public static Collider[] GetCollidersAt(Vector3 position, int layerMask = -1, float boxSize = 0.25f)
	{
		int count = Physics.OverlapBoxNonAlloc(position, Vector3.one * boxSize, colliderBuffer, Quaternion.identity, layerMask);
		Collider[] results = new Collider[count];
		Array.Copy(colliderBuffer, results, count);
		return results;
	}

	/// <summary>Finds and returns a component with a given tag.</summary>
	public static T GetComp<T>(string tag)
		=> GameObject.FindWithTag(tag).GetComponent<T>();

	/// <summary>Checks if the given layer is inside the given LayerMask.</summary>
	public static bool IsInLayerMask(int layer, LayerMask layerMask)
		=> (layerMask.value & (1 << layer)) != 0;

	public static void PrefixAndUnParent(string prefix, params Transform[] transforms)
	{
		foreach (Transform t in transforms)
		{
			t.name = $"{prefix}/{t.name}";
			t.SetParent(null);
		}
	}

	public static float FromInOut(Vector2 inOut, bool inValue)
		=> inValue ? inOut.x : inOut.y;

	public static float DirectionToAngle(Direction direction) => (int)direction * 90f;

	/// <summary>Clones and updates the current material of the GameObject's Renderer.</summary>
	/// <returns>The created material.</returns>
	public static Material CloneUIMaterial(GameObject obj)
	{
		Graphic graphic = obj.GetComponent<Graphic>();
		Material newMat = UnityEngine.Object.Instantiate(graphic.material);

		graphic.material = newMat;
		return newMat;
	}


	public static class Time
	{
		///<summary>Lets you await the specified seconds.</summary>
		public static async Task Wait(float seconds)
		{
			if (seconds < 0f)
				throw new ArgumentOutOfRangeException(nameof(seconds), "Delay time must be non-negative.");

			int milliseconds = (int)(seconds * 1000f);
			await Task.Delay(milliseconds);
		}

		///<summary>Executes <c>onFinish</c> callback after the specified seconds.</summary>
		public static void After(float seconds, Action onFinish)
		{
			_ = AfterInternal(seconds, onFinish);
		}

		// Internal Task that actually does the waiting & callback
		static async Task AfterInternal(float seconds, Action onFinish)
		{
			await Wait(seconds);
			onFinish?.Invoke();
		}
	}


	public static class Vectors
	{
		/// <summary>Rounds all the components of the given vector.</summary>
		public static Vector2 Round(Vector2 vector)
			=> new(Mathf.Round(vector.x), Mathf.Round(vector.y));

		/// <summary>Rounds all the components of the given vector.</summary>
		public static Vector3 Round(Vector3 vector)
			=> new(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));

		/// <summary>Gets the direction from vector a to b.</summary>
		public static Vector2 CalcDirection(Vector3 a, Vector3 b)
		{
			Vector3 difference = a - b;
			return -ToDirectional(IgnoreY(difference));
		}

		/// <summary>Parses a Vector2 into a directional vector.</summary>
		public static Vector2 ToDirectional(Vector2 v)
		{
			bool IsOnHorizontalAxis = Mathf.Abs(v.x) >= Mathf.Abs(v.y);

			return IsOnHorizontalAxis
				? Vector2.right * Mathf.Sign(v.x)
				: Vector2.up * Mathf.Sign(v.y);
		}

		/// <summary>Gets the angle from a Vector2.</summary>
		public static float VecToDeg(Vector2 vector)
			=> Mathf.Round(Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg);


		/// <summary>Generates a Vector2 with the given degrees.</summary>
		public static Vector2 DegToVec(float deg)
		{
			float angle = Mathf.Repeat(deg, 360f) * Mathf.Deg2Rad;
			var (x, y) = (Mathf.Round(Mathf.Cos(angle)), Mathf.Round(Mathf.Sin(angle)));
			return new(y, x);
		}

		/// <summary>Creates a Vector2 with the X and Z axis of a given Vector3</summary>
		public static Vector2 IgnoreY(Vector3 vector)
			=> new(vector.x, vector.z);

		/// <summary>Gets the distance between two vectors ignoring the Y axis.</summary>
		public static float DistanceIgnoreY(Vector3 a, Vector3 b)
			=> Vector2.Distance(IgnoreY(a), IgnoreY(b));

		/// <summary>
		/// Parses a 0-3 range index into a directional vector.
		///	The mapping follows a clockwise order. 
		/// </summary>
		public static Vector2 FromIndexToDirectional(int index)
		{
			if (index < 0 || index >= 4)
				throw new IndexOutOfRangeException($"DirectionalFromIndex expects an index between 0 and 3. Value recieved: {index}");

			Vector2 direction = index % 2 == 0 ? Vector2.up : Vector2.right;
			if (index >= 2) direction *= -1;
			return direction;
		}
	}

#if UNITY_EDITOR
	public static class Dev
	{
		public static GameObject FindOrCreateGameObject(string objName, Action<GameObject> setup = null, bool select = true)
		{
			GameObject obj = GameObject.Find(objName);

			if (obj == null)
			{
				obj = new GameObject(objName);
				setup?.Invoke(obj);
			}

			if (select)
			{
				EditorGUIUtility.PingObject(obj);
				Selection.activeGameObject = obj;
			}

			return obj;
		}
	}
#endif
}
