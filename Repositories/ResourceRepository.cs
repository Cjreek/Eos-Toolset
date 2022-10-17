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

namespace Eos.Repositories
{
    public class NWNResource
    {
        public String? ResRef { get; set; }
        public NWNResourceType Type { get; set; }
        public bool IsLoading { get; set; } = false;
        public bool IsLoaded { get; set; } = false;
        public object? Data { get; set; } = null;
        public Stream RawData { get; set; } = new MemoryStream();

        public NWNResource(String? resRef, NWNResourceType type)
        {
            this.ResRef = resRef;
            this.Type = type;
        }
    }

    public delegate object? ResourceLoaderFunc(Stream stream);

    internal class ResourceRepository
    {
        private Dictionary<(String? resRef, NWNResourceType type), NWNResource> _resources = new Dictionary<(String? resRef, NWNResourceType type), NWNResource>();
        private BifCollection bif = new BifCollection();
        private Dictionary<NWNResourceType, ResourceLoaderFunc> resourceLoaders = new Dictionary<NWNResourceType, ResourceLoaderFunc>();
        private ConcurrentQueue<NWNResource> loadQueue = new ConcurrentQueue<NWNResource>();
        private ConcurrentQueue<NWNResource> priorityQueue = new ConcurrentQueue<NWNResource>();
        private Thread? resourceLoaderThread;
        private bool stopThread = false;

        ~ResourceRepository()
        {
            Cleanup();
        }

        public void Initialize(String nwnBasePath)
        {
            RegisterResourceLoader(NWNResourceType.TGA, TargaResourceLoader);
            RegisterResourceLoader(NWNResourceType.NSS, ScriptSourceLoader);

            bif.Load(nwnBasePath);

            resourceLoaderThread = new Thread(ResourceLoaderThread);
            resourceLoaderThread.Start();
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
                                    resource.Data = loadResource(rawResource.RawData);
                                    resource.IsLoaded = true;
                                }
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

        public String? AddResource(String? resRef, NWNResourceType type)
        {
            resRef = resRef?.ToLower();
            if ((resRef != null) && (!_resources.ContainsKey((resRef, type))))
            {
                var resource = new NWNResource(resRef, type);
                _resources.Add((resRef, type), resource);
                loadQueue.Enqueue(resource);
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
                    AddResource(resRef, type);
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

        private object? TargaResourceLoader(Stream stream)
        {
            if (stream.Length > 0)
            {
                TargaImage tga = new TargaImage(stream);
                tga.Image?.Freeze();
                return tga.Image;
            }
            else
                return null;
        }

        private object? ScriptSourceLoader(Stream stream)
        {
            if (stream.Length > 0)
            {
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            else
                return null;
        }
    }
}
