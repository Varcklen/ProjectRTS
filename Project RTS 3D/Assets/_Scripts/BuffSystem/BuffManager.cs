using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.BuffSystem
{

    public class BuffManager
    {
        private UnitInfo unit;

        private List<BuffObject> buffs;

        public event EventHandler OnBuffListChanged;

        public BuffManager(UnitInfo unit)
        {
            buffs = new List<BuffObject>();
            this.unit = unit;
        }

        #region GetRegion
        public List<BuffObject> GetBuffList()
        {
            return buffs;
        }
        #endregion

        //Creates a buff for the unit. If the unit already has a similar buff, it returns it.
        public BuffObject CreateBuff(BuffData buffData)
        {
            BuffObject oldBuff = null;
            foreach (BuffObject buff in buffs)
            {
                if (buff.buffData == buffData)
                {
                    oldBuff = buff;
                    break;
                }
            }
            if (oldBuff != null)
            {
                return oldBuff;
            }
            else
            {
                BuffObject newBuff = new BuffObject(buffData, unit);
                buffs.Add(newBuff);
                OnBuffListChanged?.Invoke(this, EventArgs.Empty);
                return newBuff;
            }
        }

        //Adds an existing buff to a unit
        public BuffObject AddBuff(BuffObject buff)
        {
            if (buffs.Contains(buff)) return buff;
            buffs.Add(buff);
            OnBuffListChanged?.Invoke(this, EventArgs.Empty);
            return buff;
        }

        //Removes existing buff to a unit
        public void RemoveBuff(BuffObject buff)
        {
            if (!buffs.Contains(buff)) return;
            buffs.Remove(buff);
            OnBuffListChanged?.Invoke(this, EventArgs.Empty);
        }

        //Removes all buffs from a unit
        public void DispellAllBuffs(bool isDispelledCheck = true)
        {
            List<BuffObject> list = new List<BuffObject>(buffs);
            foreach (BuffObject buff in list)
            {
                if (buff.isDispelled || !isDispelledCheck)
                    buff.BuffEnd();
            }
        }

        //Removes all buffs from a unit of a certain color
        public void DispellAllBuffs(OutlineColor color)
        {
            List<BuffObject> list = new List<BuffObject>(buffs);
            foreach (BuffObject buff in list)
            {
                if (buff.outlineColor == color && buff.isDispelled)
                {
                    buff.BuffEnd();
                }
            }
        }
    }
}
