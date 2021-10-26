using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundClick : MonoBehaviour
{
    //Used in the GroundClick animation. Turns off GroundClick when the animation ends.
    public void GroundClickDisable()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
