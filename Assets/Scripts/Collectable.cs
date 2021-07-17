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
        GameObject instantiatedModel;

        private void Start()
        {
            if(type == CollectableType.item)
            {
                ItemBase item = InventoryManager.instance.GetItemData(value);

                if (item.data && item.data.model)
                {
                    instantiatedModel = Instantiate(item.data.model, transform);
                    if (instantiatedModel)
                    {
                        instantiatedModel.transform.localScale *= 3;
                        GetComponent<Renderer>().enabled = false;
                        instantiatedModel.transform.localRotation = Quaternion.Euler(0, 0, -90);
                    }
                }
            }
        }

        private void Update()
        {
            if (instantiatedModel)
            {
                instantiatedModel.transform.localRotation *= Quaternion.Euler(90 * Time.deltaTime, 0, 0) ;
            }
        }
        public void Collected()
        {
            Destroy(gameObject);
        }
    }
}
