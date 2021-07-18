using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class FallState : StateBase
    {
        private Ray ray;
        private StateBase nextState = null;
        private NetworkPlayer player;

        public FallState(SharedStateData data, MovementStateType type) : base(data, type)
        {
            ray = new Ray();
        }

        public override void EnterState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (player == null || animator == null || stats == null) return;
            
            animator.SetBool("Fall", true);
            this.player = player;

            nextState = null;

        }

        public override void ExitState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (player == null || animator == null || stats == null) return;
            
            animator.SetBool("Fall", false);

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
            if (player == null || animator == null || stats == null) return null;

            if (nextState != null)
            {
                return nextState;
            }
            float xOffset = 0.25f;
            ray.origin = player.transform.position - new Vector3(xOffset, 0, 0);
            ray.direction = Vector3.up * -1;

            //Debug.DrawRay(ray.origin, ray.direction*0.1f, Color.red, 1);
            if (Physics.Raycast(ray, NetworkPlayer.FallRaycastDownLength, player.groundLayer))
            {
                return player.runState;
            }

            ray.origin = player.transform.position + new Vector3(xOffset, 0, 0);
            if (Physics.Raycast(ray, NetworkPlayer.FallRaycastDownLength, player.groundLayer))
            {
                return player.runState;
            }

            return null;
        }
    }
}