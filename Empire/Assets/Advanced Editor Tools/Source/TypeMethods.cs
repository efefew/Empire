#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace AdvancedEditorTools
{
    public abstract class TypeMethods
    {
        public abstract ValueWrapper GetValueWrapper(Type type);

        public void OnInspector(ref SerializedField serializedField, Rect? rect = null)
        {
            if (!rect.HasValue)
                rect = EditorGUILayout.GetControlRect(true, GetPropertyFieldSize(ref serializedField));

            if (serializedField.isUsingRealContainer)
                OnInspectorLayout(ref serializedField, rect.Value);
            else
                serializedField.containerTarget.OnInspector(rect.Value);
        }

        protected virtual void OnInspectorLayout(ref SerializedField serializedField, Rect rect)
            => PaintField(serializedField.DisplayName, serializedField.SerializedProperty, rect);
        protected virtual void PaintField(string label, SerializedProperty property, Rect rect)
            => EditorGUI.PropertyField(rect, property, new GUIContent(label), true);
        public virtual float GetPropertyFieldSize(ref SerializedField serializedField)
            => EditorGUI.GetPropertyHeight(serializedField.SerializedProperty, true);

        public virtual bool CanBeCloned() => true;
    }
    public class ByteTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<ByteValueWrapper>().Init(type);
    }
    public class CharTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<CharValueWrapper>().Init(type);
    }
    public class IntTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<IntValueWrapper>().Init(type);
    }
    public class FloatTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<FloatValueWrapper>().Init(type);
    }
    public class BoolTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<BoolValueWrapper>().Init(type);
    }
    public class StringTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<StringValueWrapper>().Init(type);
    }
    public class ColorTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<ColorValueWrapper>().Init(type);
    }
    public class Vector2TypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<Vector2ValueWrapper>().Init(type);
    }
    public class Vector3TypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<Vector3ValueWrapper>().Init(type);
    }
    public class Vector4TypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<Vector4ValueWrapper>().Init(type);

        public override float GetPropertyFieldSize(ref SerializedField serializedField)
        {
            var container = serializedField.containerTarget;
            if (container != null)
                return EditorGUI.GetPropertyHeight(container.SerializedProperty, true);
            return base.GetPropertyFieldSize(ref serializedField);
        }
    }
    public class BoundsTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<BoundsValueWrapper>().Init(type);
    }
    public class RectTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<RectValueWrapper>().Init(type);
    }
    public class Vector2IntTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<Vector2IntValueWrapper>().Init(type);
    }
    public class Vector3IntTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<Vector3IntValueWrapper>().Init(type);
    }
    public class BoundsIntTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<BoundsIntValueWrapper>().Init(type);
    }
    public class RectIntTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<RectIntValueWrapper>().Init(type);
    }
    public class Hash128TypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<Hash128ValueWrapper>().Init(type);
    }
    public class QuaternionTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<QuaternionValueWrapper>().Init(type);

        public override float GetPropertyFieldSize(ref SerializedField serializedField) => EditorGUIUtility.singleLineHeight;
    }
    public class LayerMaskTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<LayerMaskValueWrapper>().Init(type);
    }
    public class SortingLayerTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<SortingLayerValueWrapper>().Init(type);
        public override float GetPropertyFieldSize(ref SerializedField serializedField) => EditorGUIUtility.singleLineHeight;
    }

    // #########################################

    public class AnimationCurveTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<AnimationCurveValueWrapper>().Init(type);
    }
    public class GradientTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<GradientValueWrapper>().Init(type);
    }

    // #########################################   

    public class ObjectTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<ObjectValueWrapper>().Init(type);
    }
    public class EnumTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<EnumValueWrapper>().Init(type);
    }

    // #########################################

    public abstract class EnumerableTypeMethods : TypeMethods
    {
        protected abstract Type GetElementType(Type enumerableType);

        public override float GetPropertyFieldSize(ref SerializedField serializedField)
        {
            if (serializedField.isUsingRealContainer)
                return base.GetPropertyFieldSize(ref serializedField);

            var enumerableWrapper = serializedField.containerTarget as EnumerableValueWrapper;
            return enumerableWrapper.foldout ? enumerableWrapper.ReorderableList.GetHeight() + EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight;
        }
    }
    public class ArrayTypeMethods : EnumerableTypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<ArrayValueWrapper>().Init(type);
        protected override Type GetElementType(Type enumerableType) => enumerableType.GetElementType();
    }
    public class ListTypeMethods : EnumerableTypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<ListValueWrapper>().Init(type);
        protected override Type GetElementType(Type enumerableType) => enumerableType.GetGenericArguments()[0];
    }

    // ##########################################

    public abstract class CustomTypeTypeMethods : TypeMethods
    {
        public override bool CanBeCloned() => false;
    }
    public class ClassTypeMethods : CustomTypeTypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<ClassValueWrapper>().Init(type);
    }
    public class StructTypeMethods : CustomTypeTypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => ScriptableObject.CreateInstance<StructValueWrapper>().Init(type);
    }

    /*
    public abstract class CustomTypeTypeMethods : TypeMethods
    {
        protected override void PaintTypePropertyFieldWithRect(string label, Type type, ref ValueWrapper valueWrapper, Rect rect)
        {
                

            var wrapper = valueWrapper as CustomTypeValueWrapper;

            PaintFoldout(ref wrapper, label, rect);
            if (wrapper.isInstantiated && wrapper.foldout)
            {
                var rootUndoMsg = DefaultSerializableTypes.undoMsg1;
                DefaultSerializableTypes.undoMsg1 += ".";

                EditorGUI.indentLevel++;
                var fields = wrapper.GetFieldsUpdated();
                if (fields == null || fields.Count == 0)
                {
                    rect.y += EditorGUIUtility.singleLineHeight;
                    rect.x += EditorGUIUtility.singleLineHeight * 1.75f;
                    rect.width -= EditorGUIUtility.singleLineHeight * 1.75f;
                    rect.height = EditorGUIUtility.singleLineHeight * 2.15f;

                    EditorGUI.HelpBox(rect, "This type has no serializable fields.", MessageType.Info);
                }
                else
                {
                    GUIStyle customWindowStyle = new("window");
                    customWindowStyle.padding = new RectOffset(20, 5, 5, 5);
                    customWindowStyle.margin = new RectOffset(20, 15, 5, 5);
                    customWindowStyle.contentOffset = new Vector2(-300f, -300f);

                    var prevElemSize = EditorGUIUtility.singleLineHeight * 1.25f;
                    var manager = AEAManager.Instance;
                    var rectVal = rect;

                    if (Event.current.type == EventType.Repaint)
                    {
                        rectVal.y = rect.y + EditorGUIUtility.singleLineHeight;
                        rectVal.x += EditorGUI.indentLevel * indentSize;
                        rectVal.width -= EditorGUI.indentLevel * indentSize;
                        rectVal.height = GetPropertyFieldSize(ref valueWrapper) - EditorGUIUtility.singleLineHeight * 1.5f;
                        customWindowStyle.Draw(rectVal, GUIContent.none, false, false, false, false);
                    }

                    rectVal = rect;
                    rectVal.x += EditorGUI.indentLevel * (indentSize - 2);
                    rectVal.width -= EditorGUI.indentLevel * (indentSize + 2);
                    foreach (var field in fields)
                    {
                        var elemTypeMethods = manager.RetrieveTypeMethods(field.Type);

                        rectVal.y += prevElemSize + 2f;
                        elemTypeMethods.PaintTypePropertyField(field.fieldName, field.Type, ref field.valueWrapper, rectVal);

                        prevElemSize = elemTypeMethods.GetPropertyFieldSize(ref field.valueWrapper);
                    }

                }
                EditorGUI.indentLevel--;

                DefaultSerializableTypes.undoMsg1 = rootUndoMsg;
            }                
        }

        protected override void PaintTypePropertyFieldWithoutRect(string label, Type type, ref ValueWrapper valueWrapper)
        {
                
            var wrapper = valueWrapper as CustomTypeValueWrapper;

            PaintFoldout(ref wrapper, label);

            if (wrapper.isInstantiated && wrapper.foldout)
            {
                var rootUndoMsg = DefaultSerializableTypes.undoMsg1;
                DefaultSerializableTypes.undoMsg1 += ".";

                EditorGUI.indentLevel++;
                var fields = wrapper.GetFieldsUpdated();
                if (fields == null || fields.Count == 0)
                    EditorGUILayout.HelpBox("This type has no serializable fields.", MessageType.Info);
                else
                {
                    GUIStyle customWindowStyle = new("window");
                    customWindowStyle.padding = new RectOffset(20, 5, 5, 5);
                    customWindowStyle.margin = new RectOffset(20, 15, 5, 5);

                    var manager = AEAManager.Instance; 
                    EditorGUILayout.BeginVertical(customWindowStyle);
                    foreach (var field in fields)
                    {
                        var elemTypeMethods = manager.RetrieveTypeMethods(field.Type);
                        elemTypeMethods.PaintTypePropertyField(field.fieldName, field.Type, ref field.valueWrapper);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;

                DefaultSerializableTypes.undoMsg1 = rootUndoMsg;
            }                
        }

        public override float GetPropertyFieldSize(ref ValueWrapper valueWrapper)
        {
            var customValueWrapper = (CustomTypeValueWrapper)valueWrapper;
            if (customValueWrapper.isInstantiated && customValueWrapper.foldout)
            {
                float extraSize = 0;

                var fields = customValueWrapper.GetFieldsUpdated();
                if (fields != null && fields.Count > 0)
                {
                    var manager = AEAManager.Instance;
                    foreach (var field in fields)
                    {
                        var typeMethods = manager.RetrieveTypeMethods(field.Type);
                        extraSize += typeMethods.GetPropertyFieldSize(ref field.valueWrapper) + 2;
                    }
                    return EditorGUIUtility.singleLineHeight * 2f + extraSize;
                }

                return EditorGUIUtility.singleLineHeight * 3.35f;
            }

            return EditorGUIUtility.singleLineHeight;
        }

        public override object Clone(ValueWrapper valueWrapper)
        {
            var customTypeWrapper = valueWrapper as CustomTypeValueWrapper;
            return customTypeWrapper.GetClone();
        }

        protected virtual void PaintFoldout(ref CustomTypeValueWrapper wrapper, string label, Rect rect) {
            if(!wrapper.isInstantiated)
                EditorGUI.LabelField(rect, label);
            else
            {
                EditorGUI.BeginChangeCheck();
                var foldout = EditorGUI.Foldout(rect, wrapper.foldout, label);
                if (EditorGUI.EndChangeCheck())
                {
                    var rootUndoMsg = DefaultSerializableTypes.undoMsg1;
                    DefaultSerializableTypes.undoMsg1 += "-Foldout";
                    // Undo.RecordObject(DefaultSerializableTypes.UndoTarget, DefaultSerializableTypes.UndoMsg);
                    wrapper.foldout = foldout;
                    DefaultSerializableTypes.undoMsg1 = rootUndoMsg;
                }
            }
        }
        protected virtual void PaintFoldout(ref CustomTypeValueWrapper wrapper, string label) {
            if (!wrapper.isInstantiated)
                EditorGUILayout.LabelField(label);
            else
            {
                EditorGUI.BeginChangeCheck();
                var foldout = EditorGUILayout.Foldout(wrapper.foldout, label);
                if (EditorGUI.EndChangeCheck())
                {
                    var rootUndoMsg = DefaultSerializableTypes.undoMsg1;
                    DefaultSerializableTypes.undoMsg1 += "-Foldout";
                    // Undo.RecordObject(DefaultSerializableTypes.UndoTarget, DefaultSerializableTypes.UndoMsg);
                    wrapper.foldout = foldout;
                    DefaultSerializableTypes.undoMsg1 = rootUndoMsg;
                }
            }
        }
    }
    public class ClassTypeMethods : CustomTypeTypeMethods {
        public override ValueWrapper GetValueWrapper(Type type) => new ClassValueWrapper(type, value);

        static readonly GUIContent iconPlus = new (EditorGUIUtility.IconContent("Toolbar Plus").image, "Create Instance");
        static readonly GUIContent iconMinus = new (EditorGUIUtility.IconContent("Toolbar Minus").image, "Delete Instance");
        protected override void PaintFoldout(ref CustomTypeValueWrapper wrapper, string label)
        {
            PaintFoldout(ref wrapper, label, EditorGUILayout.GetControlRect());
        }

        protected override void PaintFoldout(ref CustomTypeValueWrapper wrapper, string label, Rect rect)
        {
            EditorGUILayout.BeginHorizontal();
            base.PaintFoldout(ref wrapper, label, rect);
            rect.y += 4;
            rect.x += rect.width - 20;
            rect.width = 20; 
            if (!wrapper.isInstantiated)
            {
                if (GUI.Button(rect, iconPlus, GUIStyle.none))
                    wrapper.CreateDefaultInstance();
            }
            else if (GUI.Button(rect, iconMinus, GUIStyle.none))
                wrapper.SetValue(null);

            EditorGUILayout.EndHorizontal();
        }
    }
    public class StructTypeMethods : CustomTypeTypeMethods {
        public override ValueWrapper GetValueWrapper(Type type) => new StructValueWrapper(type, value);

    }

    public class SerializedPropertyTypeMethods : TypeMethods
    {
        public override ValueWrapper GetValueWrapper(Type type) => new GenericWrapperSerializedProperty(type, (SerializedProperty)value);

        protected override void PaintTypePropertyFieldWithoutRect(string label, Type type, ref ValueWrapper valueWrapper)
        {
            var wrapper = valueWrapper as GenericWrapperSerializedProperty;
            var property = wrapper.SerializedProperty;
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }

        protected override void PaintTypePropertyFieldWithRect(string label, Type type, ref ValueWrapper valueWrapper, Rect rect)
        {
            var wrapper = valueWrapper as GenericWrapperSerializedProperty;
            var property = wrapper.SerializedProperty;
            EditorGUI.PropertyField(rect, property, new GUIContent(label));
        }
    }    
    */
}
#endif
