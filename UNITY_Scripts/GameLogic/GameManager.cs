using System;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public event Action<int, bool> OnGameFinished;
    public event Action OnReset;
    public event Action<int, int> OnCellChanged;
    private bool xUserTurn=true;
    public event Action<int, int> OnMoveMade;// index, value
    // 0 empty, 1 X, 2 O
    private readonly int[] board = new int[9];
    private bool gameOver;
    public bool isMyTurn;
    private readonly int[][] winConditions =
    {
        new[] { 0, 1, 2 },
        new[] { 3, 4, 5 },
        new[] { 6, 7, 8 },
        new[] { 0, 3, 6 },
        new[] { 1, 4, 7 },
        new[] { 2, 5, 8 },
        new[] { 0, 4, 8 },
        new[] { 2, 4, 6 }
    };

    private void Start()
    {
        ResetGame();
    }

    public int GetValue(int index) => board[index];

    public bool TryPlayMove(int index, int value)
    {
        if (gameOver) return false;
        if (index < 0 || index >= 9) return false;
        if (board[index] != 0) return false;
        board[index] = value;
        OnCellChanged?.Invoke(index, value);
        SwitchTurn();
        OnMoveMade?.Invoke(index, value);
        CheckWinCondition(value);
        return true;
    }
    private void CheckWinCondition(int value)
    {
        foreach (var condition in winConditions)
        {
            if (board[condition[0]] == value &&
                board[condition[1]] == value &&
                board[condition[2]] == value &&
                value != 0)
            {
                gameOver = true;
                OnGameFinished?.Invoke(value, true);
                return;
            }
        }
        bool allFilled = true;
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == 0)
            {
                allFilled = false;
                break;
            }
        }
        if (allFilled)
        {
            gameOver = true;
            OnGameFinished?.Invoke(0, false);
        }
    }

    public int[] GetBoardCopy()
    {
        int[] copy = new int[9];
        Array.Copy(board, copy, 9);
        return copy;
    }

    public void ResetGame()
    {
        gameOver = false;

        for (int i = 0; i < 9; i++)
        {
            board[i] = 0;
            OnCellChanged?.Invoke(i, 0);
        }

        if (GameModeConfig.Mode != GameMode.Online)
            xUserTurn=true;
        OnReset?.Invoke();
    }
    public bool GetTurn()
    {
        return xUserTurn;
    }
    public void SwitchTurn()
    {
        if (GameModeConfig.Mode == GameMode.Online){
            isMyTurn=!isMyTurn;
            return;
        }
        xUserTurn = !xUserTurn;
        
    }
    protected override void Initialize()
    {
        xUserTurn=true;
    }
}
