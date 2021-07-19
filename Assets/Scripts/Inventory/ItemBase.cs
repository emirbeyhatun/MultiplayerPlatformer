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
        protected event Action<bool> OnSwitchTo;
        protected bool readyToBeRemoved = false;
        public  void Initialize(int inventoryIndex, Player owner, Action<bool> OnSwitchTo = null)
        {
            this.inventoryIndex = inventoryIndex;
            this.owner = owner;

            if (OnSwitchTo != null)
            {
                this.OnSwitchTo = OnSwitchTo;
            }
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

        //Called when animation triggers
        public abstract void AnimationEvent();
        public abstract void SwitchToItem();
        public abstract void SwitchFromItem();
        public abstract void RemoveItem();
       
    }
}
