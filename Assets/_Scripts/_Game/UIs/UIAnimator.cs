using System;

using UnityEngine;

namespace _Scripts._Game.UIs
{
    public partial class UIAnimator : MonoBehaviour
    {
        [SerializeField]
        private RectTransform visualRectTransform;

        [SerializeField]
        private UIAnimationType uiAnimationType;

        public float duration;
        public float delay;

        public bool loop;
        public bool pingPong;

        [SerializeField]
        private SlideInFromType slideInFromType;

        [SerializeField]
        private LeanTweenType easeType;
        
        public AnimationType animationType;

        private LTDescr _tweenObject;

        private bool _done = false;
        
        public void HandleTweening(RectTransform rectTransform)
        {
            visualRectTransform = rectTransform;
            
            switch (uiAnimationType)
            {
                case UIAnimationType.Move:
                    MoveObject();
                    break;

                case UIAnimationType.Fade:
                    HandleFading();
                    break;

                case UIAnimationType.Scale:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            _tweenObject.setDelay(delay);
            _tweenObject.setEase(easeType);

            if (loop)
            {
                _tweenObject.loopCount = int.MaxValue;
            }

            if (pingPong)
            {
                _tweenObject.setLoopPingPong();
            }
        }

        private void HandleFading()
        {
            var alphas = animationType switch
            {
                AnimationType.Show => (0, 1),
                AnimationType.Hide => (1, 0),
                _ => (1,1)
            };

            var canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = alphas.Item1;
            
            _tweenObject = LeanTween.alphaCanvas(canvasGroup, alphas.Item2, duration).setDelay(delay);
        }

        private void MoveObject()
        {
            var toPos =  visualRectTransform.anchoredPosition;
            
            var fromPos = GetStartPosition();
            visualRectTransform.position = fromPos;
            
            _tweenObject = LeanTween.move(visualRectTransform, toPos, duration).setDelay(delay);
        }

        private Vector2 GetStartPosition()
        {
            var rect = visualRectTransform.rect;
            var halfWidth = rect.width / 2;
            var halfHeight = rect.height / 2;
            
            // var screenWidth = resolution.width;
            // var screenHeight = resolution.height;
            
            var screenWidth = 1920;
            var screenHeight = 1080;

            var pos = visualRectTransform.position;
            
            switch (slideInFromType)
            {
                case SlideInFromType.Left:
                    var distanceLeft = pos.x - pos.x - halfWidth;
                    return new Vector2(distanceLeft, pos.y);

                case SlideInFromType.Right:
                    var distanceRight = pos.x - pos.x + screenWidth + halfWidth;
                    return new Vector2(distanceRight, pos.y);
                
                case SlideInFromType.Up:
                    var distanceUp = pos.y - pos.y + screenHeight + halfHeight;
                    return new Vector2(pos.x, distanceUp);
                    
                case SlideInFromType.Down:
                    var distanceDown = pos.y - pos.y - halfHeight;
                    return new Vector2(pos.x, distanceDown);
                default:
                    return visualRectTransform.position;
            }
        }
    }

    public enum UIAnimationType
    {
        Move,
        Fade,
        Scale,
    }

    public enum AnimationType
    {
        Show,
        Hide
    }

    public enum SlideInFromType
    {
        Left,
        Right,
        Up,
        Down
    }
}