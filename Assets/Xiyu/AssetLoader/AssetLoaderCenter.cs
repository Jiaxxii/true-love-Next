using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ScriptableObjectSettings;
using UnityEngine;
using Xiyu.DataStructure;
using Xiyu.StandingIllustration;

namespace Xiyu.AssetLoader
{
    public static class AssetLoaderCenter
    {
        [JetBrains.Annotations.PublicAPI] public const string BodyOffsetSettings = "_BodyOffsetSettings";
        [JetBrains.Annotations.PublicAPI] public const string EmotionSettings = "_EmotionsSettings";

        public static async UniTask LoadingAssetsAsync(SceneAssetsSettings sceneAssetsSettings, IProgress<float> progress)
        {
            // 加载需要用到的溶解遮罩图
            await PreloadRuleMapAsync(sceneAssetsSettings.LoaderMapInfo, progress);


            foreach (var loaderCharacterInfo in sceneAssetsSettings.LoaderCharacterInfo)
            {
                // 加载角色的身体偏移信息
                await LoadingBodyOffsetSettingsAsync(loaderCharacterInfo.CharacterCode, progress);

                await LoadingEmotionsSettingsAsync(loaderCharacterInfo.CharacterCode, progress);


                // 是否加载这个角色的所有身体资源
                await PreloadBodySpriteAsync(loaderCharacterInfo, sceneAssetsSettings.BodyLabel, progress);

                // 是否加载这个角色的所有脸部资源
                await PreloadFaceSpriteAsync(loaderCharacterInfo, sceneAssetsSettings.FaceLabel, progress);
            }
        }

        public static UniTask<CharacterBodyOffsetSettings> LoadingBodyOffsetSettingsAsync(string characterCode, IProgress<float> progress)
        {
            return CharacterBodyOffsetSettingsManager.CreateAsync(characterCode, string.Concat(characterCode, BodyOffsetSettings), progress);
        }

        public static UniTask<CharacterEmotionsSettings> LoadingEmotionsSettingsAsync(string characterCode, IProgress<float> progress)
        {
            return CharacterEmotionsSettingsManager.CreateAsync(characterCode, string.Concat(characterCode, EmotionSettings), progress);
        }


        /// <summary>
        /// 预加载溶解遮罩图 （黑白色的）
        /// </summary>
        /// <param name="loaderMapInfo"></param>
        /// <param name="progress">仅加载全部遮罩时有用</param>
        /// <returns></returns>
        public static UniTask<Sprite[]> PreloadRuleMapAsync(LoaderMapInfo loaderMapInfo, IProgress<float> progress)
        {
            // 加载需要用到的溶解遮罩图
            if (loaderMapInfo.IsAllMap)
            {
                // 加载所有
                return SceneRuleMapSettings.LoadSpritesAsync(loaderMapInfo.MapLabel, progress);
            }

            // 加载指定的
            return UniTask.WhenAll(
                loaderMapInfo.LoaderMapItems.Select(x => SceneRuleMapSettings.LoadSpriteAsync(x.AddressableName)));
        }

        /// <summary>
        /// 预加载角色的身体资源
        /// </summary>
        /// <param name="loaderCharacterInfo"></param>
        /// <param name="bodyLabel"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static UniTask PreloadBodySpriteAsync(LoaderCharacterInfo loaderCharacterInfo, string bodyLabel, IProgress<float> progress)
        {
            // 是否加载这个角色的所有身体资源
            if (loaderCharacterInfo.BodySpriteInfo.AllLoader)
            {
                // 加载所有
                return StandingIllustrationLoaderManager.CreatePreloadAsync(
                    loaderCharacterInfo.CharacterCode,
                    loaderCharacterInfo.CharacterLabel,
                    bodyLabel, progress);
            }

            // 加载指定的
            var standingIllustrationLoader = StandingIllustrationLoaderManager.Create(loaderCharacterInfo.CharacterCode, progress);
            return UniTask.WhenAll(loaderCharacterInfo.BodySpriteInfo.ResourceCode.Select(x => standingIllustrationLoader.LoadSpriteAsync(x)));
        }


        /// <summary>
        /// 预加载角色的脸部资源 
        /// <para>*注意：当选项不为全部加载时 由于加载的是一组脸部资源，emotionCode 作为 key ，需要提前调用 <see cref="CharacterEmotionsSettingsManager.CreateAsync"/> 以便根据 emotionCode
        /// 来查找对应的资源地址名称</para>
        /// </summary>
        /// <param name="loaderCharacterInfo"></param>
        /// <param name="faceLabel"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static UniTask PreloadFaceSpriteAsync(LoaderCharacterInfo loaderCharacterInfo, string faceLabel, IProgress<float> progress)
        {
            if (loaderCharacterInfo.FaceSpriteInfo.AllLoader)
            {
                // 加载所有
                return StandingIllustrationLoaderManager.CreatePreloadAsync(
                    loaderCharacterInfo.CharacterCode,
                    loaderCharacterInfo.CharacterLabel,
                    faceLabel, progress);
            }

            // 加载指定的
            var standingIllustrationLoader = StandingIllustrationLoaderManager.Create(loaderCharacterInfo.CharacterCode, progress);

            var emotionsSettings = CharacterEmotionsSettingsManager.Get(loaderCharacterInfo.CharacterCode);
            var tasks = new List<UniTask>();
            foreach (var emotionCode in loaderCharacterInfo.FaceSpriteInfo.ResourceCode)
            {
                // ec = znm_001 ai-shi_001
                tasks.AddRange(emotionsSettings.GetEmotionInfoItems(emotionCode).Select(x => standingIllustrationLoader.LoadSpriteAsync(x.AddressableName).AsUniTask()));
            }

            return UniTask.WhenAll(tasks);
        }
    }
}