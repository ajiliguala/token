﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace filecopy
{
    public partial class Form1 : Form
    {
        FolderBrowserDialog folderBrowserDialog1;
        FolderBrowserDialog folderBrowserDialog2;
        public Form1()
        {
            InitializeComponent();
            folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog2 = new FolderBrowserDialog();

            // PopulateFileTypeComboBox(); // 初始化文件类型下拉框  
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {


            string sourcePath = Path.GetFullPath(textBox1.Text.Trim());
            string targetPath = Path.GetFullPath(textBox2.Text.Trim());
            string fileType = comboBox1.SelectedItem?.ToString(); // 添加了null条件运算符以防止NullReferenceException  

            if (!Directory.Exists(sourcePath))
            {
                MessageBox.Show("源文件夹不存在，请检查路径是否正确。");
                return;
            }

            if (!Directory.Exists(targetPath))
            {
                MessageBox.Show("目标文件夹不存在，请检查路径是否正确。");
                return;
            }

            if (string.IsNullOrEmpty(fileType))
            {
                MessageBox.Show("请选择要复制的文件类型。");
                return;
            }

            try
            {
                Task.Run(() => CopyFiles(sourcePath, targetPath, fileType))
                    .ContinueWith(t => { if (t.IsFaulted) MessageBox.Show("文件复制失败：" + t.Exception.Message); else MessageBox.Show("文件复制已完成。"); }, TaskScheduler.FromCurrentSynchronizationContext()); // 使用ContinueWith在UI线程上显示消息框  
            }
            catch (Exception ex)
            {
                MessageBox.Show("文件复制失败：" + ex.Message);
            }
        }

        private void CopyFiles(string sourcePath, string targetPath, string fileType)
        {
            string[] files = Directory.GetFiles(sourcePath, "*." + fileType, SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destination = Path.Combine(targetPath, fileName);
                File.Copy(file, destination, true); // 覆盖同名文件    
            }
        }

        private void PopulateFileTypeComboBox()
        {
            string sourcePath = textBox1.Text; // 获取源文件夹路径    

            if (Directory.Exists(sourcePath))
            {
                // 获取文件夹中所有文件的扩展名    

                IEnumerable<string> files = Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                IEnumerable<string> extensions = files.Select(Path.GetExtension).Distinct();

                // 清除现有的文件类型选项    
                comboBox1.Items.Clear();
                comboBox1.Items.Add("所有文件 (*.*)"); // 添加一个默认选项以复制所有文件类型  
                // 添加新的文件类型选项    
                foreach (string extension in extensions)
                {
                    // 去掉扩展名前面的点（.）    
                    string fileType = extension.TrimStart('.');
                    comboBox1.Items.Add(fileType);
                }
            }
            else
            {
                MessageBox.Show("源文件夹不存在，请检查路径是否正确。");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
                PopulateFileTypeComboBox(); // 初始化文件类型下拉框  
            }

        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog2.SelectedPath;

            }

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}

