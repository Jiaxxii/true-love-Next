using System;
using System.Collections.Concurrent;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xiyu.AssetLoader
{
    public static class CharacterPrefabricateAssetLoader
    {
        private static readonly ConcurrentDictionary<string, GameObject> Buffer = new();


        public static async UniTask<GameObject> LoadInstanceObjectAsync(string addressableName, Transform parent, IProgress<float> progress = null)
        {
            if (Buffer.TryGetValue(addressableName, out var gameObjectInstance))
            {
                Debug.LogWarning($"Addressable资源\"{addressableName}\"已经创建实例对象，本次加载将直接返回实例对象引用！");
                return gameObjectInstance;
            }

            var asyncOperationHandle = Addressables.InstantiateAsync(addressableName, parent: parent);
            Buffer.TryAdd(addressableName, await asyncOperationHandle.ToUniTask(progress));

            return Buffer[addressableName];
        }

        public static void Release(string addressableName)
        {
            if (Buffer.TryRemove(addressableName, out var gameObject))
            {
                Addressables.ReleaseInstance(gameObject);
            }
        }

        public static void ReleaseAll()
        {
            foreach (var gameObject in Buffer.Values.ToArray())
            {
                Addressables.ReleaseInstance(gameObject);
            }

            Buffer.Clear();
        }
    }
}