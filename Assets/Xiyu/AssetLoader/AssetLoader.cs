using System.Diagnostics;
using Cysharp.Threading.Tasks;
using ScriptableObjectSettings;
using Xiyu.StandingIllustration;
using Debug = UnityEngine.Debug;

namespace Xiyu.AssetLoader
{
    public static class AssetLoader
    {
        public static async UniTask LoadingAssetsAsync(SceneAssetsSettings sceneAssetsSettings)
        {
            // 加载需要用到的溶解遮罩图
            if (sceneAssetsSettings.LoaderMapInfo.IsAllMap)
            {
                // 加载所有
                await SceneRuleMapSettings.LoadSpritesAsync(sceneAssetsSettings.LoaderMapInfo.MapLabel);
            }
            else
            {
                // 加载指定的
                await UniTask.WhenAll(
                    sceneAssetsSettings.LoaderMapInfo.LoaderMapItems.Select(x => SceneRuleMapSettings.LoadSpriteAsync(x.AddressableName)));
            }

            

            foreach (var loaderCharacterInfo in sceneAssetsSettings.LoaderCharacterInfo)
            {
                // 加载角色的身体偏移信息
                await CharacterBodyOffsetSettingsManager.CreateAsync(loaderCharacterInfo.CharacterCode, $"{loaderCharacterInfo.CharacterCode}_BodyOffsetSettings", null);

                await CharacterEmotionsSettingsManager.CreateAsync(loaderCharacterInfo.CharacterCode, $"{loaderCharacterInfo.CharacterCode}_EmotionsSettings", null);


                // 是否加载这个角色的所有身体资源
                if (loaderCharacterInfo.BodySpriteInfo.AllLoader)
                {
                    // 加载所有
                    await StandingIllustrationLoaderManager.CreatePreloadAsync(
                        loaderCharacterInfo.CharacterCode,
                        loaderCharacterInfo.CharacterLabel,
                        sceneAssetsSettings.BodyLabel, null);
                }
                else
                {
                    // 加载指定的
                    var standingIllustrationLoader = StandingIllustrationLoaderManager.Create(loaderCharacterInfo.CharacterCode, null);
                    await UniTask.WhenAll(loaderCharacterInfo.BodySpriteInfo.ResourceCode.Select(x => standingIllustrationLoader.LoadSpriteAsync(x)));
                }

                if (loaderCharacterInfo.FaceSpriteInfo.AllLoader)
                {
                    // 加载所有
                    await StandingIllustrationLoaderManager.CreatePreloadAsync(
                        loaderCharacterInfo.CharacterCode,
                        loaderCharacterInfo.CharacterLabel,
                        sceneAssetsSettings.FaceLabel, null);
                }
                else
                {
                    // 加载指定的
                    var standingIllustrationLoader = StandingIllustrationLoaderManager.Create(loaderCharacterInfo.CharacterCode, null);

                    var emotionsSettings = CharacterEmotionsSettingsManager.Get(loaderCharacterInfo.CharacterCode);
                    foreach (var emotionCode in loaderCharacterInfo.FaceSpriteInfo.ResourceCode)
                    {
                        // ec = znm_001 ai-shi_001
                        foreach (var emotionInfoItem in emotionsSettings.GetEmotionInfoItems(emotionCode))
                        {
                            await standingIllustrationLoader.LoadSpriteAsync(emotionInfoItem.AddressableName);
                        }
                    }
                }
            }
        }
    }
}