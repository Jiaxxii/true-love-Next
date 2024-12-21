using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Xiyu.GlobalEffect
{
    public sealed class MaskEffect : MonoBehaviour
    {
        private static readonly Dictionary<string, ResourceRequest> Buffer = new();

        [SerializeField] private Image effectImage;
        private static readonly int ShaderPropertyIDStrength = Shader.PropertyToID("Strength");
        private static readonly int ShaderPropertyIDRuleTexture = Shader.PropertyToID("Rule");
        private static readonly int ShaderPropertyIDSmooth = Shader.PropertyToID("Smooth");

        private Tween _tweenStrength;
        private Tween _tweenAlpha;

        /// <summary>
        /// 遮罩溶解程度 [0-1] 0为完全透明，1为完全不透明
        /// </summary>
        public float Strength
        {
            get => effectImage.material.GetFloat(ShaderPropertyIDStrength);
            set => effectImage.material.SetFloat(ShaderPropertyIDStrength, value);
        }

        /// <summary>
        /// 遮罩平滑程度 [0-1] 1为完全不平滑，0为完全平滑
        /// <para>建议值：0.01F</para>
        /// </summary>
        public float Smooth
        {
            get => effectImage.material.GetFloat(ShaderPropertyIDSmooth);
            set => effectImage.material.SetFloat(ShaderPropertyIDSmooth, value);
        }

        /// <summary>
        /// 图像的透明度 [0-1] 0为完全透明，1为完全不透明
        /// </summary>
        public float Alpha
        {
            get => effectImage.color.a;
            set => effectImage.color = new Color(effectImage.color.r, effectImage.color.g, effectImage.color.b, value);
        }

        /// <summary>
        /// 图像的精灵
        /// </summary>
        public Sprite SetSprite
        {
            get => effectImage.sprite;
            set => effectImage.sprite = value;
        }

        /// <summary>
        /// 将图像设置为纯色 （会将<see cref="Alpha"/>设置为0）
        /// </summary>
        /// <param name="color">目标颜色（包含透明度）</param>
        public void SetPureColor(Color color)
        {
            effectImage.sprite = null;
            effectImage.color = color;
            Strength = 0;
        }

        /// <summary>
        /// 设置遮罩的纹理 （rule001...rule133）
        /// </summary>
        /// <param name="resourcesRuleName"></param>
        /// <returns></returns>
        public async UniTask<Sprite> SetRuleSprite(string resourcesRuleName)
        {
            if (!Buffer.ContainsKey(resourcesRuleName))
            {
                Buffer.Add(resourcesRuleName, Resources.LoadAsync<Sprite>($"rule/{resourcesRuleName}"));
            }

            var resourceRequest = Buffer[resourcesRuleName];

            while (!resourceRequest.isDone)
            {
                await UniTask.NextFrame();
            }

            var sprite = (Sprite)resourceRequest.asset;

            effectImage.material.SetTexture(ShaderPropertyIDRuleTexture, sprite.texture);

            return sprite;
        }

        #region Animation

        public async UniTask RuleFadeIn(float duration, float startValue = 0, Ease ease = Ease.Unset)
        {
            _tweenStrength?.Kill(true);
            Strength = startValue;
            await (_tweenStrength = DOTween.To(() => Strength, x => Strength = x, 1, duration).SetEase(ease))
                .AsyncWaitForCompletion().AsUniTask();
        }

        public async UniTask RuleFadeOut(float duration, float startValue, Ease ease = Ease.Unset)
        {
            _tweenStrength?.Kill(true);
            Strength = startValue;
            await (_tweenStrength = DOTween.To(() => Strength, x => Strength = x, 0, duration).SetEase(ease))
                .AsyncWaitForCompletion().AsUniTask();
        }

        public async UniTask FadeIn(float duration, float startValue = 0, Ease ease = Ease.Unset)
        {
            _tweenAlpha?.Kill(true);
            Alpha = startValue;
            await (_tweenAlpha = effectImage.DOFade(1, duration).SetEase(ease))
                .AsyncWaitForCompletion().AsUniTask();
        }

        public async UniTask FadeOut(float duration, float startValue = 1, Ease ease = Ease.Unset)
        {
            _tweenAlpha?.Kill(true);
            Alpha = startValue;
            await (_tweenAlpha = effectImage.DOFade(0, duration).SetEase(ease))
                .AsyncWaitForCompletion().AsUniTask();
        }

        #endregion
    }
}