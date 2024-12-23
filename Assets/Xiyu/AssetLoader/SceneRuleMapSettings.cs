using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xiyu.AssetLoader
{
    public static class SceneRuleMapSettings
    {
        private static readonly ConcurrentDictionary<string, Sprite> Buffer = new();


        public static async UniTask<Sprite> LoadSpriteAsync(string addressableName, IProgress<float> progress = null)
        {
            if (Buffer.TryGetValue(addressableName, out var sprite))
            {
                return sprite;
            }

            var asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(addressableName);
            Buffer.TryAdd(addressableName, await asyncOperationHandle.ToUniTask(progress));

            Addressables.Release(asyncOperationHandle);
            return Buffer[addressableName];
        }

        public static async UniTask LoadSpriteAsync(string addressableName, IProgress<float> progress, Action<Sprite> onCompleted)
        {
            if (Buffer.TryGetValue(addressableName, out var sprite))
            {
                onCompleted?.Invoke(sprite);
                return;
            }

            var asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(addressableName);
            Buffer.TryAdd(addressableName, await asyncOperationHandle.ToUniTask(progress));

            Addressables.Release(asyncOperationHandle);
            onCompleted?.Invoke(Buffer[addressableName]);
        }


        public static async UniTask<Sprite[]> LoadSpritesAsync(string ruleMapLabel, IProgress<float> progress = null)
        {
            var loadResourceLocationsAsync = Addressables.LoadResourceLocationsAsync(new List<string> { ruleMapLabel }, Addressables.MergeMode.Union, typeof(Sprite));
            await loadResourceLocationsAsync.ToUniTask();

            var sprites = new Sprite[loadResourceLocationsAsync.Result.Count];
            var task = new List<UniTask>();


            for (var i = 0; i < loadResourceLocationsAsync.Result.Count; i++)
            {
                var resourceLocation = loadResourceLocationsAsync.Result[i];
                if (Buffer.ContainsKey(resourceLocation.PrimaryKey))
                    continue;

                var index = i;
                task.Add(LoadSpriteAsync(resourceLocation.PrimaryKey, progress, sprite => sprites[index] = sprite));
            }


            await UniTask.WhenAll(task);


            Addressables.Release(loadResourceLocationsAsync);
            return sprites;
        }


        public static void Release(string addressableName)
        {
            if (Buffer.TryRemove(addressableName, out var sprite))
            {
                Addressables.Release(sprite);
            }
        }

        public static void ReleaseAll()
        {
            Buffer.Clear();
        }
    }
}