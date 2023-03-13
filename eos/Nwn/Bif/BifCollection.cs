using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.Bif
{

    public class RawResourceData
    {
        public NWNResourceType Type { get; set; }
        public Stream RawData { get; set; }

        public RawResourceData(NWNResourceType type, Stream rawData)
        {
            Type = type;
            RawData = rawData;
        }
    }

    public class BifCollection
    {
        private static readonly String BASE_KEY_FILE = Path.Combine("data", "nwn_base.key");

        private String _nwnBasePath = "";
        private KeyFile _keyFile = new KeyFile();
        private Dictionary<string, BifFile> bifCache = new Dictionary<string, BifFile>();

        public IEnumerable<(String? resRef, NWNResourceType type)> Resources => _keyFile;

        ~BifCollection()
        {
            ClearCache();
        }

        public void Load(String nwnBasePath)
        {
            _nwnBasePath = Path.EndsInDirectorySeparator(nwnBasePath) ? nwnBasePath : nwnBasePath + Path.DirectorySeparatorChar;
            _keyFile.Load(_nwnBasePath + BASE_KEY_FILE);
        }

        public bool ContainsResource(String? resRef, NWNResourceType type)
        {
            return _keyFile.Contains(resRef, type);
        }

        public RawResourceData ReadResource(String? resRef, NWNResourceType type, bool throwExceptionOnMissingResource = false)
        {
            if (resRef == null || !_keyFile.Contains(resRef, type))
            {
                if (throwExceptionOnMissingResource)
                    throw new Exception();
                else
                    return new RawResourceData(NWNResourceType.INVALID_RESOURCE_TYPE, new MemoryStream());
            }

            var resKey = _keyFile[resRef, type];
            if (!bifCache.TryGetValue(resKey.SourceBif, out BifFile? bif))
            {
                bif = new BifFile();
                bif.Load(_nwnBasePath + resKey.SourceBif);
            }
          
            return new RawResourceData(resKey.ResourceType, bif.Read((int)resKey.BifIndex));
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
