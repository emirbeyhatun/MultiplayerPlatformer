using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame 
{
    public class GetPulledState : StateBase
    {
        public Transform targetTransform;
        float timeLimit = 3;
        float timer;
        bool savedAimStatus;
        public GetPulledState(SharedStateData data) : base(data)
        {

        }

        public override void EnterState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (targetTransform == null || player == null || animator == null || stats == null) return;
            
            animator.speed = 0;
            savedAimStatus = player.GetAiminStatus();
            player.EnablePlayerGravity(false);
            player.EnableAimingToTarget(false);
            player.EnablePlayerControls(false);
            timer = 0;
        }

        public override void ExitState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (targetTransform == null || player == null || animator == null || stats == null) return;
                
            animator.speed = 1;

            player.EnablePlayerGravity(true);
            player.EnableAimingToTarget(savedAimStatus);
            player.EnablePlayerControls(true);

        }

        public override StateBase UpdateState(Animator animator, NetworkPlayer player, PlayerStat stats)
        {
            if (targetTransform == null || player == null || animator == null || stats == null) return null;

            Vector3 dir = targetTransform.position - player.transform.position;
            player.Rb.velocity = (dir).normalized * 20;

            timer += Time.deltaTime;

            if (dir.magnitude <= 1 || timer >= timeLimit)
            {
                return player.fallState;
            }

            return null;
        }
    }
}
