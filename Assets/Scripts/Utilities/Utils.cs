using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

public class Utils
{
    public static NormalItem.eNormalType GetRandomNormalType()
    {
        Array values = Enum.GetValues(typeof(NormalItem.eNormalType));
        NormalItem.eNormalType result = (NormalItem.eNormalType)values.GetValue(URandom.Range(0, values.Length));

        return result;
    }

    public static NormalItem.eNormalType GetRandomNormalTypeExcept(NormalItem.eNormalType[] types)
    {
        List<NormalItem.eNormalType> list = Enum.GetValues(typeof(NormalItem.eNormalType)).Cast<NormalItem.eNormalType>().Except(types).ToList();

        int rnd = URandom.Range(0, list.Count);
        NormalItem.eNormalType result = list[rnd];

        return result;
    }
    // public static List<NormalItem.eNormalType> GetRandomNormalTypeDivisibleby3()
    // {
    //     List<NormalItem.eNormalType> list = Enum.GetValues(typeof(NormalItem.eNormalType)).Cast<NormalItem.eNormalType>().Except(types).ToList();
        
    // }
    public static IEnumerator SmoothMovement(Transform start, Transform destination, float duration = 0.3f)
    {
        float elapsed = 0f;
        Vector3 startPos = start.position;
        Vector3 endPos = destination.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            start.position = Vector3.Lerp(startPos, endPos, t);
            yield return null; // Wait for next frame
        }

        // Ensure exact final position
        start.position = endPos;
    }

    
}
