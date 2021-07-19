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

        private float timeLimit = 0.15f;
        private float timer = 0;
        private bool jumpReset = false;
        public RunState(SharedStateData data, MovementStateType type) :base(data, type)
        {

        }

        public override void EnterState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (player == null || animator == null || stats == null) return;

            this.player = player;
            animator.SetBool("Run", true);
            
            nextState = null;

            jumpReset = false;
            timer = 0;
        }

        public override void ExitState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (player == null || animator == null || stats == null) return;
            
            animator.SetBool("Run", false);
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

            timer += Time.deltaTime;

            if(timer > timeLimit && jumpReset == false)
            {
                jumpReset = true;
                ResetAvailableJump();
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