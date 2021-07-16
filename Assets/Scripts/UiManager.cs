using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlatformerGame
{
    public class UiManager : MonoBehaviour
    {
        public static UiManager instance;
        public List<ItemButton> itemButtons;
        public Button useItemButton;
        public Button jumpButton;
        private void Awake()
        {
            instance = this;
        }
    }
}