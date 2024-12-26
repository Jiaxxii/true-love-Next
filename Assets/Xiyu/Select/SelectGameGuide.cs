using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ScriptableObjectSettings;
using UnityEngine;
using Xiyu.AssetLoader;
using Xiyu.DataStructure;

namespace Xiyu.Select
{
    public class SelectGameGuide : MonoBehaviour
    {
        [SerializeField] private SceneAssetsSettings sceneAssetsSettings;

        [SerializeField] private ButtonSelectPanel buttonSelectPanel;
        [SerializeField] private CharacterSelectPanel characterSelectPanel;
        [SerializeField] private CharacterResumePanel characterResumePanel;
        [SerializeField] private ButtonSelectionComplete buttonSelectionComplete;

        private async void Start()
        {
            var globalEffectManger = GlobalEffect.GlobalEffectManger.Instance;

            // 重置全局特效
            globalEffectManger.AllDefault();

            // 隐藏顶层特效
            globalEffectManger.MaskEffect.Alpha = 0;
            // 弹出加载
            globalEffectManger.LoaderEffect.Loading();

            // 预加载本创建需要的资源
            await AssetLoaderCenter.LoadingAssetsAsync(sceneAssetsSettings, null);


            var characterCodes = new HashSet<string>(sceneAssetsSettings.LoaderCharacterInfo.Select(x => x.CharacterCode));

            buttonSelectPanel.LoadHeadIconAsync(characterCodes);

            buttonSelectPanel.OnSelectCharacter += x => OnSelectCharacterEventHandler(x).Forget();

            characterSelectPanel.OnCharacterPopUp += characterInfo =>
            {
                characterResumePanel.PopUpAsync(ref characterInfo);
                buttonSelectionComplete.PopUp(characterInfo.Use);
            };


            await characterSelectPanel.Init(sceneAssetsSettings);

            // 刷新头像
            buttonSelectPanel.RefreshTopToDownAnimation().Forget();

            await globalEffectManger.LoaderEffect.TryEndLoading();

            await UniTask.WaitForSeconds(0.5F);

            await globalEffectManger.LoaderEffect.FadeOut(buttonSelectPanel.RefreshDuration);
            globalEffectManger.SetTopActive(false);
        }

        private async UniTaskVoid OnSelectCharacterEventHandler((bool isUndefined, string characterCode) selectCharacter)
        {
            if (!selectCharacter.isUndefined)
            {
                // 如果 selectCharacter.isUndefined 为 false 那么这个角色可能只有头像没有立绘等资源
                return;
            }

            await characterSelectPanel.PopUpAsync(selectCharacter.characterCode);
        }
    }
}