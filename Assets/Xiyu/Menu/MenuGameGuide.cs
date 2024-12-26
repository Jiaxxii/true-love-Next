using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ScriptableObjectSettings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Xiyu.AssetLoader;
using Xiyu.DataStructure;
using Xiyu.GlobalEffect;
using Xiyu.StandingIllustration;

namespace Xiyu.Menu
{
    public sealed class MenuGameGuide : MonoBehaviour
    {
        [SerializeField] private Transform panel;

        [SerializeField] private AssetLabelReference defaultCharacterLabelReference;
        [SerializeField] private AssetLabelReference defaultGuideLabelReference;
        [SerializeField] private SceneAssetsSettings menuSceneAssetsSettings;

        [SerializeField] private float duration = 0.5f;


        // ReSharper disable RedundantNameQualifier
        [SerializeField] private Xiyu.Menu.Button firstEncounterButton;
        [SerializeField] private Xiyu.Menu.Button atThisTimeButton;
        [SerializeField] private Xiyu.Menu.Button atThatTimeButton;
        [SerializeField] private Xiyu.Menu.Button examinationButton;
        [SerializeField] private Xiyu.Menu.Button conclusionButton;
        // ReSharper restore RedundantNameQualifier

        private Character _currentCharacter;
        private string _lastEmotionCode;


        private void Awake()
        {
            firstEncounterButton.OnPointerClickAction += UniTask.UnityAction<PointerEventData>(_ => LoadSelectScene());
        }

        private async void Start()
        {
            var globalEffectManger = GlobalEffectManger.Instance;

            globalEffectManger.AllDefault();

            // 打开加载界面
            globalEffectManger.LoaderEffect.Loading();

            // 由于菜单场景需要角色引导信息这个是动态的，我们需要实时解析
            var guideInfo = await CharacterMenuGuideManager.LoadCharacterMenuGuideInfoAsync(defaultGuideLabelReference.labelString, defaultCharacterLabelReference.labelString);


            // 加载角色立绘偏移信息与表情信息
            var bodyOffsetSettings = await AssetLoaderCenter.LoadingBodyOffsetSettingsAsync(guideInfo.CharacterCode, null);
            var emotionsSettings = await AssetLoaderCenter.LoadingEmotionsSettingsAsync(guideInfo.CharacterCode, null);

            var faceInfo = new LoaderSpriteInfo(guideInfo.OptionSelected.GetSelectedOptions(guideInfo.EmotionCode));
            var bodyInfo = new LoaderSpriteInfo(guideInfo.BodyCode);

            var loaderCharacterInfo = new LoaderCharacterInfo(guideInfo.CharacterCode, string.Empty, bodyInfo, faceInfo);


            await AssetLoaderCenter.PreloadRuleMapAsync(menuSceneAssetsSettings.LoaderMapInfo, null);
            await AssetLoaderCenter.PreloadBodySpriteAsync(loaderCharacterInfo, string.Empty, null);
            await AssetLoaderCenter.PreloadFaceSpriteAsync(loaderCharacterInfo, string.Empty, null);


            _currentCharacter = await Character.CreateAsync(guideInfo.CharacterCode, panel, true, false);
            _currentCharacter.SetAnchoredPosition(guideInfo.Transform.Position);

            var standingIllustrationLoader = StandingIllustrationLoaderManager.Get(guideInfo.CharacterCode);

            // 更新身体
            await _currentCharacter.UpdateBodyAsync(standingIllustrationLoader, bodyOffsetSettings, guideInfo.BodyCode);

            // 更新表情
            await _currentCharacter.UpdateFaceAsync(standingIllustrationLoader, emotionsSettings, guideInfo.EmotionCode);

            // 注册事件
            RegisterEvents(guideInfo.EmotionCode, guideInfo.OptionSelected);

            // 设置溶解
            var dissolveMap = await SceneRuleMapSettings.LoadSpriteAsync(menuSceneAssetsSettings.LoaderMapInfo.LoaderMapItems.First().AddressableName);
            globalEffectManger.MaskEffect.SetRuleSprite(dissolveMap);

            // 结束加载界面
            await globalEffectManger.LoaderEffect.TryEndLoading();
            await UniTask.WaitForSeconds(0.5F);
            globalEffectManger.LoaderEffect.SetActive(false);

            await globalEffectManger.MaskEffect.RuleFadeIn(1, ease: Ease.OutQuad);
            globalEffectManger.SetTopActive(false);
            globalEffectManger.MaskEffect.SetRuleSprite(null);
        }


