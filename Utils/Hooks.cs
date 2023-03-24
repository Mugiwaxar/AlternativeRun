using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Reflection;

namespace AlternativeRun.Utils
{
    internal class Hooks
    {

        public static void AddHooks(Type classType, string methodName, Delegate calledMethod)
        {
            AddHooks(classType, methodName, null, calledMethod);
        }

        public static void AddHooks(Type classType, string methodName, Type[] methodType, Delegate calledMethod)
        {

            // Check all values //
            if (classType == null || methodName == null || calledMethod == null)
            {
                UnityEngine.Debug.LogWarning("AlternativeRun -> Failed to create hook, argument null: " + classType.Name + "." + methodName + " -> " + calledMethod.Method.Name);
                return;
            }

            // Get the Method //
            MethodInfo method = null;
            if (methodType != null)
                method = classType.GetMethod(methodName, (BindingFlags)(-1), null, methodType, null);
            else
                method = classType.GetMethod(methodName, (BindingFlags)(-1));


            // Check the Method //
            if (method == null)
            {
                UnityEngine.Debug.LogWarning("AlternativeRun -> Failed to create hook, method null : " + classType.Name + "." + methodName + " -> " + calledMethod.Method.Name);
                return;
            }

            HookEndpointManager.Add(method, calledMethod);
            UnityEngine.Debug.Log("AlternativeRun -> Hook Added for " + classType.Name + "." + methodName + " -> " + calledMethod.Method.Name);

        }

    }
}
