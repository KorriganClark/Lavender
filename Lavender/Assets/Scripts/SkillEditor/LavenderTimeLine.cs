using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Lavender
{
    public class LavenderTimeLine
    {
        public int trackNum;
        public float length;
        public int animationTrackNum;
        public float totalTime;
        public List<LavenderTrack> tracks = new List<LavenderTrack>();
        public SkillEditor owner;
        public Rect timeLineRect;
        public List<TrackType> trackTypes;
        private float time;
        private int frame;
        public float currentTime
        {
            get
            {
                if (owner.byFrame == 0)
                {
                    return time;
                }
                else
                {
                    return frame / 30f;
                }
            }
            set
            {
                if (owner.byFrame == 0)
                {
                    time = value;
                }
                else
                {
                    frame = (int)(value * 30 + 0.5);
                }
            }
        }
        public int currentFrame
        {
            get
            {
                if (owner.byFrame == 0)
                {
                    return (int)time * 30;
                }
                else
                {
                    return frame;
                }
            }
            set
            {
                if (owner.byFrame == 0)
                {
                    time = value / 30f;
                }
                else
                {
                    currentFrame = value;
                }
            }
        }
        public LavenderTimeLine()
        {

            //             tracks[0].trackType = TrackType.Anim;
            //             tracks[1].trackType = TrackType.Audio;
            //             tracks[2].trackType = TrackType.Particle;
        }
        public void Init()
        {
            totalTime = 10;
            trackNum = owner.skillConfig.LavenderClips.Count;
            trackTypes = owner.skillConfig.trackTypes;
            tracks.Clear();
            for (int i = 0; i < trackNum; i++)
            {
                tracks.Add(new LavenderTrack());
                tracks[i].owner = this;
                tracks[i].trackType = trackTypes[i];
                owner.SaveChosedVal += tracks[i].SetChosedVal;
            }
        }

        /// <summary>
        /// 绘制时间轴、轨道
        /// </summary>
        /// <param name="width"></param>
        public void DrawTimeLine(float width)
        {
            if (tracks.Count <= 0)
            {
                return;
            }
            width -= 40;
            EditorGUILayout.BeginVertical();
            timeLineRect = EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(20f));
            DrawScale();
            foreach (var track in tracks)
            {
                if (track.owner != this)
                {
                    track.owner = this;
                }
                Rect trackRect = EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(20f));
                track.DrawTrack(trackRect);
                if (track.trackType == TrackType.Anim)
                {
                    continue;
                }
                if (GUI.Button(new Rect(trackRect.position + new Vector2(trackRect.size.x, 0f), new Vector2(60f, 20f)), "delete"))
                {
                    owner.RemoveTrack(tracks.IndexOf(track));
                    tracks.Remove(track);
                    break;
                }
            }
            timeLineRect.height = tracks[0].height * (trackNum + 2);
            float curx = timeLineRect.x + width * currentTime / totalTime;
            if (owner.byFrame == 1)
            {
                curx = timeLineRect.x + width * currentFrame / (totalTime * 30);
            }
            Handles.color = Color.blue;
            Handles.DrawLine(new Vector3(curx, timeLineRect.y, 0), new Vector3(curx, timeLineRect.y + timeLineRect.height, 0));
            EditorGUILayout.EndVertical();
            if (timeLineRect.Contains(Event.current.mousePosition))
            {
                if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && (Event.current.button == 0))
                {
                    bool isTrackDragging = false;
                    foreach (var track in tracks)
                    {
                        if (track.IsDraggingAnim)
                        {
                            isTrackDragging = true;
                        }
                        if (track.isModifingCut)
                        {
                            isTrackDragging = true;
                        }
                    }
                    if (!isTrackDragging)
                    {
                        currentTime = GetTimeByMousePos();
                        if (currentTime > totalTime - 0.1)
                        {
                            totalTime += totalTime - currentTime;
                            owner.SetScaleTotalTime(totalTime);
                        }
                    }
                }
                if (Event.current.type == EventType.ScrollWheel)
                {
                    totalTime = Math.Max(owner.GetAnimTotalTime(), totalTime * (1 + Event.current.delta.y / 10));
                    owner.SetScaleTotalTime(totalTime);
                }
            }

            foreach (var track in tracks)
            {
                if (track.owner != this)
                {
                    track.owner = this;
                }
                if (track.isShowingMenu)
                {
                    if (Event.current.isMouse && Event.current.button == 1)
                    {
                        Event.current.Use();
                    }
                    track.DrawAnimMenu();
                }
            }

        }
        public float GetTimeByMousePos()
        {
            float ptrTime = (Event.current.mousePosition.x - timeLineRect.x) / timeLineRect.width * totalTime;
            ptrTime = Math.Max(0, ptrTime);
            ptrTime = Math.Min(totalTime, ptrTime);
            return ptrTime;
        }
        public void DrawScale()
        {
            bool drawLabel = true;
            if (owner.byFrame == 0)
            {
                DrawSpeciScale(30, 1, ref drawLabel);
                DrawSpeciScale(10, 2, ref drawLabel);
                DrawSpeciScale(5, 3, ref drawLabel);
                DrawSpeciScale(1, 4, ref drawLabel);
                DrawSpeciScale((float)0.2, 5, ref drawLabel);
                DrawSpeciScale((float)0.1, 6, ref drawLabel);
            }
            else
            {
                DrawSpeciScale(1f / 31f, 1, ref drawLabel);
            }
        }
        public void DrawSpeciScale(float singlevalue, int level, ref bool drawLabel)
        {
            if (totalTime / singlevalue > 100 || totalTime / singlevalue <= 0 || timeLineRect.width < 100)
            {
                return;
            }
            for (int i = 0; i < totalTime / singlevalue; i++)
            {
                Handles.color = Color.white;
                float beginHeight = timeLineRect.y + timeLineRect.height * level / 7;
                float endHeight = timeLineRect.y + timeLineRect.height;
                float curPos = timeLineRect.x + i * singlevalue * timeLineRect.width / totalTime;
                if (singlevalue < 1 / 30f + 0.01)
                {
                    curPos = timeLineRect.x + i * timeLineRect.width / (30 * totalTime);
                }
                if (curPos > timeLineRect.width)
                {
                    break;
                }
                //TODO 应当用DrawLines加速
                Handles.DrawLine(new Vector3(curPos, beginHeight, 0), new Vector3(curPos, endHeight, 0));
                if (drawLabel && totalTime / singlevalue >= 2)
                {
                    object labelValue = i * singlevalue;
                    if (owner.byFrame == 1)
                    {
                        labelValue = i;
                    }
                    GUI.Label(new Rect(curPos, timeLineRect.y, 100, 50), new GUIContent(Convert.ToString(labelValue)));
                }
            }
            if (drawLabel && totalTime / singlevalue >= 2)
            {
                drawLabel = false;
            }
        }
    }
}
