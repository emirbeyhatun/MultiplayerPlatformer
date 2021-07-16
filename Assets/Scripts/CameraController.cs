using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class CameraController : MonoBehaviour
    {
        private Transform target;
        [SerializeField]private Vector3 distance;
        private void LateUpdate()
        {
            if (target)
            {
                transform.position = Vector3.Lerp(transform.position, target.transform.position + distance, Time.deltaTime * 4);
            }
        }

        public void SetTarget(Transform target)
        {
            this.target = target;

            if (target)
            {
                transform.position = target.transform.position + distance;
            }
        }
    }
}