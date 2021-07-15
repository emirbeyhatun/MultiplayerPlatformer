using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace PlatformerGame
{

    public class NetworkPlayer : MonoBehaviourPun, IPunObservable
    {
        public const float MovementRaycastDownLength = 0.5f;
        private PlayerStat stats;
        private Animator animator;
        public Rigidbody Rb { get; private set; }

        [SerializeField] private Transform targetObject;
        [SerializeField] private Transform aimTransform;
        [SerializeField] private Transform weaponSlot;
        [SerializeField] private Transform bulletSpawnSlot;
        public Transform spine;

        public LayerMask groundLayer;
        public Canvas localPlayerIndicator;

        public RunState runState = new RunState();
        public JumpState jumpState = new JumpState();
        public FallState fallState = new FallState();
        private IState currentMovementState = null;

        [HideInInspector]public Quaternion spineNewRotation;

        private Vector3 networkPosition;
        private Quaternion networkRotation;
        private bool enableAiming = false;

        private Inventory inventory;


        public void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            if (photonView.IsMine == false)
            {
                localPlayerIndicator.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (photonView.IsMine)
            {
                inventory = new Inventory(ref InventoryManager.instance.itemData, ref UiManager.instance.itemButtons, this, animator, NetworkManager.instance.inventoryEventData);

                UiManager.instance.useItemButton.onClick.AddListener(delegate { inventory.UseItem(0); });
            }
            else
            {
                inventory = new Inventory(ref InventoryManager.instance.itemData, this, animator);
            }
        }

        public void SetStats(PlayerStat NewStats)
        {
            stats = NewStats;
        }

        public void StartRunning(/*double serverTimeStamp*/)
        {
            if (Rb != null && stats != null)
            {
                currentMovementState = runState;
            }

            //float delay = (float)(PhotonNetwork.GetPing() / 2 - remotePing / 2);
            //transform.position += stats.SpeedVector * (float)(PhotonNetwork.Time - serverTimeStamp);
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

                //rb.position = networkPosition;
            }
        }

        public void FixedUpdate()
        {
            if (currentMovementState != null)
            {
                currentMovementState = currentMovementState.UpdateState(animator, this, stats);
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
                //Debug.DrawRay(aimTransform.position, aimTransform.forward * 10, Color.red, 1);
                //Debug.DrawRay(spine.position, spine.forward * 10, Color.green, 1);
            }

            NetworkManager.instance.SyncPlayerSpineRotation();
            if (!photonView.IsMine)
            {
                spine.rotation = spineNewRotation;
            }
        }


        void AimAtTarget(Transform bone, Vector3 targetPosition)
        {
            //bone.forward = (targetPosition - bone.transform.position).normalized;

            Vector3 aimDirection = aimTransform.forward;
            Vector3 targetDir = targetPosition - aimTransform.position;
            Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDir);
            //Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, 1);
            bone.rotation = aimTowards * bone.rotation;
        }

        public bool EnableAimingToTarget(bool enable)
        {
            enableAiming = enable;

            if(enable == false && spine)
            {
                spine.rotation = Quaternion.identity;
            }

            return enableAiming;
        }

        //private void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawSphere(targetObject.position, 0.05f);
        //}

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
            }

            if (other.CompareTag("Bullet"))
            {
                Bullet bl = other.GetComponent<Bullet>();
                if (bl)
                {
                    float newSpeed = stats.Speed - bl.speedDecreaseAmount;

                    SetSpeed(newSpeed);
                    //PlayEffects

                    if (photonView.IsMine)
                    {
                        NetworkManager.instance.ReplicateSpeed(newSpeed);
                    }
                }
            }
            
        }

        public  ItemBase CreateNewInventoryItem(ItemBase itemBase, int inventoryIndex)
        {
            ItemBase clone = Instantiate(itemBase);
            if((RifleItem)clone)
            {
                ((RifleItem)clone).Initialize(inventoryIndex, photonView.Owner ,bulletSpawnSlot, targetObject, weaponSlot, animator, EnableAimingToTarget);
            }
            else
            {
                clone.Initialize(inventoryIndex, photonView.Owner);
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

        public void SwitchTo(int itemID)
        {
            if (inventory != null)
            {
                inventory.SwitchTo(itemID);
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
            }
        }


    }
}

