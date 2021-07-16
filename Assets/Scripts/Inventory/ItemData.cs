using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/Itemdata", order = 1)]
    public class ItemData : ScriptableObject
    {
        public enum ItemType
        {
            Rifle, DoubleJump
        }

        public ItemType itemType;
        public int itemID;
        public int totalUsage;
        public float value;
        public float cooldown;
        public string itemName;
        public string animationKey;
        public GameObject model;
        public GameObject projectile;
        public Quaternion localRotation;
        public Vector3 localPosition;
        public Vector3 localBulletSpawnPosition;

    }
}