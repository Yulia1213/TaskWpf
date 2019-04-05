using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskWpf
{
    public class Module
    {
        private String moduleName;
        private String moduleFile;
        public Module(ProcessModule module)
        {
            moduleName = module.ModuleName;
            moduleFile = module.FileName;
        }
        public String ModuleName
        {
            get { return this.moduleName; }
            set
            {
                this.moduleName = value;

            }
        }
        public String ModuleFile
        {
            get { return moduleFile; }
            set
            {
                this.moduleFile = value;
            }
        }


    }

}
