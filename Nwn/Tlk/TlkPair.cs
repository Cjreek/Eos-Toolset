using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.Tlk
{
    internal class TlkPair
    {
        private TlkFile tlk = new TlkFile();
        private TlkFile tlkF = new TlkFile();

        public void Load(String tlkFile)
        {
            tlk.Load(tlkFile);

            var tlkfFile = Path.GetDirectoryName(tlkFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(tlkFile) + "f.tlk";
            if (File.Exists(tlkfFile))
                tlkF.Load(tlkfFile);
            else
                tlkF = tlk;
        }

        public String GetString(bool female, int? stringRef)
        {
            return female ? tlkF.GetString(stringRef) : tlk.GetString(stringRef);
        }
    }
}
