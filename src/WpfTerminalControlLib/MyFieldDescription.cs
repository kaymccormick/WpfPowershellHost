using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terminal1
{
    public class MyFieldDescription
    {
        public MyFieldDescription(object defaultValue, string helpMessage, bool isMandatory, string label, string name, string parameterAssemblyFullName, string parameterTypeFullName, string parameterTypeName)
        {
            DefaultValue = defaultValue;
            HelpMessage = helpMessage;
            IsMandatory = isMandatory;
            Label = label;
            Name = name;
            ParameterAssemblyFullName = parameterAssemblyFullName;
            ParameterTypeFullName = parameterTypeFullName;
            ParameterTypeName = parameterTypeName;
        }

        public IList<Attribute> Attributes { get; set; } = new List<Attribute>();

        public object DefaultValue { get; set; }

        public string HelpMessage { get; set; }

        public bool IsMandatory { get; set; }

        public string Label { get; set; }

        public string Name { get; set; }

        public string ParameterAssemblyFullName { get; set; }

        public string ParameterTypeFullName { get; set; }

        public string ParameterTypeName { get; set; }
    }
}