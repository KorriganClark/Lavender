using Lavender.Cardinal;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Lavender
{
    public enum TrackType
    {
        Anim,
        Audio,
        Particle,
        Attack,
        Move
    }
    public class LavenderTrack
    {

        public List<LavenderClip> lavenderClips;
        public Texture2D bar;
        public int height = 20;
        public float totalTime;
        public Rect trackRect;
        public float currentTime;
        public LavenderTimeLine owner;
        public TrackType trackType = TrackType.Audio;

        enum MenuType
        {
            ModifyClip,
            AddClip
        }
        MenuType menuType;
        public bool isShowingMenu;
        private Rect menuRect;
        private int menuChosedAnimId = -1;
        public float deviation = 0.03f;
        public bool IsDraggingAnim
        {
            get { return isDraggingAnim; }
        }
        public LavenderTrack()
        {
            bar = new Texture2D(500, height);
            //lavenderClips = new List<LavenderClip>();
            totalTime = 60;
        }
        /// <summary>
        /// 是否匹配当前轨道类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool TypeMatch(Type type)
        {
            switch (trackType)
            {
                case TrackType.Anim: return type == typeof(AnimationClip);
                case TrackType.Audio: return type == typeof(AudioClip);
                case TrackType.Particle: return type == typeof(ParticleSystem);
                case TrackType.Attack: return type == typeof(AttackField);
                case TrackType.Move: return type == typeof(LavenderUnitMoveControl.MoveUnit);
            }
            return false;
        }
        /// <summary>
        /// 生成新的轨道图片
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public Texture2D newBar(int width)
        {
            if (bar == null)
            {
                bar = new Texture2D(width, height);
            }
            bar.Resize(width, height);
            Color[] colors = new Color[width * height];
            for (int i = 0; i < width * height; i++)
            {
                switch (trackType)
                {
                    case TrackType.Anim: colors[i] = Color.white; break;
                    case TrackType.Audio: colors[i] = Color.Lerp(Color.green, Color.white, (float)0.8); break;
                    case TrackType.Particle: colors[i] = Color.Lerp(Color.blue, Color.white, (float)0.8); break;
                    case TrackType.Attack: colors[i] = Color.Lerp(Color.red, Color.white, (float)0.8); break;
                    case TrackType.Move: colors[i] = Color.Lerp(Color.yellow, Color.white, (float)0.8); break;
                }

            }
            //Texture2D backGroud = new Texture2D((int)(width), 20);
            DrawClips(width, colors);
            ///DrawTimeCur(width, colors);
            bar.SetPixels(0, 0, width, height, colors);
            bar.Apply();
            return bar;
        }
        /// <summary>
        /// 画出每个动画片段
        /// </summary>
        /// <param name="width"></param>
        public void DrawClips(int width, Color[] colors)
        {
            if (lavenderClips != null)
            {
                foreach (var clip in lavenderClips)
                {
                    int left = Math.Max(0, (int)(clip.startTime / totalTime * width));
                    int right = Math.Min(width, (int)(clip.ScaledEnd / totalTime * width));
                    int cutLeft = (int)(clip.ScaledValidStart / totalTime * width);
                    int cutRight = (int)(clip.ScaledValidEnd / totalTime * width);

                    for (int i = left; i < right; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            if (i + j * width >= width * height)
                            {
                                continue;
                            }
                            if (colors[i + j * width] == Color.black)
                            {
                                continue;
                            }
                            if (i < cutLeft || i > cutRight)
                            {
                                if (colors[i + j * width] != Color.gray)
                                {
                                    colors[i + j * width] = Color.Lerp(Color.gray, Color.white, (float)0.5);
                                }
                            }
                            else
                            {
                                if (colors[i + j * width] == Color.gray)
                                {
                                    colors[i + j * width] = Color.black;
                                }
                                else
                                {
                                    colors[i + j * width] = Color.gray;
                                }
                            }
                            if (i == left || i == right - 1 || j == 0 || j == height - 1 || i == cutLeft || i == cutRight - 1)
                            {
                                colors[i + j * width] = Color.black;
                            }
                        }
                    }
                }
            }


        }
        /// <summary>
        /// 绘制片段名称
        /// </summary>
        /// <param name="width"></param>
        public void DrawClipLabels(float width)
        {
            if (lavenderClips != null)
            {
                foreach (var clip in lavenderClips)
                {
                    //int left = Math.Max(0, (int)(clip.startTime / totalTime * width));
                    //int right = Math.Min(width, (int)(clip.ScaledEnd / totalTime * width));
                    int cutLeft = (int)(clip.ScaledValidStart / totalTime * width);
                    int cutRight = (int)(clip.ScaledValidEnd / totalTime * width);
                    if (cutRight - cutLeft > 30)
                    {
                        GUI.Label(new Rect(cutLeft + trackRect.x, trackRect.y, 200, 200), new GUIContent(clip.GetClipName()));
                    }
                }
            }
        }
        /// <summary>
        /// 绘制轨道
        /// </summary>
        /// <param name="width"></param>
        public void DrawTrack(Rect rect)
        {
            totalTime = owner.totalTime;

            trackRect = rect;//EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(20f));
            float width = trackRect.width;
            var id = DragAndDropUtilities.GetDragAndDropId(trackRect);
            DragAndDropUtilities.DrawDropZone(trackRect, null, null, id);
            object value = null;
            //根据轨道类型筛选可加入的文件
            switch (trackType)
            {
                case TrackType.Anim: value = DragAndDropUtilities.DropZone(trackRect, null, typeof(AnimationClip), id); break;
                case TrackType.Audio: value = DragAndDropUtilities.DropZone(trackRect, null, typeof(AudioClip), id); break;
                case TrackType.Particle: value = DragAndDropUtilities.DropZone(trackRect, null, typeof(ParticleSystem), id); break;
            }
            if (value == null && trackRect.Contains(Event.current.mousePosition))
            {
                if (DragAndDropManager.CurrentDraggingHandle != null)
                {
                    //DragHandle dragHandle = DragAndDropManager.CurrentDraggingHandle;
                    var tmp = DragAndDropManager.CurrentDraggingHandle.Object;
                    if (TypeMatch(tmp.GetType()))
                    {
                        //if (trackType != TrackType.Particle || ((GameObject)tmp).GetComponent<ParticleSystem>())
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                            if (DragAndDropManager.WasDragPerformed)
                            {
                                value = DragAndDropManager.CurrentDraggingHandle.Object;
                            }
                        }
                    }
                }
            }
            if (value != null)
            {
                float startTime = (Event.current.mousePosition.x - trackRect.x) / width * totalTime;
                GetNewClip(value, startTime);
            }
            AddModifyCutRect();
            if (Event.current.type == EventType.MouseDown)
            {
                TryChooseClip();
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !isDraggingAnim && !isModifingCut)
            {
                BeginDraggingClip();
            }
            if (Event.current.type == EventType.MouseDrag && isDraggingAnim == true)
            {
                RefreshDraggedAnim();
            }
            if (Event.current.type == EventType.MouseUp && isDraggingAnim == true)
            {
                EndDraggingAnim();
            }

            newBar((int)width);
            GUI.DrawTexture(trackRect, bar);
            DrawClipLabels(width);
            if (Event.current.type == EventType.MouseDown && isShowingMenu && !menuRect.Contains(Event.current.mousePosition))
            {
                isShowingMenu = false;
                menuChosedAnimId = -1;
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && !isShowingMenu && trackRect.Contains(Event.current.mousePosition))
            {
                isShowingMenu = true;
                if (GetClipIdByMousePos() != -1)
                {
                    menuType = MenuType.ModifyClip;
                }
                else
                {

                    menuType = MenuType.AddClip;
                }
                if (menuRect == null)
                {
                    menuRect = new Rect();
                }
                menuRect.position = Event.current.mousePosition;
                menuRect.size = new Vector2(100, 20);
                menuChosedAnimId = GetClipIdByMousePos();
            }
        }

        public float ToFrame(float time)
        {
            return (int)(time * 30 + 0.5) / 30f;
        }

        #region 修改片段起始、结束时间
        public bool isModifingCut = false;
        private int ModifingCutid;
        private bool modifingLeft = false;

        /// <summary>
        /// 拖拽边缘进行动画片段剪辑
        /// </summary>
        public void AddModifyCutRect()
        {
            float width = trackRect.width;
            if (lavenderClips != null)
            {
                foreach (var clip in lavenderClips)
                {
                    //int left = Math.Max(0, (int)(clip.startTime / totalTime * width));
                    //int right = Math.Min(width, (int)(clip.ScaledEnd / totalTime * width));
                    int cutLeft = (int)(clip.ScaledValidStart / totalTime * width);
                    int cutRight = (int)(clip.ScaledValidEnd / totalTime * width);
                    Rect leftRect = new Rect(trackRect.position.x + cutLeft - 2, trackRect.position.y, 4, trackRect.size.y);
                    Rect rightRect = new Rect(trackRect.position.x + cutRight - 2, trackRect.position.y, 4, trackRect.size.y);
                    EditorGUIUtility.AddCursorRect(leftRect, MouseCursor.ResizeHorizontal);
                    EditorGUIUtility.AddCursorRect(rightRect, MouseCursor.ResizeHorizontal);
                    if (leftRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && isModifingCut == false)
                        {
                            modifingLeft = true;
                            BeginModifyCut(lavenderClips.IndexOf(clip));
                        }
                        if (Event.current.type == EventType.MouseDrag && isModifingCut == true)
                        {
                            RefreshModifiedCut();
                        }
                        if (Event.current.type == EventType.MouseUp && isModifingCut == true)
                        {
                            EndModifyCut();
                        }
                    }
                    if (rightRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && isModifingCut == false)
                        {
                            modifingLeft = false;
                            BeginModifyCut(lavenderClips.IndexOf(clip));
                        }
                    }
                    if (Event.current.type == EventType.MouseDrag && isModifingCut == true)
                    {
                        RefreshModifiedCut();
                    }
                    if (Event.current.type == EventType.MouseUp && isModifingCut == true)
                    {
                        EndModifyCut();
                    }
                }
            }
        }
        public void BeginModifyCut(int id)
        {
            ModifingCutid = id;
            isModifingCut = true;
        }
        public void RefreshModifiedCut()
        {
            float ptrTime = GetTimebyMouPos();
            if (owner.owner.byFrame == 1)
            {
                ptrTime = ToFrame(ptrTime);
            }
            var clip = lavenderClips[ModifingCutid];
            if (modifingLeft)
            {
                clip.cutStartTime = Math.Min(clip.ScaledValidEnd - clip.ScaledStart - (float)0.1, (ptrTime - clip.startTime)) * clip.rate;
                //Debug.Log(clip.ScaledValidEnd - (float)0.1);
                if (clip.cutStartTime < 0)
                {
                    clip.cutStartTime = 0;
                }
            }
            else
            {
                clip.cutEndTime = Math.Min(clip.ScaledEnd - clip.ScaledValidStart - (float)0.1, (clip.ScaledEnd - ptrTime)) * clip.rate;
                if (clip.cutEndTime < 0)
                {
                    clip.cutEndTime = 0;
                }
            }
        }
        public void EndModifyCut()
        {
            ModifingCutid = -1;
            isModifingCut = false;
            owner.owner.SetScaleTotalTime(owner.owner.GetAnimTotalTime());
            owner.owner.DataSetDirty();
            //owner.owner.InitPlayGraph();
        }
        #endregion

        #region 选中动画
        public bool hasChosed;
        public int chosedClip;
        public void TryChooseClip()
        {
            if (GetClipIdByMousePos() != -1)
            {
                hasChosed = true;
                chosedClip = GetClipIdByMousePos();
                owner.owner.SetChosedClip(lavenderClips[chosedClip]);
            }
            else
            {
                CancelChooseClip();
            }
        }
        public void CancelChooseClip()
        {
            hasChosed = false;
            //chosedClip = -1;
        }
        public void SetChosedVal()
        {
            if (chosedClip != -1)
            {
                owner.owner.SetChosedVal(lavenderClips[chosedClip]);
            }
        }
        #endregion

        /// <summary>
        /// 根据鼠标位置获取所指时间
        /// </summary>
        /// <returns></returns>
        public float GetTimebyMouPos()
        {
            return owner.GetTimeByMousePos();
        }
        /// <summary>
        /// 根据鼠标位置获取所指的动画片段，-1代表未指向任何动画片段
        /// </summary>
        /// <returns></returns>
        public int GetClipIdByMousePos()
        {
            if (trackRect.Contains(Event.current.mousePosition))
            {
                float ptrTime = GetTimebyMouPos();
                if (lavenderClips == null)
                {
                    return -1;
                }
                for (int i = 0; i < lavenderClips.Count; i++)
                {
                    if (ptrTime <= lavenderClips[i].ScaledEnd && ptrTime >= lavenderClips[i].startTime)
                    {
                        return i;
                    }
                }
                return -1;
            }
            return -1;
        }

        #region 拖拽片段
        private bool isDraggingAnim;
        private int draggedAnimId;
        private float dragAtTime;
        /// <summary>
        /// 开始拖拽动画片段
        /// </summary>
        public void BeginDraggingClip()
        {
            if (isDraggingAnim)
            {
                return;
            }
            draggedAnimId = GetClipIdByMousePos();
            if (draggedAnimId != -1)
            {
                dragAtTime = GetTimebyMouPos() - lavenderClips[draggedAnimId].startTime;
                isDraggingAnim = true;
            }
        }
        /// <summary>
        /// 结束拖拽动画片段
        /// </summary>
        public void EndDraggingAnim()
        {
            draggedAnimId = -1;
            isDraggingAnim = false;

            owner.owner.SetScaleTotalTime(owner.owner.GetAnimTotalTime());
            owner.owner.DataSetDirty();
            //owner.owner.InitPlayGraph();
        }
        /// <summary>
        /// 刷新动画拖拽位置
        /// </summary>
        public void RefreshDraggedAnim()
        {
            LavenderClip clip = lavenderClips[draggedAnimId];
            float newStart = GetTimebyMouPos() - dragAtTime;
            float newCutStart = newStart + clip.cutStartTime / clip.rate;
            float newEnd = newStart + clip.length / clip.rate;
            float newCutEnd = newEnd - clip.cutEndTime / clip.rate;
            bool changeByFront = false;
            for (int i = 0; i < lavenderClips.Count; i++)
            {
                if (newCutStart < 0)
                {
                    newCutStart = 0;
                    changeByFront = true;
                    //                     newStart = newCutStart - clip.cutStartTime;
                    //                     newEnd = newStart + clip.length;
                }
                if (newCutEnd > totalTime)
                {
                    newCutEnd = totalTime;
                    //                     newEnd = newCutEnd + clip.cutEndTime;
                    //                     newStart = newEnd - clip.length;
                }
                if (i == draggedAnimId)
                    continue;
                //             if (newStart < lavenderClips[i].startTime && newEnd > lavenderClips[i].startTime)
                //             {
                //                 newEnd = lavenderClips[i].startTime;
                //                 newStart = newEnd - clip.length;
                //             }
                //             if (newStart > lavenderClips[i].startTime && lavenderClips[i].endTime > newStart)
                //             {
                // 
                //                 newStart = lavenderClips[i].endTime;
                //                 newEnd = newStart + clip.length;
                //             }
                if (Math.Abs(newCutStart - lavenderClips[i].ScaledValidStart) < deviation)
                {
                    newCutStart = lavenderClips[i].ScaledValidStart;
                    changeByFront = true;
                    //                     newStart = newCutStart-clip.cutStartTime;
                    //                     newEnd = newStart + clip.length;
                }
                if (Math.Abs(newCutStart - lavenderClips[i].ScaledValidEnd) < deviation)
                {
                    newCutStart = lavenderClips[i].ScaledValidEnd;
                    changeByFront = true;
                    //                     newStart = newCutStart - clip.cutStartTime;
                    //                     newEnd = newStart + clip.length;
                }
                if (Math.Abs(newCutEnd - lavenderClips[i].ScaledValidStart) < deviation)
                {
                    newCutEnd = lavenderClips[i].ScaledValidStart;
                    //                     newEnd = newCutEnd + clip.cutEndTime;
                    //                     newStart = newEnd - clip.length;
                }
                if (Math.Abs(newCutEnd - lavenderClips[i].ScaledValidEnd) < deviation)
                {
                    newCutEnd = lavenderClips[i].ScaledValidEnd;
                    //                     newEnd = newCutEnd + clip.cutEndTime;
                    //                     newStart = newEnd - clip.length;
                }

            }
            if (owner.owner.byFrame == 1)
            {
                newCutStart = ToFrame(newCutStart);
                newCutEnd = ToFrame(newCutEnd);
            }
            //再检查一次，还有冲突就不改了
            for (int i = 0; i < lavenderClips.Count; i++)
            {
                if (i == draggedAnimId)
                    continue;
                if (newCutStart < lavenderClips[i].ScaledValidStart && newCutEnd > lavenderClips[i].ScaledValidEnd)
                {
                    return;
                }
                if (newCutStart > lavenderClips[i].ScaledValidStart && lavenderClips[i].ScaledValidEnd > newCutEnd)
                {
                    return;
                }
                if (newCutStart == lavenderClips[i].ScaledValidStart && newCutEnd == lavenderClips[i].ScaledValidEnd)
                {
                    return;
                }
            }
            if (newCutEnd > totalTime || newCutStart < 0)
            {
                return;
            }
            if (changeByFront)
            {
                clip.ScaledValidStart = newCutStart;
            }
            else
            {
                clip.ScaledValidEnd = newCutEnd;
            }
        }
        #endregion

        /// <summary>
        /// 右键出现的菜单
        /// </summary>
        public void DrawAnimMenu()
        {
            //GUI.BeginGroup(rect);
            if (menuType == MenuType.ModifyClip)
            {
                if (GUI.Button(menuRect, "删除片段"))
                {
                    RemoveClip(menuChosedAnimId);
                    menuChosedAnimId = -1;
                    isShowingMenu = false;
                }
            }
            else
            {
                if (GUI.Button(menuRect, "添加片段"))
                {
                    //RemoveClip(menuChosedAnimId);
                    if (trackType == TrackType.Attack)
                    {
                        var attack = new AttackField();
                        GetNewClip(attack, GetTimebyMouPos());
                    }
                    else if (trackType == TrackType.Move)
                    {
                        var move = new LavenderUnitMoveControl.MoveUnit(0, null, new Vector3());
                        GetNewClip(move, GetTimebyMouPos());
                    }
                    menuChosedAnimId = -1;
                    isShowingMenu = false;
                }
            }
            //if(GUI.Button())
            //GUI.EndGroup();
        }
        /// <summary>
        /// 加载新片段
        /// </summary>
        /// <param name="newClip"></param>
        /// <param name="startTime"></param>
        public void GetNewClip(object newClip, float startTime)
        {
            LavenderClip newLavenderClip;
            switch (trackType)
            {
                case TrackType.Anim: newLavenderClip = new LavenderClip((AnimationClip)newClip); break;
                case TrackType.Audio: newLavenderClip = new LavenderClip((AudioClip)newClip); break;
                case TrackType.Particle: newLavenderClip = new LavenderClip((ParticleSystem)newClip); break;
                case TrackType.Attack: newLavenderClip = new LavenderClip((AttackField)newClip); break;
                default: newLavenderClip = new LavenderClip((LavenderUnitMoveControl.MoveUnit)newClip); break;
            }
            newLavenderClip.startTime = startTime;
            newLavenderClip.endTime += startTime;
            newLavenderClip.ownerConfig = owner.owner.skillConfig;
            lavenderClips.Add(newLavenderClip);
            owner.owner.InitPlayGraph();
            owner.owner.DataSetDirty();
        }
        /// <summary>
        /// 删除片段
        /// </summary>
        /// <param name="id"></param>
        public void RemoveClip(int id)
        {
            if (id != -1)
            {

                lavenderClips.RemoveAt(id);
                owner.owner.InitPlayGraph();
                owner.owner.DataSetDirty();
            }
        }
    }
}


