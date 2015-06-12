using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using FileFunctions;

namespace Consolidator.ViewModel
{
    public class MainFunc : ViewModelBase
    {
        
        private const string UserMachinePrefs = "UserMachinePrefs.xml";

        private List<String> allPicts { get; set; }
        public List<string> included { get; set; }
        public List<string> excluded { get; set; }
        public List<string> extentions { get; set; } 
        public ObservableCollection<string>  FilesToCopy { get; set; }
        private readonly Dispatcher _dispatcher;
        private IObservable<FileInfoItem> pictFileObserver = null;
        private IDisposable pictFileDisposable = null;
           
        private bool IsStopping = false;
        private bool CurrentlyRunning = false;

         RelayCommand _CmdFind;

        public ICommand CmdFind
        {
            get
            {
                if (_CmdFind == null)
                {
                    _CmdFind = new RelayCommand(param => FindFiles(), CanRun);
                }
                return _CmdFind;
            }
        }


        public ICommand _CmdExit;

        public ICommand CmdExit
        {
            get
            {
                if (_CmdExit == null)
                {
                    _CmdExit = new RelayCommand(win =>
                    {
                        IsStopping = true;
                        if (pictFileDisposable != null)
                        {
                          pictFileDisposable.Dispose();
                        }
                        ((Window)win).Close();
                    });
                }
                return _CmdExit;
            }
        }

        public MainFunc()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            DirLoad();
             FilesToCopy= new ObservableCollection<string>();
            FilesToCopy.Add("Test");

        }

        private void DirLoad()
        {
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.None);
            System.IO.FileInfo infoConfig = new System.IO.FileInfo(config.FilePath);
            string pathToConfig = infoConfig.DirectoryName;
            string userMachinePref = System.IO.Path.Combine(pathToConfig, MainFunc.UserMachinePrefs);
            XDocument doc = XDocument.Load(userMachinePref);
            if (doc.Root == null)
            {
                MessageBox.Show("Problem Not find 'UserMachinePrefs.xml", "Error", MessageBoxButton.OK);
                return;
            }

            //  System.Xml.XPath.XPathDocument doc = new System.Xml.XPath.XPathDocument(@"config.xml");

            var excludedDirs =
                doc.Root.DescendantsAndSelf().Select(x => x.Element("Exclude"))
                    .Descendants("dirs")
                    .Descendants()
                    .Select(d => d.Value);
            excluded = excludedDirs.ToList();
            //.Select( y=> y.Element("dirs"))
            //;
            XElement xElement = XElement.Parse(doc.ToString());

            var includedDirs =
                doc.Root.DescendantsAndSelf()
                    .Select(x => x.Element("Include"))
                    .Descendants("dirs")
                    .Descendants()
                    .Select(d => d.Value);
            included = includedDirs.ToList();

            extentions = doc.Root.Element("Picts").Descendants().Select(v => v.Value).ToList();
        }

        bool IsStoping(object o)
        {
            return IsStopping;
        }

        bool CanRun(object o)
        {
            return !IsStopping && !CurrentlyRunning;
        }

        void AddTolstFileToCopy(string filePath)
        {
            FilesToCopy.Add(filePath);
        }

        public void FindFiles()
        {
            CurrentlyRunning = true;
            var task = Task.Factory.StartNew(() =>
            {
                FileFunctions.Manipulate fileFuncs = new Manipulate();


                //RenameDirToDate();

                foreach (var source in included)
                {
                   pictFileObserver = fileFuncs.CreateObservable(source, included, excluded, extentions);
                   pictFileDisposable =  pictFileObserver.Subscribe(fii =>
                    {
                        _dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (Action) (() =>
                            {
                                if (IsStopping)
                                    return;
                                FilesToCopy.Add(fii.FullPathName);
                            }));
                    });


                    //allPicts = fileFuncs.GetAllFilesInCriteria(source, included, excluded, extentions, FilesToCopy);

                    //CopyAllToDest(source, defArgs["d"], extensions.ToList(), excludeDirs.ToList());
                }
            }).ContinueWith((prevTask) =>
            {
                CurrentlyRunning = false;
            } );
        }
    }
}
