using System;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public event Action<int, bool, int[]> OnGameFinished;
    public event Action OnReset;
    public event Action<int, int> OnCellChanged;
    private bool xUserTurn=true;
    public event Action<int, int> OnMoveMade;// index, value
    // 0 empty, 1 X, 2 O
    private int size;
    private int[] board;
    private bool gameOver;
    public bool isMyTurn;

    private void Start()
    {
        size=GameModeConfig.boardSize;
        board= new int[size*size];
        ResetGame();
    }

    public int GetValue(int index) => board[index];

    public bool TryPlayMove(int index, int value)
    {
        if (gameOver) return false;
        if (index < 0 || index >= board.Length) return false;
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
        if (value == 0) return;

        // -------- ROWS --------
        for (int r = 0; r < size; r++)
        {
            bool win = true;
            int[] line = new int[size];

            for (int c = 0; c < size; c++)
            {
                int index = r * size + c;
                line[c] = index;

                if (board[index] != value)
                {
                    win = false;
                    break;
                }
            }

            if (win)
            {
                gameOver = true;
                OnGameFinished?.Invoke(value, true, line);
                return;
            }
        }

        // -------- COLUMNS --------
        for (int c = 0; c < size; c++)
        {
            bool win = true;
            int[] line = new int[size];

            for (int r = 0; r < size; r++)
            {
                int index = r * size + c;
                line[r] = index;

                if (board[index] != value)
                {
                    win = false;
                    break;
                }
            }

            if (win)
            {
                gameOver = true;
                OnGameFinished?.Invoke(value, true, line);
                return;
            }
        }

        // -------- DIAGONAL \ --------
        bool diag1 = true;
        int[] diag1Line = new int[size];

        for (int i = 0; i < size; i++)
        {
            int index = i * size + i;
            diag1Line[i] = index;

            if (board[index] != value)
            {
                diag1 = false;
                break;
            }
        }

        if (diag1)
        {
            gameOver = true;
            OnGameFinished?.Invoke(value, true, diag1Line);
            return;
        }

        // -------- DIAGONAL / --------
        bool diag2 = true;
        int[] diag2Line = new int[size];

        for (int i = 0; i < size; i++)
        {
            int index = i * size + (size - 1 - i);
            diag2Line[i] = index;

            if (board[index] != value)
            {
                diag2 = false;
                break;
            }
        }

        if (diag2)
        {
            gameOver = true;
            OnGameFinished?.Invoke(value, true, diag2Line);
            return;
        }

        // -------- DRAW --------
        bool allFilled = true;

        for (int i = 0; i < board.Length; i++)
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
            OnGameFinished?.Invoke(0, false, null);
        }
    }

    public int[] GetBoardCopy()
    {
        int[] copy = new int[board.Length];
        Array.Copy(board, copy, board.Length);
        return copy;
    }

    public void ResetGame()
    {
        gameOver = false;
        for (int i = 0; i < board.Length; i++)
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
    private void SwitchTurn()
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
