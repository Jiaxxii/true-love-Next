using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Xiyu.GlobalEffect
{
    public class GlobalEffectManger : MonoBehaviour
    {
        private static readonly Lazy<GlobalEffectManger> LazyInstance = new(GetLazyInstance);

        public static GlobalEffectManger Instance => LazyInstance.Value;

        #region Unity References

        [Header("Unity 引用")] [SerializeField] private Canvas canvas;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image effectImage;

        [SerializeField] private MaskEffect maskEffect;
        [SerializeField] private LoaderEffect loaderEffect;

        #endregion

        private static int _firstLoaderManagerInstanceID;


        public MaskEffect MaskEffect => maskEffect;
        public LoaderEffect LoaderEffect => loaderEffect;


        public float TopAlpha
        {
            get => canvasGroup.alpha;
            set => canvasGroup.alpha = value;
        }

        public void SetTopActive(bool active)
        {
            canvasGroup.alpha = active ? 1 : 0;
            canvasGroup.blocksRaycasts = active;
            canvasGroup.interactable = active;
        }

        private static GlobalEffectManger GetLazyInstance()
        {
            var loaderManager = FindObjectOfType<GlobalEffectManger>();

            if (loaderManager == null)
            {
                // 特殊情况：LoaderManager 在其他场景中，需要从Resources中加载
                var loaderManagerPrefab = Resources.Load<GlobalEffectManger>("Canvas-Global");
                loaderManager = Instantiate(loaderManagerPrefab);
            }

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (_, _) => { loaderManager.canvas.worldCamera = Camera.main; };

            _firstLoaderManagerInstanceID = loaderManager.GetInstanceID();
            DontDestroyOnLoad(loaderManager.gameObject);

            return loaderManager;
        }

        private void Awake()
        {
            // 针对一： Menu 调用过 LoaderManager.Instance，加载第二个场景时场景中也存在一个 LoaderManager
            if (!LazyInstance.IsValueCreated)
                return;

            var findObjectsOfType = FindObjectsOfType<GlobalEffectManger>();
            // 删除场景中存在的 LoaderManager只保留上一个场景过来的
            if (findObjectsOfType.Length == 2)
            {
                Destroy(gameObject);
                return;
            }

            // 针对二：Menu 调用过 LoaderManager.Instance，加载第二个场景时场景中存在多个 LoaderManager （按道理这是开发者犯下的错误）
            if (findObjectsOfType.Length <= 2) return;


            // 如果所有的 LoaderManager 都与 第一次缓存的 LoaderManager 的 InstanceID 不匹配，那么缓存的 InstanceID 可能在赋值错了或者被意外修改了
            if (findObjectsOfType.All(obj => obj.GetInstanceID() != _firstLoaderManagerInstanceID))
            {
                // 对于这种情况，我未能找到一个好地解决方案，所以我只能抛出一个异常来提醒开发者
                throw new System.ArgumentNullException($"缓存ID\"{nameof(_firstLoaderManagerInstanceID)}\"可能被意外修改或者赋值错误，请检查代码！");
            }

            foreach (var manager in findObjectsOfType)
            {
                // 删除所有的 LoaderManager 除了第一个
                if (manager.GetInstanceID() != _firstLoaderManagerInstanceID)
                {
                    Destroy(manager);
                }
            }
        }
    }
}