        private async UniTaskVoid LoadSelectScene()
        {
            var globalEffectManger = GlobalEffectManger.Instance;
            globalEffectManger.AllDefault();


            globalEffectManger.MaskEffect.Alpha = 0;
            await globalEffectManger.MaskEffect.FadeIn(2);

            globalEffectManger.LoaderEffect.Loading();
            globalEffectManger.MaskEffect.Alpha = 0;

            // 释放资源
            Addressables.ReleaseInstance(_currentCharacter.gameObject);
            await Resources.UnloadUnusedAssets().ToUniTask();

            var loadSceneAsync = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);

            if (loadSceneAsync == null)
            {
                throw new NullReferenceException($"场景\"{SceneManager.GetActiveScene().name}\"没有下一个场景，场景编号为{SceneManager.GetActiveScene().buildIndex + 1}是不存在的！");
            }

            loadSceneAsync.allowSceneActivation = false;

            await UniTask.WaitUntil(() => loadSceneAsync.progress >= 0.9f);

            await globalEffectManger.LoaderEffect.TryEndLoading();
            loadSceneAsync.allowSceneActivation = true;
        }

        private void RegisterEvents(string rawEmotionCode, LoaderOptionSelectedInfo optionSelectedInfo)
        {
            if (!string.IsNullOrEmpty(optionSelectedInfo.FirstEncounter) && !optionSelectedInfo.FirstEncounter.StartsWith("未定义-"))
            {
                firstEncounterButton.OnPointerEnterAction += UniTask.UnityAction<PointerEventData>(_ => OnAnimationEnter(optionSelectedInfo.FirstEncounter));
            }

            if (!string.IsNullOrEmpty(optionSelectedInfo.AtThisTime) && !optionSelectedInfo.AtThisTime.StartsWith("未定义-"))
            {
                atThisTimeButton.OnPointerEnterAction += UniTask.UnityAction<PointerEventData>(_ => OnAnimationEnter(optionSelectedInfo.AtThisTime));
            }

            if (!string.IsNullOrEmpty(optionSelectedInfo.AtThatTime) && !optionSelectedInfo.AtThatTime.StartsWith("未定义-"))
            {
                atThatTimeButton.OnPointerEnterAction += UniTask.UnityAction<PointerEventData>(_ => OnAnimationEnter(optionSelectedInfo.AtThatTime));
            }

            if (!string.IsNullOrEmpty(optionSelectedInfo.Examination) && !optionSelectedInfo.Examination.StartsWith("未定义-"))
            {
                examinationButton.OnPointerEnterAction += UniTask.UnityAction<PointerEventData>(_ => OnAnimationEnter(optionSelectedInfo.Examination));
            }

            if (!string.IsNullOrEmpty(optionSelectedInfo.Conclusion) && !optionSelectedInfo.Conclusion.StartsWith("未定义-"))
            {
                conclusionButton.OnPointerEnterAction += UniTask.UnityAction<PointerEventData>(_ => OnConclusionAnimationEnter(optionSelectedInfo.Conclusion));
                conclusionButton.OnPointerExitAction += UniTask.UnityAction<PointerEventData>(_ => OnConclusionAnimationExit(rawEmotionCode));
            }
        }

        private async UniTaskVoid OnConclusionAnimationEnter(string code)
        {
            _lastEmotionCode = code;
            await _currentCharacter.UpdateFaceFadeAsync(StandingIllustrationLoaderManager.Get(_currentCharacter.CharacterCode),
                CharacterEmotionsSettingsManager.Get(_currentCharacter.CharacterCode), code, duration);
        }

        private async UniTaskVoid OnConclusionAnimationExit(string rawEmotionCode)
        {
            await _currentCharacter.UpdateFaceFadeAsync(StandingIllustrationLoaderManager.Get(_currentCharacter.CharacterCode),
                CharacterEmotionsSettingsManager.Get(_currentCharacter.CharacterCode), rawEmotionCode, duration);
        }

        private async UniTaskVoid OnAnimationEnter(string code)
        {
            if (code == _lastEmotionCode)
            {
                return;
            }

            _lastEmotionCode = code;
            await _currentCharacter.UpdateFaceFadeAsync(StandingIllustrationLoaderManager.Get(_currentCharacter.CharacterCode),
                CharacterEmotionsSettingsManager.Get(_currentCharacter.CharacterCode), code, duration);
        }
    }
}