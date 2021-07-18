using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class Lever : MonoBehaviour
    {
        [SerializeField] private Rigidbody platformToAnimate;
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Quaternion targetRotation;
        [SerializeField] private float timeLimit = 1f;
        private float timer = 0;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private const string animationKey = "UseLever";

        private bool startMoving = false;


        private void Start()
        {
            startPosition = platformToAnimate.transform.position;
            startRotation = platformToAnimate.transform.rotation;
        }
        private void OnTriggerEnter(Collider other)
        {

            if (other.GetComponent<NetworkPlayer>() && startMoving == false && platformToAnimate)
            {
                startPosition = platformToAnimate.transform.position;
                startRotation = platformToAnimate.transform.rotation;
                startMoving = true;
                timer = 0;
            }
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.A))
            //{
            //    startMoving = true;
            //}
            if (startMoving && timer <= timeLimit)
            {
                timer += Time.deltaTime;
            }
        }


        private void FixedUpdate()
        {
            if (startMoving)
            {
                if (platformToAnimate && timer <= timeLimit)
                {
                    Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, timer / timeLimit);
                    Quaternion newRotation = Quaternion.Lerp(startRotation, targetRotation, timer / timeLimit);

                    platformToAnimate.position = newPosition;
                    platformToAnimate.rotation = newRotation;

                }
            }
        }
    }
}