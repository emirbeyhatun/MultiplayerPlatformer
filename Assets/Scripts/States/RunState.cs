using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class RunState : StateBase
    {
        private Ray ray;
        private StateBase nextState = null;
        private NetworkPlayer player;
        public RunState(SharedStateData data):base(data)
        {
            ray = new Ray();
        }

        public override void EnterState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            this.player = player;
            if (animator)
            {
                animator.SetBool("Run", true);
            }
            
            nextState = null;
            ResetAvailableJump();
        }

        public override void ExitState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (animator)
            {
                animator.SetBool("Run", false);
            }
            nextState = null;
        }

        public override void JumpKeyPressed()
        {
            if (CanJump() && nextState == null)
            {
                nextState = player.jumpState;
            }
        }

        
        public override StateBase UpdateState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (nextState != null)
            {
                return nextState;
            }

            Vector3 speedVec = stats.SpeedVector;
            speedVec.y = player.Rb.velocity.y;
            player.Rb.velocity = speedVec;

            ray.origin = player.transform.position;
            ray.direction = player.transform.up * -1;

            if (Physics.Raycast(ray, NetworkPlayer.MovementRaycastDownLength, player.groundLayer) == false)
            {
                return player.fallState;
            }

            return null;
        }
    }
}