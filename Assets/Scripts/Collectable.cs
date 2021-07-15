using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public enum CollectableType
    {
        item,
        speedBoost
    }
    public class Collectable : MonoBehaviour
    {
        public CollectableType type;
        public int value = -1;

        public void Collected()
        {
            Destroy(gameObject);
        }
    }
}
