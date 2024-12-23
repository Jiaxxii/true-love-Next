using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ScriptableObjectSettings;
using UnityEngine;
using UnityEngine.UI;
using Xiyu.AssetLoader;
using Xiyu.DataStructure;

namespace Xiyu.StandingIllustration
{
    public class Character : MonoBehaviour
    {
        private CanvasGroup _baseCanvasGroup;
        private Image _bodyImage;
        private readonly List<Image> _faceImages = new();

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

        private UniTask InitAsync(string addressableName)
        {
            _baseCanvasGroup = GetComponent<CanvasGroup>();
            _bodyImage = transform.Find("Body").GetComponent<Image>();

            AddressableName = addressableName;

            return UniTask.CompletedTask;
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
            _bodyImage.sprite = sprite;
            _bodyImage.rectTransform.Apply(lrt);
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

        private void AutoFillFaceImages(int targetCount)
        {
            if (_faceImages.Count == targetCount) return;

            if (_faceImages.Count < targetCount)
            {
                var count = targetCount - _faceImages.Count;
                for (var i = 0; i < count; i++)
                {
                    var faceImage = new GameObject("Face-item").AddComponent<Image>();
                    faceImage.transform.SetParent(transform);
                    _faceImages.Add(faceImage);
                }
            }
            else
            {
                for (var i = _faceImages.Count - 1; i >= targetCount; i--)
                {
                    Destroy(_faceImages[i].gameObject);
                    _faceImages.RemoveAt(i);
                }
            }
        }


        public static async UniTask<Character> CreateAsync(string addressableName, Transform parent, bool active, bool blocksRayCasts, IProgress<float> progress = null)
        {
            var character = (await CharacterPrefabricateAssetLoader.LoadInstanceObjectAsync(addressableName, parent, progress)).GetComponent<Character>();

            await character.InitAsync(addressableName);

            character.SetActive(active, blocksRayCasts);

            return character;
        }

        private void OnDestroy()
        {
            foreach (var faceImage in _faceImages)
            {
                faceImage.sprite = null;
            }

            CharacterPrefabricateAssetLoader.Release(AddressableName);
            Debug.Log("Character资源已经被释放！");
        }
    }
}