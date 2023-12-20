using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ExcelFromal
{
    public partial class Form1 : Form
    {
        //OpenFileDialog openFileDialog1;  
        private List<string> excelFiles = new List<string>(); // 用于存储选择的 Excel 文件路径  
        public Form1()
        {
            InitializeComponent();
            openFileDialog1 = new OpenFileDialog();
            // 设置只能选择 Excel 文件    
            openFileDialog1.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            openFileDialog1.Multiselect = true; // 允许选择多个文件  
        }

        private void Button1_Click(object sender, EventArgs e)
        {

        }

        // 遍历读取表格的每一个 sheet 表    
        public List<List<object>> ReadTable(List<string> files)
        {
            var allData = new List<List<object>>();
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine($"File not found: {file}");
                    continue;
                }

                using var package = new ExcelPackage(new FileInfo(file));
                //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                if (package.Workbook == null)
                {
                    Console.WriteLine($"Workbook is null for file: {file}");
                    continue;
                }

                var fileData = new List<object>();
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    if (worksheet == null)
                    {
                        Console.WriteLine($"Worksheet is null in file: {file}");
                        continue;
                    }

                    var dimension = worksheet.Dimension;
                    if (dimension == null)
                    {
                        Console.WriteLine($"Dimension is null for worksheet in file: {file}");
                        continue;
                    }

                    for (var row = 1; row <= dimension.Rows; row++)
                    {
                        for (var col = 1; col <= dimension.Columns; col++)
                        {
                            var cell = worksheet.Cells[row, col];
                            if (cell != null && cell.Value != null)
                            {
                                fileData.Add(cell.Value);
                            }
                        }
                    }
                }
                allData.Add(fileData);
            }
            return allData;
        }

        // 填入临时表中    


        public void WriteTable(List<List<object>> allData)
        {
            using var package = new ExcelPackage();
            int sheetIndex = 1;
            foreach (var fileData in allData)
            {
                var workSheet = package.Workbook.Worksheets.Add($"Sheet{sheetIndex}");
                for (var i = 0; i < fileData.Count; i++)
                {
                    workSheet.Cells[i + 1, 1].Value = fileData[i];
                }
                sheetIndex++;
            }
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // 获取桌面路径      
            var outPath = Path.Combine(desktopPath, "Output.xlsx"); // 输出文件路径      

            // 检查文件是否存在，如果存在则删除它以便覆盖    
            if (File.Exists(outPath))
            {
                File.Delete(outPath);
            }

            FileInfo excelFile = new FileInfo(outPath);
            package.SaveAs(excelFile);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Button2_Click_1(object sender, EventArgs e)
        {

        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // 获取所有选择的文件路径  
                excelFiles.Clear();
                foreach (string fileName in openFileDialog1.FileNames)
                {
                    excelFiles.Add(fileName);
                }
                // 显示选择的第一个文件路径  
                if (excelFiles.Count > 0)
                {
                    textBox1.Text = excelFiles[0];
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (excelFiles.Count > 0)
            {
                try
                {
                    var listTemp = ReadTable(excelFiles);
                    WriteTable(listTemp);
                    MessageBox.Show("处理完成。");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("请先选择 Excel 文件。");
            }
        }
    }
}