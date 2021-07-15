using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlatformerGame
{
    public class ItemButton : MonoBehaviour
    {
        [HideInInspector] public Button button;
        public Text nameText;
        public Text amountText;
        const string emptyText = "Empty";
        const string zeroText = "0";
        public int itemID;

        public delegate void onButtonClickedWithParameter(int Id);
        public onButtonClickedWithParameter onClicked;

        void Awake()
        {
            itemID = -1;
            button = GetComponent<Button>();
            button.interactable = false;

            button.onClick.AddListener(OnButtonClicked);
        }

        internal void PrepareButton(ItemData data)
        {
            nameText.text = data.name;
            amountText.text = data.totalUsage.ToString();
            itemID = data.itemID;
            button.interactable = true;
        }

        public void ResetButton()
        {
            nameText.text = emptyText;
            amountText.text = zeroText;
            itemID = -1;
            button.interactable = false;
        }

        void OnButtonClicked()
        {
            onClicked?.Invoke(itemID);
        }

        internal void SetColor(bool isSelected)
        {
            if (isSelected)
            {
                button.GetComponent<Image>().color = Color.green;
            }
            else
            {
                button.GetComponent<Image>().color = Color.white;
            }
        }
    }
}