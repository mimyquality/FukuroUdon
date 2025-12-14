using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    internal class GUIDLibrary : ScriptableObject
    {
        #if UNITY_EDITOR
        public UnityEditor.DefaultAsset folder;
        #else
        public UnityEngine.Object folder;
        #endif
        public string unitypackage;

        public string displayName;
        public new string name;
        public string url;
        public string repo;
        internal string[] guids;

        internal static void Update()
        {
            var zipPath = "Temp/guid_database_temp";
            var filesPath = "Temp/guid_database";

            var wc = new WebClient();
            wc.DownloadFile("https://github.com/lilxyzw/guid_database/archive/refs/heads/main.zip", zipPath);
            var zip = ZipFile.OpenRead(zipPath);
            Directory.CreateDirectory(filesPath);
            foreach(var entry in zip.Entries)
            {
                if(!entry.Name.EndsWith(".txt")) continue;
                using var sr = new StreamReader(entry.Open(), Encoding.Unicode);
                try{DeserializeFromText(sr).SerializeToBinary($"{filesPath}/{Path.GetFileNameWithoutExtension(entry.Name)}.bin");}catch{}
            }
            wc.Dispose();
            zip.Dispose();

            if(File.Exists(Grimoire.PATH_DATABASE)) File.Delete(Grimoire.PATH_DATABASE);
            if(!Directory.Exists(Common.PREFERENCE_FOLDER)) Directory.CreateDirectory(Common.PREFERENCE_FOLDER);
            ZipFile.CreateFromDirectory(filesPath, Grimoire.PATH_DATABASE);
            File.Delete(zipPath);
            Directory.Delete(filesPath, true);
        }

        internal void SerializeToText(string path)
        {
            using var sw = new StreamWriter(path, false, Encoding.Unicode);

            // Header
            if(!string.IsNullOrEmpty(displayName)) sw.WriteLine($"displayName: {displayName}");
            if(!string.IsNullOrEmpty(name)) sw.WriteLine($"name: {name}");
            if(!string.IsNullOrEmpty(url)) sw.WriteLine($"url: {url}");
            if(!string.IsNullOrEmpty(repo)) sw.WriteLine($"repo: {repo}");
            sw.WriteLine("guids:");

            // GUID Binary
            foreach(var guid in guids) sw.WriteLine(guid);
        }

        internal static GUIDLibrary DeserializeFromText(StreamReader sr)
        {
            var lib = CreateInstance<GUIDLibrary>();

            // Header
            string line;
            while((line = sr.ReadLine()) != null)
            {
                if(line.StartsWith("displayName: ")) lib.displayName = line.Substring("displayName: ".Length);
                else if(line.StartsWith("name: ")) lib.name = line.Substring("name: ".Length);
                else if(line.StartsWith("url: ")) lib.url = line.Substring("url: ".Length);
                else if(line.StartsWith("repo: ")) lib.repo = line.Substring("repo: ".Length);
                else if(line.StartsWith("guids:")) break;
            }

            // GUID Binary
            var guids = new List<string>();
            while((line = sr.ReadLine()) != null)
            {
                if(Guid.TryParse(line, out _)) guids.Add(line);
            }
            lib.guids = guids.ToArray();

            return lib;
        }

        internal void SerializeToBinary(string path)
        {
            using var sw = new StreamWriter(path, false, Encoding.Unicode);

            // Header
            if(!string.IsNullOrEmpty(displayName)) sw.WriteLine($"displayName: {displayName}");
            if(!string.IsNullOrEmpty(name)) sw.WriteLine($"name: {name}");
            if(!string.IsNullOrEmpty(url)) sw.WriteLine($"url: {url}");
            if(!string.IsNullOrEmpty(repo)) sw.WriteLine($"repo: {repo}");
            sw.WriteLine("guids:");

            // GUID Binary
            foreach(var guid in guids) sw.Write(CompressGUID(guid));
        }

        internal static GUIDLibrary SearchLib(string path, ReadOnlySpan<char> guid)
        {
            using var zip = ZipFile.OpenRead(path);
            var lib = CreateInstance<GUIDLibrary>();
            foreach(var entry in zip.Entries)
            {
                if(!entry.Name.EndsWith(".bin")) continue;
                using var sr = new StreamReader(entry.Open(), Encoding.Unicode);

                // Header
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    if(line.StartsWith("displayName: ")) lib.displayName = line.Substring("displayName: ".Length);
                    else if(line.StartsWith("name: ")) lib.name = line.Substring("name: ".Length);
                    else if(line.StartsWith("url: ")) lib.url = line.Substring("url: ".Length);
                    else if(line.StartsWith("repo: ")) lib.repo = line.Substring("repo: ".Length);
                    else if(line.StartsWith("guids:")) break;
                }

                // GUID Binary
                var bytes = new Span<char>(new char[8]);
                while(sr.ReadBlock(bytes) == 8)
                {
                    if(bytes.SequenceEqual(guid)) return lib;
                }

                lib.displayName = null;
                lib.name = null;
                lib.url = null;
                lib.repo = null;
            }
            return null;
        }

        internal static ReadOnlySpan<char> CompressGUID(string guid) => Encoding.Unicode.GetString(Guid.Parse(guid).ToByteArray()).AsSpan();
    }
}
