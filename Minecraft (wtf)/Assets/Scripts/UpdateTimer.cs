using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateTimer : MonoBehaviour
{
    const int timerFalling = 2;
    const int timerWater = 10;
    int timer;

    ChunkManager chunk;
    int x;
    int y;
    int z;

    public void CreateUpdateTimer(ChunkManager _chunk, int _x, int _y, int _z)
    {
        chunk = _chunk;
        x = _x;
        y = _y;
        z = _z;

        timer = 0;
    }

    void FixedUpdate()
    {
        timer++;
        if(timer > timerFalling)
        {
            chunk.UpdateAroundsFallingBlocs(x, y, z);
        }
        if(timer > timerWater)
        {
            chunk.UpdateAroundsWaterBlocs(x, y, z);
            Destroy(this.gameObject);
        }
    }
}
