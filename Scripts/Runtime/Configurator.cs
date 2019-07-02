using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

namespace Jbltx.Configurator
{
    public static class Configurator 
    {
        private static bool m_isInitialized = false;

        private static Dictionary<FieldInfo, ConfigVarAttribute> m_fields = new Dictionary<FieldInfo, ConfigVarAttribute>();
        private static IniFile m_ini = null;

        /// <summary>
        /// initialize the configuration module with a given INI file
        /// </summary>
        /// <param name="configFilepath">The INI file path</param>
        /// <param name="defautValueOverride">If true, all fields value will be overriden by the default one
        /// from the ConfigVar attribute
        /// </param>
        public static void Init(string configFilepath, bool defautValueOverride = true)
        {
            if (m_isInitialized)
                return;

            Debug.LogFormat("[Configurator] Loading configuration file at {0}", configFilepath);
            m_ini = new IniFile(configFilepath);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass)
                    {
                        foreach (FieldInfo fieldInfo in type.GetFields())
                        {
                            if (fieldInfo.IsDefined(typeof(ConfigVarAttribute), false) && fieldInfo.IsStatic)
                            {
                                ConfigVarAttribute attr = fieldInfo.GetCustomAttributes(typeof(ConfigVarAttribute), false)[0] as ConfigVarAttribute;
                                string[] sectionKey = attr.path.Split('.');

                                if (sectionKey.Length != 2)
                                {
                                    Debug.LogWarningFormat("[Configurator] The field {0} from class {1} use an invalid path, you need 1 section and 1 key separated by a dot", fieldInfo.Name, type.Name);
                                }
                                else
                                {
                                    if (defautValueOverride)
                                    {
                                        if (string.IsNullOrEmpty(attr.defaultValue))
                                            Debug.LogWarningFormat("[Configurator] The field {0} from class {1} will be overrided using a null value", fieldInfo.Name, type.Name);
                                        fieldInfo.SetValue(null, attr.defaultValue);
                                    }
                                    fieldInfo.SetValue(null, m_ini.GetValue(sectionKey[0], sectionKey[1], fieldInfo.GetValue(null) as string));

                                    m_fields.Add(fieldInfo, attr);
                                }                              
                            }
                        }
                    }
                }
            }
        }

        

        /// <summary>
        /// Save the current configuration
        /// </summary>
        /// <param name="filepath"></param>
        public static void Save(string filepath = "")
        {
            if (m_ini != null)
            {
                Debug.LogFormat("[Configurator] Saving configuration at {0}", m_ini.TheFile);
                foreach (KeyValuePair<FieldInfo, ConfigVarAttribute> kvp in m_fields)
                {
                    string[] sectionKey = kvp.Value.path.Split('.');
                    m_ini.SetValue(sectionKey[0], sectionKey[1], kvp.Key.GetValue(null) as string);
                }

                m_ini.Save(filepath);
            }
            else
            {
                Debug.LogWarning("[Configurator] Call Save() but no INI file loaded");
            }
        }
    }
}