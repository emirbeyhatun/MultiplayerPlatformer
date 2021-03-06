using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    [CreateAssetMenu(fileName = "Rifle", menuName = "ScriptableObjects/Rifle", order = 1)]

    public class RifleItem : ItemBase
    {
        Transform bulletSpawnSlot;
        Transform weaponSpawnSlot;
        Transform targetTransform;


        GameObject modelInstance;
        Animator animator;


        float latestLag;

        public void Initialize(int inventoryIndex, Player owner, Transform bulletSpawnSlot, Transform targetTransform, Transform weaponSpawnSlot, Animator animator, Action<bool> OnSwitchTo)
        {
            Initialize(inventoryIndex, owner, OnSwitchTo);
            this.bulletSpawnSlot = bulletSpawnSlot;
            this.weaponSpawnSlot = weaponSpawnSlot;
            this.targetTransform = targetTransform;

            this.animator = animator;
        }

        public override void SwitchFromItem()
        {
            CallOnSwitchEvent(false);
            if (modelInstance)
            {
                modelInstance.gameObject.SetActive(false);
            }

            animator.ResetTrigger(data.animationKey);
        }

        public override void SwitchToItem()
        {
            CallOnSwitchEvent(true);
            if (data.model && modelInstance == null)
            {
                modelInstance = Instantiate(data.model, weaponSpawnSlot);
                modelInstance.transform.localPosition = data.localPosition;
                modelInstance.transform.localRotation = data.localRotation;
            }
            else
            {
                modelInstance.gameObject.SetActive(true);
            }

            if (animator)
            {
                //animator.SetLayerWeight(1, 1);
                animator.SetTrigger(data.animationKey);
            }

        }

        public override ItemBase UseItem(float lag)
        {
            //if item is dependent to animation it returns itself
            if(data.totalUsage > 0)
            {
                latestLag = lag;
                animator.SetTrigger("UseItem");
                data.totalUsage--;

            }

            return this;
        }


        public override void AnimationEvent()
        {
            if (data.model)
            {
                Bullet bl = Instantiate(data.projectile, bulletSpawnSlot.position, Quaternion.identity).GetComponent<Bullet>();

                if (bl)
                {
                    //creates bullet locally, in inventory function UseItem we replicate item use 
                    bl.Init(owner, weaponSpawnSlot, (targetTransform.position - bulletSpawnSlot.position).normalized, data.speed, data.value, latestLag);
                }
            }

            if (data.totalUsage <= 0)
            {
                readyToBeRemoved = true;
            }
        }

        public override void RemoveItem()
        {
            if (modelInstance)
            {
                Destroy(modelInstance);
            }
            CallOnSwitchEvent(false);
            animator.ResetTrigger("UseItem");
        }
    }
}