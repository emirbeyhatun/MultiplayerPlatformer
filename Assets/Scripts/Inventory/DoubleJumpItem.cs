using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    [CreateAssetMenu(fileName = "DoubleJump", menuName = "ScriptableObjects/DoubleJump", order = 2)]
    public class DoubleJumpItem : ItemBase
    {
        event Action jumpEvent;
        event Action removeItem;
        event Action updateButtonInfo;
        JumpState jumpState;

        public void Initialize(int inventoryIndex, Player owner, Action<bool> OnSwitchTo, Action OnJump,  JumpState jumpState, Action removeItem, Action updateButtonInfo)
        {
            Initialize(inventoryIndex, owner, OnSwitchTo);
            this.jumpEvent = OnJump;
            this.jumpState = jumpState;
            this.removeItem = removeItem;
            this.updateButtonInfo = updateButtonInfo;
        }

        public override void SwitchFromItem()
        {
            CallOnSwitchEvent(false);

            jumpState.onDoubleJump -= DecreaseUsage;
        }

        public override void SwitchToItem()
        {
            CallOnSwitchEvent(true);
            jumpState.onDoubleJump = DecreaseUsage;
        }

        public override ItemBase UseItem(float lag)
        {
            //if item is dependent to animation it returns itself
            if (data.totalUsage > 0 && jumpEvent != null)
            {
                jumpEvent.Invoke();
            }

            return null;
        }

        public void DecreaseUsage()
        {
            data.totalUsage--;

            if (data.totalUsage <= 0)
            {
                readyToBeRemoved = true;

                if (removeItem != null)
                    removeItem.Invoke();
            }
            else
            {
                if(updateButtonInfo != null)
                    updateButtonInfo.Invoke();
            }

           
        }

        public override void AnimationEvent()
        {
            
        }

        public override void RemoveItem()
        {
            CallOnSwitchEvent(false);
        }
    }
}