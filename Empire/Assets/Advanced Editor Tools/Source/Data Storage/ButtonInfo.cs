#if UNITY_EDITOR

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdvancedEditorTools.Attributes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace AdvancedEditorTools
{
    [Serializable]
    public class ButtonInfo : IEnumerableMatcheable<ButtonInfo>
    {
        public string methodName;
        public string buttonName;
        public bool foldout = true;
        public int buttonSize;

        [SerializeField] public SerializedField[] parameters;

        public MethodInfo method;

        public bool Matches(ButtonInfo obj)
        {
            return methodName.Equals(obj.methodName) && buttonName.Equals(obj.buttonName);
        }

        public bool PartiallyMatches(ButtonInfo obj)
        {
            return methodName.Equals(obj.methodName) || buttonName.Equals(obj.buttonName);
        }

        public ButtonInfo UpdateWith(ButtonInfo obj)
        {
            foldout = obj.foldout;
            parameters = parameters.MatchEnumerables(obj.parameters, true);
            return this;
        }

        public static void PaintButtonMethods(Object target, ref AETData data)
        {
            GUIStyle customWindowStyle = new("window");
            customWindowStyle.padding = new RectOffset(5, 5, 5, 5);
            customWindowStyle.margin = new RectOffset(5, 5, 5, 5);
            GUIStyle buttonStyle = new(GUI.skin.button);
            GUIStyle paramLabelStyle = new(EditorStyles.foldout);
            paramLabelStyle.fontStyle = FontStyle.Bold;

            foreach (ButtonInfo buttonMethod in data.buttonMethods)
            {
                EditorGUILayout.BeginVertical(customWindowStyle);
                buttonStyle.fontSize = buttonMethod.buttonSize;
                if (GUILayout.Button(buttonMethod.buttonName, buttonStyle))
                {
                    object[] args = new object[buttonMethod.parameters.Length];
                    for (int i = 0; i < buttonMethod.parameters.Length; i++)
                    {
                        object arg = buttonMethod.parameters[i].GetValue();
                        args[i] = arg == null || arg is null || arg.Equals(null) ? null : arg;
                    }

                    buttonMethod.method.Invoke(target, args);
                }

                if (buttonMethod.parameters.Length > 0)
                {
                    EditorGUI.indentLevel++;
                    buttonMethod.foldout = EditorGUILayout.Foldout(buttonMethod.foldout, "Parameters", paramLabelStyle);
                    if (buttonMethod.foldout)
                    {
                        EditorGUI.indentLevel++;
                        AETManager manager = AETManager.Instance;

                        foreach (SerializedField methodParam in buttonMethod.parameters)
                        {
                            ValueWrapper valueWrapper = methodParam.target as ValueWrapper;
                            if (valueWrapper == null)
                                methodParam.target = manager.RetrieveTypeMethods(methodParam.Type)
                                    .GetValueWrapper(methodParam.Type);

                            methodParam.OnInspector();
                        }

                        EditorGUI.indentLevel--;
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        public static void UpdateButtonMethods(Object target, ref AETData data)
        {
            var oldButtonMethods = data.buttonMethods;
            data.buttonMethods = new List<ButtonInfo>();

            foreach (MethodInfo method in target.GetType()
                         .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object[] attributes = method.GetCustomAttributes(typeof(ButtonAttribute), true);
                if (attributes.Length > 0)
                {
                    ButtonInfo buttonMethod = new();
                    buttonMethod.buttonName = ((ButtonAttribute)attributes[0]).buttonName;
                    buttonMethod.buttonSize = ((ButtonAttribute)attributes[0]).buttonSize;
                    buttonMethod.method = method;
                    buttonMethod.methodName = method.Name;

                    var paramsInfo = method.GetParameters();
                    buttonMethod.parameters = new SerializedField[paramsInfo.Length];

                    bool validParams = true;

                    foreach (ParameterInfo param in paramsInfo)
                        if (!IsParameterValidType(param, method))
                        {
                            validParams = false;
                            break;
                        }

                    if (!validParams) continue;

                    for (int i = 0; i < paramsInfo.Length; i++)
                    {
                        ParameterInfo paramInfo = paramsInfo[i];
                        buttonMethod.parameters[i] = new SerializedField(
                            paramInfo.Name,
                            paramInfo.ParameterType
                        );
                    }

                    data.buttonMethods.Add(buttonMethod);
                }
            }

            // Matchear listas y actualizar valores (no reemplazar)
            if (oldButtonMethods != null)
                data.buttonMethods = data.buttonMethods.MatchEnumerables(oldButtonMethods, true).ToList();
        }

        private static bool IsParameterValidType(ParameterInfo paramInfo, MethodInfo method)
        {
            Type paramType = paramInfo.ParameterType;

            if (!AETManager.Instance.IsTypeImplemented(paramType))
            {
                Debug.LogException(new NotSupportedException(
                    "Advanced Editor Tools\n" +
                    "Parameters of type 'struct' or 'class' are not yet supported.\n" +
                    $"Method: {method.DeclaringType}.{method.Name}()\n" +
                    $"Invalid parameter found: {paramType} {paramInfo.Name}"
                ));
                return false;
            }

            if (!paramType.IsValueType)
            {
                if (paramType.IsAbstract)
                {
                    Debug.LogWarning(
                        $"Error while creating method's button for {method.Name}: Parameter {paramInfo.Name} of type 'Abstract class' is not valid.");
                    return false;
                }

                if (paramType.IsInterface)
                {
                    Debug.LogWarning(
                        $"Error while creating method's button for {method.Name}: Parameter {paramInfo.Name} of type 'Interface' is not valid.");
                    return false;
                }
                /*else if (paramType.IsSealed)
                {
                    Debug.LogWarning($"Error while creating method's button for {method.Name}: Parameter {paramInfo.Name} of type 'Sealed class' is not valid.");
                    return false;
                }*/
            }

            return true;
        }
    }
}
#endif