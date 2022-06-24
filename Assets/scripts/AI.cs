using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public List<Amazons> AI_amazons = new List<Amazons>();
    [SerializeField] private float pieceOffset;

    public Amazons selectPiecetoMove(ref Amazons[,] amazons, int team, int _width, int _height)
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if(amazons[x,y] != null)
                {
                    if (amazons[x, y].team == team)
                    {
                        AI_amazons.Add(amazons[x, y]);

                    }
                }
            }
        }
        return AI_amazons[Random.Range(0, 3)];
    }

    Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x + .5f, y + .5f, pieceOffset);
    }

    public void positionAIpiece(Amazons aiPiece, int x, int y)
    {
        aiPiece.CurrentX = x;
        aiPiece.CurrentY = y;
        aiPiece.setPosition(GetTileCenter(x, y));
    }




}
