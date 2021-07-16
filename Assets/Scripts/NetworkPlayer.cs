using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace PlatformerGame
{

    public class NetworkPlayer : MonoBehaviourPun, IPunObservable
    {
        public const float MovementRaycastDownLength = 0.5f;
        public const float FallRaycastDownLength = 0.1f;

        private Inventory inventory;
        private PlayerStat stats;
        private Animator animator;
        public Rigidbody Rb { get; private set; }

        [SerializeField] private PlayerIndicator indicator;
        [SerializeField] private Transform targetObject;
        [SerializeField] private Transform aimTransform;
        [SerializeField] private Transform weaponSlot;
        [SerializeField] private Transform bulletSpawnSlot;
        public Transform spine;
        public LayerMask groundLayer;

        private SharedStateData stateData = new SharedStateData();
        public RunState runState;
        public JumpState jumpState;
        public FallState fallState;
        private StateBase currentMovementState = null;


        [HideInInspector]public Quaternion spineNewRotation;
        private Vector3 networkPosition;
        private Quaternion networkRotation;
        private bool enableAiming = false;


        public void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            

            runState = new RunState(stateData);
            jumpState = new JumpState(stateData);
            fallState = new FallState(stateData);
    }

        private void Start()
        {
            if (photonView.IsMine)
            {
                inventory = new Inventory(ref InventoryManager.instance.itemData, ref UiManager.instance.itemButtons, this, animator, NetworkManager.instance.inventoryEventData);

                EventTrigger trigger = UiManager.instance.useItemButton.gameObject.AddComponent<EventTrigger>();
                var pointerDown = new EventTrigger.Entry();
                pointerDown.eventID = EventTriggerType.PointerDown;
                pointerDown.callback.AddListener(delegate { inventory.UseItem(0); });
                trigger.triggers.Add(pointerDown);

                pointerDown = new EventTrigger.Entry();
                trigger = UiManager.instance.jumpButton.gameObject.AddComponent<EventTrigger>();
                pointerDown.eventID = EventTriggerType.PointerDown;
                pointerDown.callback.AddListener(delegate { Jump(); });
                trigger.triggers.Add(pointerDown);
            }
            else
            {
                inventory = new Inventory(ref InventoryManager.instance.itemData, this, animator);
            }
        }

        public void SetStats(PlayerStat NewStats)
        {
            stats = NewStats;

            if (indicator)
            {
                indicator.SetPercentage(stats.Speed);
            }
        }

        public void StartRunning()
        {
            if (Rb != null && stats != null)
            {
                currentMovementState = runState;
                currentMovementState.EnterState(animator, this, stats);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //Movement Lag Compansation
            if (stream.IsWriting)
            {
                PhotonNetwork.SerializationRate = 12;
                stream.SendNext(Rb.position);
                stream.SendNext(Rb.rotation);
                stream.SendNext(Rb.velocity);
            }
            else
            {
                networkPosition = (Vector3)stream.ReceiveNext();
                networkRotation = (Quaternion)stream.ReceiveNext();
                Rb.velocity = (Vector3)stream.ReceiveNext();

                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                networkPosition += (Rb.velocity * lag);
            }
        }

        public void FixedUpdate()
        {
            if (currentMovementState != null)
            {
                StateBase newState = currentMovementState.UpdateState(animator, this, stats);
                if(newState != null)
                {
                    currentMovementState.ExitState(animator, this, stats);
                    newState.EnterState(animator, this, stats);
                    currentMovementState = newState;
                }
            }


            if (!photonView.IsMine)
            {
                Rb.position = Vector3.MoveTowards(Rb.position, networkPosition, Time.fixedDeltaTime);
                Rb.rotation = Quaternion.RotateTowards(Rb.rotation, networkRotation, Time.fixedDeltaTime * 100.0f);

                if (PhotonNetwork.PlayerList.Length > 1)
                    targetObject.transform.position = NetworkManager.instance.GetSpinePos(false);
            }
            else
            {
                if(PhotonNetwork.PlayerList.Length > 1)
                    targetObject.transform.position = NetworkManager.instance.GetSpinePos(true);
            }

        }


        private void LateUpdate()
        {
            if (enableAiming)
            {
                Vector3 targetTransformPos = targetObject.position;

                for (int i = 0; i < 20; i++)
                {
                    AimAtTarget(spine, targetTransformPos);
                }
            }

            NetworkManager.instance.SyncPlayerSpineRotation();
            if (!photonView.IsMine)
            {
                spine.rotation = spineNewRotation;
            }
        }


        void AimAtTarget(Transform bone, Vector3 targetPosition)
        {
            Vector3 aimDirection = aimTransform.forward;
            Vector3 targetDir = targetPosition - aimTransform.position;
            Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDir);
            bone.rotation = aimTowards * bone.rotation;
        }

        public void EnableAimingToTarget(bool enable)
        {
            enableAiming = enable;

            if(enable == false && spine)
            {
                spine.rotation = Quaternion.identity;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Collectable collectable = other.GetComponent<Collectable>();
            if (collectable)
            {
                if(collectable.type == CollectableType.item && inventory != null)
                {
                    bool isAdded = inventory.AddItem(collectable.value);

                    if (isAdded)
                    {
                        collectable.Collected();
                    }
                }
                else if(collectable.type == CollectableType.speedBoost)
                {
                    AddSpeed(collectable.value);
                    collectable.Collected();
                }
            }

            if (other.CompareTag("Bullet"))
            {
                Bullet bl = other.GetComponent<Bullet>();
                if (bl)
                {
                    AddSpeed(bl.speedDecreaseAmount);
                }
            }
            
        }

        public  ItemBase CreateNewInventoryItem(ItemBase itemBase, int inventoryIndex)
        {
            ItemBase clone = null;
            if(itemBase.data.itemType == ItemData.ItemType.Rifle)
            {
                clone = Instantiate(((RifleItem)itemBase));
                clone.data = Instantiate(clone.data);
                ((RifleItem)clone).Initialize(inventoryIndex, photonView.Owner ,bulletSpawnSlot, targetObject, weaponSlot, animator, EnableAimingToTarget);
            }
            else if (itemBase.data.itemType == ItemData.ItemType.DoubleJump)
            {
                clone = Instantiate(((DoubleJumpItem)itemBase));
                clone.data = Instantiate(clone.data);
                ((DoubleJumpItem)clone).Initialize(inventoryIndex, photonView.Owner, EnableDoubleJump, Jump , jumpState, inventory.RemoveCurrentItem, inventory.UpdateButtonUsages);
            }
            return clone;
        }

        public void UseItem(float lag)
        {
            if (inventory != null)
            {
                inventory.UseItem(lag);
            }
        }

        public void SwitchTo(int inventoryIndex)
        {
            if (inventory != null)
            {
                inventory.SwitchTo(inventoryIndex);
            }
        }

        public void AddItem(int itemID)
        {
            if (inventory != null)
            {
                inventory.AddItem(itemID);
            }
        }

        public void TriggerItemAnimationEvent()
        {
            if (inventory != null)
            {
                inventory.TriggerAnimationEvent();
            }
        }

        public void SetSpeed(float newSpeed)
        {
            if (stats != null)
            {
                stats.Speed = newSpeed;

                if (indicator)
                {
                    indicator.SetPercentage(stats.Speed);
                }
            }
        }

        public void AddSpeed(float addAmount)
        {
            float newSpeed = stats.Speed + addAmount;
            SetSpeed(newSpeed);
            //PlayEffects

            if (photonView.IsMine)
            {
                NetworkManager.instance.ReplicateSpeed(newSpeed);
            }
        }

        public void Jump()
        {
            if (currentMovementState != null)
            {
                currentMovementState.JumpKeyPressed();
            }
        }

        public void EnableDoubleJump(bool enable)
        {
            if (currentMovementState != null)
            {
                currentMovementState.EnableDoubleJump(enable);
            }
        }

    }
}

