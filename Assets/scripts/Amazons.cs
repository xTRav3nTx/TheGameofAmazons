using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amazons : MonoBehaviour
{
    public int team;
    public List<Vector2Int> currentmoves = new List<Vector2Int>();
    public int CurrentX;
    public int CurrentY;

    private Vector3 desiredPosition;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
    }

    public virtual void setPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if(force)
        {
            transform.position = desiredPosition;
        }
    }

    public List<Vector2Int> GetAvailableMoves(ref Amazons[,] board,ref Fire[,] fires, int tileCountX, int tileCountY)
    {
        List<Vector2Int> arr = new List<Vector2Int>();

        //Down
        for(int i = CurrentY - 1; i >= 0; i--)
        {
            if(board[CurrentX, i] == null || fires[CurrentX, i] == null)
            {
                arr.Add(new Vector2Int(CurrentX, i));
            }
            if (board[CurrentX, i] != null || fires[CurrentX, i] != null)
            {
                arr.RemoveAt(arr.Count - 1);
                break;
            }

        }
        //Up
        for (int i = CurrentY + 1; i < tileCountY; i++)
        {
            if (board[CurrentX, i] == null || fires[CurrentX, i] == null)
            {
                arr.Add(new Vector2Int(CurrentX, i));
            }
            if (board[CurrentX, i] != null || fires[CurrentX, i] != null)
            {
                arr.RemoveAt(arr.Count - 1);
                break;
            }

        }
        //Left
        for (int i = CurrentX - 1; i >= 0; i--)
        {
            if (board[i, CurrentY] == null || fires[i, CurrentY] == null)
            {
                arr.Add(new Vector2Int(i, CurrentY));
            }
            if (board[i, CurrentY] != null || fires[i, CurrentY] != null)
            {
                arr.RemoveAt(arr.Count - 1);
                break;
            }

        }
        //Right
        for (int i = CurrentX + 1; i < tileCountX; i++)
        {
            if (board[i, CurrentY] == null || fires[i, CurrentY] == null)
            {
                arr.Add(new Vector2Int(i, CurrentY));
            }
            if (board[i, CurrentY] != null || fires[i, CurrentY] != null)
            {
                arr.RemoveAt(arr.Count - 1);
                break;
            }

        }
        //Up-Right
        for (int x = CurrentX + 1, y = CurrentY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            if(board[x, y] == null || fires[x, y] == null)
            {
                arr.Add(new Vector2Int(x,y));
            }
            if (board[x,y] != null || fires[x, y] != null)
            {
                arr.RemoveAt(arr.Count - 1);
                break;
            }
        }
        //Up-Left
        for (int x = CurrentX - 1, y = CurrentY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            if (board[x, y] == null || fires[x, y] == null)
            {
                arr.Add(new Vector2Int(x, y));
            }
            if (board[x, y] != null || fires[x, y] != null)
            {
                arr.RemoveAt(arr.Count - 1);
                break;
            }
        }
        //Down-Right
        for (int x = CurrentX + 1, y = CurrentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (board[x, y] == null || fires[x, y] == null)
            {
                arr.Add(new Vector2Int(x, y));
            }
            if (board[x, y] != null || fires[x, y] != null)
            {
                arr.RemoveAt(arr.Count - 1);
                break;
            }
        }
        //Down-Left
        for (int x = CurrentX - 1, y = CurrentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null || fires[x, y] == null)
            {
                arr.Add(new Vector2Int(x, y));
            }
            if (board[x, y] != null || fires[x, y] != null)
            {
                arr.RemoveAt(arr.Count - 1);
                break;
            }
        }

        return arr;
    }
}
