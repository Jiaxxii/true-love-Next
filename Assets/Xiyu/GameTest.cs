using System;
using UnityEngine;
using Xiyu.StandingIllustration;


namespace MyNamespace
{
    public class GameTest : MonoBehaviour
    {
        private async void Start()
        {
            var standingIllustrationLoader = new StandingIllustrationLoader();
            await standingIllustrationLoader.LoadStandingIllustrationAsync("真奈美a_0_1196");
        }
    }
}