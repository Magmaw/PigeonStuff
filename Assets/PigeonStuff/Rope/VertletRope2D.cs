﻿using UnityEngine;

namespace Pigeon
{
    [System.Serializable]
    public class VertletRope2D
    {
        [HideInInspector] public RopeSegment[] ropeSegments;
        public LineRenderer lineRenderer;
        public float width = 0.1f;

        [Space(10)]
        public bool autoSetSegmentLength = true;
        public bool interpolate;

        [Space(10)]
        public float maxLength = 50;
        public float looseRopeLength = 0.65f;
        public int segments;

        public float SegmentLength { get; private set; }
        public int constraintResolution = 8;
        public Vector2 gravityForce = new Vector2(0f, -1f);

        [Space(10)]
        public Vector3 startPosition;
        public Vector2 endPosition;

        /// <summary>
        /// Call this on initialization (Awake)
        /// </summary>
        public void Setup()
        {
            constraintResolution = segments * 2;

            ropeSegments = new RopeSegment[segments];
            SetSegmentLength();

            try
            {
                lineRenderer.widthMultiplier = width;
            }
            catch (System.Exception) { }
        }

        public void Setup(float length)
        {
            constraintResolution = segments * 2;

            ropeSegments = new RopeSegment[segments];
            SetSegmentLength(length);

            try
            {
                lineRenderer.widthMultiplier = width;
            }
            catch (System.Exception) { }
        }

        public void SetSegmentLength()
        {
            float length = Vector3.Distance(startPosition, endPosition) + looseRopeLength;
            if (length > maxLength)
            {
                length = maxLength;
            }
            SegmentLength = (float)length / segments;
        }

        public void SetSegmentLength(float length)
        {
            SegmentLength = (length + looseRopeLength) / segments;
        }

        /// <summary>
        /// Simulate should be called in a fixed timestep (FixedUpdate)
        /// </summary>
        public void Simulate()
        {
            if (autoSetSegmentLength)
            {
                SetSegmentLength();
            }

            for (int i = 1; i < segments; i++)
            {
                RopeSegment firstSegment = ropeSegments[i];
                Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
                firstSegment.posOld = firstSegment.posNow;
                firstSegment.posNow += velocity;
                firstSegment.posNow += gravityForce * Time.fixedDeltaTime;
                ropeSegments[i] = firstSegment;
            }

            for (int i = 0; i < constraintResolution; i++)
            {
                ApplyConstraint();
            }
        }

        void ApplyConstraint()
        {
            //Constrant to First Point 
            ropeSegments[0].posNow = startPosition;

            //Constrant to Second Point
            ropeSegments[ropeSegments.Length - 1].posNow = endPosition;

            RopeSegment firstSeg;
            RopeSegment secondSeg;
            Vector2 changeAmount;
            for (int i = 0; i < segments - 1; i++)
            {
                firstSeg = ropeSegments[i];
                secondSeg = ropeSegments[i + 1];

                float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
                float error = dist - SegmentLength;
                if (error < 0f)
                {
                    error = -error;
                }

                Vector2 changeDir = Vector2.zero;

                if (dist != 0f)
                {
                    if (dist > SegmentLength)
                    {
                        changeDir = (firstSeg.posNow - secondSeg.posNow) / dist;
                    }
                    else if (dist < SegmentLength)
                    {
                        changeDir = (secondSeg.posNow - firstSeg.posNow) / dist;
                    }
                }

                changeAmount = changeDir * error;
                if (i != 0)
                {
                    firstSeg.posNow -= changeAmount * 0.5f;
                    ropeSegments[i] = firstSeg;
                    secondSeg.posNow += changeAmount * 0.5f;
                    ropeSegments[i + 1] = secondSeg;
                }
                else
                {
                    secondSeg.posNow += changeAmount;
                    ropeSegments[i + 1] = secondSeg;
                }
            }
        }

        /// <summary>
        /// DrawRope should be called after other transformations are complete (LateUpdate)
        /// </summary>
        public void DrawRope()
        {
            Vector3[] ropePositions = new Vector3[segments];

            ropePositions[0] = startPosition;
            ropePositions[segments - 1] = endPosition;

            if (interpolate)
            {
                float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

                for (int i = 1; i < segments - 1; i++)
                {
                    Vector3 position = ropeSegments[i].posNow + (ropeSegments[i].posNow - ropeSegments[i].posOld) * t;
                    position.z = startPosition.z;
                    ropePositions[i] = position;
                }
            }
            else
            {
                for (int i = 1; i < segments - 1; i++)
                {
                    Vector3 position = ropeSegments[i].posNow;
                    position.z = startPosition.z;
                    ropePositions[i] = position;
                }
            }

            try
            {
                lineRenderer.positionCount = segments;
                lineRenderer.SetPositions(ropePositions);
            }
            catch (System.Exception e) { }
        }

        public struct RopeSegment
        {
            public Vector2 posNow;
            public Vector2 posOld;

            public RopeSegment(Vector2 pos)
            {
                posNow = pos;
                posOld = pos;
            }

            public static implicit operator Vector2(RopeSegment r) => r.posNow;
        }
    } 
}