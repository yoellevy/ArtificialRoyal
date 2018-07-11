using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HumanController", menuName = "Controller/HumanPC", order = 1)]
public class HumanPcController : PlayerControllerScriptable
{
    public override void CalculateNextAction()
    {
        base.move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        base.shot = new Vector2(Input.GetAxis("HorizontalShot"), Input.GetAxis("VerticalShot"));
    }
}