using UnityEngine;

public static class LayerMasks
{
	public class Layer
	{
		public readonly LayerMask Mask;
		public readonly int Index;
		public readonly string Name;

		public Layer(string name)
		{
			Mask = LayerMask.GetMask(name);
			Index = LayerMask.NameToLayer(name);
			Name = name;
		}
	}
	public static readonly Layer Player = new("Player");
	public static readonly Layer Sled = new("Sled");
	public static readonly Layer Snow = new("Snow");
	public static readonly Layer Ice = new("Ice");
	public static readonly Layer Pressables = new("Pressables");
	public static readonly Layer Obstacle = new("Obstacle");
	public static readonly Layer Wall = new("Wall");
}
