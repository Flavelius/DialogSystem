using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace DialogSystem
{
    [CreateAssetMenu]
    public class DialogCollection: ScriptableObject
    {
        public List<Dialog> dialogs = new List<Dialog>();
    }
}
