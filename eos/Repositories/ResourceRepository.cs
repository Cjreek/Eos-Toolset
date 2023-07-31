using Eos.Nwn;
using Eos.Nwn.Bif;
using Nwn.Tga;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using Eos.Types;
using Eos.Extensions;
using Eos.Services;

namespace Eos.Repositories
{
    public enum NWNResourceSource
    {
        Unknown, BIF, External
    }

    public class NWNResource
    {
        public NWNResourceSource Source { get; set; }
        public String? ResRef { get; set; }
        public NWNResourceType Type { get; set; }
        public String FilePath { get; set; }
        public bool IsLoading { get; set; } = false;
        public bool IsLoaded { get; set; } = false;
        public object? Data { get; set; } = null;
        public Stream RawData { get; set; } = new MemoryStream();

        public NWNResource(NWNResourceSource source, String? resRef, NWNResourceType type, string filePath = "")
        {
            this.Source = source;
            this.ResRef = resRef;
            this.Type = type;
            this.FilePath = filePath;
        }
    }

    public delegate object? ResourceLoaderFunc(Stream stream);

    public class ResourceRepository
    {
        private Dictionary<(String? resRef, NWNResourceType type), NWNResource> _resources = new Dictionary<(String? resRef, NWNResourceType type), NWNResource>();
        private BifCollection bif = new BifCollection();
        private Dictionary<NWNResourceType, ResourceLoaderFunc> resourceLoaders = new Dictionary<NWNResourceType, ResourceLoaderFunc>();
        private ConcurrentQueue<NWNResource> loadQueue = new ConcurrentQueue<NWNResource>();
        private ConcurrentQueue<NWNResource> priorityQueue = new ConcurrentQueue<NWNResource>();
        private Thread? resourceLoaderThread;
        private bool stopThread = false;
        private Dictionary<NWNResourceType, HashSet<String?>> filesByTypeDict = new Dictionary<NWNResourceType, HashSet<string?>>();
        private List<NWNResource> externalResources = new List<NWNResource>();

        ~ResourceRepository()
        {
            Cleanup();
        }

        public IEnumerable<String?> GetResourceKeys(NWNResourceType type)
        {
            if (!filesByTypeDict.ContainsKey(type))
                filesByTypeDict[type] = new HashSet<String?>();
            return filesByTypeDict[type];
        }

        public IEnumerable<NWNResource> GetExternalResources()
        {
            return externalResources;
        }

        private void LoadBif(String nwnBasePath)
        {
            Log.Info("Loading resources from game data...");

            bif.Load(nwnBasePath);
            foreach (var res in bif.Resources)
            {
                if (!filesByTypeDict.ContainsKey(res.type))
                    filesByTypeDict.Add(res.type, new HashSet<String?>());
                filesByTypeDict[res.type].Add(res.resRef);
            }

            // Load Icons
            var iconPrefixes = new HashSet<String>()
            {
                "iit_", "ief_", "ife_", "ir_", "is_", "isk_", "iss_"
            };
            foreach (var img in filesByTypeDict[NWNResourceType.TGA])
            {
                if ((img != null) && ((iconPrefixes.Contains(img.Substring(0, Math.Min(3, img.Length)))) || (iconPrefixes.Contains(img.Substring(0, Math.Min(4, img.Length))))))
                    AddResource(NWNResourceSource.BIF, img, NWNResourceType.TGA);
            }
        }

        public void Initialize(String nwnBasePath)
        {
            Log.Info("Initializing resource repository...");

            filesByTypeDict.Clear();

            LoadBif(nwnBasePath);

            resourceLoaderThread = new Thread(ResourceLoaderThread);
            resourceLoaderThread.Start();

            Log.Info("Resource repository initialization complete!");
        }

