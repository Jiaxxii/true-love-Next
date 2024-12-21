using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Xiyu.StandingIllustration
{
    public sealed class StandingIllustrationLoader : IAsyncEnumerable<KeyValuePair<string, Sprite>>, IDisposable
    {
        private readonly ConcurrentDictionary<string, AsyncOperationHandle<Sprite>> _bufferMap = new();


        public async UniTask<Sprite> LoadStandingIllustrationAsync(string addressableName)
        {
            _bufferMap.TryAdd(addressableName, Addressables.LoadAssetAsync<Sprite>(addressableName));

            var asyncOperationHandle = _bufferMap[addressableName];
            if (!asyncOperationHandle.IsDone || asyncOperationHandle.Status == AsyncOperationStatus.None)
            {
                await UniTask.NextFrame();
            }

            if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new ArgumentException("资源加载失败！状态：" + asyncOperationHandle.Status);
            }

            return asyncOperationHandle.Result;
        }

        public async UniTask<IEnumerable<Sprite>> LoadStandingIllustrationAsync(IEnumerable<string> addressableNames)
        {
            return await UniTask.WhenAll(System.Linq.Enumerable.Select(addressableNames, LoadStandingIllustrationAsync));
        }


        public void ReleaseStandingIllustration(string addressableName)
        {
            if (_bufferMap.TryRemove(addressableName, out var asyncOperationHandle))
            {
                Addressables.Release(asyncOperationHandle);
            }
        }

        public void ReleaseStandingIllustration(IEnumerable<string> addressableNames)
        {
            foreach (var addressableName in addressableNames)
            {
                ReleaseStandingIllustration(addressableName);
            }
        }

        public void ReleaseAllStandingIllustration()
        {
            foreach (var asyncOperationHandle in _bufferMap.Values)
            {
                Addressables.Release(asyncOperationHandle);
            }

            _bufferMap.Clear();
        }

        public async IAsyncEnumerator<KeyValuePair<string, Sprite>> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            foreach (var (addressableName, asyncOperationHandle) in _bufferMap)
            {
                if (!asyncOperationHandle.IsDone || asyncOperationHandle.Status == AsyncOperationStatus.None)
                {
                    await UniTask.NextFrame();
                }

                if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new ArgumentException("资源加载失败！状态：" + asyncOperationHandle.Status);
                }

                yield return new KeyValuePair<string, Sprite>(addressableName, asyncOperationHandle.Result);
            }
        }

        public void Dispose()
        {
            if (_bufferMap.Count == 0) return;
            ReleaseAllStandingIllustration();
        }
    }
}