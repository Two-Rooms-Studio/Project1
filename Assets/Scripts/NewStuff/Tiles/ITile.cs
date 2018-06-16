using UnityEngine;

namespace Tiles
{
	public interface ITile
	{
		//unity specific properties
		GameObject UnityObject { get; set; }
		Sprite UnitySprite { get; }
		Color UnityColor { get; }

		//game properties
		uint X { get; }
		uint Y { get; }
		bool IsMoveable { get; }
		//List<IEnemy> SpawnableMonsters { get; }
	}
}
