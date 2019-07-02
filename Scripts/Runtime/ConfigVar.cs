using System;

namespace Jbltx.Configurator
{
    public class ConfigVarAttribute : Attribute 
    {
        public string path;
        public string defaultValue;
    }
}