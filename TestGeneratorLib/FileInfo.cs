﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGeneratorLib
{
    public class FileInfo
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public FileInfo(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }
}
