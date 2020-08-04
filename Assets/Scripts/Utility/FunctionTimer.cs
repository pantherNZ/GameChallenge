using System;
using UnityEngine;
using System.Collections.Generic;

public static partial class Utility
{
    class FunctionTimerHandler : MonoBehaviour
    {
        static FunctionTimerHandler functionTimerHandler;
        public static FunctionTimerHandler Instance
        {
            get
            {
                if( functionTimerHandler == null )
                {
                    var obj = new GameObject( "FunctionTimerHandler" );
                    functionTimerHandler = obj.AddComponent<FunctionTimerHandler>();
                }
                return functionTimerHandler;
            }
        }

        List<FunctionTimer> timerList = new List<FunctionTimer>();

        public FunctionTimer AddTimer( FunctionTimer timer )
        {
            timerList.Add( timer );
            return timerList.Back();
        }

        public FunctionTimer GetTimer( string name )
        {
            return timerList.Find( ( timer ) => { return timer.name == name; } );
        }

        public bool RemoveTimer( string name )
        {
            return RemoveTimer( GetTimer( name ) );
        }

        public bool RemoveTimer( FunctionTimer timer )
        {
            if( timer != null )
                timerList.Remove( timer );
            return timer != null;
        }

        void Update()
        {
            for( var idx = timerList.Count - 1; idx >= 0; --idx )
            {
                var timer = timerList[idx];
                timer.timeLeft -= ( timer.useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime );

                if( timer.timeLeft <= 0.0f )
                {
                    // Timer complete, trigger Action
                    timer.action();
                    
                    if( timer.loop )
                    {
                        timer.timeLeft += timer.duration;
                    }
                    else
                    {
                        timerList.RemoveAt( idx );

                        if( timerList.IsEmpty() )
                        {
                            functionTimerHandler.gameObject.Destroy();
                            functionTimerHandler = null;
                        }
                    }
                }
            }
        }
    }

    public class FunctionTimer
    {
        public static FunctionTimer CreateTimer( float duration, Action action, string name = "", bool loop = false, bool useUnscaledDeltaTime = false )
        {
            return FunctionTimerHandler.Instance.AddTimer( new FunctionTimer( duration, action, name, loop, useUnscaledDeltaTime ) );
        }

        public static bool StopTimer( string name )
        {
            return FunctionTimerHandler.Instance.RemoveTimer( name );
        }

        public static bool PauseTimer( string name )
        {
            var timer = FunctionTimerHandler.Instance.GetTimer( name );
            if( timer == null )
                return false;
            timer.active = false;
            return true;
        }

        public static bool ResumeTimer( string name )
        {
            var timer = FunctionTimerHandler.Instance.GetTimer( name );
            if( timer == null )
                return false;
            timer.active = true;
            return true;
        }

        public bool StopTimer()
        {
            return FunctionTimerHandler.Instance.RemoveTimer( this );
        }

        public void PauseTimer()
        {
            active = false;
        }

        public void ResumeTimer()
        {
            active = true;
        }

        public static FunctionTimer GetTimer( string name )
        {
            return FunctionTimerHandler.Instance.GetTimer( name );
        }

        public float duration;
        public float timeLeft;
        public string name;
        public bool active;
        public bool useUnscaledDeltaTime;
        public bool loop;
        public Action action;

        public FunctionTimer( float duration, Action action, string name, bool loop, bool useUnscaledDeltaTime )
        {
            this.action = action;
            this.duration = duration;
            this.timeLeft = duration;
            this.name = name;
            this.loop = loop;
            this.useUnscaledDeltaTime = useUnscaledDeltaTime;
        }
    }
}