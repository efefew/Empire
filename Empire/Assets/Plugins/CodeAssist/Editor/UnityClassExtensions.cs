﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Meryel.UnityCodeAssist.Synchronizer.Model;
using UnityEditor;
using UnityEngine;
using GameObject = UnityEngine.GameObject;
using Object = UnityEngine.Object;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor
{
    internal static class UnityClassExtensions
    {
        private static GameObject? GetParentGO(GameObject go)
        {
            if (!go)
                return null;

            Transform? parentTransform = go.transform.parent;

            if (parentTransform && parentTransform.gameObject)
                return parentTransform.gameObject;
            return null;
        }

        private static string GetId(Object? obj)
        {
            // obj can be null

            GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(obj);
            string? objectGuid = globalObjectId.ToString();
            return objectGuid;
        }

        internal static Synchronizer.Model.GameObject? ToSyncModel(this GameObject go, int priority = 0)
        {
            if (!go)
                return null;

            Synchronizer.Model.GameObject data = new()
            {
                Id = GetId(go),

                Name = go.name,
                Layer = go.layer.ToString(),
                Tag = go.tag,
                Scene = go.scene.name,

                ParentId = GetId(GetParentGO(go)),
                ChildrenIds = getChildrenIds(go),

                Components = getComponents(go),

                Priority = priority
            };
            return data;

            static string[] getChildrenIds(GameObject g)
            {
                var ids = new List<string>();
                int limit = 10; //**--
                foreach (Transform child in g.transform)
                {
                    if (!child || !child.gameObject)
                        continue;

                    ids.Add(GetId(child.gameObject));

                    if (--limit <= 0)
                        break;
                }

                return ids.ToArray();
            }

            //**--limit/10
            static string[] getComponents(GameObject g)
            {
                return g.GetComponents<Component>().Where(c => c).Select(c => c.GetType().FullName).Take(10).ToArray();
            }
            /*(string[] componentNames, Synchronizer.Model.ComponentData[] componentData) getComponents(GameObject g)
            {
                var components = g.GetComponents<Component>();
                var names = components.Select(c => c.name).ToArray();

                var data = new List<Synchronizer.Model.ComponentData>();
                foreach (var comp in components)
                {
                    var name = comp.name;


                }

                return (names, data.ToArray());
            }*/
        }

        internal static Synchronizer.Model.GameObject[]? ToSyncModelOfHierarchy(this GameObject go)
        {
            if (!go)
                return null;

            var list = new List<Synchronizer.Model.GameObject>();

            GameObject? parent = GetParentGO(go);
            if (parent != null && parent)
            {
                Synchronizer.Model.GameObject? parentModel = parent.ToSyncModel();
                if (parentModel != null)
                    list.Add(parentModel);
            }

            int limit = 10;
            foreach (Transform child in go.transform)
            {
                if (!child || !child.gameObject)
                    continue;

                Synchronizer.Model.GameObject? childModel = child.gameObject.ToSyncModel();
                if (childModel == null)
                    continue;

                list.Add(childModel);

                if (--limit <= 0)
                    break;
            }

            return list.ToArray();
        }

        internal static ComponentData[]? ToSyncModelOfComponents(this GameObject go)
        {
            if (!go)
                return null;

            int limit = 10; //**--
            return go.GetComponents<Component>().Where(c => c).Select(c => c.ToSyncModel(go)).Where(cd => cd != null)
                .Take(limit).ToArray()!;

            /*
            var components = go.GetComponents<Component>();
            var len = components.Count(c => c != null);
            len = Math.Min(len, limit);//**--limit

            var array = new Synchronizer.Model.ComponentData[len];

            var arrayIndex = 0;
            foreach (var component in components)
            {
                if (component == null)
                    continue;

                array[arrayIndex++] = component.ToSyncModel(go);

                if (arrayIndex >= len)
                    break;
            }

            return array;
            */
        }

        internal static ComponentData? ToSyncModel(this Component component, GameObject go)
        {
            if (!component || !go)
                return null;

            Type type = component.GetType();
            var list = new List<(string, string)>();
            ShowFieldInfo(type, component, list);

            ComponentData data = new()
            {
                GameObjectId = GetId(go),
                Component = component.GetType().FullName,
                Type = ComponentData.DataType.Component,
                Data = list.ToArray()
            };
            return data;
        }

        internal static ComponentData? ToSyncModel(this ScriptableObject so)
        {
            if (!so)
                return null;

            Type type = so.GetType();
            var list = new List<(string, string)>();
            ShowFieldInfo(type, so, list);

            ComponentData data = new()
            {
                GameObjectId = GetId(so),
                Component = so.GetType().FullName,
                Type = ComponentData.DataType.ScriptableObject,
                Data = list.ToArray()
            };
            return data;
        }


        private static bool IsTypeCompatible(Type type)
        {
            if (type == null ||
                !(type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(ScriptableObject))))
                return false;
            return true;
        }

        private static void
            ShowFieldInfo(
                Type type) //, MonoImporter importer, List<string> names, List<Object> objects, ref bool didModify)
        {
            // Only show default properties for types that support it (so far only MonoBehaviour derived types)
            if (!IsTypeCompatible(type))
                return;

            ShowFieldInfo(type.BaseType); //, importer, names, objects, ref didModify);

            FieldInfo[] infos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                               BindingFlags.DeclaredOnly);
            foreach (FieldInfo field in infos)
            {
                if (!field.IsPublic)
                {
                    object[] attr = field.GetCustomAttributes(typeof(SerializeField), true);
                    if (attr == null || attr.Length == 0)
                        continue;
                }

                /*
                if (field.FieldType.IsSubclassOf(typeof(Object)) || field.FieldType == typeof(Object))
                {
                    Object oldTarget = importer.GetDefaultReference(field.Name);
                    Object newTarget = EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(field.Name), oldTarget, field.FieldType, false);

                    names.Add(field.Name);
                    objects.Add(newTarget);

                    if (oldTarget != newTarget)
                        didModify = true;
                }
                */

                if (field.FieldType.IsValueType && field.FieldType.IsPrimitive && !field.FieldType.IsEnum)
                {
                }
                else if (field.FieldType == typeof(string))
                {
                }
            }
        }

        private static void
            ShowFieldInfo(Type type, Object unityObjectInstance,
                List<(string, string)> fields) //, MonoImporter importer, List<string> names, List<Object> objects, ref bool didModify)
        {
            // Only show default properties for types that support it (so far only MonoBehaviour derived types)
            if (!IsTypeCompatible(type))
                return;

            if (!unityObjectInstance)
                return;

            ShowFieldInfo(type.BaseType, unityObjectInstance, fields); //, importer, names, objects, ref didModify);

            FieldInfo[] infos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                               BindingFlags.DeclaredOnly);
            foreach (FieldInfo field in infos)
            {
                if (!field.IsPublic)
                {
                    object[] attr = field.GetCustomAttributes(typeof(SerializeField), true);
                    if (attr == null || attr.Length == 0)
                        continue;
                }

                // check attribute [HideInInspector]
                {
                    object[] attr = field.GetCustomAttributes(typeof(HideInInspector), true);
                    if (attr != null && attr.Length > 0)
                        continue;
                }

                // readonly
                if (field.IsInitOnly)
                    continue;


                /*
                if (field.FieldType.IsSubclassOf(typeof(Object)) || field.FieldType == typeof(Object))
                {
                    Object oldTarget = importer.GetDefaultReference(field.Name);
                    Object newTarget = EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(field.Name), oldTarget, field.FieldType, false);

                    names.Add(field.Name);
                    objects.Add(newTarget);

                    if (oldTarget != newTarget)
                        didModify = true;
                }
                */

                if (field.FieldType.IsValueType && field.FieldType.IsPrimitive && !field.FieldType.IsEnum)
                {
                    object? val = field.GetValue(unityObjectInstance);
                    fields.Add((field.Name, val.ToString())); //**--culture
                }
                else if (field.FieldType == typeof(string))
                {
                    string? val = (string)field.GetValue(unityObjectInstance);
                    fields.Add((field.Name, val));
                }
            }
        }
    }
}