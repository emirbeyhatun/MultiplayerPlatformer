using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlatformerGame
{
    public class InventoryEventData
    {
        public NetworkManager.ReplicateItem replicateItemUsage;
        public NetworkManager.ReplicateItemWithInt replicateItemAddition;
        public NetworkManager.ReplicateItemWithInt replicateItemSwitch;
    }
    public class Inventory
    {
        ItemBase currentItem;
        const int InventoryLimit = 3;
        int inventoryCount = 0;
        List<ItemBase> itemData;
        public List<ItemButton> itemButtons;
        ItemBase[] items;
        NetworkPlayer player;
        Animator animator;
        InventoryEventData eventdata = null;
        bool isPlayerLocal = false;

        public Inventory(ref List<ItemBase> itemData, ref List<ItemButton> itemButtons, NetworkPlayer player, Animator animator, InventoryEventData eventData)
        {
            //If PLayer is Local
            items = new ItemBase[InventoryLimit];

            isPlayerLocal = true;
            this.itemButtons = itemButtons;
            this.itemData = itemData;
            this.player = player;
            this.animator = animator;
            this.eventdata = eventData;

            for (int i = 0; i < itemButtons.Count; i++)
            {
                itemButtons[i].onClicked = SwitchTo;
            }
        }

        public Inventory(ref List<ItemBase> itemData, NetworkPlayer player, Animator animator)
        {
            //If PLayer is Remote
            items = new ItemBase[InventoryLimit];

            this.itemData = itemData;
            this.player = player;
            this.animator = animator;
        }

        public bool AddItem(int itemID)
        {
            if (inventoryCount >= InventoryLimit) return false;

            ItemBase newItem = GetItemData(itemID);

            int emptyIndex = GetEmptyIndex();
            if (newItem && emptyIndex >= 0)
            {
                ItemBase clone = player.CreateNewInventoryItem(newItem, emptyIndex);

                items[emptyIndex] = clone;
                inventoryCount++;


                if (isPlayerLocal)
                {
                    int emptyButtonIndex = GetEmptyButtonIndex();

                    if (emptyButtonIndex >= 0)
                    {
                        itemButtons[emptyButtonIndex].PrepareButton(clone.data);
                    }
                }
                return true;
            }

            return false;
        }

        private int GetEmptyIndex()
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetEmptyButtonIndex()
        {
            for (int i = 0; i < itemButtons.Count; i++)
            {
                if (itemButtons[i].itemID == -1)
                {
                    return i;
                }
            }

            return -1;
        }

        private ItemBase GetItemData(int itemID)
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

        private ItemBase GetItem(int itemID)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].data.itemID == itemID)
                {
                    return items[i];
                }
            }

            return null;
        }
        public void SwitchTo(int itemID)
        {
            ItemBase selectedItem = GetItem(itemID);
            if (selectedItem && selectedItem != currentItem)
            {
                if (isPlayerLocal && eventdata != null && eventdata.replicateItemUsage != null)
                {
                    eventdata.replicateItemSwitch.Invoke(itemID);
                }

                player.EnableAimingToTarget(false);

                if (currentItem)
                    currentItem.SwitchFromItem();

                currentItem = selectedItem;

                if (currentItem)
                    currentItem.SwitchToItem();


                UpdateButtonColors();
            }
        }

        private void UpdateButtonColors()
        {
            if (isPlayerLocal == false) return;

            if (currentItem)
            {
                for (int i = 0; i < itemButtons.Count; i++)
                {
                    if (itemButtons[i].itemID == currentItem.data.itemID)
                    {
                        itemButtons[i].SetColor(true);
                    }
                    else
                    {
                        itemButtons[i].SetColor(false);
                    }
                }
            }
        }

        public void UseItem(float lag)
        {
            if (currentItem)
            {
                currentItem.UseItem(lag);

                if (isPlayerLocal && eventdata != null && eventdata.replicateItemUsage != null)
                {
                    eventdata.replicateItemUsage.Invoke();
                }
            }
        }

        void RemoveItem(ItemBase item)
        {


        }

        public void TriggerAnimationEvent()
        {
            if (currentItem)
            {
                currentItem.AnimationEvent();
            }
        }
    }
}

      
    