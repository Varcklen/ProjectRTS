using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    private Slider healthBarSlider;
    private Slider shieldBarSlider;
    private Slider manaBarSlider;

    private bool isManaBarVisible = true;
    public bool IsManaBarVisible { 
        get
        {
            return isManaBarVisible;
        }
        set
        {
            isManaBarVisible = value;
            manaBarSlider?.gameObject?.SetActive(value);
        }
    }

    private void Awake()
    {
        healthBarSlider = transform.Find("Bars").Find("HealthBar").GetComponent<Slider>();
        shieldBarSlider = transform.Find("Bars").Find("ShieldBar").GetComponent<Slider>();
        manaBarSlider = transform.Find("ManaBar").GetComponent<Slider>();
    }

    public void SetHealthBar(float health, float maxHealth, float shield, float maxShield)
    {
        if (health <= 0)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            
            gameObject.SetActive(true);
        }
        shieldBarSlider.value = shield;
        shieldBarSlider.maxValue = maxShield;

        healthBarSlider.value = health;
        healthBarSlider.maxValue = maxHealth;
    }

    public void SetManaBar(float mana, float maxMana)
    {
        if (!isManaBarVisible)
        {
            return;
        }
        manaBarSlider.value = mana;
        manaBarSlider.maxValue = maxMana;
    }
}
