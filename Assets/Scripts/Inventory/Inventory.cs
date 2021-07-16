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
        private ItemBase[] items;
        private ItemBase currentItem;
        private ItemBase currentItemWithAnimation;
        private List<ItemBase> itemData;
        private int inventoryCount = 0;
        private const int InventoryLimit = 3;

        private InventoryEventData eventdata = null;
        private Animator animator;
        private NetworkPlayer player;
        private bool isPlayerLocal = false;

        public List<ItemButton> itemButtons;

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
                        itemButtons[emptyButtonIndex].PrepareButton(clone.data, emptyButtonIndex);
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
                if (itemButtons[i].inventoryIndex == -1)
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

        private ItemBase GetItem(int inventoryIndex)
        {
            if(items[inventoryIndex] != null)
            {
                return items[inventoryIndex];
            }

            return null;
        }
        public void SwitchTo(int inventoryID)
        {
            if (currentItemWithAnimation != null || inventoryID < 0 || inventoryID >= items.Length) return;

            ItemBase selectedItem = GetItem(inventoryID);
            if (selectedItem && selectedItem != currentItem)
            {
                if (isPlayerLocal && eventdata != null && eventdata.replicateItemUsage != null)
                {
                    eventdata.replicateItemSwitch.Invoke(inventoryID);
                }

                if (currentItem)
                    currentItem.SwitchFromItem();

                currentItem = selectedItem;

                if (currentItem)
                    currentItem.SwitchToItem();


                UpdateButtonColors();

                animator.ResetTrigger("IdleUpper");
            }
        }

        private void UpdateButtonColors()
        {
            if (isPlayerLocal == false) return;

            for (int i = 0; i < itemButtons.Count; i++)
            {
                if (currentItem && itemButtons[i].inventoryIndex == currentItem.GetInventoryIndex())
                {
                    itemButtons[i].SetColor(true);
                }
                else
                {
                    itemButtons[i].SetColor(false);
                }
            }
        }

        public void UpdateButtonUsages()
        {
            if (isPlayerLocal == false) return;

            for (int j = 0; j < items.Length; j++)
            {
                if (items[j])
                {
                    for (int i = 0; i < itemButtons.Count; i++)
                    {
                        if (itemButtons[i].inventoryIndex == items[j].GetInventoryIndex())
                        {
                            itemButtons[i].SetUsageAmount(items[j].data.totalUsage);
                            break;
                        }
                        
                    }
                }
            }
            
        }

        private void ResetButtonByIndex(int Index)
        {
            if (isPlayerLocal == false) return;


            for (int i = 0; i < itemButtons.Count; i++)
            {
                if (itemButtons[i].inventoryIndex == Index)
                {
                    itemButtons[i].ResetButton();
                    break;
                }
            }
        }
        public void UseItem(float lag)
        {
            if (currentItem)
            {
                if(currentItem.data.totalUsage > 0 && currentItemWithAnimation == null)
                {
                    currentItemWithAnimation = currentItem.UseItem(lag);
                    UpdateButtonUsages();
                }

                if (currentItem.IsReadyToBeRemoved())
                {
                    RemoveCurrentItem();
                }

                if (isPlayerLocal && eventdata != null && eventdata.replicateItemUsage != null)
                {
                    eventdata.replicateItemUsage.Invoke();
                }
            }
        }

        public void RemoveCurrentItem()
        {
            if (currentItem == null) return;

            items[currentItem.GetInventoryIndex()] = null;
            currentItem.RemoveItem();

            animator.SetTrigger("IdleUpper");

            UpdateButtonColors();
            UpdateButtonUsages();
            ResetButtonByIndex(currentItem.GetInventoryIndex());
            currentItem = null;


        }

        public void TriggerAnimationEvent()
        {
            if (currentItemWithAnimation)
            {
                currentItemWithAnimation.AnimationEvent();
                if (currentItemWithAnimation.IsReadyToBeRemoved())
                {
                    RemoveCurrentItem();
                }
                UpdateButtonUsages();
                currentItemWithAnimation = null;
            }

        }
    }
}

      
    