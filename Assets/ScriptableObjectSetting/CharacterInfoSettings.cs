using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ScriptableObjectSetting
{
    public class CharacterInfoSettings<TValue> : ScriptableObject
    {
        [Serializable]
        private class InfoSettings
        {
            [SerializeField] public string characterCode;
            [SerializeField] public TextAsset offsetFile;
        }

        [SerializeField] private string assetName;
        [SerializeField] private InfoSettings[] data;


        private static Dictionary<string, TValue> _dictionary;
        public static IReadOnlyDictionary<string, TValue> ReadOnlyDictionary => _dictionary;


        public static async UniTask LoadInitSettingsAsync(string assetName)
        {
            var resourceRequest = Resources.LoadAsync<CharacterInfoSettings<TValue>>($"Settings/{assetName}");
            await resourceRequest;

            var data = ((CharacterInfoSettings<TValue>)resourceRequest.asset).data;

            _dictionary = new Dictionary<string, TValue>(data.Length);
            foreach (var item in data)
            {
                var lightweightRectTransform = JsonConvert.DeserializeObject<TValue>(item.offsetFile.text);
                _dictionary.Add(item.characterCode, lightweightRectTransform);
            }

            Resources.UnloadAsset(resourceRequest.asset);
            var asyncOperation = Resources.UnloadUnusedAssets();
            while (!asyncOperation.isDone)
            {
                await UniTask.NextFrame();
            }
        }
    }
}