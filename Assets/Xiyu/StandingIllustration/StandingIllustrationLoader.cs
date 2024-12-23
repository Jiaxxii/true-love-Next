using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xiyu.StandingIllustration
{
    public sealed class StandingIllustrationLoader
    {
        public IProgress<float> Progress { get; private set; }

        public string CharacterCode { get; private set; }

        public StandingIllustrationLoader(string characterCode, IProgress<float> progress = null)
        {
            CharacterCode = characterCode;
            Progress = progress;
        }


        private readonly ConcurrentDictionary<string, Sprite> _bufferMap = new();

        public async UniTask<Sprite> LoadSpriteAsync(string addressableName)
        {
            if (_bufferMap.TryGetValue(addressableName, out var sprite))
            {
                return sprite;
            }

            var asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(addressableName);
            _bufferMap.TryAdd(addressableName, await asyncOperationHandle.ToUniTask(Progress));

            Addressables.Release(asyncOperationHandle);

            return _bufferMap[addressableName];
        }

        public async UniTask PreloadSpritesAsync(string characterLabel, string bodyOrFaceLabel)
        {
            var loadResourceLocationsAsync =
                Addressables.LoadResourceLocationsAsync(new List<string> { characterLabel, bodyOrFaceLabel }, Addressables.MergeMode.Intersection, typeof(Sprite));

            await loadResourceLocationsAsync.ToUniTask();

            var task = new List<UniTask>();
            foreach (var resourceLocation in loadResourceLocationsAsync.Result)
            {
                if (_bufferMap.ContainsKey(resourceLocation.PrimaryKey))
                    continue;

                task.Add(LoadSpriteAsync(resourceLocation.PrimaryKey));
            }

            await UniTask.WhenAll(task);
        }

        public void Release(string addressableName)
        {
            if (_bufferMap.TryRemove(addressableName, out _))
            {
            }
        }

        public void ReleaseAll()
        {
            _bufferMap.Clear();
        }

        public static async UniTask<StandingIllustrationLoader> PreloadAsync(string characterCode, string characterLabel, string bodyOrFaceLabel, IProgress<float> progress = null)
        {
            var loader = new StandingIllustrationLoader(characterCode, progress);
            await loader.PreloadSpritesAsync(characterLabel, bodyOrFaceLabel);
            return loader;
        }
    }
}