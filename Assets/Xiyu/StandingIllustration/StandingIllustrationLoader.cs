using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Xiyu.StandingIllustration
{
    public class StandingIllustrationLoader
    {
        private readonly ConcurrentDictionary<string, AsyncOperationHandle<Sprite>> _bufferMap = new();

        public async UniTask<Sprite> LoadStandingIllustrationAsync(string addressableName)
        {
            _bufferMap.TryAdd(addressableName, Addressables.LoadAssetAsync<Sprite>(addressableName));


            var asyncOperationHandle = _bufferMap[addressableName];
            Debug.Log(asyncOperationHandle.Status);
            if (!asyncOperationHandle.IsDone)
            {
                await UniTask.NextFrame();
            }

            if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new ArgumentException("资源加载失败！状态：" + asyncOperationHandle.Status);
            }

            return asyncOperationHandle.Result;
        }
    }
}