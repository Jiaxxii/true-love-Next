using ScriptableObjectSettings;
using UnityEngine;
using Xiyu.AssetLoader;
using Xiyu.DataStructure;
using Xiyu.GlobalEffect;
using Xiyu.StandingIllustration;

namespace Xiyu.Menu
{
    public sealed class MenuGameGuide : MonoBehaviour
    {
        [SerializeField] private SceneAssetsSettings menuSceneAssetsSettings;

        private async void Start()
        {
            GlobalEffectManger.Instance.TopAlpha = 1;

            // 等待本场景的所有资源加载完成
            await GlobalEffectManger.Instance.LoaderEffect.LoadingWaitForUniTask(
                AssetLoader.AssetLoader.LoadingAssetsAsync(menuSceneAssetsSettings));

            GlobalEffectManger.Instance.MaskEffect.Alpha = 0;
            GlobalEffectManger.Instance.TopAlpha = 0;

            // var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            // await GlobalEffectManger.Instance.LoaderEffect.FadeOut(1);


            var character = await Character.CreateAsync("znm", GameObject.Find("Panel").transform, true, false);


            var standingIllustrationLoader = StandingIllustrationLoaderManager.Get("znm");


            var bodyOffsetSettings = CharacterBodyOffsetSettingsManager.Get("znm");

            var bodySprite = await standingIllustrationLoader.LoadSpriteAsync("znm_a_0_1465");
            var bodyLrt = bodyOffsetSettings.GetBodyOffset("znm_a_0_1465");

            character.UpdateBody(bodySprite, bodyLrt);


            var emotionsSettings = CharacterEmotionsSettingsManager.Get("znm");

            var emotionInfoItems = emotionsSettings.GetEmotionInfoItems("znm_001");
            var faceData = new (Sprite, LightweightRectTransform)[emotionInfoItems.Length];


            for (var i = 0; i < emotionInfoItems.Length; i++)
            {
                var sprite = await standingIllustrationLoader.LoadSpriteAsync(emotionInfoItems[i].AddressableName);
                faceData[i] = (sprite, emotionInfoItems[i].Transform);
            }


            character.UpdateFace(faceData);
        }
    }
}