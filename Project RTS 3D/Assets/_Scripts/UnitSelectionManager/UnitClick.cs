using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitClick : MonoBehaviour
{

    public LayerMask clickable;
    public LayerMask ground;

    private Camera myCam;

    void Start()
    {
        myCam = Camera.main;
    }

    //Selects a unit.
    void Update()
    {
        //ChooseClick();
    }

    
}
