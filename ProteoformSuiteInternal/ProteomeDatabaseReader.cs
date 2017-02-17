using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Proteomics;

//Inspired by the class by the same name from Morpheus (http://cwenger.github.io/Morpheus) by Craig Wenger
namespace ProteoformSuiteInternal
{
    public class ProteomeDatabaseReader
    {
        //public static string oldPtmlistFilePath;

        //public Dictionary<string, Modification> ReadUniprotPtmlist()
        //{
        //    string ptmFilePath = GetPtmlistPath_AfterFileRefresh();
        //    int modCount = File.ReadAllText(ptmFilePath).Split(new string[] { "//" }, StringSplitOptions.None).Length - 2; //There will be an extra element from the legend
        //    return LoadUniprotModifications(ptmFilePath, modCount);
        //}

        //static string GetPtmlistPath_AfterFileRefresh()
        //{
        //    try
        //    {
        //        string new_ptmlist_filepath = Path.Combine(Path.GetDirectoryName(oldPtmlistFilePath), "ptmlist_new.txt");
        //        using (WebClient client = new WebClient())
        //            client.DownloadFile("http://www.uniprot.org/docs/ptmlist.txt", new_ptmlist_filepath);
        //        string old_ptmlist = File.ReadAllText(oldPtmlistFilePath);
        //        string new_ptmlist = File.ReadAllText(new_ptmlist_filepath);
        //        if (string.Equals(old_ptmlist, new_ptmlist))
        //            File.Delete(new_ptmlist_filepath);
        //        else
        //        {
        //            File.Delete(oldPtmlistFilePath);
        //            File.Move(new_ptmlist_filepath, oldPtmlistFilePath);
        //        }
        //    }
        //    catch { }
        //    return oldPtmlistFilePath;
        //}
    }
}


