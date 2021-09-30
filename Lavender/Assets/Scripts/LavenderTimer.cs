using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Lavender
{
    public class LavenderTimer
    {
        private static LavenderTimer m_Instance = null;
        public static LavenderTimer Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new LavenderTimer();
                    m_Instance.stopwatch.Start();
                }
                return m_Instance;
            }
        }

        private LavenderTimer() { }

        private const float EPS = (float)1e-6;

        private int eventId = 0;
        //private Dictionary<int, CzcTimer> DTimers = new Dictionary<int, CzcTimer>();
        private Dictionary<int, TimerEvent> DEvents = new Dictionary<int, TimerEvent>();
        private HashSet<int> SEventsToRemove = new HashSet<int>();
        private List<TimerEvent> eventsToAdd = new List<TimerEvent>();
        //private List<int> eventsToRemove = new List<int>();
        private Stopwatch stopwatch = new Stopwatch();
        private float currentTime = 0;

        public int AddEvent(Action action,
                             float startTime = 0f,
                             float timerLength = 0f,
                             float loopingDeltaTime = 0f,
                             float runningSpeed = 1f,
                             bool isAutoStart = true,
                             bool isAutoDelete = true)
        {
            TimerEvent timerEvent = new TimerEvent(action,
                                                   startTime,
                                                   timerLength,
                                                   loopingDeltaTime,
                                                   runningSpeed,
                                                   isAutoStart,
                                                   isAutoDelete);
            this.eventsToAdd.Add(timerEvent);
            return timerEvent.Id;
        }

        public void StartEvent(int id)
        {
            TimerEvent timerEvent;
            if (this.DEvents.TryGetValue(id, out timerEvent))
            {
                timerEvent.Start();
            }
        }

        public void StopEvent(int id)
        {
            TimerEvent timerEvent;
            if (this.DEvents.TryGetValue(id, out timerEvent))
            {
                timerEvent.Stop();
            }
        }

        public void RemoveEvent(int id)
        {
            this.SEventsToRemove.Add(id);
        }

        public bool ContainsEvent(int id)
        {
            return this.DEvents.ContainsKey(id);
        }

        /// <summary>
        /// Ensure the timers can be updated.
        /// You can create an empty GameObject and add Main.cs to it to execute <see cref="UpdateTimers"/> function.
        /// </summary>
        public void UpdateEvents()
        {
            foreach (var timerEvent in eventsToAdd)
            {
                this.DEvents.Add(timerEvent.Id, timerEvent);
            }
            eventsToAdd.Clear();
            float deltaTime = (float)this.stopwatch.Elapsed.TotalSeconds - this.currentTime;
            this.currentTime += deltaTime;
            foreach (var keyValuePair in DEvents)
            {
                if (SEventsToRemove.Contains(keyValuePair.Key))
                {
                    continue;
                }
                keyValuePair.Value.TryUpdate(deltaTime);
            }
            foreach (var id in SEventsToRemove)
            {
                this.DEvents.Remove(id);
            }
            SEventsToRemove.Clear();
        }

        public void ClearEvents()
        {
            foreach (var keyValuePair in DEvents)
            {
                this.SEventsToRemove.Add(keyValuePair.Key);
            }
            this.eventId = 0;
        }

        public class Builder
        {
            //private Action action = null;
            private float startTime = 0f;
            private float timerLength = 0f;
            private float loopingDeltaTime = 0f;
            private float runningSpeed = 1f;
            private bool isAutoStart = true;
            private bool isAutoDelete = true;

            public Builder SetStartTime(float startTime) { this.startTime = startTime; return this; }
            public Builder SetTimerLength(float timerLength) { this.timerLength = timerLength; return this; }
            public Builder SetLooping(float loopingDeltaTime) { this.loopingDeltaTime = loopingDeltaTime; return this; }
            public Builder SetRunningSpeed(float runningSpeed) { this.runningSpeed = runningSpeed; return this; }
            public Builder SetIsAutoStart(bool isAutoStart) { this.isAutoStart = isAutoStart; return this; }
            public Builder SetIsAutoDelete(bool isAutoDelete) { this.isAutoDelete = isAutoDelete; return this; }
            /// <summary>
            /// 使每帧调用, 不能和SetLooping同时使用
            /// </summary>
            /// <returns></returns>
            public Builder SetExecuteInUpdate() { this.loopingDeltaTime = 0.033f; return this; }

            public int CreateTimer(Action action)
            {
                return LavenderTimer.Instance.AddEvent(action,
                                      this.startTime,
                                      this.timerLength,
                                      this.loopingDeltaTime,
                                      this.runningSpeed,
                                      this.isAutoStart,
                                      this.isAutoDelete);
            }
        }

        private class TimerEvent
        {
            private int m_Id = 0;
            public int Id
            {
                get
                {
                    if (m_Id == 0)
                    {
                        m_Id = ++LavenderTimer.Instance.eventId;
                    }
                    return this.m_Id;
                }
            }
            private readonly Action action;
            private readonly float startTime;
            private readonly float timerLength;
            private readonly float loopingDeltaTime;
            private readonly float runningSpeed;
            private readonly bool isAutoStart;
            private readonly bool isAutoDelete;
            private float endTime
            {
                get
                {
                    return this.startTime + this.timerLength;
                }
            }
            private bool isLooping
            {
                get
                {
                    return this.loopingDeltaTime > 0;
                }
            }
            private bool isEnd
            {
                get
                {
                    return this.endTime < this.currentTime;
                }
            }
            private float currentTime;
            private EventStatus eventStatus;

            private float nextExecuteTime;
            public TimerEvent(Action action,
                              float startTime = 0f,
                              float timerLength = 0f,
                              float loopingDeltaTime = 0f,
                              float runningSpeed = 1f,
                              bool isAutoStart = true,
                              bool isAutoDelete = true)
            {
                this.action = action;
                this.startTime = startTime;
                this.timerLength = timerLength;
                this.loopingDeltaTime = loopingDeltaTime;
                this.runningSpeed = runningSpeed;
                this.isAutoStart = isAutoStart;
                this.isAutoDelete = isAutoDelete;
                if (isAutoStart)
                {
                    this.eventStatus = EventStatus.Stop;
                }

                this.m_Id = ++LavenderTimer.Instance.eventId;
                this.nextExecuteTime = startTime;
                this.eventStatus = TimerEvent.EventStatus.Running;
            }

            public bool TryUpdate(float deltaTime)
            {
                deltaTime *= this.runningSpeed;
                if (this.eventStatus == TimerEvent.EventStatus.Stop)
                {
                    return false;
                }
                else if (this.eventStatus == TimerEvent.EventStatus.Running)
                {
                    float nextTime = this.currentTime + deltaTime;
                    TryExecute(this.currentTime, nextTime);
                    if (this.isAutoDelete && this.isEnd)
                    {
                        LavenderTimer.Instance.RemoveEvent(this.Id);
                    }
                    this.currentTime = nextTime;
                    return true;
                }
                return true;
            }

            private bool TryExecute(float previousTime, float currentTime)
            {
                if (isEnd)
                {
                    return false;
                }
                if (previousTime <= this.nextExecuteTime && this.nextExecuteTime <= currentTime)
                {
                    this.action();
                    if (this.isLooping)
                    {
                        this.nextExecuteTime += this.loopingDeltaTime;
                    }
                    return true;
                }
                //else if (currentTime <= this.nextExecuteTime && this.nextExecuteTime <= previousTime)
                //{
                //    // 反向执行
                //    // Todo: 添加反向执行的事件, 但目前还没这个需求
                //    this.action();
                //    if (this.isLooping)
                //    {
                //        this.nextExecuteTime -= this.loopingDeltaTime;
                //    }
                //    return true;
                //}
                return false;
            }

            private enum EventStatus
            {
                Running = 0,
                Stop = 1
            }

            public void Start()
            {
                this.eventStatus = TimerEvent.EventStatus.Running;
            }

            public void Stop()
            {
                this.eventStatus = TimerEvent.EventStatus.Stop;
            }
        }
    }

}