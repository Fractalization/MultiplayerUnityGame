using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfo : ScriptableObject {
    public string itemName;
    public int itemType;//0 = any, 1 = primary, 2=secondary, 3 = melee
}