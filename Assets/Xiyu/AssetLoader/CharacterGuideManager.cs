using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Xiyu.DataStructure;

namespace Xiyu.AssetLoader
{
    public static class CharacterGuideManager
    {
        private static readonly string CharacterGuideFullName = Path.Combine(Application.persistentDataPath, "CharacterGuide.json");
        private const string DefaultGuideAddressableName = "DefaultGuideCharacterSettings";


        /// <summary>
        /// 尝试从持久化路径(<see cref="CharacterGuideFullName"/>)加载角色引导信息，如果路径不存在或者序列化失败则从 <see cref="Addressables"/> 中加载默认引导信息(名称：<see cref="DefaultGuideAddressableName"/>)。
        /// </summary>
        /// <returns>不会返回 <see cref="LoaderCharacterGuideInfo"/>.<see cref="LoaderCharacterGuideInfo.None"/> 无需进行判断</returns>
        /// <exception cref="JsonSerializationException">从 <see cref="Addressables"/> 中加载资源尝试序列化失败（可能此文件受损）</exception>
        public static async UniTask<LoaderCharacterGuideInfo> LoadCharacterGuideInfoAsync()
        {
            var fileInfo = new FileInfo(CharacterGuideFullName);

            if (fileInfo.Directory!.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            if (fileInfo.Exists)
            {
                var readAllTextAsync = await File.ReadAllTextAsync(fileInfo.FullName);
                if (TryParseCharacterGuideInfo(readAllTextAsync, out var loaderCharacterGuideInfo))
                {
                    return loaderCharacterGuideInfo;
                }
            }

            var asyncOperationHandle = Addressables.LoadAssetAsync<TextAsset>(DefaultGuideAddressableName);
            await asyncOperationHandle.ToUniTask();
            try
            {
                await File.WriteAllTextAsync(CharacterGuideFullName, asyncOperationHandle.Result.text);
                if (TryParseCharacterGuideInfo(asyncOperationHandle.Result.text, out var loaderCharacterGuideInfo))
                {
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


        /// <summary>
        /// 将角色引导信息保存到持久化路径(<see cref="CharacterGuideFullName"/>)
        /// </summary>
        /// <param name="loaderCharacterGuideInfo">角色引导信息文件</param>
        /// <returns>文件可能存储失败</returns>
        public static async UniTask<bool> SaveCharacterGuideInfoAsync(LoaderCharacterGuideInfo loaderCharacterGuideInfo)
        {
            var fileInfo = new FileInfo(CharacterGuideFullName);

            if (fileInfo.Directory!.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            try
            {
                await File.WriteAllTextAsync(CharacterGuideFullName, JsonConvert.SerializeObject(loaderCharacterGuideInfo));

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"保存角色引导信息失败！(\"{CharacterGuideFullName}\")\n{e}");
            }

            return false;
        }
 

        private static bool TryParseCharacterGuideInfo(string json, out LoaderCharacterGuideInfo loaderCharacterGuideInfo)
        {
            loaderCharacterGuideInfo = LoaderCharacterGuideInfo.None;
            if (!json.StartsWith('{') || !json.EndsWith('}'))
            {
                return false;
            }

            try
            {
                var characterGuideInfo = JsonConvert.DeserializeObject<LoaderCharacterGuideInfo>(json);

                if (LoaderCharacterGuideInfo.IsNone(characterGuideInfo))
                {
                    return false;
                }

                loaderCharacterGuideInfo = characterGuideInfo;
                return true;
            }
            catch (JsonSerializationException)
            {
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"解析角色引导信息失败！(\"{CharacterGuideFullName}\")\n{e}");
            }

            return false;
        }
    }
}