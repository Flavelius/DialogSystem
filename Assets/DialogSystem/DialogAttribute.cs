using System;
using UnityEngine;

namespace DialogSystem
{
    public class DialogAttribute: ScriptableObject
    {

        [NonSerialized]
        string _cachedName = "";

        public virtual string CachedName
        {
            get { return _cachedName; }
        }

        public virtual Color GetColor()
        {
            return Color.white;
        }

        public virtual string GetToolTip()
        {
            return GetType().Name;
        }

        public virtual string GetShortIdentifier()
        {
            return GetType().Name[0].ToString();
        }

        /// <summary>
        /// override this if usage of [Editor]GUILayout.X functions is needed. Important: for EditorGUILayout.Xxx wrap the calls in #if UNITY_EDITOR [..] #endif, to avoid build errors. Return true from the overridden method
        /// </summary>
        public virtual bool DrawComplexGui()
        {
            return false;
        }

        void OnEnable()
        {
            var rns = GetType().GetCustomAttributes(typeof(ReadableNameAttribute), false) as ReadableNameAttribute[];
            if (rns.Length > 0)
            {
                _cachedName = rns[0].Name;
            }
            else
            {
                _cachedName = GetType().Name;
            }
        }
    }
}
