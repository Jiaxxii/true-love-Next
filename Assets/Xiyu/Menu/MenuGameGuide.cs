using ScriptableObjectSetting;
using UnityEngine;
using Xiyu.GlobalEffect;

namespace Xiyu.Menu
{
    public sealed class MenuGameGuide : MonoBehaviour
    {
        private async void Start()
        {
            // 显示顶层
            GlobalEffectManger.Instance.SetTopActive(true);
            // 不显示遮罩
            GlobalEffectManger.Instance.MaskEffect.Alpha = 0;

            // 弹出加载条并且异步初始化配置文件
            await GlobalEffectManger.Instance.LoaderEffect
                .LoadingWaitForUniTask(SceneRuleMapSettings.InitAsync());


            // 预加载"menu"场景的所有 rule map（除非超时）
            await SceneRuleMapSettings.PreloadAsync("menu", 10000);

            // 等待加载完成
            await GlobalEffectManger.Instance.LoaderEffect.TryEndLoading();

            // 淡出加载条
            await GlobalEffectManger.Instance.LoaderEffect.FadeOut(1);

            var spriteAsync = await SceneRuleMapSettings.GetSpriteAsync("menu", "rule002");
            Debug.Log(spriteAsync.name);
            Debug.Log((await SceneRuleMapSettings.GetSpriteAsync("menu", "rule002")).name);
        }
    }
}