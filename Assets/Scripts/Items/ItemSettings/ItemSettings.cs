using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.ProjectWindowCallback;
using UnityEditorInternal;
public class ItemSettings : ScriptableObject {
	[Header("Item Settings")]
	[Tooltip("Is this item a piece of armor?")]
	public bool armor = false;
	[Tooltip("Is this item a weapon? (shields should be considered armor)")]
	public bool weapon = false;
	[Tooltip("Is this item a consumable (potion or food)?")]
	public bool consumable = false;
	[Tooltip("Sprite to use for this item if it appears on the board")]
	public Sprite itemSprite = null;
	[Tooltip("The color this item should appear as on the game board")]
	public Color itemColor;

	[Header("GUI Settings")]
	[Tooltip("The sprite to use if this item appears in the inventory GUI")]
	public Sprite guiSprite = null;
	[Tooltip("The color this item should appear as in the inventory GUI")]
	public Color guiColor;
	[Tooltip("The text/name to use if this item appears in the inventory GUI")]
	public string guiText = null;

	[Header("Armor Settings")]
	[Tooltip("The AC value of this piece of armor")]
	public int armorClass;

	[Header("Weapon Settings")]
	[Tooltip("Minimum damage this weapon can do")]
	public int minimumDamage = 0;
	[Tooltip("Maximum damage this weapon can do")]
	public int maximumDamage = 0;
	[Tooltip("The additional elemental damage this weapon can do")]
	public int elementalDamage = 0;

	[Header("Consumable Settings")]
	[Tooltip("Does this item damage the player's health instead of adding to it?")]
	public bool damagesHealth;
	[Tooltip("The min health that is added or taken away from the player by this item")]
	public int minHealthChange = 0;
	[Tooltip("The max health that is added or taken away from the player by this item")]
	public int maxHalthChange = 0;
	[Tooltip("A string describing the effect this consumable applies")]
	public string statusEffect = null;

	#if UNITY_EDITOR
	[UnityEditor.MenuItem("Tools/Create/ItemSettings/BlankItemSettings")]
	private static void CreateAsset()
	{
		string path = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject);
		string assetPath = path + "/BlankItemSettings.asset";
		ItemSettings item = ScriptableObject.CreateInstance<ItemSettings>();
		UnityEditor.ProjectWindowUtil.CreateAsset(item, assetPath);
	}
	#endif
}