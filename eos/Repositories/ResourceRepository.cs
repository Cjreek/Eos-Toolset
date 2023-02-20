﻿using Eos.Nwn;
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
            bif.Load(nwnBasePath);
            foreach (var res in bif.Resources)
            {
                if (!filesByTypeDict.ContainsKey(res.type))
                    filesByTypeDict.Add(res.type, new HashSet<String?>());
                filesByTypeDict[res.type].Add(res.resRef);
            }
        }

        public void Initialize(String nwnBasePath)
        {
            filesByTypeDict.Clear();

            LoadBif(nwnBasePath);

            resourceLoaderThread = new Thread(ResourceLoaderThread);
            resourceLoaderThread.Start();
        }

        private NWNResourceType ExtensionToResourceType(string extension)
        {
            switch (extension)
            {
                case ".bmp": return NWNResourceType.BMP;
                case ".tga": return NWNResourceType.TGA;
                case ".wav": return NWNResourceType.WAV;
                case ".plt": return NWNResourceType.PLT;
                case ".ini": return NWNResourceType.INI;
                case ".txt": return NWNResourceType.TXT;
                case ".mdl": return NWNResourceType.MDL;
                case ".nss": return NWNResourceType.NSS;
                case ".ncs": return NWNResourceType.NCS;
                case ".are": return NWNResourceType.ARE;
                case ".set": return NWNResourceType.SET;
                case ".ifo": return NWNResourceType.IFO;
                case ".bic": return NWNResourceType.BIC;
                case ".wok": return NWNResourceType.WOK;
                case ".2da": return NWNResourceType.TWODA;
                case ".txi": return NWNResourceType.TXI;
                case ".git": return NWNResourceType.GIT;
                case ".uti": return NWNResourceType.UTI;
                case ".utc": return NWNResourceType.UTC;
                case ".dlg": return NWNResourceType.DLG;
                case ".itp": return NWNResourceType.ITP;
                case ".utt": return NWNResourceType.UTT;
                case ".dds": return NWNResourceType.DDS;
                case ".uts": return NWNResourceType.UTS;
                case ".ltr": return NWNResourceType.LTR;
                case ".gff": return NWNResourceType.GFF;
                case ".fac": return NWNResourceType.FAC;
                case ".ute": return NWNResourceType.UTE;
                case ".utd": return NWNResourceType.UTD;
                case ".utp": return NWNResourceType.UTP;
                case ".ssf": return NWNResourceType.SSF;
                case ".ndb": return NWNResourceType.NDB;
                case ".ptm": return NWNResourceType.PTM;
                case ".ptt": return NWNResourceType.PTT;
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