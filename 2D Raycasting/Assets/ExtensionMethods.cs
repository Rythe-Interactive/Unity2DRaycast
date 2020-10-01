using UnityEngine;

public static class ExtensionMethods
{
    public static void RemoveComponent<Component>(this GameObject obj, bool immediate = false)
    {
        Component component = obj.GetComponent<Component>();

        if (component != null)
        {
            //use destroy immediate if not ingame
            if (Application.isPlaying)
            {
                Object.Destroy(component as Object);
            }
            else
            {
                Object.DestroyImmediate(component as Object, true);
            }
        }
    }
}
