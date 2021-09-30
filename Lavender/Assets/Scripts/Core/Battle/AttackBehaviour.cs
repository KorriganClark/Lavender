using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lavender.Cardinal
{
    [ExecuteInEditMode]
    //[ExecuteAlways]
    public class AttackBehaviour : MonoBehaviour
    {
        public List<Collider> colliders = new List<Collider>();
        public List<Shape> shapes = new List<Shape>();
        void Start()
        {
            //Debug.Log("start");
            enabled = true;
            runInEditMode = true;
        }
        void Awake()
        {
            //Debug.Log("awake");
            enabled = true;
        }
        void OnEnable()
        {
            //Debug.Log("enable");
            enabled = true;

        }
        public void AddShape(Shape target)
        {
            target.OnCollisionEnter += Attack;
            shapes.Add(target);
        }
        void Update()
        {

        }
        private void Attack(Shape target)
        {
            var cardinalSystem = gameObject.GetComponentInParent<CardinalSystem>();
            cardinalSystem.ApplyAttackTo(target.gameObject);
        }
    }
}

