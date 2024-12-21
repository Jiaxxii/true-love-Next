using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Xiyu.DataStructure;

namespace ScriptableObjectSetting
{
    [CreateAssetMenu(fileName = "SceneRuleMapSettings", menuName = "ScriptableObject/场景遮罩图名称加载表", order = 0)]
    public class SceneRuleMapSettings : ScriptableObject
    {
        public SceneRuleMapRefs[] sceneRuleMapInfos;

        private static Dictionary<string, SceneRuleMapRefs> _readOnlyDictionary;
        public static IReadOnlyDictionary<string, SceneRuleMapRefs> SceneRuleMapRefsDictionary => _readOnlyDictionary;


        public static async UniTask PreloadAsync(string sceneName, int outTImeMilliseconds)
        {
            if (!_readOnlyDictionary.TryGetValue(sceneName, out var sceneRuleMapRefs))
            {
                throw new KeyNotFoundException("找不到对应的资源！key：" + sceneName);
            }

            await sceneRuleMapRefs.PreloadAsync(outTImeMilliseconds);
        }


        public static UniTask<Sprite> GetSpriteAsync(string sceneName, string subObjectName)
        {
            if (!_readOnlyDictionary.TryGetValue(sceneName, out var sceneRuleMapRefs))
            {
                throw new KeyNotFoundException("找不到对应的资源！key：" + sceneName);
            }

            return sceneRuleMapRefs.GetSprite(subObjectName);
        }


        public static void Release(string sceneName)
        {
            if (!_readOnlyDictionary.Remove(sceneName, out var sceneRuleMapRefs))
            {
                throw new KeyNotFoundException("找不到对应的资源！key：" + sceneName);
            }

            sceneRuleMapRefs.Dispose();
        }


        /// <summary>
        /// 只加载一次 (初始化字典)
        /// </summary>
        public static async UniTask InitAsync()
        {
            var resourceRequest = Resources.LoadAsync<SceneRuleMapSettings>("Settings/SceneRuleMapSettings");
            while (!resourceRequest.isDone)
            {
                await UniTask.NextFrame();
            }

            _readOnlyDictionary = ((SceneRuleMapSettings)resourceRequest.asset).sceneRuleMapInfos.ToDictionary(x => x.SceneName, x => x);
        }
    }
}