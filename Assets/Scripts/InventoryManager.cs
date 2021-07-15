using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager instance;
        public List<ItemBase> itemData;

        private void Awake()
        {
            instance = this;
        }
    }
}