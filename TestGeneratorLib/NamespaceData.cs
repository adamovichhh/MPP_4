using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGeneratorLib
{
    public class NamespaceData
    {
        public string Name { get; set; }
        public List<ClassData> Classes { get; set; }

        public NamespaceData(string name, List<ClassData> classes)
        {
            Name = name;
            Classes = classes;
        }

    }
}