        private NWNResourceType ExtensionToResourceType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".res": return NWNResourceType.RES;
                case ".bmp": return NWNResourceType.BMP;
                case ".mve": return NWNResourceType.MVE;
                case ".tga": return NWNResourceType.TGA;
                case ".wav": return NWNResourceType.WAV;
                case ".wfx": return NWNResourceType.WFX;
                case ".plt": return NWNResourceType.PLT;
                case ".ini": return NWNResourceType.INI;
                case ".mp3": return NWNResourceType.MP3;
                case ".mpg": return NWNResourceType.MPG;
                case ".mpeg": return NWNResourceType.MPG;
                case ".txt": return NWNResourceType.TXT;
                case ".plh": return NWNResourceType.PLH;
                case ".tex": return NWNResourceType.TEX;
                case ".mdl": return NWNResourceType.MDL;
                case ".thg": return NWNResourceType.THG;
                case ".fnt": return NWNResourceType.FNT;
                case ".lua": return NWNResourceType.LUA;
                case ".slt": return NWNResourceType.SLT;
                case ".nss": return NWNResourceType.NSS;
                case ".ncs": return NWNResourceType.NCS;
                case ".mod": return NWNResourceType.MOD;
                case ".are": return NWNResourceType.ARE;
                case ".set": return NWNResourceType.SET;
                case ".ifo": return NWNResourceType.IFO;
                case ".bic": return NWNResourceType.BIC;
                case ".wok": return NWNResourceType.WOK;
                case ".2da": return NWNResourceType.TWODA;
                case ".tlk": return NWNResourceType.TLK;
                case ".txi": return NWNResourceType.TXI;
                case ".git": return NWNResourceType.GIT;
                case ".bti": return NWNResourceType.BTI;
                case ".uti": return NWNResourceType.UTI;
                case ".btc": return NWNResourceType.BTC;
                case ".utc": return NWNResourceType.UTC;
                case ".dlg": return NWNResourceType.DLG;
                case ".itp": return NWNResourceType.ITP;
                case ".btt": return NWNResourceType.BTT;
                case ".utt": return NWNResourceType.UTT;
                case ".dds": return NWNResourceType.DDS;
                case ".bts": return NWNResourceType.BTS;
                case ".uts": return NWNResourceType.UTS;
                case ".ltr": return NWNResourceType.LTR;
                case ".gff": return NWNResourceType.GFF;
                case ".fac": return NWNResourceType.FAC;
                case ".bte": return NWNResourceType.BTE;
                case ".ute": return NWNResourceType.UTE;
                case ".btd": return NWNResourceType.BTD;
                case ".utd": return NWNResourceType.UTD;
                case ".btp": return NWNResourceType.BTP;
                case ".utp": return NWNResourceType.UTP;
                case ".dft": return NWNResourceType.DFT;
                case ".gic": return NWNResourceType.GIC;
                case ".gui": return NWNResourceType.GUI;
                case ".css": return NWNResourceType.CSS;
                case ".ccs": return NWNResourceType.CCS;
                case ".btm": return NWNResourceType.BTM;
                case ".utm": return NWNResourceType.UTM;
                case ".dwk": return NWNResourceType.DWK;
                case ".pwk": return NWNResourceType.PWK;
                case ".btg": return NWNResourceType.BTG;
                case ".utg": return NWNResourceType.UTG;
                case ".jrl": return NWNResourceType.JRL;
                case ".sav": return NWNResourceType.SAV;
                case ".utw": return NWNResourceType.UTW;
                case ".4pc": return NWNResourceType.FOURPC;
                case ".ssf": return NWNResourceType.SSF;
                case ".hak": return NWNResourceType.HAK;
                case ".nwm": return NWNResourceType.NWM;
                case ".bik": return NWNResourceType.BIK;
                case ".ndb": return NWNResourceType.NDB;
                case ".ptm": return NWNResourceType.PTM;
                case ".ptt": return NWNResourceType.PTT;
                case ".bak": return NWNResourceType.BAK;
                case ".dat": return NWNResourceType.DAT;
                case ".shd": return NWNResourceType.SHD;
                case ".xbc": return NWNResourceType.XBC;
                case ".wbm": return NWNResourceType.WBM;
                case ".webm": return NWNResourceType.WBM;
                case ".mtr": return NWNResourceType.MTR;
                case ".ktx": return NWNResourceType.KTX;
                case ".ttf": return NWNResourceType.TTF;
                case ".sql": return NWNResourceType.SQL;
                case ".tml": return NWNResourceType.TML;
                case ".sq3": return NWNResourceType.SQ3;
                case ".sqlite3": return NWNResourceType.SQ3;
                case ".lod": return NWNResourceType.LOD;
                case ".gif": return NWNResourceType.GIF;
                case ".png": return NWNResourceType.PNG;
                case ".jpg": return NWNResourceType.JPG;
                case ".jpeg": return NWNResourceType.JPG;
                case ".caf": return NWNResourceType.CAF;
                case ".jui": return NWNResourceType.JUI;
                case ".ids": return NWNResourceType.IDS;
                case ".erf": return NWNResourceType.ERF;
                case ".bif": return NWNResourceType.BIF;
                case ".key": return NWNResourceType.KEY;
            }

