using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoreDrawablesGenerator
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectControl : Attribute
    {
        public string ControlName { get; set; }
        public InjectControl() { }

        public InjectControl(string controlName)
        {
            ControlName = controlName;
        }
    }

    public static class ControlInjector
    {
        private static readonly BindingFlags searchFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Injects all [InjectControl] fields and properties.
        /// </summary>
        /// <param name="window">Window to inject fields/properties in.</param>
        public static void InjectControls(this Window window, bool overrideExisting = false)
        {
            InjectProperties(window, overrideExisting);
            InjectFields(window, overrideExisting);
        }

        private static void InjectProperties(Window window, bool overrideExisting)
        {
            foreach (PropertyInfo property in window.GetType().GetProperties(searchFlags))
            {
                Type t = property.PropertyType;

                if (overrideExisting || property.GetValue(window) == null)
                {
                    foreach (InjectControl item in property.GetCustomAttributes(typeof(InjectControl), false))
                    {
                        property.SetValue(window, GenericFindControl(t).Invoke(window, new object[] { window, item.ControlName != null ? item.ControlName : property.Name }));
                    }
                }
            }
        }

        private static void InjectFields(Window window, bool overrideExisting)
        {
            foreach (FieldInfo field in window.GetType().GetFields(searchFlags))
            {
                Type t = field.FieldType;
                
                if (overrideExisting || field.GetValue(window) == null)
                {
                    foreach (InjectControl item in field.GetCustomAttributes(typeof(InjectControl), false))
                    {
                        field.SetValue(window, GenericFindControl(t).Invoke(window, new object[] { window, item.ControlName != null ? item.ControlName : field.Name }));
                    }
                }
            }
        }

        private static MethodInfo GenericFindControl(Type type)
        {
            MethodInfo m = typeof(ControlExtensions).GetMethod("FindControl");
            return m.MakeGenericMethod(type);
        }
    }
}
