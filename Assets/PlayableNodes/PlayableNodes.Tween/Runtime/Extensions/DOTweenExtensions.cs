﻿#if UNITY_EDITOR
using DG.DOTweenEditor;
#endif
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using PlayableNodes.Values;
using UnityEngine;

namespace PlayableNodes
{
    public static class DOTweenExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PlayOrPreview<T>(this T tween) where T : Tween
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DOTweenEditorPreview.PrepareTweenForPreview(tween, false);
                DOTweenEditorPreview.Start();
            }
#endif

            return tween.Play();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RestartOrPreview<T>(this T tween) where T : Tween
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DOTweenEditorPreview.PrepareTweenForPreview(tween, false);
                DOTweenEditorPreview.Start();
            }
#endif
            tween.Restart();
            return tween;
        }

        public static Tweener DOMove(this Transform transform, MoveSpace space, ToFromValue<Vector3> to, float duration)
        {
            return space == MoveSpace.Global
                ? transform.DOMove(to.ConvertValue(transform.position), duration)
                : transform.DOLocalMove(to.ConvertValue(transform.localPosition), duration);
        }

        public static Tweener DORotate(this Transform transform, MoveSpace space, ToFromValue<Vector3> to,
            float duration)
        {
            return space == MoveSpace.Global
                ? transform.DORotate(to.ConvertValue(transform.position), duration)
                : transform.DOLocalRotate(to.ConvertValue(transform.localPosition), duration);
        }
        
        public static Sequence DOMoveConstraint(this Transform transform, 
            ToFromValue<Vector3> from,
            ToFromValue<Vector3> to,
            Easing x,
            Easing y,
            Easing z,
            float duration,
            bool recyclable = true)
        {
            var currentPosition = transform.position;
            var toPosition = to.ConvertValue(currentPosition);
            var fromPosition = from.ConvertValue(currentPosition);
            return DOMoveConstraint(transform, fromPosition, toPosition, x,y,z,duration, recyclable);
        }
        
        public static Sequence DOMoveConstraint(this Transform transform, 
            Vector3 from,
            Vector3 to,
            Easing x,
            Easing y,
            Easing z,
            float duration,
            bool recyclable = true)
        {
            return DOTween.Sequence()
                .Join(transform
                    .DOMoveX(to.x, duration)
                    .SetEase(x)
                    .ChangeStartValue(from)
                    .SetRecyclable(recyclable))
                .Join(transform
                    .DOMoveY(to.y, duration)
                    .SetEase(y)
                    .ChangeStartValue(from)
                    .SetRecyclable(recyclable))
                .Join(transform
                    .DOMoveZ(to.z, duration)
                    .SetEase(z)
                    .ChangeStartValue(from)
                    .SetRecyclable(recyclable));
        }
        
        
        public static Tweener DOFollowTarget(this Transform transform, Transform target, float duration)
        {
            var endPosition = target.position;
            var t = transform.DOMove(endPosition, duration);
            return t.OnUpdate(OnUpdate);
            
            void OnUpdate()
            {
                var position = target.position;
                if(position == endPosition)
                    return;
                endPosition = position;

                var newDuration = t.Duration() - Time.deltaTime;
                t.ChangeValuesVector(position, transform.position, newDuration);
                if (newDuration < 0f)
                {
                    t.OnUpdate(null);
                    t.Complete(true);
                }
            }
        }
    }
}