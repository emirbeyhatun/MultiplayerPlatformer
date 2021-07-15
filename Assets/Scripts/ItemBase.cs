using Photon.Realtime;
using PlatformerGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public enum ItemType
    {
        rifle, sword
    }
    public abstract class ItemBase : ScriptableObject
    {
        public ItemData data;
        protected Player owner;
        protected int inventoryIndex;
        protected event Func<bool, bool> OnSwitchTo;
        protected bool readyToBeRemoved = false;
        public  void Initialize(int inventoryIndex, Player owner, Func<bool, bool> OnSwitchTo = null)
        {
            this.inventoryIndex = inventoryIndex;
            this.owner = owner;

            if (OnSwitchTo != null)
            {
                this.OnSwitchTo = OnSwitchTo;
            }
            Debug.Log("Added Weapon");
        }

        protected void CallOnSwitchEvent(bool param)
        {
            if(OnSwitchTo != null)
            {
                OnSwitchTo.Invoke(param);
            }
        }

        public float GetCooldown()
        {
            return data.cooldown;
        }

        public int GetInventoryIndex()
        {
            return inventoryIndex;
        }

        public bool IsReadyToBeRemoved()
        {
            return readyToBeRemoved;
        }
        public abstract ItemBase UseItem(float lag);
        public abstract void AnimationEvent();
        public abstract void SwitchToItem();
        public abstract void SwitchFromItem();
        public abstract void RemoveItem();
       
    }
}
