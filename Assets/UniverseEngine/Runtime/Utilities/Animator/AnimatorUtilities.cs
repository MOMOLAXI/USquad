using UnityEngine;

namespace UniverseEngine
{
    public static class AnimatorUtilities
    {
        /// <summary>
        /// Gets the current clip effective length, that is, the original length divided by the playback speed. The length value is always positive, regardless of the speed sign. 
        /// It returns false if the clip is not valid.
        /// </summary>
        public static bool GetCurrentClipLength(this Animator animator, ref float length)
        {
            if (animator.runtimeAnimatorController == null)
                return false;

            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

            if (clipInfo.Length == 0)
                return false;


            float clipLength = clipInfo[0].clip.length;
            float speed = animator.GetCurrentAnimatorStateInfo(0).speed;


            length = Mathf.Abs(clipLength / speed);

            return true;
        }

        public static bool MatchTarget(this Animator animator, Vector3 targetPosition, Quaternion targetRotation, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime)
        {
            if (animator.runtimeAnimatorController == null)
                return false;

            if (animator.isMatchingTarget)
                return false;

            if (animator.IsInTransition(0))
                return false;

            MatchTargetWeightMask weightMask = new(Vector3.one, 1f);

            animator.MatchTarget(targetPosition,
                                 targetRotation,
                                 avatarTarget,
                                 weightMask,
                                 startNormalizedTime,
                                 targetNormalizedTime);


            return true;
        }

        public static bool MatchTarget(this Animator animator, Vector3 targetPosition, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime)
        {
            if (animator.runtimeAnimatorController == null)
                return false;

            if (animator.isMatchingTarget)
                return false;

            if (animator.IsInTransition(0))
                return false;

            MatchTargetWeightMask weightMask = new(Vector3.one, 0f);

            animator.MatchTarget(targetPosition,
                                 Quaternion.identity,
                                 avatarTarget,
                                 weightMask,
                                 startNormalizedTime,
                                 targetNormalizedTime);

            return true;
        }

        public static bool MatchTarget(this Animator animator, Transform target, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime)
        {
            if (animator.runtimeAnimatorController == null)
                return false;

            if (animator.isMatchingTarget)
                return false;

            if (animator.IsInTransition(0))
                return false;

            MatchTargetWeightMask weightMask = new(Vector3.one, 1f);

            animator.MatchTarget(target.position,
                                 target.rotation,
                                 avatarTarget,
                                 weightMask,
                                 startNormalizedTime,
                                 targetNormalizedTime);


            return true;
        }

        public static bool MatchTarget(this Animator animator, Transform target, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime, MatchTargetWeightMask weightMask)
        {
            if (animator.runtimeAnimatorController == null)
                return false;

            if (animator.isMatchingTarget)
                return false;

            if (animator.IsInTransition(0))
                return false;

            animator.MatchTarget(target.position,
                                 target.rotation,
                                 AvatarTarget.Root,
                                 weightMask,
                                 startNormalizedTime,
                                 targetNormalizedTime);


            return true;
        }
    }
}
