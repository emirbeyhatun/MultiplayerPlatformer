using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class SharedStateData
    {
        public const int doubleJumpAmount = 2;
        public const int singleJumpAmount = 1;
        public int defaultJumpAmount = singleJumpAmount;
        public int availableJumpAmount { get; set; } = singleJumpAmount;
    }
    public  class StateBase
    {
        public SharedStateData sharedData { protected get; set; }

        public StateBase(SharedStateData data)
        {
            sharedData = data;
        }
        protected void ResetAvailableJump()
        {
            sharedData.availableJumpAmount = sharedData.defaultJumpAmount;
        }

        public void EnableDoubleJump(bool enable)
        {
            if (enable)
            {
                sharedData.defaultJumpAmount = SharedStateData.doubleJumpAmount;
            }
            else
            {
                sharedData.defaultJumpAmount = SharedStateData.singleJumpAmount;
            }
            ResetAvailableJump();
        }

        protected void DecreaseJumpAvailibilty()
        {
            sharedData.availableJumpAmount = Mathf.Max(--sharedData.availableJumpAmount, 0);
        }
        protected bool CanJump()
        {
            return sharedData.availableJumpAmount > 0;
        }
        public virtual void EnterState(Animator animator, NetworkPlayer player, PlayerStat stats) { }
        public virtual void ExitState(Animator animator, NetworkPlayer player, PlayerStat stats) { }
        public virtual StateBase UpdateState(Animator animator, NetworkPlayer player, PlayerStat stats) { return null; }
        public virtual void JumpKeyPressed() {  }
    }
}