using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LoLChatCleanup
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        public string ShowFileOpenDialog()
        {
            //파일오픈창 생성 및 설정
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "r3dlog need a text";
            ofd.FileName = "r3dlog";
            ofd.Filter = "텍스트 파일 *.txt | *.txt";

            //파일 오픈창 로드
            DialogResult dr = ofd.ShowDialog();

            string path = "";
            //OK버튼 클릭시
            if (dr == DialogResult.OK)
            {
                path = ofd.FileName;
            }
            //취소버튼 클릭시 또는 ESC키로 파일창을 종료 했을경우
            else if (dr == DialogResult.Cancel)
            {
                path = "";
            }

            return path;
        }

        private void ReadTextFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            Console.WriteLine(lines.Length);
            var ownLine = lines.FirstOrDefault(x => x.Contains("**LOCAL**"));
            var owndisplayName = Regex.Match(ownLine, @"'(.*?)'").Groups[1].Value;
            var logstartTime = lines[0].Replace("000000.000| ALWAYS| Logging started at ", "");
            this.Text += $" [ {owndisplayName} has Game ({logstartTime}) ]";
            //채팅
            foreach (string line in lines)
            {
                //레드팀
                if (line.Contains("TeamChaos"))
                {
                    var displayName = Regex.Match(line, @"'(.*?)'").Groups[1].Value;
                    var champ = Regex.Match(line, @"Champion\((.*?)\)").Groups[1].Value;
                    var l_line = Regex.Match(line, @"TeamBuilderRole\((.*?)\)").Groups[1].Value;
                    rTeam.Items.Add($"{displayName}({champ}, {l_line})");
                }

                //블루팀
                if (line.Contains("TeamOrder"))
                {
                    var displayName = Regex.Match(line, @"'(.*?)'").Groups[1].Value;
                    var champ = Regex.Match(line, @"Champion\((.*?)\)").Groups[1].Value;
                    var l_line = Regex.Match(line, @"TeamBuilderRole\((.*?)\)").Groups[1].Value;
                    bTeam.Items.Add($"{displayName}({champ}, {l_line})");
                }

                if (line.Contains("Chat received valid message: "))
                {
                    string displayName = Regex.Match(line, @"DisplayName\s(.*?)\sand").Groups[1].Value;
                    string channel = Regex.Match(line, @"\[(.*?)\]").Groups[1].Value;
                    string message = Regex.Match(line, @"Chat received valid message:\s(.*?)\swith speaker").Groups[1].Value;
                    string timestamp = Regex.Match(line, @"^(.*?)\|").Groups[1].Value;
                    DateTime startTime = DateTime.Parse(logstartTime);
                    DateTime logTime = startTime.AddSeconds(Convert.ToDouble(timestamp));
                    TimeSpan elapsedTime = logTime - startTime;
                    var t = ConvertTime((int)elapsedTime.TotalSeconds);

                    ListViewItem lv = new ListViewItem();
                    lv.Text = $"{t}";
                    lv.SubItems.Add($"{channel}");
                    lv.SubItems.Add($"{displayName}");
                    lv.SubItems.Add($"{message}");

                    listView1.Items.Add(lv);
                }

                

            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void openFileBtn_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            rTeam.Items.Clear();
            bTeam.Items.Clear();

            var path = ShowFileOpenDialog();
            Console.WriteLine(path);
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Cancel");
                return;
            }

            ReadTextFile(path);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string fileName;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Please specify the storage path (경로를 지정해주세요)";
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Filter = "Text File (*.txt) | *.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialog.FileName;

                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"Timestamp | Channel | DisplayName | Message");
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    var item = listView1.Items[i];

                    sb.AppendLine($"{item.Text} | {item.SubItems[1].Text} | {item.SubItems[2].Text} | {item.SubItems[3].Text}");
                }
                sb.AppendLine("----------------------------------------------------");
                sb.AppendLine("Red Team");
                for (int i = 0; i < rTeam.Items.Count; i++)
                {
                    sb.AppendLine($"{rTeam.Items[i]}");
                }
                sb.AppendLine("----------------------------------------------------");
                sb.AppendLine("Blue Team");
                for (int i = 0; i < bTeam.Items.Count; i++)
                {
                    sb.AppendLine($"{bTeam.Items[i]}");
                }
                sb.AppendLine("----------------------------------------------------");

                File.WriteAllText(fileName, sb.ToString());
            }
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                MessageBox.Show("Cancel Save");
            }
        }

        private string ConvertTime(int seconds)
        {
            seconds = seconds - 42;

            int convertedHours = seconds / 3600;
            int convertedMinutes = (seconds % 3600) / 60;
            int convertedSeconds = seconds % 60;

            string timeString = "";

            if (convertedHours != 0)
            {
                timeString += $"{convertedHours:D2}:";
            }

            if (convertedMinutes != 0 || convertedHours == 0)
            {
                timeString += $"{convertedMinutes:D2}:";
            }

            timeString += $"{convertedSeconds:D2}";

            return timeString;
        }
    }
}
