using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public enum BulletType
    {
        speedChanger, hook
    }
    public class Bullet : MonoBehaviour
    {
        private float speed = 20;
        public Player Owner { get; private set; }
        public Transform OwnerTransform { get; set; }
        public float speedDecreaseAmount;
        private Rigidbody rb;

        public BulletType bulletType;
        Ray ray;
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        protected virtual void Start()
        {
            Destroy(gameObject, 10.0f);
        }

        public virtual void Update()
        {
            ray.origin = transform.position;
            ray.direction = rb.velocity.normalized;
            RaycastHit hiInfo;
            Debug.DrawRay(ray.origin, ray.direction * 0.2f, Color.red, 1);
            if (Physics.Raycast(ray, out hiInfo, 0.5f))
            {
                OnCollision(hiInfo.collider);
                Destroy(gameObject);
            }
           
        }
        public void OnCollision(Collider collider)
        {
            NetworkPlayer player = collider.GetComponent<NetworkPlayer>();
            if (player && player.photonView.Owner.ActorNumber != Owner.ActorNumber)
            {
                player.OnHitByBullet(this);
            }
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