using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Xiyu.GlobalEffect;

namespace Xiyu.Menu
{
    public sealed class ExitGameButton : Xiyu.Menu.Button
    {
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable)
            {
                return;
            }


            transform.DOKill(true);
            transform.DOShakeScale(animationDuration, animationScale).OnComplete(() =>
            {
                transform.localScale = Vector3.one;
                UnderlayDilate = 1;
                GlowPower = 0.2F;
            });
            DOTween.To(() => GlowPower, x => GlowPower = x, 0.2F, animationDuration)
                .SetEase(Ease.Linear)
                .OnComplete(OnPointerEnterEventHandle);

            OnOnPointerEnterActionInvoke(eventData);
        }

        private void OnPointerEnterEventHandle()
        {
        }
    }
}