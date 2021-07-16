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
        public int usageAmount;
        const string emptyText = "Empty";
        const string zeroText = "0";
        public int itemID;
        public int inventoryIndex;

        public delegate void onButtonClickedWithParameter(int Id);
        public onButtonClickedWithParameter onClicked;

        void Awake()
        {
            itemID = -1;
            inventoryIndex = -1;
            button = GetComponent<Button>();
            button.interactable = false;

            button.onClick.AddListener(OnButtonClicked);
        }

        internal void PrepareButton(ItemData data, int itemIndex)
        {
            nameText.text = data.itemName;
            SetUsageAmount(data.totalUsage);
            itemID = data.itemID;
            button.interactable = true;
            inventoryIndex = itemIndex;
        }

        public void ResetButton()
        {

            nameText.text = emptyText;
            amountText.text = zeroText;
            usageAmount = 0;
            itemID = -1;
            inventoryIndex = -1;
            button.interactable = false;
            SetColor(false);
        }

        void OnButtonClicked()
        {
            onClicked?.Invoke(inventoryIndex);
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

        internal void SetUsageAmount(int totalUsage)
        {
            if (totalUsage == usageAmount) return;

            amountText.text = totalUsage.ToString();
            usageAmount = totalUsage;
        }
    }
}