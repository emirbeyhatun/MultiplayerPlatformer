using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlatformerGame
{
    public class UiManager : MonoBehaviour
    {
        public static UiManager instance;
        public GameObject itemButtonsParent;
        public List<ItemButton> itemButtons;
        public Button useItemButton;
        public Button jumpButton;
        private void Awake()
        {
            instance = this;
        }

        internal void EnableControls(bool enable, PhotonView photonView)
        {
            if (photonView.IsMine == false) return;


            if (itemButtonsParent)
            {
                itemButtonsParent.gameObject.SetActive(enable);
            }

            if (useItemButton)
            {
                useItemButton.gameObject.SetActive(enable);
            }
            if (jumpButton)
            {
                jumpButton.gameObject.SetActive(enable);
            }
        }
    }
}