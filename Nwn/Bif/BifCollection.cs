using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.Bif
{
    internal class BifCollection
    {
        private static readonly String BASE_KEY_FILE = @"data\nwn_base.key";

        private String _nwnBasePath = "";
        private KeyFile _keyFile = new KeyFile();
        private Dictionary<string, BifFile> bifCache = new Dictionary<string, BifFile>();

        ~BifCollection()
        {
            ClearCache();
        }

        public void Load(String nwnBasePath)
        {
            _nwnBasePath = Path.EndsInDirectorySeparator(nwnBasePath) ? nwnBasePath : nwnBasePath + Path.DirectorySeparatorChar;
            _keyFile.Load(_nwnBasePath + BASE_KEY_FILE);
        }

        public Stream ReadResource(String resRef)
        {
            if (!_keyFile.Contains(resRef))
                throw new Exception();

            var resKey = _keyFile[resRef];
            if (!bifCache.TryGetValue(resKey.SourceBif, out BifFile? bif))
            {
                bif = new BifFile();
                bif.Load(_nwnBasePath + resKey.SourceBif);
            }
          
            return bif.Read((int)resKey.BifIndex);
        }

        public void ClearCache()
        {
            foreach (BifFile bif in bifCache.Values)
            {
                bif.Close();
            }
            bifCache.Clear();
        }
    }
}
