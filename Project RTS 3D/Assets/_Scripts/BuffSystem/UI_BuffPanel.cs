using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Project.BuffSystem
{
    public class UI_BuffPanel : MonoBehaviour
    {
        private List<Image> buffIcons;
        private List<Image> buffOutlines;

        private BuffManager buffManager;

        private UnitInfo unit;

        private void Awake()
        {
            buffIcons = new List<Image>();
            buffOutlines = new List<Image>();
            List<GameObject> childs = GlobalMethods.GetChildrens(gameObject);
            foreach (GameObject child in childs)
            {
                buffIcons.Add(child.GetComponent<Image>());
                buffOutlines.Add(child.transform.GetChild(0).GetComponent<Image>());
            }
        }

        //Sets the unit that will have the buff panel visible
        public void SetUnit(UnitInfo unit)
        {
            this.unit = unit;
            if (buffManager != null) buffManager.OnBuffListChanged -= BuffManager_OnBuffListChanged;
            buffManager = unit?.buffManager;
            if (buffManager != null) buffManager.OnBuffListChanged += BuffManager_OnBuffListChanged;
            RefreshBuffPanel();
        }

        //Event
        private void BuffManager_OnBuffListChanged(object sender, System.EventArgs e)
        {
            RefreshBuffPanel();
        }

        //Refreshes the information in the panel
        public void RefreshBuffPanel()
        {
            //if (unit == null) return;
            List<BuffObject> buffs = unit?.buffManager?.GetBuffList();
            for (int i = 0; i < buffIcons.Count; i++)
            {
                if (buffs?.Count <= i || unit == null)
                {
                    buffIcons[i].gameObject.SetActive(false);
                }
                else
                {
                    buffIcons[i].gameObject.SetActive(true);
                    buffIcons[i].GetComponent<BuffSlot>().SetBuff(buffs[i]);
                    buffIcons[i].sprite = buffs[i].icon;
                    buffOutlines[i].color = buffs[i].GetColor(buffs[i].outlineColor);
                }
            }
        }
    }
}
