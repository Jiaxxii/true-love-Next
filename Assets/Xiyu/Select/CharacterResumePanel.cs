using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Xiyu.DataStructure;

namespace Xiyu.Select
{
    public class CharacterResumePanel : MonoBehaviour
    {
        [Header("引用")] [SerializeField] private RectTransform panel;
        [SerializeField] private TextMeshProUGUI resumeText;
        [SerializeField] private TextMeshProUGUI nameText;

        [Header("赋值")] [SerializeField] private float popUpEndPosY = 290;
        [SerializeField] private float popUpDuration = 0.5f;

        private bool _isRunning;


        private void Start()
        {
            nameText.alpha = 0;
            
            nameText.fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0.1F);
            resumeText.fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0.35F);
        }

        public void PopUpAsync(ref LoaderSelectShowCharacterInfo characterInfo)
        {
            resumeText.text = $"{characterInfo.Content}\n攻略难度：{LevelToText(characterInfo.DifficultyLevel)}";

            if (_isRunning)
            {
                return;
            }

            _isRunning = true;

            panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, -100);
            nameText.alpha = 0;

            nameText.text = characterInfo.CharacterName;

            nameText.DOFade(0.9F, popUpDuration).SetEase(Ease.Unset);
            panel.DOAnchorPosY(popUpEndPosY, popUpDuration).SetEase(Ease.OutBounce).OnComplete(() => _isRunning = false);
        }

        private static string LevelToText(int level) => level switch
        {
            <= 0 => "倒追",
            1 => "一场甜甜的恋爱",
            2 => "略有波折的恋爱",
            3 => "坎坷",
            4 => "殊途同归",
            5 => "同途异归",
            _ => "<color=red>病娇</color>，爱的不是你！"
        };
    }
}