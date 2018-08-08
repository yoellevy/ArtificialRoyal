using UnityEngine;

[CreateAssetMenu(fileName = "RandomController", menuName = "Controller/RandomController", order = 1)]
public class RandomController : PlayerControllerScriptable
{    

    public override void CalculateNextAction()
    {
        base.Move = new Vector2((int)Random.Range(-1, 2), (int)Random.Range(-1, 2));
        base.Shot = new Vector2((int)Random.Range(-1, 2), (int)Random.Range(-1, 2));
    }
}