using System;
using UnityEngine;

namespace PlatformerGame
{
    public class JumpState : StateBase
    {
        private float yPosition;
        private bool didTookOffFromGround = false;
        private NetworkPlayer player;
        private Ray ray;
        private StateBase nextState = null;

        private float timeLimit = 0.2f;
        private float timer = 0;

        public Action onDoubleJump;
        public JumpState(SharedStateData data, MovementStateType type) : base(data,  type)
        {
        }

        public override void EnterState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (player == null || animator == null || stats == null) return;

            DecreaseJumpAvailibilty();
            if (CanJump() == false && sharedData.defaultJumpAmount > 1 && onDoubleJump != null)
            {
                onDoubleJump.Invoke();
            }
            timer = 0;

            animator.SetBool("Jump", true);

            this.player = player;
            yPosition = player.transform.position.y;
            Vector3 vel = player.Rb.velocity;
            vel.y = Mathf.Max(0, vel.y);
            vel.y += stats.JumpForce;
            player.Rb.velocity = vel;

            nextState = null;
        }

        public override void ExitState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (player == null || animator == null || stats == null) return;

            animator.SetBool("Jump", false);
            nextState = null;
        }

        public override void JumpKeyPressed()
        {
            if (didTookOffFromGround && CanJump() && nextState == null)
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

            ray.origin = player.transform.position;
            ray.direction = player.transform.up * -1;

            bool endState = false;

            if (Physics.Raycast(ray, NetworkPlayer.MovementRaycastDownLength, player.groundLayer) == false)
            {
                didTookOffFromGround = true;
            }
            else
            {
                if (didTookOffFromGround)
                {
                    endState = true;
                }
            }

            if (player.transform.position.y < yPosition || endState || (didTookOffFromGround == false && timer > timeLimit))
            {
                return player.fallState;
            }

            yPosition = player.transform.position.y;
            return null;
        }
    }
}