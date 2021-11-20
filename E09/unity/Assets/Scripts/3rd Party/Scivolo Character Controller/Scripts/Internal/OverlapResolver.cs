//#define MB_DEBUG

using UnityEngine;
using static MenteBacata.ScivoloCharacterController.Internal.OverlapChecker;
using static MenteBacata.ScivoloCharacterController.Internal.DisplacementOfBestFitCalculator;

namespace MenteBacata.ScivoloCharacterController.Internal
{
    public static class OverlapResolver
    {
        private const int maxIterations = 3;

        internal const int maxOverlaps = 5;

        private static readonly Collider[] overlaps = new Collider[maxOverlaps];

        // Buffer for displacement directions from single colliders.
        private static readonly Vector3[] directions = new Vector3[maxOverlaps];

        // Buffer for displacement distances from single colliders.
        private static readonly float[] distances = new float[maxOverlaps];

        /// <summary>
        /// Tries to resolve capsule's overlaps, if it succeeds it return true, false otherwise.
        /// </summary>
        public static bool TryResolveCapsuleOverlap(CharacterCapsule capsule, in Vector3 initialPosition, float contactOffset, LayerMask collisionMask, out Vector3 suggestedPosition)
        {
            Vector3 toLowerCenter = capsule.ToLowerHemisphereCenter;
            Vector3 toUpperCenter = capsule.ToUpperHemisphereCenter;
            CapsuleCollider capsuleCollider = capsule.Collider;
            Quaternion rotation = capsule.Rotation;

            // It uses inflated capsule to collect and resolve overlaps but the original it's still used to check overlaps.
            float originalRadius = capsuleCollider.radius;
            float originalHeight = capsuleCollider.height;

            Vector3 currentPosition = initialPosition;
            bool success = false;
            bool isCapsuleInflated = false;

            for (int i = 0; i < maxIterations; i++)
            {
                Vector3 lowerCenter = currentPosition + toLowerCenter;
                Vector3 upperCenter = currentPosition + toUpperCenter;

                if (!CheckCapsuleOverlap(lowerCenter, upperCenter, originalRadius, collisionMask, capsuleCollider))
                {
                    success = true;
                    break;
                }

                // It inflates the capsule only if it needs to resolve overlaps, just in case changing collider's fields triggers something on
                // the unity side.
                if (!isCapsuleInflated)
                {
                    InflateCapsule(capsuleCollider, contactOffset);
                    isCapsuleInflated = true;
                }

                int overlapsCount = Physics.OverlapCapsuleNonAlloc(lowerCenter, upperCenter, capsuleCollider.radius, overlaps, collisionMask, QueryTriggerInteraction.Ignore);

                if (TryGetDisplacement(currentPosition, rotation, capsuleCollider, overlapsCount, out Vector3 displacement))
                {
                    currentPosition += displacement;

                    if (Math.IsCircaZero(displacement))
                        break;
                }
                else
                {
                    success = false;
                    break;
                }
            }

            if (isCapsuleInflated)
            {
                capsuleCollider.radius = originalRadius;
                capsuleCollider.height = originalHeight;
            }

            suggestedPosition = currentPosition;

            return success || !CheckCapsuleOverlap(currentPosition + toLowerCenter, currentPosition + toUpperCenter, originalRadius, collisionMask, capsuleCollider);
        }

        private static bool TryGetDisplacement(in Vector3 position, in Quaternion rotation, CapsuleCollider capsuleCollider, int overlapsCount, out Vector3 displacement)
        {
            int displacementsCount = PopulateDisplacements(position, rotation, capsuleCollider, overlapsCount);

            if (displacementsCount < 1)
            {
                displacement = default;
                return false;
            }

            if (displacementsCount > 1)
            {
                displacement = GetDisplacementOfBestFit(directions, distances, displacementsCount);

                // Sometime displacement of best fit can have very high magnitude so it's better to clamp it.
                Math.ClampMagnitude(ref displacement, capsuleCollider.radius);
            }
            else
                displacement = distances[0] * directions[0];

            return true;
        }

        private static int PopulateDisplacements(in Vector3 position, in Quaternion rotation, CapsuleCollider capsuleCollider, int overlapsCount)
        {
            int k = 0;

            for (int i = 0; i < overlapsCount; i++)
            {
                Collider otherCollider = overlaps[i];

                if (capsuleCollider == otherCollider)
                    continue;

                if (Physics.ComputePenetration(capsuleCollider, position, rotation, otherCollider, otherCollider.transform.position, otherCollider.transform.rotation,
                    out Vector3 direction, out float distance))
                {
                    directions[k] = direction;
                    distances[k++] = distance;
                }
            }

            return k;
        }

        private static void InflateCapsule(CapsuleCollider capsuleCollider, float offset)
        {
            capsuleCollider.radius += offset;
            capsuleCollider.height += 2f * offset;
        }
    }
}
