using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using ScriptableObjectSettings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Xiyu.AssetLoader;
using Xiyu.DataStructure;
using Xiyu.Maths;
using Xiyu.StandingIllustration;

namespace Xiyu.Select
{
    public class CharacterSelectPanel : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform root;

        [SerializeField] private float showPositionDuration = 0.5F;

        [SerializeField] private float respiratoryIntensity = 8F;
        [SerializeField] private float respiratoryRate = 3;

        private readonly Dictionary<string, Character> _characterDict = new();

        private ConcurrentDictionary<string, LightweightRectTransform> _characterShowPositionDict;


        private SceneAssetsSettings _sceneAssetsSettings;

        private Character _lastCharacter;

        private RectTransform _canvasTransform;

        private Sequence _lastSequence;


        public async UniTask Init(SceneAssetsSettings sceneAssetsSettings)
        {
            _sceneAssetsSettings = sceneAssetsSettings;

            _canvasTransform = (RectTransform)canvas.transform;

            var handle = Addressables.LoadAssetAsync<TextAsset>("SelectSceneCharacterShowPosition");
            await handle.ToUniTask();

            _characterShowPositionDict = JsonConvert.DeserializeObject<ConcurrentDictionary<string, LightweightRectTransform>>(handle.Result.text);
            Addressables.Release(handle);
        }


        public UniTask PopAsync(string characterCode)
        {
            var (bodySpriteInfo, faceSpriteInfo) = Find(characterCode);
            return PopAsync(characterCode, bodySpriteInfo, faceSpriteInfo);
        }

        public async UniTask PopAsync(string characterCode, LoaderSpriteInfo bodySpriteInfo, LoaderSpriteInfo faceSpriteInfo)
        {
            if (_lastCharacter != null)
            {
                _lastCharacter.SetActive(false, false);
            }

            var character = await LoadCharacterAsync(characterCode, bodySpriteInfo, faceSpriteInfo);

            if (_characterShowPositionDict.TryGetValue(characterCode, out var lrt))
            {
                character.Root.anchoredPosition = lrt.Position;
                character.Root.localEulerAngles = lrt.EulerAngle;


                character.Root.anchoredPosition = new Vector2(
                    -(_canvasTransform.sizeDelta.x.Half() + character.Root.sizeDelta.x.Half()),
                    character.Root.anchoredPosition.y);

                SetAnimation(character, ref lrt);
            }
            else Debug.LogWarning($"注意：没有找到角色({characterCode})的显示位置信息，配置文件中可能未定义或者名称拼写有误！");


            _lastCharacter = character;
            _lastCharacter.SetActive(true, false);
        }


        private void SetAnimation(Character character, ref LightweightRectTransform lrt)
        {
            character.Root.DOKill();
            character.Root.DOAnchorPosX(lrt.Position.x, showPositionDuration).SetEase(Ease.OutElastic);

            if (_lastCharacter != null)
                _lastSequence.Kill();

            character.Root.eulerAngles = character.Root.eulerAngles;

            _lastSequence = DOTween.Sequence()
                .Append(character.Root.DORotate(new Vector3(respiratoryIntensity, 0, 0), respiratoryRate).SetEase(Ease.Linear))
                .Append(character.Root.DORotate(new Vector3(-respiratoryIntensity, 0, 0), respiratoryRate).SetEase(Ease.Linear))
                .SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }

        private (LoaderSpriteInfo bodySpriteInfo, LoaderSpriteInfo faceSpriteInfo) Find(string characterCode)
        {
            var characterInfo = _sceneAssetsSettings.LoaderCharacterInfo.First(x => x.CharacterCode == characterCode);

            return (characterInfo.BodySpriteInfo, characterInfo.FaceSpriteInfo);
        }


        private async UniTask<Character> LoadCharacterAsync(string characterCode, LoaderSpriteInfo bodySpriteInfo, LoaderSpriteInfo faceSpriteInfo)
        {
            if (_characterDict.TryGetValue(characterCode, out var character))
            {
                return character;
            }


            character = await Character.CreateAsync(addressableName: characterCode, root, true, true);

            var illustrationLoader = StandingIllustrationLoaderManager.Get(characterCode);

            var bodyOffsetSettings = CharacterBodyOffsetSettingsManager.Get(characterCode);
            var bodyCode = bodySpriteInfo.ResourceCode[0];
            await character.UpdateBodyAsync(illustrationLoader, bodyOffsetSettings, bodyCode);

            var emotionsSettings = CharacterEmotionsSettingsManager.Get(characterCode);
            var emotionCode = faceSpriteInfo.ResourceCode[0];
            await character.UpdateFaceAsync(illustrationLoader, emotionsSettings, emotionCode);

            _characterDict.Add(characterCode, character);
            return character;
        }
    }
}