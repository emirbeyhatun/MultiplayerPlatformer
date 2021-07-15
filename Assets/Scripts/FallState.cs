using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class FallState : IState
    {
        private bool didJumpStarted = false;
        Ray ray;

        public FallState()
        {
            ray = new Ray();
        }

        public IState UpdateState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (didJumpStarted == false)
            {
                if (animator)
                {
                    animator.SetBool("Fall", true);
                    Debug.Log("FALL");
                }
                didJumpStarted = true;
            }

            ray.origin = player.transform.position;
            ray.direction = player.transform.up * -1;

            Debug.DrawRay(ray.origin, ray.direction * 3, Color.red, 1);
            if (Physics.Raycast(ray, NetworkPlayer.MovementRaycastDownLength, player.groundLayer))
            {
                Debug.Log("FALL END");
                animator.SetBool("Fall", false);
                didJumpStarted = false;
                return player.runState;
            }
            return this;
        }
    }
}