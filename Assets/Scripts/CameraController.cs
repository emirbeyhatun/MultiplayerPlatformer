using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class CameraController : MonoBehaviour
    {
        Transform target;

        Vector3 distance;
        private void LateUpdate()
        {
            if (target)
            {
                transform.position = target.transform.position + distance;
            }
        }

        public void SetTarget(Transform target)
        {
            this.target = target;

            distance = transform.position - target.position;
        }
    }
}