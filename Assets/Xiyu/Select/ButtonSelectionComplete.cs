using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Xiyu.Select
{
    public class ButtonSelectionComplete : MonoBehaviour
    {
        [SerializeField] private Button button;

        [SerializeField] private Vector3 targetScale = new(1f, 1f, 1f);

        [SerializeField] private float duration = 0.5f;

        public event Action OnSelectionCompleteClick;


        private void Start()
        {
            button.onClick.AddListener(() => { OnSelectionCompleteClick?.Invoke(); });
        }


        public void PopUp(bool use)
        {
            if (use)
            {
                transform.localScale = Vector3.zero;
                transform.DOKill();
                transform.DOScale(targetScale, duration).SetEase(Ease.OutBounce);
            }
            else
            {
                transform.DOKill();
                transform.DOScale(Vector3.zero, duration).SetEase(Ease.OutBounce);
            }
        }
    }
}