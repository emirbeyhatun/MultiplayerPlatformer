using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlatformerGame
{
    public class PlayerIndicator : MonoBehaviour
    {
        [SerializeField] private Image bar;
        [SerializeField] private Image localPlayerIndicator;
        [SerializeField] private NetworkPlayer player;
        [SerializeField] private Text nameTag;
        private Camera cam;


        private void Start()
        {
            cam = Camera.main;
            if (player && localPlayerIndicator)
            {
                if (player.photonView.IsMine == true)
                {
                    localPlayerIndicator.color = Color.green;
                }
                else
                {
                    localPlayerIndicator.color = Color.red;
                }
            }

            if(nameTag && player && player.photonView.Owner != null)
            {
                nameTag.text = player.photonView.Owner.NickName;
            }
        }

        private void FixedUpdate()
        {
            if (cam)
            {
                transform.transform.LookAt(cam.transform);
            }
        }

        public void SetPercentage(float value)
        {
            float baseValue = PlayerStat.maxSpeed - PlayerStat.minSpeed;
            if(baseValue > 0 && bar)
            {
                float topValue = Mathf.Max(value - PlayerStat.minSpeed, 0);

                bar.fillAmount = topValue / value;
            }
        }
    }
}