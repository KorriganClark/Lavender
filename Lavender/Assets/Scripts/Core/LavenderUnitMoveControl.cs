using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Lavender
{
    [ExecuteInEditMode]
    public class LavenderUnitMoveControl : MonoBehaviour
    {
        public class MoveUnit
        {
            private float currentTime;
            /// <summary>
            /// 总时间
            /// </summary>
            private float endTime;
            /// <summary>
            /// 基点坐标
            /// </summary>
            private Transform baseTransform;
            /// <summary>
            /// 要控制的对象
            /// </summary>
            private Transform unitTransform;
            /// <summary>
            /// 是否跟随
            /// </summary>
            private bool followBase;
            /// <summary>
            /// 使用世界绝对坐标
            /// </summary>
            private bool useWorldPos;
            /// <summary>
            /// 初始位置
            /// </summary>
            private Vector3 originPosition;
            /// <summary>
            /// 目标位置、该变量为相对值
            /// </summary>
            private Vector3 targetPosition;
            /// <summary>
            /// 位移开始时目标位置的世界绝对坐标
            /// </summary>
            private Vector3 fixedTargetPosition;
            /// <summary>
            /// 根据是否跟随返回实时目标绝对坐标
            /// </summary>
            /// <value></value>
            private Vector3 GetTargetPosition
            {
                get
                {
                    var trueTargetPosition = fixedTargetPosition;
                    if (followBase)
                    {
                        trueTargetPosition = baseTransform.position + targetPosition;
                    }
                    return trueTargetPosition;
                }
            }
            /// <summary>
            /// 是否改变朝向
            /// </summary>
            private bool editRotation;
            /// <summary>
            /// 编辑模式下暂停
            /// </summary>
            private bool editorModePause;
            private bool completed = false;
            private AnimationCurve animCurve;
            private Vector3 currentPos
            {
                get
                {
                    if (endTime == 0)
                    {
                        endTime = 1;
                        currentTime = 1;
                    }
                    var curveVal = animCurve.Evaluate(currentTime / endTime);
                    //Debug.Log("time" + currentTime);
                   // Debug.Log("val" + curveVal);
                    return originPosition * (1-curveVal) + GetTargetPosition * (curveVal);
                }
            }
            public bool Completed
            {
                get
                {
                    return completed;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="totalTime"></param>
            /// <param name="unit"></param>
            /// <param name="targetPos"></param>
            /// <param name="useworldPos"></param>
            /// <param name="originPos"></param>
            /// <param name="baseTran"></param>
            /// <param name="useNowOriPos"></param>
            /// <param name="follow"></param>
            /// <param name="editRot"></param>
            /// <param name="pause"></param>
            /// <param name="currentTime"></param>
            /// <returns></returns>
            public MoveUnit(float totalTime, Transform unit, Vector3 targetPos, bool useworldPos = false, Vector3 originPos = new Vector3(), Transform baseTran = null, bool useNowOriPos = true, bool follow = false, bool editRot = false, bool pause = false, float currentTime = 0, AnimationCurve animationCurve = null)
            {
                this.currentTime = 0;
                endTime = totalTime;
                useWorldPos = useworldPos;
                if (!useWorldPos)
                {
                    baseTransform = baseTran;
                    if (baseTransform == null)
                    {
                        baseTransform = unit;
                    }

                }
                unitTransform = unit;
                followBase = follow;
                if (useNowOriPos && unitTransform)
                {
                    originPosition = unitTransform.position;
                    if (!useWorldPos)
                    {
                        //originPosition -= baseTransform.position;
                    }
                }
                else
                {
                    originPosition = originPos;
                }
                targetPosition = targetPos;
                if (baseTransform)
                {
                    fixedTargetPosition = targetPos + baseTransform.position;
                }
                editRotation = editRot;
                editorModePause = pause;
                if (editorModePause)
                {
                    this.currentTime = currentTime;
                }
                animCurve = animationCurve;
                if (animCurve == null)
                {
                    animCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }
            }
            public void Update(float deltaTime)
            {
                if (editorModePause || completed)
                {
                    return;
                }
                currentTime += deltaTime;
                if (currentTime >= endTime)
                {
                    EndInstant(true);
                    return;
                }
                //var currentPos = (originPosition) * (1 - currentTime / endTime) + (GetTargetPosition) * currentTime / endTime;
                unitTransform.position = currentPos;
            }
            public void EditPauseSetPos()
            {
                if (!editorModePause)
                {
                    return;
                }
                //var currentPos = (originPosition) * (1 - currentTime / endTime) + (GetTargetPosition) * currentTime / endTime;
                unitTransform.position = currentPos;
                EndInstant(false);
            }
            public void EndInstant(bool moveToTarget)
            {
                if (moveToTarget)
                {
                    currentTime = endTime;
                    unitTransform.position = currentPos;
                }
                completed = true;
                var moveControl = unitTransform.GetComponentInParent<LavenderUnitMoveControl>();
                moveControl.EndCurrentMove(this);
            }
        }
        private MoveUnit currentMove;
        private void ApplyMove(MoveUnit thisMove, bool lastInstantMove = false)
        {
            if (currentMove != null)
            {
                currentMove.EndInstant(lastInstantMove);
            }
            currentMove = thisMove;
            currentMove.Update(0);
        }
        private void EndCurrentMove(MoveUnit target)
        {
            if (target == currentMove)
            {
                currentMove = null;
            }
        }
        private void UpdateMove(float deltaTime)
        {
            currentMove.Update(deltaTime);
        }
        public void CompleteCurrentMove()
        {
            if (currentMove != null)
            {
                currentMove.EndInstant(true);
            }
        }
        public void TerminateCurrentMove()
        {
            if (currentMove != null)
            {
                currentMove.EndInstant(false);
            }
        }
        public void MoveByMoveUnit(MoveUnit action)
        {
            ApplyMove(action);
        }
        public void MoveRelative(Vector3 relativePos)
        {
            var moveUnit = new MoveUnit(0, gameObject.transform, relativePos);
            ApplyMove(moveUnit);
        }
        public void MoveRelative(Vector3 relativePos, float time)
        {
            var moveUnit = new MoveUnit(time, gameObject.transform, relativePos);
            ApplyMove(moveUnit);
        }
        public void MoveByBase(float totalTimeTime, Transform unit, Vector3 targetPos, Transform baseTransform, AnimationCurve curve)
        {
            var moveUnit = new MoveUnit(totalTimeTime, unit, targetPos, false, new Vector3(), baseTransform);
        }
        public void EditMoveToPosPaused(float totalTime, Transform unit, Vector3 targetPos, bool useworldPos = false, Vector3 originPos = new Vector3(), Transform baseTran = null, bool useNowOriPos = true, bool follow = false, bool editRot = false, float currentTime = 0, AnimationCurve curve = null)
        {
            var moveUnit = new MoveUnit(totalTime, unit, targetPos, useworldPos, originPos, baseTran, useNowOriPos, follow, editRot, true, currentTime, curve);
            TerminateCurrentMove();
            moveUnit.EditPauseSetPos();
        }
        void Update()
        {
            if (currentMove != null)
            {
                UpdateMove(Time.deltaTime);
            }
        }
    }
}