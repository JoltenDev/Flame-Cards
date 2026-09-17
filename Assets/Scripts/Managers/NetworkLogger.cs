using UnityEngine;

public class NetworkLogger : MonoBehaviour
{
    public static void LogDebug(Object obj, string message, Color nameColor)
    {
        var color_1 = ColorUtility.ToHtmlStringRGBA(nameColor);
        Debug.Log($"<color=#ffffff>[</color><color=#{color_1}>{obj.name}</color><color=#ffffff>]</color> <color=#e8e8e8>{message}</color>");
    }

    public static void LogError(Object obj, string message)
    {
        Debug.Log($"<color=#ffffff>[</color><color=#ff6e6e>{obj.name}</color><color=#ffffff>]</color> <color=#ff7373>{message}</color>");
    }
}
