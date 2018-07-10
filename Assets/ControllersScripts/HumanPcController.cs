using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HumanController", menuName = "Controller/HumanPC", order = 1)]
public class HumanPcController : PlayerControllerScriptable
{
    public override Vector2 getMove()
    {
        float x = Input.GetAxis("Horizontal"), y = Input.GetAxis("Vertical");

        return new Vector2(x, y);
    }

    public override Vector2 getShot()
    {
        return new Vector2(Input.GetAxis("HorizontalShot"), Input.GetAxis("VerticalShot"));
    }
}