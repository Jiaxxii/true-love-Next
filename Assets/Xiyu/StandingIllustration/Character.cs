using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Xiyu.AssetLoader;
using Xiyu.DataStructure;

namespace Xiyu.StandingIllustration
{
    public sealed class Character : MonoBehaviour
    {
        private CanvasGroup _baseCanvasGroup;
        private Image _bodyImage;
        private readonly List<Image> _faceImages = new();

        private readonly List<UniTask> _taskList = new(2);

        private static readonly int ShaderPropertyColor = Shader.PropertyToID("_Color");
        private static readonly int ShaderPropertyStrength = Shader.PropertyToID("_Strength");

        public Color OutlineColor
        {
            get => _bodyImage.material.GetColor(ShaderPropertyColor);
            set => _bodyImage.material.SetColor(ShaderPropertyColor, value);
        }

        public float OutlineStrength
        {
            get => _bodyImage.material.GetFloat(ShaderPropertyStrength);
            set => _bodyImage.material.SetFloat(ShaderPropertyStrength, value);
        }

        public string AddressableName { get; private set; }
        public string CharacterCode { get; private set; }

        public RectTransform Root { get; private set; }

        private UniTask InitAsync(string addressableName)
        {
            _baseCanvasGroup = GetComponent<CanvasGroup>();
            _bodyImage = transform.Find("Body").GetComponent<Image>();
            Root = (RectTransform)transform;

            CharacterCode = AddressableName = addressableName;

            return UniTask.CompletedTask;
        }

        public void SetAnchoredPosition(Vector2 anchoredPosition)
        {
            Root.anchoredPosition = anchoredPosition;
        }

        /// <summary>
        /// 设置是否可见
        /// </summary>
        /// <param name="active">如果为true则表示完全可见否则完全不可见</param>
        /// <param name="blocksRayCasts">是否阻挡射线检测</param>
        public void SetActive(bool active, bool blocksRayCasts)
        {
            _baseCanvasGroup.alpha = active ? 1 : 0;
            _baseCanvasGroup.blocksRaycasts = blocksRayCasts;
            _baseCanvasGroup.interactable = active;
        }

        public void UpdateBody(Sprite sprite, LightweightRectTransform lrt)
        {
            Root.sizeDelta = new Vector2(lrt.Size.Width * lrt.Scale.x, lrt.Size.Height * lrt.Scale.y);

            _bodyImage.sprite = sprite;
            _bodyImage.rectTransform.Apply(lrt);
        }

        public async UniTask UpdateBodyFadeAsync(Sprite sprite, LightweightRectTransform lrt, float duration)
        {
            _bodyImage.DOKill();
            await _bodyImage.DOFade(0.2f, duration).AsyncWaitForCompletion().AsUniTask();

            Root.sizeDelta = lrt.Size;
            _bodyImage.sprite = sprite;
            _bodyImage.rectTransform.Apply(lrt);

            await _bodyImage.DOFade(1, duration).AsyncWaitForCompletion().AsUniTask();
        }

        public async UniTaskVoid UpdateBodyFadeForget(Sprite sprite, LightweightRectTransform lrt, float duration)
        {
            await UpdateBodyFadeAsync(sprite, lrt, duration);
        }


        public void UpdateFace((Sprite sprite, LightweightRectTransform lrt)[] data)
        {
            AutoFillFaceImages(data.Length);
            for (var i = 0; i < data.Length; i++)
            {
                _faceImages[i].sprite = data[i].sprite;
                _faceImages[i].rectTransform.Apply(data[i].lrt);
            }
        }


        public async UniTask UpdateFaceFadeAsync((Sprite sprite, LightweightRectTransform lrt)[] data, float duration)
        {
            duration *= 0.5f;

            _taskList.Clear();

            foreach (var image in _faceImages)
            {
                image.DOKill();
                _taskList.Add(image.DOFade(0.2f, duration).SetEase(Ease.Unset).AsyncWaitForCompletion().AsUniTask());
            }

            await UniTask.WhenAll(_taskList);

            UpdateFace(data);

            _taskList.Clear();
            foreach (var image in _faceImages)
            {
                image.DOKill();
                _taskList.Add(image.DOFade(1, duration).SetEase(Ease.Unset).AsyncWaitForCompletion().AsUniTask());
            }

            await UniTask.WhenAll(_taskList);
        }

        public async UniTaskVoid UpdateFaceFadeForget((Sprite sprite, LightweightRectTransform lrt)[] data, float duration)
        {
            await UpdateFaceFadeAsync(data, duration);
        }

