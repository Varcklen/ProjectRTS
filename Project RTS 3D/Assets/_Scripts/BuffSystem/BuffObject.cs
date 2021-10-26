using System.Collections;
using UnityEngine;

namespace Project.BuffSystem
{
    public class BuffObject : Buff
    {
        public bool isInvoked;

        private UnitInfo unit;

        private const float TICK_TIMER_MIN = 0.1f;

        private float time;
        private float tickTime;

        //ONLY use this to set tickTime
        private float TickTimeProp
        {
            set
            {
                tickTime = Mathf.Clamp(value, TICK_TIMER_MIN, Mathf.Infinity);
            }
        }

        private IBuffInvoke interfaceInvoke;

        private IBuffDoT interfaceDoT;

        private BuffStruct param;

        private Coroutine coroutineEnd;

        public BuffObject(BuffData buffData, UnitInfo unit)
        {
            this.unit = unit;
            SetBuffFromData(buffData);
        }

        //For the case when you need to use IBuffInvoke
        public void Invoke(BuffStruct param, float time, IBuffInvoke interfaceInvoke)
        {
            this.interfaceInvoke = interfaceInvoke;
            InvokeGeneral(time, param);
            coroutineEnd = unit.StartCoroutine(TimeEnd());
        }

        //For the case when you need to use IBuffDoT
        public void Invoke(BuffStruct param, float time, IBuffDoT interfaceDoT, float tickTime = 1f)
        {
            this.interfaceDoT = interfaceDoT;
            TickTimeProp = tickTime;
            InvokeGeneral(time, param);
            coroutineEnd = unit.StartCoroutine(TimeTick());
        }

        //For the case when you need to use IBuffInvoke and IBuffDoT
        public void Invoke(BuffStruct param, float time, IBuffInvoke interfaceInvoke, IBuffDoT interfaceDoT, float tickTime = 1f)
        {
            this.interfaceDoT = interfaceDoT;
            this.interfaceInvoke = interfaceInvoke;
            InvokeGeneral(time, param);
            TickTimeProp = tickTime;
            coroutineEnd = unit.StartCoroutine(TimeTick());
        }

        //Sets the data common to all Invoke methods.
        private void InvokeGeneral(float time, BuffStruct param)
        {
            this.param = param;
            this.time = time;
            if (coroutineEnd != null) unit.StopCoroutine(coroutineEnd);
            if (!isInvoked) interfaceInvoke?.BuffStart(param);
            isInvoked = true;
        }

        //Each frame is activated and after a certain time a tick is triggered
        IEnumerator TimeTick()
        {
            float updateTick = 0;
            float updateEnd = 0;
            while (updateEnd <= time)
            {
                updateEnd += Time.deltaTime;
                if (tickTime != 0)
                {
                    updateTick += Time.deltaTime;
                    if (updateTick >= tickTime)
                    {
                        updateTick = Time.deltaTime;
                        interfaceDoT.DoTUse(param);
                    }
                }
                yield return null;
            }
            BuffEnd();
        }

        //When finished, completes the buff.
        IEnumerator TimeEnd()
        {
            yield return new WaitForSeconds(time);
            BuffEnd();
        }

        //Completes all actions on the buff
        public void BuffEnd()
        {
            interfaceInvoke?.BuffEnd(param);
            unit.buffManager.RemoveBuff(this);
            isInvoked = false;
            if (coroutineEnd != null) unit.StopCoroutine(coroutineEnd);
        }
    }

    //Called when a signal is given that a buff has ticked.
    public interface IBuffDoT
    {
        void DoTUse(BuffStruct param);
    }

    //It works when receiving a signal "Buff has started" and "Buff has ended".
    public interface IBuffInvoke
    {
        void BuffStart(BuffStruct param);
        void BuffEnd(BuffStruct param);
    }

    //Transmits data about the owner of the buff and other parameters.
    public struct BuffStruct
    {
        public UnitInfo target;
        public int alternative; //If you need to use several buffs in an ability, then using this parameter you can set the "id" for the buff in the current ability.
    }
}