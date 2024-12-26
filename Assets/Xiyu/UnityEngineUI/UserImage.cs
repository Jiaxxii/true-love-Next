using UnityEngine;

namespace Xiyu.UnityEngineUI
{
    public class UserImage : UnityEngine.UI.Image
    {
        private RectTransform _rectTransform;

        [SerializeField] private Collider2D checkCollider;

        protected override void Start()
        {
            _rectTransform = transform as RectTransform;
            if (checkCollider == null)
                checkCollider = GetComponent<Collider2D>();
        }

        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransform, screenPoint, eventCamera, out var worldPoint);
            worldPoint.z = 0;

            return checkCollider.OverlapPoint(worldPoint);
        }
    }
}