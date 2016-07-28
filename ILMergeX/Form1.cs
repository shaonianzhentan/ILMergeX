using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using GoodHelper;
using System.Diagnostics;

namespace ILMergeX
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.FormClosed += Form1_FormClosed;
            cmbFrameWork.SelectedIndex = 1;
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists("ILMerge.exe"))
            {
                File.Delete("ILMerge.exe");
                File.Delete("ILMergeX.bat");                
            }
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            string exeFile = "",exeFileName="",fileName;
            List<string> list = new List<string>();
            int i = 0,index=0;
            foreach(string name in listBoxFileName.Items){
                fileName = index + Path.GetExtension(name);
                if (File.Exists(fileName)) {
                    File.Delete(fileName);
                }
                index++;
                if (Path.GetExtension(name).ToLower() == ".exe") {
                    i++;
                    exeFile = fileName;
                    exeFileName=Path.GetFileName(name);
                    continue;
                }
                list.Add(fileName);                
            }
            if (i != 1)
            {
                MessageBox.Show("只能有一个exe主程序，不能存在两个exe主程序！");
            }
            else
            {
                //将核心文件写入到本地
                if (!File.Exists("ILMerge.exe"))
                {

                    System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                    Stream fs = asm.GetManifestResourceStream("ILMergeX.Resources.ILMerge.exe");

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, (int)fs.Length);

                    File.WriteAllBytes("ILMerge.exe", buffer);
                }

                //将所有要合并的程序复制到本地
                index = 0;
                foreach (string name in listBoxFileName.Items)
                {
                    fileName = index + Path.GetExtension(name);
                    if (!File.Exists(fileName))
                    {
                        File.Copy(name, fileName);
                    }
                    index++;
                }

                //拼接所有DLL文件
                string dll = "";
                foreach (string dllname in list)
                {
                    dll += dllname + " ";
                }

                string cmd = "cd " + Application.StartupPath + " \n";
                cmd += "ILMerge /ndebug /target:winexe /out:tmp.exe " + exeFile + " /log " + dll + " /targetplatform:"+cmbFrameWork.Text + " \n";
                File.WriteAllText("ILMergeX.bat", cmd);
                Process p = Process.Start("ILMergeX.bat");
                while (p.WaitForExit(0) == false)
                {

                }

                if (File.Exists("tmp.exe"))
                {
                    try
                    {
                        if (File.Exists(exeFileName))
                        {
                            exeFileName = "New-" + exeFileName;
                            if (File.Exists(exeFileName)) File.Delete(exeFileName);
                        }
                        //当所有数据完事之后，删除数据
                        index = 0;
                        foreach (string name in listBoxFileName.Items)
                        {
                            File.Delete(index + Path.GetExtension(name));
                            index++;
                        }
                        listBoxFileName.Items.Clear();

                        FileInfo fi = new FileInfo("tmp.exe");
                        fi.MoveTo(exeFileName);

                        File.Delete("ILMerge.exe");
                        File.Delete("ILMergeX.bat");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }



        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "exe文件和dll文件|*.exe;*.dll";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string name in fileDialog.FileNames) {
                    listBoxFileName.Items.Add(name);
                }
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxFileName.SelectedIndex >= 0) {
                listBoxFileName.Items.RemoveAt(listBoxFileName.SelectedIndex);
            }
        }

        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBoxFileName.Items.Clear();
        }
        
    }
}
