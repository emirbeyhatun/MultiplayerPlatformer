using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class PlayerStat
    {
        private float speed = 10;
        public float Speed { get=> speed;  set { SetSpeed(value); }}
        private const float minSpeed = 5;
        private const float maxSpeed = 20;
        public Vector3 SpeedVector { get { return new Vector3(speed, 0, 0); } }
        public float JumpForce { get; private set; } = 8;

        List<int> inventory;

        public PlayerStat()
        {
            inventory = new List<int>();
        }

        private void SetSpeed(float value)
        {
            speed = Mathf.Clamp(value, minSpeed, maxSpeed);
        }
    }
}