using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Xiyu.GlobalEffect
{
    public sealed class LoaderEffect : MonoBehaviour
    {
        [SerializeField] private Image effectImage;

        [SerializeField] private CanvasGroup canvasGroup;

        private Tween _tweenAlpha;

        private bool _isLoaded;

        private int _taskCount;

        public float Alpha
        {
            get => canvasGroup.alpha;
            set => canvasGroup.alpha = value;
        }

        public void SetActive(bool active)
        {
            canvasGroup.alpha = active ? 1 : 0;
            canvasGroup.blocksRaycasts = active;
            canvasGroup.interactable = active;
        }

        public void Loading()
        {
            if (_isLoaded)
            {
                return;
            }

            SetActive(true);

            _isLoaded = true;

            effectImage.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart);
        }

        public async UniTask LoadingWaitForWhen(Expression expression)
        {
            Loading();

            Interlocked.Increment(ref _taskCount);
            while (!expression())
            {
                await UniTask.NextFrame();
            }

            Interlocked.Decrement(ref _taskCount);

            await TryEndLoading();
        }

        public async UniTask LoadingWaitForUniTask(UniTask uniTask)
        {
            Loading();

            Interlocked.Increment(ref _taskCount);
            await uniTask;
            Interlocked.Decrement(ref _taskCount);

            await TryEndLoading();
        }

        public async UniTask LoadingWaitForUniTask(params UniTask[] uniTask)
        {
            Loading();

            Interlocked.Increment(ref _taskCount);
            await UniTask.WhenAll(uniTask);
            Interlocked.Decrement(ref _taskCount);

            await TryEndLoading();
        }


        public async UniTask LoadingWaitForAsyncOperation(AsyncOperation asyncOperation)
        {
            Loading();

            Interlocked.Increment(ref _taskCount);
            while (!asyncOperation.isDone)
            {
                await UniTask.NextFrame();
            }

            Interlocked.Decrement(ref _taskCount);

            await TryEndLoading();
        }

        public async UniTask TryEndLoading()
        {
            while (_isLoaded && _taskCount > 0)
            {
                await UniTask.NextFrame();
            }

            effectImage.transform.DOKill();
            effectImage.transform.localRotation = Quaternion.identity;

            _taskCount = 0;
            _isLoaded = false;
        }


        /// <summary>
        /// 淡入加载遮罩 （开始时：显示自身Alpha与开启射线拦截）
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="startValue"></param>
        /// <param name="ease"></param>
        public async UniTask FadeIn(float duration, float startValue = 0, Ease ease = Ease.Unset)
        {
            _tweenAlpha?.Kill(true);
            SetActive(true);
            Alpha = startValue;
            await (_tweenAlpha = DOTween.To(() => Alpha, x => Alpha = x, 1, duration).SetEase(ease))
                .AsyncWaitForCompletion().AsUniTask();
        }

        /// <summary>
        /// 淡出加载遮罩 （完成时：归零自身Alpha与关闭射线拦截）
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="startValue"></param>
        /// <param name="ease"></param>
        public async UniTask FadeOut(float duration, float startValue = 1, Ease ease = Ease.Unset)
        {
            _tweenAlpha?.Kill(true);
            Alpha = startValue;
            await (_tweenAlpha = DOTween.To(() => Alpha, x => Alpha = x, 0, duration).SetEase(ease))
                .AsyncWaitForCompletion().AsUniTask();
            SetActive(false);
        }
    }
}