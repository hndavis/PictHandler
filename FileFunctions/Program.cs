using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FileFunctions
{
    using io = System.IO;

    public class Manipulate
    {

        private static string[] fileExtensions = new string[] {"jpg", "mpeg", "png"};
        private List<string> excludeDirs = new List<string>();
        //cmd lines
        //C:\\docs\\bulk_pictures_iphone_hndavis


        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:   FileFunctions  -l=location  -m=org_by_date -c=to_destination ");
                Console.WriteLine("or");
                Console.WriteLine("Usage:   CopyAll s=source d=dest");
                return;
            }
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.None);


            var doc = XDocument.Load(config.FilePath);

            //  System.Xml.XPath.XPathDocument doc = new System.Xml.XPath.XPathDocument(@"config.xml");

            var excludeDirs =
                doc.Root.Descendants()
                    .Select(x => x.Element("Exclude"))
                    .Descendants("dirs")
                    .Descendants()
                    .Select(d => d.Value);
            //.Select( y=> y.Element("dirs"))
            //;
            XElement xElement = XElement.Parse(doc.ToString());

            var includedDirs =
               doc.Root.Descendants()
                   .Select(x => x.Element("Include"))
                   .Descendants("dirs")
                   .Descendants()
                   .Select(d => d.Value);
           

            var extensions = doc.Root.Element("FileInfoItems").Element("Picts").Descendants().Select(v => v.Value);



            //RenameDirToDate();
            if (args[0] == "FileFunctions")
                BulkToDateDir(args);
            else if (args[0] == "CopyAll")
            {
                Dictionary<string, string> defArgs = new Dictionary<string, string>();
                for (int i = 1; i < args.Count(); i++)
                {
                    if (args[i].Contains("="))
                    {
                        string[] parts = args[i].Split('=');
                        defArgs.Add(parts[0], parts[1]);

                    }
                    else
                    {
                        throw new Exception("Bad Args");
                    }


                }
                foreach (var source in includedDirs)
                {
                    CopyAllToDest(source, defArgs["d"], extensions.ToList(), excludeDirs.ToList());
                }
            }
        }

        private static void CopyAllToDest(string source, string dest, List<string> extentions,
            List<string> ExcludeDirs)
        {
        }

       public List<string> GetAllFilesInCriteria( string SourceDir, List<string>  includedDirs, 
           List<string>  excludedDirs, List<string> extentions, Action<FileInfoItem> onNext)
        {

            //if (SourceDir == dest) // dont copy itself
            //    return;
            foreach (var exdir in excludeDirs)
            {
                if (SourceDir.Contains(exdir))
                    return null;
            }
            if (excludeDirs.Exists(x => x == SourceDir))
                return null;

            List<string> foundFiles = new List<string>();

            //Regex fileExt = new Regex();
            RegexOptions ro = new RegexOptions();
            string currentFile = null;

            try
            {

                var files = System.IO.Directory.EnumerateFiles(SourceDir);

                foreach (var file in files)
                {

                    //foreach directory call CopyAllToDest( dir. dest )
                    Match m = null;
                    bool success = false;
                    foreach (var ext in extentions)
                    {

                        if (!success)
                        {

                            m = Regex.Match(file, "." + ext + "$");
                            if (m.Success)
                            {
                                success = true;
                                //does the file exists
                              //  if (!System.IO.File.Exists(dest + "\\" + file))
                              //  {
                                    // System.IO.File.Copy(file, dest);
                                    Console.WriteLine("\n" + file);
                                foundFiles.Add(file);
                                onNext(new FileInfoItem(file));
                                //   }
                                //else
                                //{
                                //    Console.Write(".");
                                //    Console.WriteLine("\n no=" + file);
                                //}

                            }

                        }

                        //Console.Write(".");
                    }
                }
            }
            catch (UnauthorizedAccessException UAEx)
            {
                Console.WriteLine(UAEx.Message);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                Console.WriteLine("NOT FOUND:" + dnfe);
            }
            catch (Exception)
            {

                throw;
            }


            foreach (var dir in System.IO.Directory.EnumerateDirectories(SourceDir))
            {
                try
                {
                    //CopyAllToDest(dir,  extentions, ExcludeDirs);
                    foundFiles.AddRange(GetAllFilesInCriteria(dir, includedDirs, excludedDirs, 
                        extentions, onNext));
                }

                catch (DirectoryNotFoundException dnfe)
                {
                    Console.WriteLine("NOT FOUND:" + dnfe);
                }
                catch (UnauthorizedAccessException UAEx)
                {
                    Console.WriteLine(UAEx.Message);
                }
                catch (Exception)
                {

                    throw;


                }
            }

           return foundFiles;
        }

        public IObservable<FileInfoItem> CreateObservable(string SourceDir, List<string> includedDirs,
            List<string> excludedDirs, List<string> extentions)
        {
            return Observable.Create<FileInfoItem>(o =>
            {
               
                GetAllFilesInCriteria(SourceDir, includedDirs, excludedDirs, extentions,o.OnNext);
                return Disposable.Empty;
            });
        }

        static void RenameDirToDate()
        {
            string Dest = @"C:\Users\hndavis\Pictures\Event";
            string src = @"C:\Users\hndavis\Pictures\2013-11-03sx40_1103";
            var dirs = System.IO.Directory.EnumerateDirectories(src);
            foreach (var dir in dirs)
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                FileSystemInfo[] filesInfo = di.GetFileSystemInfos();
               // FileInfoItems[] files = di.EnumerateFiles();
                var firstFileInfo = filesInfo.First();

                DateTime fileCreatedDate = File.GetCreationTime(firstFileInfo.FullName);
                string newDirDateTime = firstFileInfo.CreationTime.Date.ToString("s");
                string newDirDate = newDirDateTime.Split('T').ElementAt(0);
                
                 string newDirLong = io.Path.Combine(Dest, newDirDate);
                 
                 Console.WriteLine("Check Exists ..." + newDirLong);
                 if (!System.IO.Directory.Exists(newDirLong)) 
                    {
                     System.IO.Directory.CreateDirectory(newDirLong);
                     Console.WriteLine("Creating ... \t" + newDirLong);
                    }

                 foreach (var fi in filesInfo)
                 {
                     Console.WriteLine("\t" + fi.FullName);
                     var newName = io.Path.Combine(newDirLong, fi.Name);
                     try
                     {
                         System.IO.File.Copy(fi.FullName, newName);
                     }
                     catch (IOException ioe)
                     {
                         Console.WriteLine("File " + newName + " Already Exists");
                     }
                 }
            }
                
        }

        static void BulkToDateDir( string[] args)
        {
            string rootDestination = @"C:\Users\hndavis\Pictures\Event";

           
            Dictionary<DateTime, List<FileSystemInfo>> allocatedFiles = new Dictionary<DateTime, List<FileSystemInfo>>();

            string location = args[0];
            string src = @"C:\Users\hndavis\Pictures\2013-11-03sx40_1103";
            allocatedFiles = GetFutureDirStructure(src);

            Console.WriteLine("Directories to be created...");
            var dirList = allocatedFiles.ToArray();
            Console.WriteLine(dirList.ToString());

            
            foreach (var entry in dirList)
            {
                Console.WriteLine(entry.Key.Date.ToShortDateString());
                foreach (var fileInfo in entry.Value)
                {
                    Console.WriteLine("\t" + fileInfo.FullName);

                }
            }
            Console.WriteLine("Press any key to continue");
            var ch = Console.ReadKey();
            foreach (var entry in dirList)
            {
                string newDirDateTime = entry.Key.Date.ToString("s");
                 string newDirDate = newDirDateTime.Split('T').ElementAt(0);

                 string newDirLong = io.Path.Combine(rootDestination, newDirDate);

                 Console.WriteLine("Check Exists ..." + newDirLong);
                 if (!System.IO.Directory.Exists(newDirLong)) 
                    {
                     System.IO.Directory.CreateDirectory(newDirLong);
                     Console.WriteLine("Creating ... \t" + newDirLong);
                    }

                    foreach (var fileInfo in entry.Value)
                    {
                        Console.WriteLine("\t" + fileInfo.FullName);
                        var newName= io.Path.Combine(newDirLong, fileInfo.Name);
                        try
                        {
                            System.IO.File.Copy(fileInfo.FullName, newName);
                        }
                        catch (IOException ioe)
                        {
                            Console.WriteLine(ioe.Message + " " + fileInfo.FullName);
                        }
                    }
            }
            

        }

      public  static  Dictionary<DateTime, List<FileSystemInfo>> GetFutureDirStructure(string src)
        {
           // string location = @"C:\Users\hndavis\Pictures\2013-10-21";
            //string location = @" C:\Users\hndavis\Pictures\Event\2013-11-03";
           
            CultureInfo myCI = new CultureInfo("en-US", false);
            myCI.DateTimeFormat.DateSeparator = "-";
            Thread.CurrentThread.CurrentCulture = myCI;

            // FileInfoItems root = new FileInfoItems(Path);
            //DirectoryInfo di = new DirectoryInfo("C:\\docs\\bulk_pictures_iphone_hndavis");
            DirectoryInfo di = new DirectoryInfo(src);
            FileSystemInfo[] filesInfo = di.GetFileSystemInfos();
            Dictionary<DateTime, List<FileSystemInfo>> allocatedFiles = new Dictionary<DateTime, List<FileSystemInfo>>();

            foreach (FileSystemInfo fileInfo in filesInfo)
            {
                List<FileSystemInfo> files;
                if (!allocatedFiles.TryGetValue(fileInfo.CreationTime.Date, out files))
                {

                    files = new List<FileSystemInfo>();
                    allocatedFiles.Add(fileInfo.CreationTime.Date, files);
                }
                files.Add(fileInfo);
                //Console.WriteLine( filesInfo.
            }

            return allocatedFiles;
        }

        void copy( string srcDir, string strDestDir )
        {
        }
    }
}
