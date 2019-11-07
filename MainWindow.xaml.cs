using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using Path = System.IO.Path;
using System.Runtime.CompilerServices;
using System.ComponentModel;
namespace FMODAudioImporter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static TelnetConnection tc;

        public string Ip { get; set; } = "127.0.0.1";
        public string Port { get; set; } = "3663";

        public string Multi { get; set; } = "_m_";
        public string Scatterer { get; set; } = "_c_";
        public string Spatializer { get; set; } = "_z";

        public static string projectPath = null;
        private Brush previousFill = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }


        private void InitializeConnection(string projectPath)
        {
            tc = new TelnetConnection(Ip, Int32.Parse(Port));

            if (!tc.connected)
            {
                System.Windows.MessageBox.Show("Can't connect to FMOD Studio, please check IP and Port settings. Make sure FMOD Studio is open.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            List<string> directories = new List<string>(Directory.GetDirectories(projectPath, "*", SearchOption.AllDirectories));
            directories.Add(projectPath);
            ScanEvents(directories);
            directories.Clear();
        }

        public void ScanEvents(List<string> directories)
        {
            List<string> filePaths = new List<string>();

            foreach (var directory in directories)
            {
                var paths = Directory.GetFiles(directory, "*.*",
                                  SearchOption.TopDirectoryOnly).Where(s => s.ToLower().EndsWith(".wav")
                                  || s.ToLower().EndsWith(".mp3")
                                  || s.ToLower().EndsWith(".aiff")
                                  || s.ToLower().EndsWith(".ogg")
                                  || s.ToLower().EndsWith(".flac"));

                filePaths.AddRange(paths);
            }

            CreateEvents(filePaths);
        }

        public void CreateEvents(IEnumerable<string> filePaths)
        {
            bool firstRun = false;

            var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            foreach (string filePath in filePaths)
            {
                string directoryName = new DirectoryInfo(System.IO.Path.GetDirectoryName(filePath)).Name;
                string parentDirectoryName = new DirectoryInfo(System.IO.Path.GetDirectoryName(filePath)).Parent.Name;
                string parentDirectory = new DirectoryInfo(System.IO.Path.GetDirectoryName(filePath)).Parent.FullName;
                string relativePath = Extensions.GetRelativePath(projectPath, filePath);
                int lenght = relativePath.Split(Path.DirectorySeparatorChar).Length - 2;

                List<string> lines = new List<string>();

                lines.Add("var path = " + "'" + filePath.Replace(@"\", @"/") + "'" + ";");
                lines.Add("var asset = studio.project.importAudioFile(path);");


                #region Event Check & Creation

                if (!filePath.Contains(Multi) && !filePath.Contains(Scatterer) && !filePath.Contains(Spatializer))
                {
                    string eventPath = filePath.Replace(@"\", @"/");
                    eventPath = Path.GetFileNameWithoutExtension(eventPath);

                    lines.Add("var eventPath = " + "'" + eventPath + "';");
                    lines.Add("var events = studio.project.model.Event.findInstances(); " +

                    "if (events.filter(function(a) { return a.name === eventPath; }).length > 0 == false) { " +
                    "var event = studio.project.create('Event'); " +
                    "event.name = eventPath; " +
                    "var track = event.addGroupTrack(); " +
                    "var sound = track.addSound(event.timeline, 'SingleSound', 0, 10); " +
                    "sound.audioFile = asset; " +
                    "sound.length = asset.length; " +
                    "sound.name = " + "'" + eventPath + "'; " +
                    "}");
                }

                else if (!filePath.Contains(Multi) && !filePath.Contains(Scatterer) && filePath.Contains(Spatializer))
                {
                    string eventPath = filePath.Replace(@"\", @"/");
                    eventPath = Path.GetFileNameWithoutExtension(eventPath);
                    eventPath = eventPath.Replace(Spatializer, "");

                    lines.Add("var eventPath = " + "'" + eventPath + "';");
                    lines.Add("var events = studio.project.model.Event.findInstances(); " +
        
                    "if (events.filter(function(a) { return a.name === eventPath; }).length > 0 == false) { " +
                    "var event = studio.project.create('Event'); " +
                    "event.name = eventPath; " +
                    "var track = event.addGroupTrack(); " +
                    "var sound = track.addSound(event.timeline, 'SingleSound', 0, 10); " +
                    "sound.audioFile = asset; " +
                    "sound.length = asset.length; " +
                    "sound.name = " + "'" + eventPath + "'; " +
                    "event.masterTrack.mixerGroup.effectChain.addEffect('SpatialiserEffect'); " +
                    "}");
                }

                else if (filePath.Contains(Multi) && !filePath.Contains(Spatializer))
                {
                    string eventPath = filePath.Replace(@"\", @"/");
                    eventPath = Path.GetFileNameWithoutExtension(eventPath);
                    eventPath = eventPath.Trim(digits);
                    eventPath = eventPath.Replace(Multi, "");

                    lines.Add("var eventPath = " + "'" + eventPath + "';");
                    lines.Add("var events = studio.project.model.Event.findInstances(); " +             
                    "if (events.filter(function(a) { return a.name === eventPath; }).length > 0 == false) { " +
                    "var event = studio.project.create('Event'); " +
                    "event.name = eventPath; " +
                    "var track = event.addGroupTrack(); " +
                    "var multiSound = track.addSound(event.timeline, 'MultiSound', 0, 10); " +
                    "multiSound.name = " + "'" + eventPath + "'; " +
                    "}");

                    lines.Add(@"path = path.replace(/^.*[\\\/]/, ''); ");
                    lines.Add(" " +
                    "var multiSounds = studio.project.model.MultiSound.findInstances(); " +
                    "var multiSound = multiSounds.filter(function (obj) { " +
                    "return obj.name === " + "'" + eventPath + "'; " +
                    "})[0];" +
                    "if (multiSound.sounds.filter(function(e) { return e.audioFile.assetPath === path; }).length > 0 == false)" +
                    "{ " +
                    "var singleSound = studio.project.create('SingleSound');" +
                    "singleSound.audioFile = asset; singleSound.owner = multiSound;" +
                    " } ");
                }

                else if (filePath.Contains(Multi) && filePath.Contains(Spatializer))
                {
                    string eventPath = filePath.Replace(@"\", @"/");
                    eventPath = Path.GetFileNameWithoutExtension(eventPath);
                    eventPath = eventPath.Trim(digits);
                    eventPath = eventPath.Replace(Multi, "");
                    eventPath = eventPath.Replace(Spatializer, "");

                    lines.Add("var eventPath = " + "'" + eventPath + "';");
                    lines.Add("var events = studio.project.model.Event.findInstances(); " +               
                    "if (events.filter(function(a) { return a.name === eventPath; }).length > 0 == false) { " +
                    "var event = studio.project.create('Event'); " +
                    "event.name = eventPath; " +
                    "var track = event.addGroupTrack(); " +
                    "var multiSound = track.addSound(event.timeline, 'MultiSound', 0, 10); " +
                    "multiSound.name = " + "'" + eventPath + "'; " +
                    "event.masterTrack.mixerGroup.effectChain.addEffect('SpatialiserEffect'); " +
                    "}");

                    lines.Add(@"path = path.replace(/^.*[\\\/]/, ''); ");
                    lines.Add(" " +
                    "var multiSounds = studio.project.model.MultiSound.findInstances(); " +
                    "var multiSound = multiSounds.filter(function (obj) { " +
                    "return obj.name === " + "'" + eventPath + "'; " +
                    "})[0];" +
                    "if (multiSound.sounds.filter(function(e) { return e.audioFile.assetPath === path; }).length > 0 == false)" +
                    "{ " +
                    "var singleSound = studio.project.create('SingleSound');" +
                    "singleSound.audioFile = asset; singleSound.owner = multiSound;" +
                    " } ");
                }

                else if (filePath.Contains(Scatterer) && !filePath.Contains(Spatializer))
                {
                    string eventPath = filePath.Replace(@"\", @"/");
                    eventPath = Path.GetFileNameWithoutExtension(eventPath);
                    eventPath = eventPath.Trim(digits);
                    eventPath = eventPath.Replace(Scatterer, "");

                    lines.Add("var eventPath = " + "'" + eventPath + "';");
                    lines.Add("var events = studio.project.model.Event.findInstances(); " +
                    "if (events.filter(function(a) { return a.name === eventPath; }).length > 0 == false) { " +
                    "var event = studio.project.create('Event'); " +
                    "event.name = eventPath; " +
                    "var track = event.addGroupTrack(); " +
                    "var scattererSound = track.addSound(event.timeline, 'SoundScatterer', 0, 10);" +
                    "scattererSound.name = " + "'" + eventPath + "'; " +
                    "}");

                    lines.Add(@"path = path.replace(/^.*[\\\/]/, ''); ");
                    lines.Add(" " +
                    "var scattererSounds = studio.project.model.SoundScatterer.findInstances(); " +
                    "var scattererSound = scattererSounds.filter(function (obj) { " +
                    "return obj.name === " + "'" + eventPath + "'; " +
                    "})[0];" +
                    "if (scattererSound.sound.sounds.filter(function(e) { return e.audioFile.assetPath === path; }).length > 0 == false)" +
                    "{ " +
                    "var singleSound = studio.project.create('SingleSound');" +
                    "singleSound.audioFile = asset; singleSound.owner = scattererSound.sound;" +
                    " } ");
                }

                else if (filePath.Contains(Scatterer) && filePath.Contains(Spatializer))
                {
                    string eventPath = filePath.Replace(@"\", @"/");
                    eventPath = Path.GetFileNameWithoutExtension(eventPath);
                    eventPath = eventPath.Trim(digits);
                    eventPath = eventPath.Replace(Scatterer, "");
                    eventPath = eventPath.Replace(Spatializer, "");

                    lines.Add("var eventPath = " + "'" + eventPath + "';");
                    lines.Add("var events = studio.project.model.Event.findInstances(); " +

                    "if (events.filter(function(a) { return a.name === eventPath; }).length > 0 == false) { " +
                    "var event = studio.project.create('Event'); " +
                    "event.name = eventPath; " +
                    "var track = event.addGroupTrack(); " +
                    "var scattererSound = track.addSound(event.timeline, 'SoundScatterer', 0, 10);" +
                    "scattererSound.name = " + "'" + eventPath + "'; " +
                    "event.masterTrack.mixerGroup.effectChain.addEffect('SpatialiserEffect'); " +
                    "}");

                    lines.Add(@"path = path.replace(/^.*[\\\/]/, ''); ");
                    lines.Add(" " +
                    "var scattererSounds = studio.project.model.SoundScatterer.findInstances(); " +
                    "var scattererSound = scattererSounds.filter(function (obj) { " +
                    "return obj.name === " + "'" + eventPath + "'; " +
                    "})[0];" +
                    "if (scattererSound.sound.sounds.filter(function(e) { return e.audioFile.assetPath === path; }).length > 0 == false)" +
                    "{ " +
                    "var singleSound = studio.project.create('SingleSound');" +
                    "singleSound.audioFile = asset; singleSound.owner = scattererSound.sound;" +
                    " } ");
                }

                #endregion


                #region Folder/Subfolder Check & Creation

                if (lenght == 1)
                {
                    lines.Add("var folders = studio.project.model.EventFolder.findInstances(); " +
                    "" +
                    "if (folders.filter(function(a) { return a.name === " + "'" + directoryName + "'" + "; }).length > 0 == false) { " +
                    "var folder = studio.project.create('EventFolder'); folder.name = " + "'" + directoryName + "'; event.folder = folder; } else event.folder = folder;");
                }
                else if (lenght > 1)
                {
                    lines.Add("var folders = studio.project.model.EventFolder.findInstances(); " +
                    "" +
                    "if (folders.filter(function(a) { return a.name === " + "'" + directoryName + "'" + "; }).length > 0 == false) { " +
                    "var subFolder = studio.project.create('EventFolder'); subFolder.name = " + "'" + directoryName + "';}");

                    string[] files = Directory.GetFiles(parentDirectory, "*", SearchOption.TopDirectoryOnly);

                    if (files.Length > 0)
                    {
                        lines.Add(
                        "var folders = studio.project.model.EventFolder.findInstances(); " +
                        "if (folders.filter(function(a) { return a.name === " + "'" + parentDirectoryName + "'" + "; }).length > 0 == true) { " +
                        "var folder = folders.filter(function (obj) { " +
                        "return obj.name === " + "'" + parentDirectoryName + "'; " +
                        "})[0] }; subFolder.folder = folder; event.folder = subFolder;");
                    }
                    else
                    {
                        if (!firstRun)
                        {
                            lines.Add("if (folder.name != " + "'" + parentDirectoryName + "'" + ") ");
                            lines.Add("{ var folder = studio.project.create('EventFolder'); folder.name = " + "'" + parentDirectoryName + "'; event.folder = folder; } else event.folder = folder;");
                        }
                        else lines.Add(" ");

                        lines.Add("var folders = studio.project.model.EventFolder.findInstances(); " +
                        "if (folders.filter(function(a) { return a.name === " + "'" + parentDirectoryName + "'" + "; }).length > 0 == true) { " +
                        "var folder = folders.filter(function (obj) { " +
                              "return obj.name === " + "'" + parentDirectoryName + "'; " +
                              "})[0]; subFolder.folder = folder; event.folder = subFolder; } " +
                        "else { subFolder.folder = folders[x]; event.folder = subFolder;}; ");
                    }
                }

                #endregion

                firstRun = true;

                foreach (string line in lines)
                {
                    tc.WriteLine(line);
                    Extensions.Sleep(0.7f);
                }
                lines.Clear();
            }
        }

        private void Rectangle_DragEnter(object sender, DragEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                if (rectangle != null)
                {
                    previousFill = rectangle.Fill;
                    Color color = (Color)ColorConverter.ConvertFromString("#a0c334");
                    SolidColorBrush myBrush = new SolidColorBrush(color);
                    rectangle.Fill = myBrush;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Rectangle_DragLeave(object sender, DragEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            if (rectangle != null)
            {
                rectangle.Fill = previousFill;
            }
        }

        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            if (rectangle != null)
            {
                rectangle.Fill = previousFill;
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileDrops = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (Directory.Exists(fileDrops[0]))
                {
                    projectPath = fileDrops[0];
                    InitializeConnection(projectPath);
                }
            }
        }

        private void IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox ip = sender as TextBox;
            if (e.Changes.Count > 0)
            { 
            Ip = ip.Text;
            }
        }

        private void Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox port = sender as TextBox;
            if (e.Changes.Count > 0)
            {
                Port = port.Text;
            }
        }

        private void Multi_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox multi = sender as TextBox;
            if (e.Changes.Count > 0)
            {
                Multi = multi.Text;
            }
        }

        private void Scatterer_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox scatterer = sender as TextBox;
            if (e.Changes.Count > 0)
            {
                Scatterer = scatterer.Text;
            }
        }

        private void Spatializer_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox spatializer = sender as TextBox;
            if (e.Changes.Count > 0)
            {
                Spatializer = spatializer.Text;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}