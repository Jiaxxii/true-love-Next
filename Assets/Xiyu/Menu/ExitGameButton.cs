using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Xiyu.Menu
{
    public sealed class ExitGameButton : Xiyu.Menu.Button
    {
        [SerializeField] private FullScreenPassRendererFeature fullScreenPassRendererFeature;
        [SerializeField] private float strengthAnimationDuration = 0.2f;


        private static readonly int ShaderPropertyStrength = Shader.PropertyToID("_Strength");

        private Tween _strengthTween;

        private Sequence _sequence;

        public float Strength
        {
            get => fullScreenPassRendererFeature.passMaterial.GetFloat(ShaderPropertyStrength);
            set => fullScreenPassRendererFeature.passMaterial.SetFloat(ShaderPropertyStrength, value);
        }


        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable)
            {
                return;
            }


            transform.DOKill(true);
            transform.DOShakeScale(animationDuration, animationScale).OnComplete(() => { transform.localScale = Vector3.one; });

            _strengthTween = DOTween.To(() => Strength, x => Strength = x, 1, strengthAnimationDuration).SetEase(Ease.Linear);
            OnOnPointerEnterActionInvoke(eventData);


            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            _sequence.Append(transform.DOScale(new Vector3(1.2F, 1.2F, 1.2F), 0.35f))
                .Append(transform.DOScale(Vector3.one, 0.35f))
                .SetLoops(-1, LoopType.Yoyo);
        }


        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable)
            {
                return;
            }

            OnOnPointerExitActionInvoke(eventData);

            _strengthTween.Kill();
            _strengthTween = DOTween.To(() => Strength, x => Strength = x, 0, strengthAnimationDuration).SetEase(Ease.Linear);

            _sequence?.Kill();
            transform.DOScale(Vector3.one, 0.2F);
        }
    }
}