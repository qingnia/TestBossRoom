using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BagInventory", menuName = "Inventory/New BagInventory")]
public class BagInventory : ScriptableObject
{
    public List<BagItem> itemList = new List<BagItem>();
}
