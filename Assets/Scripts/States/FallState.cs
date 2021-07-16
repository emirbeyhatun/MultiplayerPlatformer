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

        public FallState(SharedStateData data) : base(data)
        {
            ray = new Ray();
        }

        public override void EnterState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (animator)
            {
                animator.SetBool("Fall", true);
            }
            this.player = player;

            nextState = null;

        }

        public override void ExitState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (animator)
            {
                animator.SetBool("Fall", false);
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
            ray.origin = player.transform.position;
            ray.direction = player.transform.up * -1;

            if (Physics.Raycast(ray, NetworkPlayer.FallRaycastDownLength, player.groundLayer))
            {
                return player.runState;
            }
            return null;
        }
    }
}