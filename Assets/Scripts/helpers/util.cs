using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
    
public static class MyExtensions
{
    private static readonly Random rng = new Random();
    
    //Fisher - Yates shuffle
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
public static class UIHelper
{
    public static bool UIOverlapCheck(RaycastResult[] raycastResults, string tagMatch)
    {
        bool hit = false;
        for(int i = 0; i <raycastResults.Length; i++){
            // GameOverlord.Instance.Log(raycastResults[i].gameObject.name);
            if (raycastResults[i].gameObject.name == tagMatch) {
                hit = true;
            }
        }
        return hit;
    }

}