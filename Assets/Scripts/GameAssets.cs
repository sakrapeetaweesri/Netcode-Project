using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _i;
    public static GameAssets i
    {
        get
        {
            if (_i == null)
            {
                _i = Instantiate(Resources.Load("GameAssets") as GameObject).GetComponent<GameAssets>();
            }
            return _i;
        }
    }

    [field: SerializeField] public Sprite[] CharacterProfiles { get; private set; }
    [field: SerializeField] public Sprite[] TaskSprites { get; private set; }
}