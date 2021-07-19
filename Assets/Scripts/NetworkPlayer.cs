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
        public const float FallRaycastDownLength = 0.3f;

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
        public LayerMask sightRaycastLayer;


        private SharedStateData stateData = new SharedStateData();
        public RunState runState;
        public JumpState jumpState;
        public FallState fallState;
        public GetPulledState getPulledState;
        private StateBase currentMovementState = null;


        [HideInInspector]public Quaternion spineNewRotation;
        private Vector3 networkPosition;
        private Quaternion networkRotation;
        private bool enableAiming = false;

        //private Vector3 targetObjectDefaultPos;



        public void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            
            //Initializing the states
            runState = new RunState(stateData, MovementStateType.run);
            jumpState = new JumpState(stateData, MovementStateType.jump);
            fallState = new FallState(stateData, MovementStateType.fall);
            getPulledState = new GetPulledState(stateData, MovementStateType.getPulled);

        }

        private void Start()
        {
            if (photonView.IsMine)
            {
                //if player is local
                //creates an inventory and injects default itemdata information, ui item buttons, and inventory event data struct which contains network delegates
                inventory = new Inventory(ref InventoryManager.instance.itemData, ref UiManager.instance.itemButtons, this, animator, NetworkManager.instance.inventoryEventData);


                //Bind Use item button(Right side of whole screen) to UseItem function
                EventTrigger trigger = UiManager.instance.useItemButton.gameObject.AddComponent<EventTrigger>();
                var pointerDown = new EventTrigger.Entry();
                pointerDown.eventID = EventTriggerType.PointerDown;
                pointerDown.callback.AddListener(delegate { inventory.UseItem(0); });
                trigger.triggers.Add(pointerDown);


                //Bind Jump Button button(Left side of whole screen) to Jump function
                pointerDown = new EventTrigger.Entry();
                trigger = UiManager.instance.jumpButton.gameObject.AddComponent<EventTrigger>();
                pointerDown.eventID = EventTriggerType.PointerDown;
                pointerDown.callback.AddListener(delegate { Jump(); });
                trigger.triggers.Add(pointerDown);

                //Set local player's layer to player
                gameObject.layer = LayerMask.NameToLayer("Player");
            }
            else
            {
                //if player is remote then we just pass in the default item list, we dont want to pass ui buttons and network delegates
                inventory = new Inventory(ref InventoryManager.instance.itemData, this, animator);
                //Set remote player's layer to player
                gameObject.layer = LayerMask.NameToLayer("Enemy");
            }
        }

        public void SetStats(PlayerStat NewStats)
        {
            //Set player stats that contains speed, jump force etc
            stats = NewStats;

            if (indicator)
            {
                //Reset 3d speed bar
                indicator.SetPercentage(stats.Speed);
            }
        }

        public void StartRunning()
        {
            //This is called when the count down ends and the game starts
            if (Rb != null && stats != null)
            {
                //We start the run state in bit remote and local because we don't want to delay local enemy players movement,
                //otherwise it would wait for the reques to come from server
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
            if (currentMovementState != null && photonView.IsMine)
            {
                //We update our state and if there is a change we chance the state, only for local player
                StateBase newState = currentMovementState.UpdateState(animator, this, stats);
                SetState(newState);
            }


            if (!photonView.IsMine)
            {
                //Lag compansation added
                Rb.position = Vector3.MoveTowards(Rb.position, networkPosition, Time.fixedDeltaTime);
                Rb.rotation = Quaternion.RotateTowards(Rb.rotation, networkRotation, Time.fixedDeltaTime * 100.0f);
            }

        }


        private void LateUpdate()
        {
            if (enableAiming && photonView.IsMine)
            {
                //if we have switched out item to a gun then we rotate spine to a rotation that it looks at the target player
                AimInUpdate();
            }


            //We send our spine rotation to remotes
            if (photonView.IsMine)
            {
                NetworkManager.instance.SyncPlayerSpineRotation();
            }
            else
            {
                spine.rotation = spineNewRotation;
            }


            //Set target objects position to enemy spine position, because we use this empty object to aim
            if (!photonView.IsMine)
            {
                if (PhotonNetwork.PlayerList.Length > 1)
                    targetObject.transform.position = NetworkManager.instance.GetSpinePos(false);
            }
            else
            {
                if (PhotonNetwork.PlayerList.Length > 1)
                    targetObject.transform.position = NetworkManager.instance.GetSpinePos(true);
            }

            
        }

        void AimInUpdate()
        {
            Vector3 targetTransformPos = targetObject.position;
            AimAtTarget(spine, targetTransformPos);
        }

        void AimAtTarget(Transform bone, Vector3 targetPosition)
        {
            //Rotate spine to target objects direction
            Vector3 aimDirection = aimTransform.forward;
            Vector3 targetDir = targetPosition - aimTransform.position;
            Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDir);
            bone.rotation = aimTowards * bone.rotation;
        }

        public bool GetAiminStatus()
        {
            return enableAiming;
        }
        public void EnableAimingToTarget(bool enable)
        {
            enableAiming = enable;
        }

        private void OnTriggerEnter(Collider other)
        {
            //if we collide with a collectable object then we want to collect them
            Collectable collectable = other.GetComponent<Collectable>();
            OnCollectCollectable(collectable);
        }

        public void OnHitByBullet(Bullet bl)
        {
            //this is called by the bullet on hit
            if (bl)
            {
                if (photonView.IsMine)
                {
                    if (bl.bulletType == BulletType.speedChanger)
                        AddSpeed(bl.speedDecreaseAmount);

                    if (bl.bulletType == BulletType.hook)
                        StartPullState(bl.OwnerTransform);
                }
            }
        }

        public void OnCollectCollectable(Collectable collectable)
        {
            if (collectable)
            {
                if (collectable.type == CollectableType.item && inventory != null)
                {
                    bool isAdded = inventory.AddItem(collectable.value);

                    if (isAdded)
                    {
                        collectable.Collected();
                    }
                }
                else if (collectable.type == CollectableType.speedBoost)
                {
                    AddSpeed(collectable.value);
                    collectable.Collected();
                }
            }
        }

        public  ItemBase CreateNewInventoryItem(ItemBase itemBase, int inventoryIndex)
        {
            //This is called by the inventory object whenever it needs a new item to add 

            //Items are injected with their needs
            ItemBase clone = null;
            if(itemBase.data.itemType == ItemData.ItemType.Rifle)
            {
                //rifle needs to enable aim on switch so we pass the function
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
            //this is called in the shoot animation, we dont spawn bullets in useitem(), rather we spawn it with animation event
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

            if (photonView.IsMine)
            {
                //On Speed change we send that data to remotes
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

        public void EnablePlayerControls(bool enable)
        {
            UiManager.instance.EnableControls(enable, photonView);
        }

        public void EnablePlayerGravity(bool enable)
        {
            if(Rb)
                Rb.useGravity = enable;
        }


        public void StartPullState(Transform targetTransform)
        {
            //when we get pulled by the hook gun then we change our state
            if (getPulledState != null && targetTransform && photonView.IsMine)
            {
                getPulledState.targetTransform = targetTransform;

                SetState(getPulledState);
            }
        }

        public void SetState(StateBase newState)
        {
            if (newState != null)
            {
                currentMovementState.ExitState(animator, this, stats);
                newState.EnterState(animator, this, stats);
                currentMovementState = newState;
            }
        }
    }
}

