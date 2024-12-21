using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

namespace Xiyu.DataStructure
{
    [Serializable]
    public class SceneRuleMapRefs : IDisposable
    {
        [SerializeField] private string sceneName;
        [SerializeField] private AssetReferenceSprite[] ruleMap;

        private Dictionary<string, AssetReferenceSprite> _readOnlyDictionary;
        public IReadOnlyDictionary<string, AssetReferenceSprite> ReadOnlyDictionary => _readOnlyDictionary ??= LoadMap();

        public string SceneName => sceneName;

        public async UniTask PreloadAsync(int outTImeMilliseconds)
        {
            var elapsedMilliseconds = 0L;
            var stopwatch = new Stopwatch();
            foreach (var assetReferenceSprite in ruleMap)
            {
                stopwatch.Restart();

                if (assetReferenceSprite.Asset == null)
                {
                    await assetReferenceSprite.LoadAssetAsync();
                }

                if (assetReferenceSprite.OperationHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new AggregateException("资源加载失败！状态：" + assetReferenceSprite.OperationHandle.Status);
                }

                elapsedMilliseconds += stopwatch.ElapsedMilliseconds;
                if (elapsedMilliseconds > outTImeMilliseconds)
                {
                    break;
                }
            }
        }

        public async UniTask<Sprite> GetSprite(string subObjectName)
        {
            if (!ReadOnlyDictionary.ContainsKey(subObjectName))
            {
                throw new ArgumentException("找不到对应的资源！key：" + subObjectName);
            }

            var asyncOperationHandle = ReadOnlyDictionary[subObjectName];

            if (asyncOperationHandle.Asset == null)
            {
                await asyncOperationHandle.LoadAssetAsync();
            }

            if (asyncOperationHandle.OperationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new AggregateException("资源加载失败！状态：" + asyncOperationHandle.OperationHandle.Status);
            }

            return (Sprite)asyncOperationHandle.OperationHandle.Result;
        }


        private Dictionary<string, AssetReferenceSprite> LoadMap()
        {
            return ruleMap.ToDictionary(x => x.SubObjectName, x => x);
        }

        public void Dispose()
        {
            foreach (var assetReferenceSprite in ruleMap)
            {
                assetReferenceSprite.ReleaseAsset();
            }

            _readOnlyDictionary.Clear();
        }
    }
}