using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGame : MonoBehaviour
{
    public LavenderCharacter DefaultCharacter;
    public bool shouldPlay = false;
    void Start()
    {
        if (shouldPlay)
        {
            DefaultCharacter.CreateCharacterInWorld(new Vector3(0, 0, 0), true);
        }
    }
}
