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
    public class CharacterBodyOffsetSettings
    {
        private ConcurrentDictionary<string, LightweightRectTransform> _buffer = new();
        public string AddressableName { get; private set; }
        public string CharacterCode { get; private set; }

        private CharacterBodyOffsetSettings(string characterCode, string addressableName)
        {
            CharacterCode = characterCode;
            AddressableName = addressableName;
        }


        public LightweightRectTransform GetBodyOffset(string bodyName)
        {
            if (_buffer.TryGetValue(bodyName, out var lrt))
            {
                return lrt;
            }


            throw new KeyNotFoundException($"Addressable资源\"{AddressableName}\"中不存在\"{bodyName}\"！");
        }

        public void Clear()
        {
            _buffer.Clear();
        }

        private static ConcurrentDictionary<string, LightweightRectTransform> LoadBodyOffsetMap(string addressableName, string text)
        {
            if (string.IsNullOrEmpty(text) || !text.StartsWith('{') || !text.EndsWith('}'))
            {
                throw new JsonSerializationException($"Addressable资源\"{addressableName}\"不是Json格式！");
            }

            var lrtMap = JsonConvert.DeserializeObject<Dictionary<string, LightweightRectTransform>>(text);
            var keyValuePairs = lrtMap.Where(x => LightweightRectTransform.IsZero(x.Value)).ToArray();
            if (keyValuePairs.Length != 0)
            {
                throw new ArgumentException(
                    $"Addressable资源\"{addressableName}\"中的对象[{string.Join(',', keyValuePairs.Select(x => x.Key))}]信息中所有的属性均为0！");
            }

            return new ConcurrentDictionary<string, LightweightRectTransform>(lrtMap);
        }


        public static async UniTask<CharacterBodyOffsetSettings> CreateAsync(string characterCode, string addressableName, IProgress<float> progress)
        {
            var characterBodyOffsetSettings = new CharacterBodyOffsetSettings(characterCode, addressableName);

            var asyncOperationHandle = Addressables.LoadAssetAsync<TextAsset>(addressableName);

            characterBodyOffsetSettings._buffer = LoadBodyOffsetMap(addressableName, (await asyncOperationHandle.ToUniTask(progress)).text);

            Addressables.Release(asyncOperationHandle);

            return characterBodyOffsetSettings;
        }
    }
}