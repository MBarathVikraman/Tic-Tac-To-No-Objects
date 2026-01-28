using UnityEngine;
public class TurnManager : MonoSingleton<TurnManager>
{
    private bool xUserTurn=true;
    public bool GetTurn()
    {
        return xUserTurn;/*
        bool turn = xUserTurn;
        xUserTurn = !xUserTurn; // Switch turns
        return turn; */
    }
    public void SwitchTurn()
    {
        xUserTurn = !xUserTurn;
    }
    public void ResetTurn()
    {
        xUserTurn=true;
    }
    protected override void Initialize()
    {
        xUserTurn=true;
    }
}