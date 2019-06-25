using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Panacea.Modules.DeveloperPanel.Models
{
    [DataContract]
    public class PluginInfo
    {
        public PluginInfo()
        {
            IsInitialized = false;

        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "className")]
        public string ClassName { get; set; }

        [DataMember(Name = "ns")]
        public string Namespace { get; set; }

        [DataMember(Name = "file")]
        public string FileName { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        public Brush Background
        {
            get
            {
                if (!IsInitialized) return Brushes.Red;
                if (Version != RealVersion) return Brushes.LightSalmon;
                return Brushes.LightGray;
            }

        }
        public string RealVersion { get; set; }

        public bool IsInitialized { get; set; }

        public string GetFullClassName()
        {
            return Namespace + "." + ClassName;
        }

        public string Error { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}
