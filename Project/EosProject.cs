using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Project
{
    public class EosProject
    {
        private String _projectFolder = "";

        public String Name { get; set; } = "";
        public String ProjectFolder { get { return _projectFolder; } }

        public void Load(String projectFolder)
        {
            this._projectFolder = projectFolder;

            MasterRepository.Clear();
        }

        public void Save()
        {

        }
    }
}
