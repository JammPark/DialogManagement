using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JaeminPark.DialogManagement
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionAttribute : Attribute
    {
        public string menu { get; private set; }
        public string iconName { get; private set; }

        public ActionAttribute(string menu, string iconName)
        {
            this.menu = menu;
            this.iconName = iconName;
        }

#if UNITY_EDITOR
        public GUIContent guiContent
        {
            get
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Gizmos/DialogSystem/" + iconName + ".png");
                string[] splitted = menu.Split('/');
                string name = splitted[splitted.Length - 1];

                if (texture != null)
                    return new GUIContent(" " + name, texture);
                else
                    return new GUIContent(name);
            }
        }

        public static List<Type> GetAllActions()
        {
            List<Type> list = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.GetCustomAttributes(typeof(ActionAttribute), true).Length > 0)
                        list.Add(type);
                }
            }
            return list;
        }

        public static ActionAttribute GetAttribute(Type t)
        {
            foreach (object attribute in t.GetCustomAttributes(typeof(ActionAttribute), true))
            {
                ActionAttribute actionAttribute = attribute as ActionAttribute;
                if (actionAttribute != null)
                    return actionAttribute;
            }
            return null;
        }

        public static ActionAttribute GetAttribute(string fullTypeName)
        {
            return GetAttribute(Type.GetType(fullTypeName));
        }
#endif
    }
}
