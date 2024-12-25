using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    public class XiyuTools : EditorWindow
    {
        [MenuItem("Tools/西雨工具箱")]
        private static void ShowWindow()
        {
            var window = GetWindow<XiyuTools>();
            window.titleContent = new UnityEngine.GUIContent("TITLE");
            window.Show();
        }


        private RectTransform _root;
        private string _characterCode;
        private string _faceEmotionCode;

        private bool _isCreateGuideSettings;

        private void OnGUI()
        {
            _root = (RectTransform)EditorGUILayout.ObjectField(_root, typeof(RectTransform), true);

            if (_root == null) return;


            _characterCode = EditorGUILayout.TextField(label: "角色代码", text: _characterCode);
            _faceEmotionCode = EditorGUILayout.TextField(label: "表情代号", text: _faceEmotionCode);
            _isCreateGuideSettings = EditorGUILayout.Toggle(label: "生成角色引导", _isCreateGuideSettings);

            if (!string.IsNullOrEmpty(_faceEmotionCode) && GUILayout.Button("追加表情信息"))
            {
                AppendEmotionItemSettings(_root);
                AssetDatabase.SaveAssets();
            }

            if (!string.IsNullOrEmpty(_characterCode) && GUILayout.Button("拷贝身体信息"))
            {
                CopyBodyOffsetToJson(_root);
            }


            if (!GUILayout.Button("保存立绘偏移信息"))
            {
                return;
            }

            BodyOffsetSaveToJson(_root);
            EmotionsSaveToJson(_root);
            AssetDatabase.SaveAssets();
        }


        private void CopyBodyOffsetToJson(Transform root)
        {
            if (root is not RectTransform rect)
            {
                throw new InvalidCastException($"根节点不是(\"{root.gameObject.name}\")RectTransform类型");
            }

            var lrt = new
            {
                Position = new { rect.anchoredPosition.x, rect.anchoredPosition.y },
                Size = new { Width = rect.sizeDelta.x, Height = rect.sizeDelta.y },
                Pivot = new { rect.pivot.x, rect.pivot.y },
                EulerAngles = new { rect.eulerAngles.x, rect.eulerAngles.y, rect.eulerAngles.z },
                Scale = new { rect.localScale.x, rect.localScale.y }
            };

            var jsonContent = JsonConvert.SerializeObject(lrt, Formatting.Indented);

            GUIUtility.systemCopyBuffer = $"\"{_characterCode}\":{jsonContent}";

            Debug.Log($"\"{_characterCode}\" 身体信息已拷贝到剪贴板");

            _root = null;
            _characterCode = string.Empty;
        }

        private void BodyOffsetSaveToJson(Transform root)
        {
            var bodyDictionary = new Dictionary<string, dynamic>();
            var isFirst = true;
            for (var i = 0; i < root.childCount; i++)
            {
                var image = CheckHasSprite(root.GetChild(i), "Body");

                if (image == null)
                {
                    continue;
                }

                if (_isCreateGuideSettings && isFirst)
                {
                    isFirst = false;
                    CreateGuideSettings(image.transform);
                }

                var lrt = new
                {
                    Position = new { image.rectTransform.anchoredPosition.x, image.rectTransform.anchoredPosition.y },
                    Size = new { Width = image.rectTransform.sizeDelta.x, Height = image.rectTransform.sizeDelta.y },
                    Pivot = new { image.rectTransform.pivot.x, image.rectTransform.pivot.y },
                    EulerAngles = new { image.rectTransform.eulerAngles.x, image.rectTransform.eulerAngles.y, image.rectTransform.eulerAngles.z },
                    Scale = new { image.rectTransform.localScale.x, image.rectTransform.localScale.y }
                };

                bodyDictionary.TryAdd(image.sprite.name, lrt);
            }

            var directory = Path.Combine(Application.dataPath, "Settings", _characterCode);
            Directory.CreateDirectory(directory);

            var file = EditorUtility.SaveFilePanel("立绘偏移信息保存路径", directory, $"{_characterCode}_BodyOffsetSettings.json", "json");
            if (string.IsNullOrEmpty(file))
            {
                Debug.Log("操作取消");
                return;
            }

            var jsonContent = JsonConvert.SerializeObject(bodyDictionary, Formatting.Indented);
            File.WriteAllText(file, jsonContent, Encoding.UTF8);
            Debug.Log("立绘偏移信息保存成功");
        }

        private List<dynamic> EmotionsSaveToJson(Transform root)
        {
            var faceData = new List<dynamic>();
            for (var i = 0; i < root.childCount; i++)
            {
                var image = CheckHasSprite(root.GetChild(i), "Face");

                if (image == null)
                {
                    continue;
                }

                var data = new
                {
                    AddressableName = image.sprite.name,
                    Transform = new
                    {
                        Position = new { image.rectTransform.anchoredPosition.x, image.rectTransform.anchoredPosition.y },
                        Size = new { Width = image.rectTransform.sizeDelta.x, Height = image.rectTransform.sizeDelta.y },
                        Pivot = new { image.rectTransform.pivot.x, image.rectTransform.pivot.y },
                        EulerAngles = new { image.rectTransform.eulerAngles.x, image.rectTransform.eulerAngles.y, image.rectTransform.eulerAngles.z },
                        Scale = new { image.rectTransform.localScale.x, image.rectTransform.localScale.y }
                    }
                };

                faceData.Add(data);
            }

            var directory = Path.Combine(Application.dataPath, "Settings", _characterCode);
            Directory.CreateDirectory(directory);

            var file = EditorUtility.SaveFilePanel("表情信息保存路径", directory, $"{_characterCode}_EmotionsSettings.json", "json");
            if (string.IsNullOrEmpty(file))
            {
                Debug.Log("操作取消");
                return null;
            }

            var jsonContent = JsonConvert.SerializeObject(faceData, Formatting.Indented);
            File.WriteAllText(file, $"{{{Environment.NewLine}\"{_faceEmotionCode}\":{jsonContent}{Environment.NewLine}}}", Encoding.UTF8);
            Debug.Log("立绘偏移信息保存成功");

            return faceData;
        }

        private void CreateGuideSettings(Transform body)
        {
            var image = CheckHasSprite(body, "Body");
            if (image == null)
            {
                return;
            }

            var instance = new
            {
                DefaultCharacterCode = image.sprite.name,
                DefaultEmotionCode = _faceEmotionCode,
                DefaultTransform = new
                {
                    Position = new { image.rectTransform.anchoredPosition.x, image.rectTransform.anchoredPosition.y },
                    Size = new { Width = image.rectTransform.sizeDelta.x, Height = image.rectTransform.sizeDelta.y },
                    Pivot = new { image.rectTransform.pivot.x, image.rectTransform.pivot.y },
                    EulerAngles = new { image.rectTransform.eulerAngles.x, image.rectTransform.eulerAngles.y, image.rectTransform.eulerAngles.z },
                    Scale = new { image.rectTransform.localScale.x, image.rectTransform.localScale.y }
                },
                OptionSelected = new
                {
                    FirstEncounter = new
                    {
                        Emotion = "未定义-生气（可选）"
                    },
                    AtThisTime = new
                    {
                        Emotion = "未定义-微笑（可选）"
                    },
                    AtThatTime = new
                    {
                        Emotion = "未定义-疑惑（可选）"
                    },
                    Examination = new
                    {
                        Emotion = "未定义-期待（可选）"
                    },
                    Conclusion = new
                    {
                        Emotion = "未定义-担心（必选）"
                    }
                }
            };

            var directory = Path.Combine(Application.dataPath, "Settings", _characterCode);
            Directory.CreateDirectory(directory);

            var file = EditorUtility.SaveFilePanel("角色默认向导信息", directory, $"{_characterCode}_GuideCharacterSettings.json", "json");
            if (string.IsNullOrEmpty(file))
            {
                Debug.Log("操作取消");
                return;
            }

            var jsonContent = JsonConvert.SerializeObject(instance, Formatting.Indented);
            File.WriteAllText(file, jsonContent, Encoding.UTF8);
            Debug.Log("角色默认向导信息保存成功");
        }

        private void AppendEmotionItemSettings(Transform root)
        {
            var faceData = new List<dynamic>();
            for (var i = 0; i < root.childCount; i++)
            {
                var image = CheckHasSprite(root.GetChild(i), "Face");

                if (image == null)
                {
                    continue;
                }

                var data = new
                {
                    AddressableName = image.sprite.name,
                    Transform = new
                    {
                        Position = new { image.rectTransform.anchoredPosition.x, image.rectTransform.anchoredPosition.y },
                        Size = new { Width = image.rectTransform.sizeDelta.x, Height = image.rectTransform.sizeDelta.y },
                        Pivot = new { image.rectTransform.pivot.x, image.rectTransform.pivot.y },
                        EulerAngles = new { image.rectTransform.eulerAngles.x, image.rectTransform.eulerAngles.y, image.rectTransform.eulerAngles.z },
                        Scale = new { image.rectTransform.localScale.x, image.rectTransform.localScale.y }
                    }
                };

                faceData.Add(data);
            }

            var directory = Path.Combine(Application.dataPath, "Settings", _characterCode);
            var openFile = EditorUtility.OpenFilePanel("表情信息保存路径", directory, "json");
            if (string.IsNullOrEmpty(openFile))
            {
                Debug.Log("操作取消");
                return;
            }


            var rawJsonContent = File.ReadAllText(openFile);
            var jsonContent = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(rawJsonContent);

            if (!jsonContent.TryAdd(_faceEmotionCode, faceData))
            {
                Debug.LogError("表情代号已存在，请重新输入！");
                return;
            }

            File.WriteAllText(openFile, JsonConvert.SerializeObject(jsonContent, Formatting.Indented), Encoding.UTF8);
            Debug.Log("表情信息追加成功");
        }


        [CanBeNull]
        private static Image CheckHasSprite(Transform current, string startWith)
        {
            var rectTransform = current as RectTransform;
            if (rectTransform == null)
            {
                Debug.LogWarning($"发现\"<color=#b4b7b5>{current.gameObject.name}</color>\"节点对象不是<color=#c191ff>RectTransform</color>类型，已跳过!");
                return null;
            }

            if (!rectTransform.name.StartsWith(startWith, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"发现\"<color=#b4b7b5>{rectTransform.name}</color>\"节点对象不是以\"<color=#f2ff28>{startWith}</color>\"开头，已跳过");
                return null;
            }

            var image = rectTransform.GetComponent<Image>();
            if (image == null || image.sprite == null)
            {
                Debug.LogWarning($"\"<color=#b4b7b5>{rectTransform.name}</color>\"节点对象没有<color=#c191ff>Image</color>组件或者没有指定身体<color=#c191ff>Sprite</color>，已跳过");
                return null;
            }

            return image;
        }
    }
}