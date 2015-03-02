using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace Dota2Replay
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.listView1.ColumnClick += listView1_ColumnClick;
        }

        private void SortByMatchId()
        {
            listView1.ListViewItemSorter = new NaturalComparer();
            listView1.Sort();
        }

        public static class Globals {
            public static string replaySource = @System.IO.File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\dota2 replay manager\\replayfolder.txt");
            public static string descSource = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\dota2 replay manager\\description.txt";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\dota2 replay manager"))
            {
                System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\dota2 replay manager");
                TextWriter tw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\dota2 replay manager\\replayfolder.txt");
                FolderBrowserDialog replayFolderDialog = new FolderBrowserDialog();
                DialogResult result = replayFolderDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    tw.Write(replayFolderDialog.SelectedPath);
                    tw.Close();
                    Application.Restart();
                }
            }
            else
            {
                textBox1.Text = Globals.replaySource;
                loadReplays();
                getDescriptionsFromFile();
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listView1.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    contextMenuStrip1.Show(Cursor.Position);
                }
            } 
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String text = listView1.SelectedItems[0].Text;
            string replay = Globals.replaySource + "\\" + text + ".dem";
            DialogResult replaydelete = MessageBox.Show("Are you sure you want to delete this replay?\n\n" + replay, "Delete Replay", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if(replaydelete == DialogResult.Yes)
            {
                File.Delete(replay);
                MessageBox.Show("Replay #"+text+" is successfully deleted.");
                loadReplays();
                getDescriptionsFromFile();
            }
            if(replaydelete == DialogResult.No)
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            selectFolder();
        }

        private void selectFolder()
        {
            FolderBrowserDialog replayFolderDialog = new FolderBrowserDialog();
            DialogResult result = replayFolderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Text = replayFolderDialog.SelectedPath;
                Globals.replaySource = replayFolderDialog.SelectedPath;
                loadReplays();
                getDescriptionsFromFile();
            }
        }

        private void getDescriptionsFromFile()
        {
            string path = Globals.descSource;
            if (!File.Exists(path))
            {
                TextWriter tw = new StreamWriter(path);
                tw.Close();
            }
            else if (File.Exists(path))
            {
                closeFile();
                foreach (ListViewItem itemRow in this.listView1.Items)
                {
                    for (int i = 0; i < listView1.Items.Count; i++)
                    {
                        var searchTarget = listView1.Items[i].SubItems[0].Text;
                        foreach (var line in File.ReadLines(Globals.descSource))
                        {                        
                            if (line.Contains(searchTarget))
                            {
                                string[] values = line.Split('=');
                                string value = values[1];
                                listView1.Items[i].SubItems[1].Text = value;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void loadReplays()
        {
            listView1.Items.Clear();
            if(System.IO.Directory.Exists(Globals.replaySource))
            {
                string[] direction = Directory.GetFiles(Globals.replaySource, "*.dem");
                int count = 0;
                foreach (string file in direction)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    FileInfo fi = new FileInfo(file);
                    var size = fi.Length;
                    double MB = (Convert.ToDouble(size) / 1024) / 1024;
                    double roundSize = Math.Round(MB, 2);
                    string[] row1 = { "No Description", roundSize.ToString() + " MB", fi.LastWriteTime.ToString()};
                    listView1.Items.Add(fileName).SubItems.AddRange(row1);
                    count++;
                }
                label2.Text = count + " replays found.";
                SortByMatchId();
            }
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            var hit = listView1.HitTest(e.Location);
            if (hit.SubItem != null && hit.SubItem == hit.Item.SubItems[0]) listView1.Cursor = Cursors.Hand;
            else listView1.Cursor = Cursors.Default;
        }

        private void closeFile()
        {
            string path = Globals.descSource;
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            fs.Close();

        }

        private void addDescriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String text = listView1.SelectedItems[0].Text;
            string input = Microsoft.VisualBasic.Interaction.InputBox("Type a description for replay"+text, "Set Description", "2k mmr", -1, -1);
            using (FileStream fs = new FileStream(Globals.descSource, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(text + " =" + input);
            }
            getDescriptionsFromFile();
        }

        private void watchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String text = listView1.SelectedItems[0].Text;
            MessageBox.Show("Console command for watching replay #"+text+" has been copied to your clipboard.\nOpen DOTA 2 and paste it to console.");
            System.Windows.Forms.Clipboard.SetText("playdemo replays/"+text+".dem");
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            Point mousePosition = listView1.PointToClient(Control.MousePosition);
            ListViewHitTestInfo hit = listView1.HitTest(mousePosition);
            if(hit!= null && hit.SubItem == hit.Item.SubItems[0])
            {
                var url = "http://www.dotabuff.com/matches/" + hit.SubItem.Text;
                System.Diagnostics.Process.Start(url);
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortByMatchId();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var url = "https://github.com/fplayer/Dota2-Replay-Manager/";
            System.Diagnostics.Process.Start(url);
        }
}//class
}