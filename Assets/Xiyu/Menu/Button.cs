using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Xiyu.AntiShake;

namespace Xiyu.Menu
{
    public class Button : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        [Header("引用")] [SerializeField] protected Image buttonImage;

        [SerializeField] protected TextMeshProUGUI textMeshProUGUI;


        [Header("参数")] [SerializeField] protected bool isInteractable = true;

        [SerializeField] protected float animationDuration = 0.2f;
        [SerializeField] protected float animationScale = 0.2f;
        [SerializeField] protected float protectionTime = 0.2f;


        [JetBrains.Annotations.PublicAPI] public event UnityAction<PointerEventData> OnPointerEnterAction;
        [JetBrains.Annotations.PublicAPI] public event UnityAction<PointerEventData> OnPointerClickAction;
        [JetBrains.Annotations.PublicAPI] public event UnityAction<PointerEventData> OnPointerExitAction;

        private float _startAlpha = -1, _textStartAlpha;

        private readonly AntiShakeManager _antiShakeManager = new();
        private int _queryId;


        [JetBrains.Annotations.PublicAPI]
        public bool IsInteractable
        {
            get => isInteractable;
            set => SetInteractable(value);
        }

        [JetBrains.Annotations.PublicAPI]
        public float UnderlayDilate
        {
            get => textMeshProUGUI.fontMaterial.GetFloat(ShaderUtilities.ID_UnderlayDilate);
            set => textMeshProUGUI.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayDilate, value);
        }

        [JetBrains.Annotations.PublicAPI]
        public float GlowPower
        {
            get => textMeshProUGUI.fontMaterial.GetFloat(ShaderUtilities.ID_GlowPower);
            set => textMeshProUGUI.fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, value);
        }

        protected virtual void OnEnable()
        {
            if (_startAlpha < 0)
            {
                _startAlpha = buttonImage.color.a;
                _textStartAlpha = textMeshProUGUI.alpha;
                _queryId = _antiShakeManager.Record(GetInstanceID(), (int)(protectionTime * 1000));
            }

            SetInteractable(isInteractable);
            UnderlayDilate = -1;
            GlowPower = 0;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
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
                .OnComplete(() => OnOnPointerEnterActionInvoke(eventData));
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable)
            {
                return;
            }

            if (!_antiShakeManager.Query(_queryId))
                return;


            OnOnPointerClickActionInvoke(eventData);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable)
            {
                return;
            }

            OnOnPointerExitActionInvoke(eventData);

            UnderlayDilate = -1;
            DOTween.To(() => GlowPower, x => GlowPower = x, -1, animationScale).SetEase(Ease.Linear).OnComplete(() => GlowPower = 0);
        }


        protected void OnOnPointerEnterActionInvoke(PointerEventData eventData) => OnPointerEnterAction?.Invoke(eventData);
        protected void OnOnPointerClickActionInvoke(PointerEventData eventData) => OnPointerClickAction?.Invoke(eventData);
        protected void OnOnPointerExitActionInvoke(PointerEventData eventData) => OnPointerExitAction?.Invoke(eventData);

        private void SetInteractable(bool value)
        {
            isInteractable = value;
            if (isInteractable)
            {
                buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, _startAlpha);
                textMeshProUGUI.alpha = _textStartAlpha;
            }
            else
            {
                buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, _startAlpha * 0.5f);
                textMeshProUGUI.alpha = _textStartAlpha * 0.5f;
            }
        }
    }
}