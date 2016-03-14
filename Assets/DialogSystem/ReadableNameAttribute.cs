using UnityEngine;
using System.Collections;
using System;

namespace DialogSystem
{
    public class ReadableNameAttribute : Attribute
    {
        public string Name;

        public ReadableNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}