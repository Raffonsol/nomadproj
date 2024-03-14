using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

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
    // https://stackoverflow.com/questions/3132126/how-do-i-select-a-random-value-from-an-enumeration
    public static Enum GetRandomEnumValue(this Type t)
    {
        return Enum.GetValues(t)          // get values from Type provided
            .OfType<Enum>()               // casts to Enum
            .OrderBy(e => Guid.NewGuid()) // mess with order of results
            .FirstOrDefault();            // take first item in result
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
public static class Util
{
    public static int TagToFaction(string tag)
    {
        switch (tag) {
            case ("Character"):
            return 0;
            case ("Monster"):
            return 1;
            case ("Npc"):
            return 2;
            case ("Marauder"):
            return 3;
            case ("Regional"):
            return 4;
            case ("Mimic"):
            return 5;
        }
        return -1;
    }
    public static string SlotToBodyPosition(Slot slot, bool left, bool bare = false) {
        string pos = "";
        if (slot == Slot.Pauldron )
        {
            pos = left? "L" :"R";
        } else if (slot == Slot.Foot) {
            pos = left? "L" :"R";
        } else if (slot == Slot.Hand) {
            pos = left? "Instance/L" :"Instance/R";
        } else if (slot == Slot.Chest) {
            if (!bare)
            pos = "Chest/";
        } else if (slot == Slot.Head) {
            if (!bare)
            pos = "Head/";
        }
        pos = pos + slot.ToString();
        if (slot == Slot.Clothing) {
            pos = "Chest/Chest";
        }
        return pos;
    }

    public static string FittablePartToString(FittablePart part, bool withEg = false)
    {
        string value = part.ToString();
        switch (part) {
            case FittablePart.BluntObject:
                value = "Blunt Object";
                if (withEg) value+=" (rock)";
                break;
            case FittablePart.ShortStick:
                value = "Short Stick";
                if (withEg) value+=" (wood)";
                break;
        }

        return value;
    }

}