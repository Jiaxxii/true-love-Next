using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Xiyu.Select
{
    public sealed class ButtonSelectPanel : MonoBehaviour
    {
        [SerializeField] private Button downToTopButton;
        [SerializeField] private Button topToDownButton;

        [SerializeField] private Transform selectTransform;
        [SerializeField] private Image[] headImages;


        [SerializeField] [Range(0.001F, 1)] private float rotateAngleTime = 0.2F;
        [SerializeField] private float topToDownRotateAngle = 10;
        [SerializeField] private float downToTopRotateAngle = -10;

        [SerializeField] private Vector2 selectShakeScale = new(0.2F, 0.2F);
        [SerializeField] private Vector3 buttonScale = new(1.2F, 1.2F);

        private Sprite[] _headsSpites;

        private int _startIndex;

        private readonly HashSet<int> _animationLock = new();

        private readonly Dictionary<string, string> _spriteNameToCharacterCode = new();
        private HashSet<string> _characterCodeSet;

        public event Action<(bool isUndefined, string characterCode)> OnSelectCharacter;

        public float RefreshDuration => rotateAngleTime * headImages.Length;


        public void LoadHeadIconAsync(HashSet<string> characterCodeSet, bool userUndefinedHead = true)
        {
            var sprites = Resources.LoadAll<Sprite>("head icon");

            var list = new List<Sprite>();

            foreach (var sprite in sprites)
            {
                var arr = sprite.name.Split('_');

                var characterCode = arr[0];

                if (!characterCodeSet.Contains(characterCode) && !userUndefinedHead)
                {
                    Debug.LogWarning($"Resources 中的 {sprite.name} 解析后的角色代码 \"{characterCode}\" 不在角色代码集合中!");
                    continue;
                }

                if (_spriteNameToCharacterCode.TryAdd(sprite.name, characterCode))
                {
                    list.Add(sprite);
                }
                else Debug.LogWarning($"Resources 中的 {sprite.name} 解析后的角色代码 \"{characterCode}\" 重复!");
            }

            _headsSpites = list.ToArray();
            _characterCodeSet = characterCodeSet;

            downToTopButton.onClick.AddListener(UniTask.UnityAction(RefreshDownToTopAnimation));
            topToDownButton.onClick.AddListener(UniTask.UnityAction(RefreshTopToDownAnimation));

            downToTopButton.onClick.AddListener(() => ButtonAnimation(ref buttonScale, downToTopButton));
            topToDownButton.onClick.AddListener(() => ButtonAnimation(ref buttonScale, topToDownButton));
        }


        /// <summary>
        /// 获取当前选择的角色代码 并且判断是否定义的角色 如果是定义的角色则返回true
        /// </summary>
        /// <param name="characterCode"></param>
        /// <returns></returns>
        [JetBrains.Annotations.PublicAPI]
        public bool CurrentSelectIsDefined(out string characterCode)
        {
            var currentSelect = headImages[headImages.Length / 2].sprite.name;
            var spriteCharacterCode = _spriteNameToCharacterCode[currentSelect];

            characterCode = spriteCharacterCode;

            return _characterCodeSet.Contains(spriteCharacterCode);
        }

        public async UniTaskVoid RefreshDownToTopAnimation()
        {
            _startIndex = (_startIndex + 1) % _headsSpites.Length;
            for (var i = headImages.Length - 1; i >= 0; i--)
            {
                await SetAnimation(i, downToTopRotateAngle);
            }
        }

        public async UniTaskVoid RefreshTopToDownAnimation()
        {
            _startIndex = (_headsSpites.Length + _startIndex - 1) % _headsSpites.Length;
            for (var i = 0; i < headImages.Length; i++)
            {
                await SetAnimation(i, topToDownRotateAngle);
            }
        }


        private async UniTask SetAnimation(int index, float angle)
        {
            var currentImage = headImages[index];
            if (!_animationLock.Add(currentImage.GetInstanceID()))
            {
                SetSprite(index);
                return;
            }

            SetSprite(index);

            if (index == headImages.Length / 2)
            {
                selectTransform.DOKill();
                selectTransform.DOShakeScale(rotateAngleTime * 3, new Vector3(selectShakeScale.x, selectShakeScale.y, 0)).SetEase(Ease.OutBounce);

                var result = CurrentSelectIsDefined(out var characterCode);
                OnSelectCharacter?.Invoke((result, characterCode));
            }

            await currentImage.transform.DOShakeRotation(rotateAngleTime, new Vector3(0, 0, angle))
                .OnComplete(() => _animationLock.Remove(currentImage.GetInstanceID()))
                .AsyncWaitForCompletion()
                .AsUniTask();
        }


        private void SetSprite(int index)
        {
            headImages[index].sprite = _headsSpites[(_startIndex + index) % _headsSpites.Length];
        }


        private static void ButtonAnimation(ref Vector3 scale, Button button)
        {
            button.DOKill();
            button.transform.DOScale(scale, 0.2F).SetEase(Ease.OutBounce)
                .OnComplete(() => button.transform.DOScale(Vector3.one, 0.2F).SetEase(Ease.Unset));
        }
    }
}