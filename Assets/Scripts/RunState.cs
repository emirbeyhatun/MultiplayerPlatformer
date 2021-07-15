using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class RunState : IState
    {
        private bool didRunStarted = false;
        Ray ray;

        public RunState()
        {
            ray = new Ray();
        }

        public IState UpdateState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (didRunStarted == false)
            {
                if (animator)
                {
                    animator.SetBool("Run", true);
                }
                didRunStarted = true;
            }
            Vector3 speedVec = stats.SpeedVector;
            speedVec.y = player.Rb.velocity.y;
            player.Rb.velocity = speedVec;

            ray.origin = player.transform.position;
            ray.direction = player.transform.up * -1;

            if (Physics.Raycast(ray, NetworkPlayer.MovementRaycastDownLength, player.groundLayer) == false)
            {
                OnExitState(animator);
                return player.fallState;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                OnExitState(animator);
                return player.jumpState;
            }
            return this;
        }

        void OnExitState(Animator animator)
        {
            if (animator)
            {
                animator.SetBool("Run", false);
            }
            didRunStarted = false;
        }

    }
}