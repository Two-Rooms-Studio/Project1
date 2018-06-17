using UnityEngine;

namespace Tiles
{
	public class Wall : ITile
	{
		public GameObject UnityObject { get; private set; }
		public Sprite UnitySprite { get; private set; }
		public Color UnityColor { get; private set; }
		
		public uint X { get; private set; }
		public uint Y { get; private set; }
		public bool IsMoveable { get; private set; }

		public Wall(uint x, uint y)
		{
			X = x;
			Y = y;
			IsMoveable = false;
		}
	}
}
