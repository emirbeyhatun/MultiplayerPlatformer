using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using System;

namespace PlatformerGame
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Text infoText;
        [SerializeField] private GameObject[] spawnPoints;
        [SerializeField] private CameraController cam;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject tutorial;

        private NetworkPlayer localPlayerInstance;
        private NetworkPlayer remotePlayerInstance;

        public static NetworkManager instance;

        public delegate void ReplicateItem();
        public delegate void ReplicateItemWithInt(int Value);

        [HideInInspector]public InventoryEventData inventoryEventData = new InventoryEventData();
        

        public override void OnEnable()
        {
            base.OnEnable();

            CountdownTimer.OnCountdownTimerHasExpired += OnCountdownTimerIsExpired;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            CountdownTimer.OnCountdownTimerHasExpired -= OnCountdownTimerIsExpired;
        }

        private void Awake()
        {
            instance = this;

            inventoryEventData.replicateItemUsage = ReplicateItemUse;
            //replicateItemAddition = ReplicateItemAdd;
            inventoryEventData.replicateItemSwitch = ReplicateItemSwitch;
        }
        public void Start()
        {
            Hashtable props = new Hashtable
            {
                {AsteroidsGame.PLAYER_LOADED_LEVEL, true}

            };


            PhotonNetwork.LocalPlayer.SetCustomProperties(props);


            InitCharacter();
        }

        private void OnCountdownTimerIsExpired()
        {
            if(tutorial)
                tutorial.gameObject.SetActive(false);
           
            StartGame();
        }

        private void InitCharacter()
        {
            //int PlayerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
            //if (PlayerNumber < spawnPoints.Length && spawnPoints[PlayerNumber] != null)
            //{
            //    localPlayerInstance = (PhotonNetwork.Instantiate("Player", spawnPoints[PlayerNumber].transform.position, spawnPoints[PlayerNumber].transform.rotation, 0)).GetComponent<NetworkPlayer>();
            //    if (localPlayerInstance && cam)
            //    {
            //        localPlayerInstance.SetStats(new PlayerStat());
            //        cam.SetTarget(localPlayerInstance.transform);
            //    }
            //}
            InstantiateLocalPlayer();
        }

        public void InstantiateLocalPlayer()
        {
            int PlayerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
            if (PlayerNumber < spawnPoints.Length && spawnPoints[PlayerNumber] != null)
            {
                localPlayerInstance = (Instantiate(playerPrefab, spawnPoints[PlayerNumber].transform.position, spawnPoints[PlayerNumber].transform.rotation)).GetComponent<NetworkPlayer>();
                if (localPlayerInstance && cam)
                {
                    //localPlayerInstance.SetOwnership(PhotonNetwork.LocalPlayer);
                    PhotonView view = localPlayerInstance.GetComponent<PhotonView>();
                    PhotonNetwork.AllocateViewID(view);

                    localPlayerInstance.SetStats(new PlayerStat());
                    cam.SetTarget(localPlayerInstance.transform);

                    photonView.RPC("RPC_InstantiateRemotePlayer", RpcTarget.OthersBuffered, view.ViewID);
                }
            }
        }

        [PunRPC]
        public void RPC_InstantiateRemotePlayer(int viewId, PhotonMessageInfo photonMessageInfo)
        {
            int PlayerNumber = photonMessageInfo.Sender.GetPlayerNumber();
            if (PlayerNumber < spawnPoints.Length && spawnPoints[PlayerNumber] != null)
            {
                remotePlayerInstance = (Instantiate(playerPrefab, spawnPoints[PlayerNumber].transform.position, spawnPoints[PlayerNumber].transform.rotation)).GetComponent<NetworkPlayer>();
                if (remotePlayerInstance)
                {
                    PhotonView view = remotePlayerInstance.GetComponent<PhotonView>();
                    view.ViewID = viewId;
                    //remotePlayerInstance.SetOwnership(photonMessageInfo.Sender);
                    remotePlayerInstance.SetStats(new PlayerStat());
                }
            }
        }

        public void SyncPlayerSpineRotation()
        {
            photonView.RPC("RPC_SyncPlayerSpineRotation", RpcTarget.Others, localPlayerInstance.spine.rotation);
        }

        [PunRPC]
        public void RPC_SyncPlayerSpineRotation(Quaternion spineRotation)
        {
            if(remotePlayerInstance)
                remotePlayerInstance.spineNewRotation = spineRotation;
        }

        public Vector3 GetSpinePos(bool remote)
        {
            if (remote)
            {
                if (remotePlayerInstance == null || remotePlayerInstance.spine == null) return Vector3.zero;

                return remotePlayerInstance.spine.position;
            }
            else
            {
                if (localPlayerInstance == null || localPlayerInstance.spine == null) return Vector3.zero;

                return localPlayerInstance.spine.position;
            }
        }

        private void StartGame()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //float ping = PhotonNetwork.GetPing();

                photonView.RPC("RPC_StartRace", RpcTarget.AllViaServer/*, PhotonNetwork.Time*/);
            }
           // StartRace(/*PhotonNetwork.Time*/);
        }

        [PunRPC]
        private void RPC_StartRace(/*double ServerTime*/)
        {
            if (localPlayerInstance)
            {
        
                localPlayerInstance.StartRunning(/*ServerTime*/);
                //playerInstance.StartRunning(PhotonNetwork.Time - ServerTime);
            }

            if (remotePlayerInstance)
            {
                remotePlayerInstance.StartRunning();
            }
        }

        public void ReplicateItemUse()
        {
            photonView.RPC("RPC_UseItem", RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_UseItem(PhotonMessageInfo photonMessageInfo)
        {
            if (remotePlayerInstance)
            {
                float lag = (float)(photonMessageInfo.SentServerTime - PhotonNetwork.Time);
                remotePlayerInstance.UseItem(lag);
            }
        }

        

        //public void ReplicateItemAdd()
        //{
        //    photonView.RPC("RPC_AddItem", RpcTarget.Others);
        //}

        //[PunRPC]
        //private void RPC_AddItem(PhotonMessageInfo photonMessageInfo)
        //{
        //    if (remotePlayerInstance)
        //    {
        //        float lag = (float)(photonMessageInfo.SentServerTime - PhotonNetwork.Time);
        //        remotePlayerInstance.AddItem(lag);
        //    }
        //}

        public void ReplicateSpeed(float newSpeed)
        {
            photonView.RPC("RPC_SetSpeed", RpcTarget.Others, newSpeed);
        }

        [PunRPC]
        private void RPC_SetSpeed(float newSpeed, PhotonMessageInfo photonMessageInfo)
        {
            if (remotePlayerInstance)
            {
                remotePlayerInstance.SetSpeed(newSpeed);
            }
        }

        public void ReplicateItemSwitch(int inventoryIndex)
        {
            photonView.RPC("RPC_SwitchItem", RpcTarget.Others, inventoryIndex);
        }

        [PunRPC]
        private void RPC_SwitchItem(int inventoryIndex)
        {
            if (remotePlayerInstance)
            {
                remotePlayerInstance.SwitchTo(inventoryIndex);
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }

        public override void OnLeftRoom()
        {
            PhotonNetwork.Disconnect();
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
            {
                
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            //if (changedProps.ContainsKey(AsteroidsGame.PLAYER_LIVES))
            //{
            //    CheckEndOfGame();
            //    return;
            //}

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            int startTimestamp;
            bool startTimeIsSet = CountdownTimer.TryGetStartTime(out startTimestamp);

            if (changedProps.ContainsKey(AsteroidsGame.PLAYER_LOADED_LEVEL))
            {
                if (CheckAllPlayerLoadedLevel())
                {
                    if (!startTimeIsSet)
                    {
                        CountdownTimer.SetStartTime();
                    }
                }
                else
                {
                    Debug.Log("setting text waiting for players! ", this.infoText);
                    infoText.text = "Waiting for other players...";
                }
            }

        }
        private bool CheckAllPlayerLoadedLevel()
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object playerLoadedLevel;

                if (p.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_LOADED_LEVEL, out playerLoadedLevel))
                {
                    if ((bool)playerLoadedLevel)
                    {
                        continue;
                    }
                }

                return false;
            }

            return true;
        }
    }
}

