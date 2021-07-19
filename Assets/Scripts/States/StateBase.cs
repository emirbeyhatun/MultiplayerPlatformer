using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public enum MovementStateType
    {
        run,
        jump, 
        fall,
        getPulled
    }
    public class SharedStateData
    {
        public const int doubleJumpAmount = 2;
        public const int singleJumpAmount = 1;
        public int defaultJumpAmount = singleJumpAmount;
        public int availableJumpAmount { get; set; } = singleJumpAmount;
    }
    public  class StateBase
    {
        protected MovementStateType stateType;
        public SharedStateData sharedData { protected get; set; }

        public StateBase(SharedStateData data, MovementStateType type)
        {
            sharedData = data;
            stateType = type;
        }
        public void ResetAvailableJump()
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

            if(stateType == MovementStateType.run)
            {
                ResetAvailableJump();
            }
                
        }

        public MovementStateType GetStateType()
        {
            return stateType;
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