using UnityEngine;

namespace Tiles
{
	public class Floor : ITile
	{
		public GameObject UnityObject { get; private set; }
		public Sprite UnitySprite { get; private set; }
		public Color UnityColor { get; private set; }
		
		public uint X { get; private set; }
		public uint Y { get; private set; }
		public bool IsMoveable { get; private set; }

		public Floor(uint x, uint y)
		{
			X = x;
			Y = y;
			IsMoveable = true;
		}

        public Floor(uint x, uint y, Sprite sprite)
        {
            UnitySprite = sprite;
            X = x;
            Y = y;
            IsMoveable = true;
        }
	}
}