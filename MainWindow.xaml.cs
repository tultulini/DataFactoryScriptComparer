using System;
using System.Text;

using System.IO;
using System.Diagnostics;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataFactoryScriptComparer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var script1 = ExtractScript(Source.Text);
            var script2 = ExtractScript(Target.Text);

            string path = Directory.GetCurrentDirectory();
            var now = DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");

            var f1 = Path.Combine(path, $"{now}-Source.txt");
            var f2 = Path.Combine(path, $"{now}-Target.txt");


            try
            {
                File.WriteAllText(f1, FormatScript(script1));
                File.WriteAllText(f2, FormatScript(script2));

            }
            catch (Exception exception)
            {

                Console.WriteLine($"Error: {exception}");
                return;
            }

            CompareFiles(f1, f2);
        }

        private string ExtractScript(string data)
        {
            return IsJson(data) ? ExtractScriptFromJson(data) : data;
        }

        private bool IsJson(string data)
        {
            return (data.TrimStart().Substring(0, 1) == "{");
        }

        private string ExtractScriptFromJson(string json)
        {
            var obj = JsonConvert.DeserializeObject<JObject>(json);
            return obj.GetValue("properties").Value<JToken>("typeProperties").Value<string>("script");

        }
        private string FormatScript(string scrip)
        {
            var sb = new StringBuilder(scrip);
            sb.Replace("\\t", "\t");
            sb.Replace("\\n", "\n");
            return sb.ToString();
        }
        private void CompareFiles(string f1, string f2)
        {
            ////var vsDevPath = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe";
            //var vsDiffToolPath = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\vsdiffMerge.exe";

            //var startInfo = new ProcessStartInfo();
            //startInfo.CreateNoWindow = false;
            //startInfo.UseShellExecute = true;
            //startInfo.FileName = "code";
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //startInfo.Arguments = $"-d {f1} {f2}";
            //Process.Start(startInfo);
            OpenWithBeyondCompare(f1, f2);
        }
        private void OpenWithBeyondCompare(string f1, string f2)
        {
            var compPath = @"C:\Program Files\beyond Compare 4\BCompare.exe";
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = true,
                FileName = compPath,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"{f1} {f2}",
            };
            Process.Start(startInfo);

        }
        private void OpenWithVsCode(string f1, string f2)
        {
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = true,
                FileName = "code",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"-d {f1} {f2}",
            };
            
            Process.Start(startInfo);

        }

        private void ReduceScriptButton_Click(object sender, RoutedEventArgs e)
        {
            var expandedScript =new StringBuilder( ExpandedScriptTextBox.Text);
            expandedScript.Replace("\t", "\\t");
            expandedScript.Replace("\r\n", "\\n");
            expandedScript.Replace("\n", "\\n");
            Clipboard.SetText(expandedScript.ToString());
            MessageBox.Show("Reduced script copied to clipboard");
        }
    }
}
