using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Xiyu.AssetLoader
{
    public static class CharacterBodyOffsetSettingsManager
    {
        private static readonly ConcurrentDictionary<string, CharacterBodyOffsetSettings> BufferMap = new();

        public static async UniTask<CharacterBodyOffsetSettings> CreateAsync(string characterCode, string addressableName, IProgress<float> progress)
        {
            if (BufferMap.TryGetValue(characterCode, out var value))
            {
                Debug.LogWarning($"\"{addressableName}\"立绘偏移信息已经创建，本次加载将直接返回实例对象引用！");
                return value;
            }

            var characterBodyOffsetSettings = await CharacterBodyOffsetSettings.CreateAsync(characterCode, addressableName, progress);
            BufferMap.TryAdd(characterCode, characterBodyOffsetSettings);
            return characterBodyOffsetSettings;
        }

        public static CharacterBodyOffsetSettings Get(string characterCode)
        {
            if (BufferMap.TryGetValue(characterCode, out var loader))
            {
                return loader;
            }

            throw new NullReferenceException($"未找到角色\"{characterCode}\"的立绘加载器！");
        }


        public static void Remove(string characterCode)
        {
            if (BufferMap.TryRemove(characterCode, out var value))
            {
                value.Clear();
            }
        }
    }
}