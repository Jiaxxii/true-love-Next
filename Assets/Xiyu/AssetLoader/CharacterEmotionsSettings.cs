using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Xiyu.DataStructure;

namespace Xiyu.AssetLoader
{
    public sealed class CharacterEmotionsSettings
    {
        private ConcurrentDictionary<string, EmotionInfoItem[]> _buffer = new();

        /// <summary>
        /// 原始资源名称 xx_EmotionsSettings
        /// </summary>
        public string AddressableName { get; private set; }
        public string CharacterCode { get; private set; }


        private CharacterEmotionsSettings(string characterCode, string addressableName)
        {
            CharacterCode = characterCode;
            AddressableName = addressableName;
        }

        public EmotionInfoItem[] GetEmotionInfoItems(string emotionCode)
        {
            if (_buffer.TryGetValue(emotionCode, out var emotionInfoItems))
            {
                return emotionInfoItems;
            }

            throw new KeyNotFoundException($"Addressable资源\"{AddressableName}\"中不存在\"{emotionCode}\"！");
        }

        public void Clear()
        {
            _buffer.Clear();
        }

        private static ConcurrentDictionary<string, EmotionInfoItem[]> LoadEmotionsMap(string addressableName, string text)
        {
            if (string.IsNullOrEmpty(text) || !text.StartsWith('{') || !text.EndsWith('}'))
            {
                throw new JsonSerializationException($"Addressable资源\"{addressableName}\"不是Json格式！");
            }

            var emotionInfoItemsMap = JsonConvert.DeserializeObject<Dictionary<string, EmotionInfoItem[]>>(text);

            var keyValuePairs = emotionInfoItemsMap.Where(x => x.Value.Any(EmotionInfoItem.IsNone)).ToArray();
            if (keyValuePairs.Length != 0)
            {
                throw new ArgumentException(
                    $"Addressable资源\"{addressableName}\"中的对象[{string.Join(',', keyValuePairs.Select(x => x.Key))}]信息中必要信息{nameof(EmotionInfoItem.AddressableName)}为空！");
            }

            return new ConcurrentDictionary<string, EmotionInfoItem[]>(emotionInfoItemsMap);
        }


        public static async UniTask<CharacterEmotionsSettings> CreateAsync(string characterCode, string addressableName, IProgress<float> progress)
        {
            var characterEmotionsSettings = new CharacterEmotionsSettings(characterCode, addressableName);

            var asyncOperationHandle = Addressables.LoadAssetAsync<TextAsset>(addressableName);

            characterEmotionsSettings._buffer = LoadEmotionsMap(addressableName, (await asyncOperationHandle.ToUniTask(progress)).text);

            Addressables.Release(asyncOperationHandle);

            return characterEmotionsSettings;
        }
    }
}