using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager instance;

        //Default item list, we dont want to alter these that's why we clone them on use
        public List<ItemBase> itemData;

        public ItemBase GetItemData(int itemID)
        {
            for (int i = 0; i < itemData.Count; i++)
            {
                if (itemData[i].data.itemID == itemID)
                {
                    return itemData[i];
                }
            }

            return null;
        }
        private void Awake()
        {
            instance = this;
        }
    }
}