using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class Bullet : MonoBehaviour
    {
        private float speed = 20;
        public Player Owner { get; private set; }
        public Transform OwnerTransform { get; set; }
        public float speedDecreaseAmount;
        private Rigidbody rb;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        protected virtual void Start()
        {
            Destroy(gameObject, 10.0f);
        }

        public void OnTriggerEnter(Collider collider)
        {
            NetworkPlayer player = collider.GetComponent<NetworkPlayer>();
            if (player && player.photonView.Owner.ActorNumber == Owner.ActorNumber)
            {
                return;
            }
            Destroy(gameObject);
        }

        public void Init(Player owner, Transform ownerTransform, Vector3 originalDirection, float initialSpeed,float speedDecreaseAmount, float lag)
        {
            Owner = owner;
            OwnerTransform = ownerTransform;
            this.speedDecreaseAmount = speedDecreaseAmount;
            transform.forward = originalDirection;
            speed = initialSpeed;

            rb.velocity = originalDirection * speed;
            rb.position += rb.velocity * lag;
        }

    }
}