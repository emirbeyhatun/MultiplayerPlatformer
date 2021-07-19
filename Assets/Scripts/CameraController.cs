using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController instance;
        private Transform target;
        [SerializeField]private Vector3 distance;

        Vector3 lastFramePosition;
        private Vector3 velocity = Vector3.zero;

        private void Awake()
        {
            instance = this;
        }
        private void LateUpdate()
        {
            lastFramePosition = transform.position;
            if (target)
            {
                //transform.position = Vector3.Lerp(transform.position, target.transform.position + distance, Time.deltaTime);
                //transform.position = target.transform.position + distance;

                transform.position = Vector3.SmoothDamp(transform.position, target.transform.position + distance, ref velocity, 0.5f);
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

        public Vector3 GetPosChange()
        {
            return lastFramePosition - transform.position;
        }
    }
}