using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Xiyu.StandingIllustration
{
    public static class StandingIllustrationLoaderManager
    {
        private static readonly ConcurrentDictionary<string, StandingIllustrationLoader> BufferMap = new();


        public static StandingIllustrationLoader Create(string characterCode, IProgress<float> progress)
        {
            if (BufferMap.TryGetValue(characterCode, out var loader))
            {
                return loader;
            }

            var newLoader = new StandingIllustrationLoader(characterCode, progress);
            BufferMap.TryAdd(characterCode, newLoader);

            return newLoader;
        }

        public static StandingIllustrationLoader Get(string characterCode)
        {
            if (BufferMap.TryGetValue(characterCode, out var loader))
            {
                return loader;
            }

            throw new NullReferenceException($"未找到角色\"{characterCode}\"的立绘加载器！");
        }


        public static async UniTask<StandingIllustrationLoader> CreatePreloadAsync(string characterCode, string characterLabel, string bodyOrFaceLabel, IProgress<float> progress)
        {
            if (BufferMap.TryGetValue(characterCode, out var loader))
            {
                Debug.LogWarning($"立绘加载器\"{characterCode}\"已经创建，本次加载将直接返回实例对象引用！");
                return loader;
            }

            var standingIllustrationLoader = await StandingIllustrationLoader.PreloadAsync(characterCode, characterLabel, bodyOrFaceLabel, progress);
            BufferMap.TryAdd(characterCode, standingIllustrationLoader);
            return standingIllustrationLoader;
        }
    }
}