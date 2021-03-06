using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class PlayerStat
    {
        public const float minSpeed = 5;
        public const float maxSpeed = 9;

        private float speed = 7;
        public float Speed { get=> speed;  set { SetSpeed(value); }}
        public Vector3 SpeedVector { get { return new Vector3(speed, 0, 0); } }
        public float JumpForce { get; private set; } = 8;

        private void SetSpeed(float value)
        {
            speed = Mathf.Clamp(value, minSpeed, maxSpeed);
        }
    }
}