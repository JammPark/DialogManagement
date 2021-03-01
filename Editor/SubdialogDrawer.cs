using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UIElements;

namespace JaeminPark.DialogManagement
{
    [CustomPropertyDrawer(typeof(Subdialog))]
    public class SubdialogDrawer : PropertyDrawer
    {
        private bool initialized = false;
        List<System.Type> actionTypes;
        
        private void Init()
        {
            actionTypes = ActionAttribute.GetAllActions();
            initialized = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!initialized)
                Init();

            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(position, label.text);
            position.y += EditorGUIUtility.singleLineHeight;

            foreach (SerializedProperty child in GetChildrenProps(property))
            {
                if (child.isArray)
                {
                    ListBar(position, child, label);
                    ListGUI(position, child, label);
                }
            }
        }

        public void ListBar(Rect position, SerializedProperty list, GUIContent label)
        {
            position.x += 5;
            position.y += 3;
            position.width = 3;
            position.height = ListHeight(list, label) - 20;
            EditorGUI.DrawRect(position, new Color(0, 0, 0, 0.3f));
        }

        public void ListGUI(Rect position, SerializedProperty list, GUIContent label)
        {
            position.x += 20;
            position.width -= 20;
            position.height = EditorGUIUtility.singleLineHeight;

            int arraySize = list.arraySize;

            bool moveDown = false;
            bool moveUp = false;
            bool insert = false;
            bool delete = false;
            int buttonIndex = -1;
            
            if (arraySize > 0)
            {
                for (int i = 0; i < arraySize; i++)
                {
                    string fullTypeName = list.GetArrayElementAtIndex(i).managedReferenceFullTypename;
                    string[] splitted = fullTypeName.Split(' ');
                    ActionAttribute actionAttribute = ActionAttribute.GetAttribute(splitted[splitted.Length - 1]);

                    if (ActionHeader(position, actionAttribute, ref moveDown, ref moveUp, ref insert, ref delete))
                        buttonIndex = i;

                    position.y += EditorGUIUtility.singleLineHeight * 1.2f;
                    position.y += ActionGUI(position, list.GetArrayElementAtIndex(i));
                    position.y += EditorGUIUtility.singleLineHeight * 1f;
                }
            }

            if (GUI.Button(position, "+"))
            {
                insert = true;
                buttonIndex = arraySize;
            }
            position.y += EditorGUIUtility.singleLineHeight;
            position.y += EditorGUIUtility.singleLineHeight * 0.5f;

            if (insert)
                ShowInsertActionContextMenu(list, buttonIndex);
            else if (delete)
                DeleteAction(list, buttonIndex);
            else if (moveUp && buttonIndex != 0)
                MoveAction(list, buttonIndex, -1);
            else if (moveDown && buttonIndex != arraySize - 1)
                MoveAction(list, buttonIndex, 1);
        }

        void Separator(Rect position)
        {
            EditorGUI.LabelField(position, "", GUI.skin.horizontalSlider);
        }

        float ActionGUI(Rect position, SerializedProperty action)
        {
            float height = 0;

            foreach (SerializedProperty child in GetChildrenProps(action))
            {
                position.height = EditorGUI.GetPropertyHeight(child);
                EditorGUI.PropertyField(position, child, true);
                position.y += position.height;

                height += position.height;
            }
            
            return height;
        }

        bool ActionHeader(Rect position, ActionAttribute actionAttribute, ref bool moveDown, ref bool moveUp, ref bool insert, ref bool delete)
        {
            bool pressed = false;
            
            EditorGUI.LabelField(position, actionAttribute.guiContent, EditorStyles.boldLabel);

            position.x = position.x + position.width - 120;
            position.width = 27;
            
            if (GUI.Button(position, "↓"))
            {
                moveDown = true;
                pressed = true;
            }

            position.x += 30;

            if (GUI.Button(position, "↑"))
            {
                moveUp = true;
                pressed = true;
            }

            position.x += 30;

            if (GUI.Button(position, "+"))
            {
                insert = true;
                pressed = true;
            }

            position.x += 30;

            if (GUI.Button(position, "-"))
            {
                delete = true;
                pressed = true;
            }

            position.x += 30;
            
            return pressed;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            foreach (SerializedProperty child in GetChildrenProps(property))
            {
                if (child.isArray)
                    height += ListHeight(child, label);
            }

            return height;
        }

        public float ListHeight(SerializedProperty list, GUIContent label)
        {
            float height = 0;

            int arraySize = list.arraySize;
            
            if (arraySize > 0)
            {
                for (int i = 0; i < arraySize; i++)
                {
                    foreach (SerializedProperty child in GetChildrenProps(list.GetArrayElementAtIndex(i)))
                    {
                        height += EditorGUI.GetPropertyHeight(child);
                    }
                    height += EditorGUIUtility.singleLineHeight * 2.2f;
                }
            }
            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.singleLineHeight * 0.5f;

            return height;
        }

        public IEnumerable<SerializedProperty> GetChildrenProps(SerializedProperty property)
        {
            property = property.Copy();
            var nextElement = property.Copy();
            bool hasNextElement = nextElement.NextVisible(false);
            if (!hasNextElement)
            {
                nextElement = null;
            }

            property.NextVisible(true);
            while (true)
            {
                if ((SerializedProperty.EqualContents(property, nextElement)))
                {
                    yield break;
                }

                yield return property;

                bool hasNext = property.NextVisible(false);
                if (!hasNext)
                {
                    break;
                }
            }
        }

        private void ShowInsertActionContextMenu(SerializedProperty prop, int index)
        {
            SerializedProperty copy = prop.Copy();

            GenericMenu context = new GenericMenu();
            foreach (System.Type t in actionTypes)
            {
                context.AddItem(
                    new GUIContent(ActionAttribute.GetAttribute(t).menu),
                    false,
                    () => {
                        copy.serializedObject.Update();
                        InsertAction(copy, index, t);
                        copy.serializedObject.ApplyModifiedProperties();
                    }
                );
            }
            context.ShowAsContext();
        }

        private void InsertAction(SerializedProperty prop, int index, System.Type t)
        {
            GUI.FocusControl(null);

            ActionBase instance = t.GetConstructor(new System.Type[] { }).Invoke(new object[] { }) as ActionBase;

            prop.InsertArrayElementAtIndex(index);
            prop.GetArrayElementAtIndex(index).managedReferenceValue = instance;
        }

        private void DeleteAction(SerializedProperty prop, int index)
        {
            GUI.FocusControl(null);
            
            prop.DeleteArrayElementAtIndex(index);
        }

        private void MoveAction(SerializedProperty prop, int index, int offset)
        {
            GUI.FocusControl(null);

            prop.MoveArrayElement(index, index + offset);
        }
    }
}