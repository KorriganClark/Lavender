using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using Dest.Math;
using UnityEngine;

namespace Lavender
{
    [ExecuteInEditMode]
    public class LavenderPhysicSystem : MonoBehaviour
    {
        private static List<Shape> targets = new List<Shape>();
        void Update()
        {
            bool allvalid = false;
            while (allvalid)
            {
                var valid = true;
                foreach (var target in targets)
                {
                    if (target == null)
                    {
                        targets.Remove(target);
                        valid = false;
                        break;
                    }
                }
                allvalid = valid;
            }
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null)
                {
                    targets.Remove(targets[i]);
                    return;
                }
                for (int j = i + 1; j < targets.Count; j++)
                {
                    if (targets[j] == null || targets[i] == targets[j])
                    {
                        targets.Remove(targets[j]);
                        return;
                    }
                    if (targets[i].IntersectWith(targets[j]))
                    {
                        if (!targets[i].CurrentCollision.Contains(targets[j]))
                        {
                            if (targets[i].OnCollisionEnter != null)
                            {
                                targets[i].OnCollisionEnter(targets[j]);
                                targets[i].CurrentCollision.Add(targets[j]);
                            }
                            if (targets[j].OnCollisionEnter != null)
                            {
                                targets[j].OnCollisionEnter(targets[i]);
                                targets[j].CurrentCollision.Add(targets[i]);
                            }
                        }
                        else
                        {
                            if (targets[i].OnCollisionStay != null)
                                targets[i].OnCollisionStay(targets[j]);
                            if (targets[i].OnCollisionStay != null)
                                targets[j].OnCollisionStay(targets[i]);
                        }
                    }
                    else
                    {
                        if (targets[i].CurrentCollision.Contains(targets[j]))
                        {
                            if (targets[i].OnCollsionExit != null)
                                targets[i].OnCollsionExit(targets[j]);
                            if (targets[j].OnCollsionExit != null)
                                targets[j].OnCollsionExit(targets[i]);
                            targets[i].CurrentCollision.Remove(targets[j]);
                            targets[j].CurrentCollision.Remove(targets[i]);
                        }
                    }
                }
            }
        }
        public static void AddShape(Shape target)
        {
            targets.Add(target);
        }
    }

    public abstract class Shape : MonoBehaviour
    {
        void Start()
        {
            LavenderPhysicSystem.AddShape(this);
        }
        public abstract bool IntersectWith(Shape target);
        public Action<Shape> OnCollisionEnter;
        public Action<Shape> OnCollisionStay;
        public Action<Shape> OnCollsionExit;
        public List<Shape> CurrentCollision = new List<Shape>();
    }
    public class Box : Shape
    {
        public Vector3 center;
        public Vector3 size;
        public Box3 box3;
        private void OnDrawGizmos()
        {
            //DrawPoints(_points);
            RefreshBox();
            DrawBox(ref box3);
        }
        protected void DrawBox(ref Box3 box)
        {
            Vector3 v0, v1, v2, v3, v4, v5, v6, v7;
            box.CalcVertices(out v0, out v1, out v2, out v3, out v4, out v5, out v6, out v7);
            Gizmos.DrawLine(v0, v1);
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v3);
            Gizmos.DrawLine(v3, v0);
            Gizmos.DrawLine(v4, v5);
            Gizmos.DrawLine(v5, v6);
            Gizmos.DrawLine(v6, v7);
            Gizmos.DrawLine(v7, v4);
            Gizmos.DrawLine(v0, v4);
            Gizmos.DrawLine(v1, v5);
            Gizmos.DrawLine(v2, v6);
            Gizmos.DrawLine(v3, v7);
        }
        public void RefreshBox()
        {
            var transform = gameObject.transform;
            var angle = transform.rotation.eulerAngles;
            var newCenter = center.x * transform.up + center.y * transform.right + center.z * transform.forward;
            box3 = new Box3(newCenter + transform.position, transform.up, transform.right, transform.forward, size / 2);
        }

        public override bool IntersectWith(Shape target)
        {
            RefreshBox();
            if (target is Box)
            {
                var targetBox = (Box)target;
                targetBox.RefreshBox();
                return Intersection.TestBox3Box3(ref box3, ref targetBox.box3);
            }
            return false;
        }
    }

}