using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Updates
{
    internal abstract class Update
    {
        public virtual int Version => 0;
        public virtual DateTime GameDataMinimumBuildDate => DateTime.MinValue;
        public virtual bool ForceGameDataUpdate => false;

        public abstract void Apply(EosProject project);
    }
}