            return NWNResourceType.INVALID_RESOURCE_TYPE;
        }

        public void LoadExternalResources(IEnumerable<String> externalBasePath)
        {
            externalResources.Clear();
            foreach (String path in externalBasePath)
            {
                if (Directory.Exists(path))
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                    {
                        var ext = Path.GetExtension(file).ToLower();
                        var resType = ExtensionToResourceType(ext);
                        if (resType != NWNResourceType.INVALID_RESOURCE_TYPE)
                        {
                            var filename = Path.GetFileName(file);
                            var resRef = filename.Substring(0, filename.Length - ext.Length);

                            AddResource(NWNResourceSource.External, resRef, resType, file, true);
                            if (!filesByTypeDict.ContainsKey(resType))
                                filesByTypeDict.Add(resType, new HashSet<string?>());

                            if (!filesByTypeDict[resType].Contains(resRef))
                                filesByTypeDict[resType].Add(resRef);
                        }
                    }
                }
            }
        }

        public void Cleanup()
        {
            stopThread = true;
            resourceLoaderThread?.Join();
            bif.ClearCache();
        }

        void ResourceLoaderThread()
        {
            NWNResource? resource = null; 
            while (!stopThread)
            {
                if (priorityQueue.TryDequeue(out resource) || loadQueue.TryDequeue(out resource))
                {
                    if (resource != null && !resource.IsLoading && !resource.IsLoaded)
                    {
                        resource.IsLoading = true;
                        try
                        {
                            if (resource.Source == NWNResourceSource.BIF)
                            {
                                if ((resource.Type == NWNResourceType.TGA) && (File.Exists(Constants.IconResourcesFilePath + resource.ResRef + ".tga")))
                                {
                                    var loadResource = GetResourceLoader(resource.Type);

                                    var resBytes = File.ReadAllBytes(Constants.IconResourcesFilePath + resource.ResRef + ".tga");
                                    resource.RawData = new MemoryStream(resBytes);
                                    resource.Data = loadResource(new MemoryStream(resBytes));
                                    resource.IsLoaded = true;
                                }
                                else
                                {
                                    var rawResource = bif.ReadResource(resource.ResRef, resource.Type);
                                    if (rawResource != null)
                                    {
                                        var loadResource = GetResourceLoader(rawResource.Type);
                                        resource.Type = rawResource.Type;
                                        rawResource.RawData.CopyTo(resource.RawData);
                                        resource.RawData.Position = 0;
                                        resource.Data = loadResource(resource.RawData);
                                        resource.IsLoaded = true;
                                    }
                                }
                            }
                            else if (resource.Source == NWNResourceSource.External)
                            {
                                var loadResource = GetResourceLoader(resource.Type);

                                var resBytes = File.ReadAllBytes(resource.FilePath);
                                resource.RawData = new MemoryStream(resBytes);
                                resource.Data = loadResource(new MemoryStream(resBytes));
                                resource.IsLoaded = true;
                            }
                        }
                        finally
                        {
                            resource.IsLoading = false;
                        }
                    }
                }
                else Thread.Sleep(1);
            }
        }

        public String? AddResource(NWNResourceSource source, String? resRef, NWNResourceType type, string filePath = "", bool overwrite = false)
        {
            resRef = resRef?.ToLower();
            if ((resRef != null) && (!_resources.ContainsKey((resRef, type)) || (overwrite)))
            {
                if (overwrite) _resources.Remove((resRef, type));
                var resource = new NWNResource(source, resRef, type, filePath);
                _resources.Add((resRef, type), resource);
                loadQueue.Enqueue(resource);

                if (source == NWNResourceSource.External)
                    externalResources.Add(resource);
            }

            return resRef;
        }

        private NWNResource? GetResource(String? resRef, NWNResourceType type)
        {
            resRef = resRef?.ToLower();
            if (resRef == null) return null;

            if (!_resources.ContainsKey((resRef, type)))
            {
                if (bif.ContainsResource(resRef, type))
                    AddResource(NWNResourceSource.BIF, resRef, type); // !
                else
                    return null;
            }

            var res = _resources[(resRef, type)];
            if (!res.IsLoaded)
            {
                priorityQueue.Enqueue(res);
                while (!res.IsLoaded) Thread.Sleep(1);
            }

            return res;
        }

        public object? Get(String? resRef, NWNResourceType type)
        {
            var res = GetResource(resRef, type);
            return res?.Data;
        }

        public Stream? GetRaw(String? resRef, NWNResourceType type)
        {
            var res = GetResource(resRef, type);
            res?.RawData.Seek(0, SeekOrigin.Begin);
            return res?.RawData;
        }

        public T? Get<T>(String? resRef, NWNResourceType type)
        {
            return (T?)Get(resRef, type);
        }

        private ResourceLoaderFunc GetResourceLoader(NWNResourceType resType)
        {
            if (!resourceLoaders.TryGetValue(resType, out ResourceLoaderFunc? loader))
                loader = DefaultResourceLoader;

            return loader;
        }

        public void RegisterResourceLoader(NWNResourceType resType, ResourceLoaderFunc function)
        {
            resourceLoaders[resType] = function;
        }

        // Resource Loaders
        private object? DefaultResourceLoader(Stream stream)
        {
            return stream;
        }
    }
}
