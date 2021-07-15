using UnityEngine;

namespace PlatformerGame
{
    public class JumpState : IState
    {
        private bool didJump = false;
        float yPosition;
        bool didTookOffFromGround = false;

        Ray ray;

        public JumpState()
        {
            ray = new Ray();
        }
        public IState UpdateState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (didJump == false)
            {
                if (animator)
                {
                    animator.SetBool("Jump", true);
                }
                yPosition = player.transform.position.y;
                player.Rb.velocity += new Vector3(0, stats.JumpForce, 0);
                didJump = true;
            }

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

            if (player.transform.position.y < yPosition || endState)
            {
                didJump = false;
                animator.SetBool("Jump", false);
                return player.fallState;
            }

            yPosition = player.transform.position.y;
            return this;
        }
    }
}