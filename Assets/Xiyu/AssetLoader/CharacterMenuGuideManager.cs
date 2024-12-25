using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Xiyu.DataStructure;

namespace Xiyu.AssetLoader
{
    public static class CharacterMenuGuideManager
    {
        private static readonly string CharacterGuideFullName = Path.Combine(Application.persistentDataPath, "CharacterGuide.json");
        private const string DefaultGuideAddressableName = "DefaultGuideCharacterSettings";


        private static readonly ConcurrentDictionary<string, LoaderCharacterMenuGuideInfo> CharacterMenuGuideInfoDict = new();

        private static readonly ConcurrentDictionary<string, string> CharacterLabelToAddressableNameDict = new();


        public static async UniTask<LoaderCharacterMenuGuideInfo> LoadCharacterGuideInfoAsync(string addressableName)
        {
            if (CharacterMenuGuideInfoDict.TryGetValue(addressableName, out var loaderCharacterGuideInfo))
            {
                return loaderCharacterGuideInfo;
            }

            var asyncOperationHandle = Addressables.LoadAssetAsync<TextAsset>(addressableName);
            await asyncOperationHandle.ToUniTask();
            try
            {
                if (TryParseCharacterMenuGuideInfo(asyncOperationHandle.Result.text, out loaderCharacterGuideInfo))
                {
                    CharacterMenuGuideInfoDict[addressableName] = loaderCharacterGuideInfo;
                    CharacterMenuGuideInfoDict.TryAdd(addressableName, loaderCharacterGuideInfo);
                    return loaderCharacterGuideInfo;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"加载角色引导信息失败！(\"{CharacterGuideFullName}\")\n{e}");
            }
            finally
            {
                Addressables.Release(asyncOperationHandle);
            }

            throw new JsonSerializationException($"默认引导文件({DefaultGuideAddressableName})序列化失败！请检查文件！");
        }

        public static async UniTask<LoaderCharacterMenuGuideInfo> LoadCharacterGuideInfoAsync(string guideLabel, string characterLabel)
        {
            var handle = Addressables.LoadResourceLocationsAsync(new List<string> { guideLabel, characterLabel }, Addressables.MergeMode.Intersection, typeof(TextAsset));
            await handle.ToUniTask();

            var resourceLocation = handle.Result.First();

            CharacterLabelToAddressableNameDict.TryAdd(characterLabel, resourceLocation.PrimaryKey);


            Addressables.Release(handle);
            return await LoadCharacterGuideInfoAsync(resourceLocation.PrimaryKey);
        }

        /// <summary>
        /// 尝试从持久化路径(<see cref="CharacterGuideFullName"/>)加载角色引导信息，如果路径不存在或者序列化失败则从 <see cref="Addressables"/> 中加载默认引导信息(名称：<see cref="DefaultGuideAddressableName"/>)。
        /// </summary>
        /// <param name="guideLabel"></param>
        /// <param name="characterLabel"></param>
        /// <returns>不会返回 <see cref="LoaderCharacterMenuGuideInfo"/>.<see cref="LoaderCharacterMenuGuideInfo.None"/> 无需进行判断</returns>
        /// <exception cref="JsonSerializationException">从 <see cref="Addressables"/> 中加载资源尝试序列化失败（可能此文件受损）</exception>
        public static async UniTask<LoaderCharacterMenuGuideInfo> LoadCharacterMenuGuideInfoAsync(string guideLabel, string characterLabel)
        {
            var fileInfo = new FileInfo(CharacterGuideFullName);

            if (fileInfo.Directory!.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            // ReSharper disable once InvertIf
            if (fileInfo.Exists)
            {
                var readAllTextAsync = await File.ReadAllTextAsync(fileInfo.FullName);
                if (TryParseCharacterMenuGuideInfo(readAllTextAsync, out var loaderCharacterGuideInfo))
                {
                    return loaderCharacterGuideInfo;
                }
            }

            return await LoadCharacterGuideInfoAsync(guideLabel, characterLabel);
        }

        /// <summary>
        /// 尝试从持久化路径(<see cref="CharacterGuideFullName"/>)加载角色引导信息，如果路径不存在或者序列化失败则从 <see cref="Addressables"/> 中加载默认引导信息(名称：<see cref="DefaultGuideAddressableName"/>)。
        /// </summary>
        /// <param name="addressableName"></param>
        /// <returns>不会返回 <see cref="LoaderCharacterMenuGuideInfo"/>.<see cref="LoaderCharacterMenuGuideInfo.None"/> 无需进行判断</returns>
        /// <exception cref="JsonSerializationException">从 <see cref="Addressables"/> 中加载资源尝试序列化失败（可能此文件受损）</exception>
        public static async UniTask<LoaderCharacterMenuGuideInfo> LoadCharacterMenuGuideInfoAsync(string addressableName)
        {
            var fileInfo = new FileInfo(CharacterGuideFullName);

            if (fileInfo.Directory!.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            // ReSharper disable once InvertIf
            if (fileInfo.Exists)
            {
                var readAllTextAsync = await File.ReadAllTextAsync(fileInfo.FullName);
                if (TryParseCharacterMenuGuideInfo(readAllTextAsync, out var loaderCharacterGuideInfo))
                {
                    return loaderCharacterGuideInfo;
                }
            }

            return await LoadCharacterGuideInfoAsync(addressableName);
        }


        /// <summary>
        /// 将角色引导信息保存到持久化路径(<see cref="CharacterGuideFullName"/>)
        /// </summary>
        /// <param name="loaderCharacterMenuGuideInfo">角色引导信息文件</param>
        /// <returns>文件可能存储失败</returns>
        public static async UniTask<bool> SaveCharacterMenuGuideInfoAsync(LoaderCharacterMenuGuideInfo loaderCharacterMenuGuideInfo)
        {
            var fileInfo = new FileInfo(CharacterGuideFullName);

            if (fileInfo.Directory!.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            try
            {
                await File.WriteAllTextAsync(CharacterGuideFullName, JsonConvert.SerializeObject(loaderCharacterMenuGuideInfo));

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"保存角色引导信息失败！(\"{CharacterGuideFullName}\")\n{e}");
            }

            return false;
        }


        private static bool TryParseCharacterMenuGuideInfo(string json, out LoaderCharacterMenuGuideInfo loaderCharacterMenuGuideInfo)
        {
            loaderCharacterMenuGuideInfo = LoaderCharacterMenuGuideInfo.None;
            if (!json.StartsWith('{') || !json.EndsWith('}'))
            {
                return false;
            }

            try
            {
                var characterGuideInfo = JsonConvert.DeserializeObject<LoaderCharacterMenuGuideInfo>(json);

                if (LoaderCharacterMenuGuideInfo.IsNone(characterGuideInfo))
                {
                    return false;
                }

                loaderCharacterMenuGuideInfo = characterGuideInfo;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"解析角色引导信息失败！(\"{CharacterGuideFullName}\")\n{e}");
            }

            return false;
        }
    }
}