        private void AutoFillFaceImages(int targetCount)
        {
            if (_faceImages.Count == targetCount) return;

            if (_faceImages.Count < targetCount)
            {
                var count = targetCount - _faceImages.Count;
                for (var i = 0; i < count; i++)
                {
                    var faceImage = new GameObject("Face-item").AddComponent<Image>();
                    faceImage.transform.SetParent(transform, false);
                    _faceImages.Add(faceImage);
                }
            }
            else
            {
                for (var i = _faceImages.Count - 1; i >= targetCount; i--)
                {
                    _faceImages[i].DOKill(true);
                    Destroy(_faceImages[i].gameObject);
                    _faceImages.RemoveAt(i);
                }
            }
        }

        private void OnDestroy()
        {
            _bodyImage.DOKill();
            _bodyImage.sprite = null;
            foreach (var faceImage in _faceImages)
            {
                faceImage.DOKill();
                faceImage.sprite = null;
            }

            CharacterPrefabricateAssetLoader.Release(AddressableName);
        }

        public static async UniTask<Character> CreateAsync(string addressableName, Transform parent, bool active, bool blocksRayCasts,
            IProgress<float> progress = null)
        {
            var character = (await CharacterPrefabricateAssetLoader.LoadInstanceObjectAsync(addressableName, parent, progress)).GetComponent<Character>();

            await character.InitAsync(addressableName);

            character.SetActive(active, blocksRayCasts);

            return character;
        }

        public static async UniTask<(Sprite sprite, LightweightRectTransform transform)> GetBodyData(StandingIllustrationLoader standingIllustrationLoader,
            CharacterBodyOffsetSettings characterBodyOffsetSettings, string bodyCode)
        {
            var bodySprite = await standingIllustrationLoader.LoadSpriteAsync(bodyCode);
            var bodyOffset = characterBodyOffsetSettings.GetBodyOffset(bodyCode);
            return (bodySprite, bodyOffset);
        }

        public static async UniTask<(Sprite sprite, LightweightRectTransform transform)[]> GetFaceData(CharacterEmotionsSettings characterEmotionsSettings,
            StandingIllustrationLoader standingIllustrationLoader, string emotionCode)
        {
            var emotionInfoItems = characterEmotionsSettings.GetEmotionInfoItems(emotionCode);
            var faceData = new (Sprite, LightweightRectTransform)[emotionInfoItems.Length];
            var task = new UniTask[emotionInfoItems.Length];
            for (var i = 0; i < emotionInfoItems.Length; i++)
            {
                var index = i;
                task[i] = standingIllustrationLoader.LoadSpriteAsync(emotionInfoItems[i].AddressableName, sprite => faceData[index] = (sprite, emotionInfoItems[index].Transform));
            }

            await task;
            return faceData;
        }
    }

    public static class CharacterExpand
    {
        public static async UniTask UpdateBodyAsync(this Character character, StandingIllustrationLoader standingIllustrationLoader,
            CharacterBodyOffsetSettings characterBodyOffsetSettings, string bodyCode)
        {
            var (sprite, lrt) = await Character.GetBodyData(standingIllustrationLoader, characterBodyOffsetSettings, bodyCode);
            character.UpdateBody(sprite, lrt);
        }

        public static async UniTask UpdateBodyFadeAsync(this Character character, StandingIllustrationLoader standingIllustrationLoader,
            CharacterBodyOffsetSettings characterBodyOffsetSettings, string bodyCode, float duration)
        {
            var (sprite, lrt) = await Character.GetBodyData(standingIllustrationLoader, characterBodyOffsetSettings, bodyCode);
            await character.UpdateBodyFadeAsync(sprite, lrt, duration);
        }

        public static async UniTask UpdateFaceAsync(this Character character, StandingIllustrationLoader standingIllustrationLoader,
            CharacterEmotionsSettings characterEmotionsSettings, string emotionCode)
        {
            var faceData = await Character.GetFaceData(characterEmotionsSettings, standingIllustrationLoader, emotionCode);
            character.UpdateFace(faceData);
        }

        public static async UniTask UpdateFaceFadeAsync(this Character character, StandingIllustrationLoader standingIllustrationLoader,
            CharacterEmotionsSettings characterEmotionsSettings, string emotionCode, float duration)
        {
            var faceData = await Character.GetFaceData(characterEmotionsSettings, standingIllustrationLoader, emotionCode);
            await character.UpdateFaceFadeAsync(faceData, duration);
        }
    }
}