using UnityEngine;

namespace KRU.Game
{
    public interface INoiseFilter
    {
        float Evaluate(Vector3 point);
    }
}