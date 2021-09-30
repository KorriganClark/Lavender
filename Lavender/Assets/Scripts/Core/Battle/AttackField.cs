using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
namespace Lavender.Cardinal
{
    [Serializable]
    public class AttackField
    {
        private GameObject attackObj;
        [Serializable]
        public class LavenderCollider
        {
            public Vector3 Center;
            public Vector3 Size = new Vector3(1, 1, 1);
            public Quaternion Rotation;
            //public Mesh mesh;
            //public Material Mat;
            //public float Radius;
            //public float Height;
            //public CapsuleDirection Direction;
            [HideInInspector]
            public Collider collider;
            public Shape shape;
        }
        [OnValueChanged("Refesh", includeChildren: true)]
        public List<LavenderCollider> colliders;
        [OnValueChanged("Refesh", includeChildren: true)]
        public bool showRender = false;
        private AttackBehaviour behaviour;
        private string parentTransformName;
        private Transform parentTransform;
        public enum CapsuleDirection
        {
            XAxis,
            YAxis,
            ZAxis
        }
        Vector3[] vertices = {
            new Vector3 (0, 0, 0),
            new Vector3 (1, 0, 0),
            new Vector3 (1, 1, 0),
            new Vector3 (0, 1, 0),
            new Vector3 (0, 1, 1),
            new Vector3 (1, 1, 1),
            new Vector3 (1, 0, 1),
            new Vector3 (0, 0, 1),
        };

        int[] triangles = {
            0, 2, 1, //face front
			0, 3, 2,
            2, 3, 4, //face top
			2, 4, 5,
            1, 2, 5, //face right
			1, 5, 6,
            0, 7, 4, //face left
			0, 4, 3,
            5, 4, 7, //face back
			5, 7, 6,
            0, 6, 7, //face bottom
			0, 1, 6
        };
        public void Init(Transform parent)
        {
            parentTransformName = parent.name;
            parentTransform = parent;
            Refesh();
        }
        public void Refesh()
        {
            if (attackObj != null)
            {
                OnDisable();
                OnEnable();
            }
        }
        public void OnEnable()
        {
            //Debug.Log(this);
            if (attackObj == null)
            {
                attackObj = new GameObject("attack!!！");
                behaviour = attackObj.AddComponent<AttackBehaviour>();
            }
            var currentcolliders = attackObj.GetComponents<Collider>();

            if (currentcolliders.Length == 0 && colliders != null)
            {
                foreach (var collider in colliders)
                {
                    var comp = attackObj.AddComponent<BoxCollider>();
                    comp.center = collider.Center;
                    comp.size = collider.Size;
                    //comp.radius = collider.Radius;
                    //comp.height = collider.Height;
                    //comp.direction = (int)collider.Direction;
                    comp.isTrigger = true;
                    collider.collider = comp;
                    if (showRender && !Application.isPlaying)
                    {
                        AddRender(collider);
                    }
                    behaviour.colliders.Add(comp);
                    //自己写的组件
                    var box = attackObj.AddComponent<Box>();
                    box.center = collider.Center;
                    box.size = collider.Size;
                    LavenderPhysicSystem.AddShape(box);
                    collider.shape = box;
                    behaviour.AddShape(box);
                }
            }
            attackObj.transform.SetParent(parentTransform);
            attackObj.transform.localPosition = new Vector3(0, 0, 0);
            attackObj.transform.rotation = new Quaternion();
        }
        public void AddRender(LavenderCollider collider)
        {
            var renderObj = new GameObject("cube");
            var meshFilter = renderObj.AddComponent<MeshFilter>();
            var mesh = new Mesh();
            var render = renderObj.AddComponent<MeshRenderer>();
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            meshFilter.mesh = SkillEditor.editorConfig.mesh;//collider.mesh;
            render.material = SkillEditor.editorConfig.material;//collider.Mat;
            renderObj.transform.SetParent(attackObj.transform);
            renderObj.transform.position = collider.Center;
            renderObj.transform.localScale = collider.Size;
        }
        public void OnDisable()
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(attackObj);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(attackObj);
            }
        }
    }
}

