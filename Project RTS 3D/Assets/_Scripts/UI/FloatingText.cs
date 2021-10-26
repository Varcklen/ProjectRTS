using System.Collections;
using TMPro;
using UnityEngine;

//Script responsible for floating text
public class FloatingText : MonoBehaviour
{
    private TextMeshPro floatingText;

    private Vector3 direction;

    private Transform parent;

    private Color textColor;

    private static int sortingOrder;

    private const float FLOATING_TEXT_SPEED = 2f;
    private const float FLOATING_TEXT_LIFE_TIME = 1.5f;
    private const float FLOATING_TEXT_FADE_TIME = 0.5f;
    private const float FLOATING_TEXT_Y_DEVIATION = 4f;
    private const string FLOATING_TEXT_PREFAB_PATH = "UI\\DamagePopup";

    private const float TEXT_SIZE_SMALL = 10f;
    private const float TEXT_SIZE_MEDIUM = 15f;
    private const float TEXT_SIZE_BIG = 25f;
    private const float TEXT_SIZE_VERY_BIG = 35f;

    private void Awake()
    {
        floatingText = transform.GetComponent<TextMeshPro>();
        parent = transform.parent;
    }

    //If you need to create floating text use this method
    public static void Create(Vector3 position, string word, Color textColor, TextSize textSize)
    {
        GameObject floatingTextPrefab = (GameObject)Resources.Load(FLOATING_TEXT_PREFAB_PATH);
        if (floatingTextPrefab == null)
        {
            Debug.Log("Floating text prefab in FloatingText.cs is not loaded. Please check FLOATING_TEXT_PREFAB_PATH variable.");
            return;
        }
        GameObject newText = GameObject.Instantiate(floatingTextPrefab, position + new Vector3(0, FLOATING_TEXT_Y_DEVIATION, 0), Quaternion.identity);
        newText.transform.Find("Text").GetComponent<FloatingText>().Launch(word, textColor, textSize);
    }

    //Triggers motion, life time, and fade time of floating text.
    void Launch(string word, Color textColor, TextSize textSize)
    {
        direction = new Vector3(Random.Range(-FLOATING_TEXT_SPEED, FLOATING_TEXT_SPEED), FLOATING_TEXT_SPEED, 0);
        floatingText.color = textColor;
        floatingText.text = word.ToString();
        floatingText.fontSize = GetTextSize(textSize);
        this.textColor = floatingText.color;

        //Each new floating text will always be before the old one.
        sortingOrder++;
        floatingText.sortingOrder = sortingOrder;

        StartCoroutine(TimeToFade());
        StartCoroutine(TextLife());
        StartCoroutine(Move());
    }

    float GetTextSize(TextSize textSize)
    {
        switch (textSize)
        {
            case TextSize.Small:    return TEXT_SIZE_SMALL;
            case TextSize.Medium:   return TEXT_SIZE_MEDIUM;
            case TextSize.Big:      return TEXT_SIZE_BIG;
            case TextSize.Very_Big: return TEXT_SIZE_VERY_BIG;
        }
        Debug.LogWarning(textSize + " in " + GetType().Name + ".cs is not set. Please check \"GetTextSize\" method.");
        return 1;
    }

    //Moves floating text. Stops working with removing floating text.
    IEnumerator Move()
    {
        while (true)
        {
            yield return null;
            transform.position += direction * Time.deltaTime;
        }
    }

    //Time to remove floating text
    IEnumerator TextLife()
    {
        yield return new WaitForSeconds(FLOATING_TEXT_LIFE_TIME);
        Destroy(parent.gameObject);
    }

    //Time before the floating text starts to fade
    IEnumerator TimeToFade()
    {
        yield return new WaitForSeconds(FLOATING_TEXT_LIFE_TIME-FLOATING_TEXT_FADE_TIME);
        StartCoroutine(Fade());
    }

    //The process of fade floating text
    IEnumerator Fade()
    {
        float disappearSpeed = 1 / FLOATING_TEXT_FADE_TIME;
        while (textColor.a > 0)
        {
            textColor.a -= disappearSpeed * Time.deltaTime;
            floatingText.color = textColor;
            yield return null;
        }
    }
}

public enum TextSize : byte
{
    Small,
    Medium,
    Big,
    Very_Big
}
