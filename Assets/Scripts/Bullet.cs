using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 20;
        public Player Owner { get; private set; }
        public float speedDecreaseAmount;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void Start()
        {
            Destroy(gameObject, 10.0f);
        }

        public void OnTriggerEnter(Collider collider)
        {
            Destroy(gameObject);
        }

        public void Init(Player owner, Vector3 originalDirection, float speedDecreaseAmount, float lag)
        {
            Owner = owner;
            this.speedDecreaseAmount = speedDecreaseAmount;
            transform.forward = originalDirection;

            rb.velocity = originalDirection * speed;
            rb.position += rb.velocity * lag;
        }

    }
}