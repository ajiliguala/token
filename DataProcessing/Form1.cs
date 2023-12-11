using System;
using System.Data;
using System.Data.OleDb;
using System.Reflection;
using System.Windows.Forms;


using Excel = Microsoft.Office.Interop.Excel;

namespace DataProcessing
{

    public partial class Form1 : Form
    {
        public bool zc = false;
        public bool fc = false;
        OpenFileDialog fileDialog_zc = new OpenFileDialog();
        OpenFileDialog fileDialog_fc = new OpenFileDialog();
        [System.Runtime.InteropServices.DllImport("User32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        public void Kill(Excel.Application excel)
        {
            IntPtr t = new IntPtr(excel.Hwnd);
            int k = 0;
            GetWindowThreadProcessId(t, out k);
            System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(k);
            p.Kill();
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            fileDialog_zc.Multiselect = false;
            fileDialog_zc.Title = "请选择总仓核算表文件";
            fileDialog_zc.Filter = "excel文件|*.xls;*.xlsx";
            if (fileDialog_zc.ShowDialog()==DialogResult.OK)
            {
                label1.Text = fileDialog_zc.FileName;
                zc = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            fileDialog_fc.Multiselect = false;
            fileDialog_fc.Title = "请选择分仓核算表文件";
            fileDialog_fc.Filter = "excel文件|*.xls;*.xlsx";
            if (fileDialog_fc.ShowDialog()==DialogResult.OK)
            {
                label2.Text = fileDialog_fc.FileName;
                fc = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (zc&&fc)
            {
                
                string ret = System.IO.Path.GetDirectoryName(fileDialog_zc.FileName) + "\\核算结果.xlsx";
                
                string connstring = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source = " + fileDialog_fc.FileName + "; Extended Properties = 'Excel 8.0;HDR=NO;IMEX=1'; ";
                System.Data.DataTable dt_fc = new System.Data.DataTable();
                using (OleDbConnection conn =new OleDbConnection(connstring))
                {
                    try
                    {
                        conn.Open();
                        OleDbDataAdapter oada = new OleDbDataAdapter("select F1,F2,F3,F4,F5,F6,F7,F10,F13,F16,F17,F18 from [sheet1$]", connstring);
                        dt_fc.TableName = "table_fc";
                        oada.Fill(dt_fc);
                        conn.Close();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("ERROR:" + ex.ToString());
                    }
                    
                }

                connstring = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source = " + fileDialog_zc.FileName + "; Extended Properties = 'Excel 8.0;HDR=NO;IMEX=1'; ";
                System.Data.DataTable dt_zc = new System.Data.DataTable();
                using (OleDbConnection conn=new OleDbConnection(connstring))
                {
                    try
                    {
                        conn.Open();
                        OleDbDataAdapter oada = new OleDbDataAdapter("select F2,F16,F18 from [sheet1$]", connstring);
                        dt_zc.TableName = "table_zc";
                        oada.Fill(dt_zc);
                        conn.Close();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("ERROR:" + ex.ToString());
                    }
                    
                }

                label3.Text = "正在处理明细数据。。。。。。";
                try
                {
                    progressBar1.Maximum = dt_fc.Rows.Count;
                    progressBar1.Value = 0;
                    progressBar1.Step = 1;
                    dt_fc.Columns.Add("FORG");

                    bool ifexist = false;

                    //明细表
                    for (int index = 3; index < dt_fc.Rows.Count-1; index++)
                    {
                        DataRow fc = dt_fc.Rows[index];
                        progressBar1.Value += progressBar1.Step;
                        if (fc["F1"].ToString().Contains("总计"))
                        {
                        }
                        else
                        {
                            //事业部
                            if (fc["F1"].ToString().Contains("一部"))
                            {
                                fc["FORG"] = "第一事业部";
                            }
                            else if (fc["F1"].ToString().Contains("二部"))
                            {
                                fc["FORG"] = "第二事业部";
                            }
                            else if (fc["F1"].ToString().Contains("三部"))
                            {
                                fc["FORG"] = "第三事业部";
                            }
                            else if (fc["F1"].ToString().Contains("五部"))
                            {
                                fc["FORG"] = "第五事业部";
                            }
                            else
                            {
                                fc["FORG"] = "芜湖长信科技股份有限公司";
                            }
                            if (fc["F2"].ToString().Contains("04.07"))//靶材不处理
                            {
                            }
                            else
                            {
                                foreach (DataRow zc in dt_zc.Rows)
                                {
                                    if (fc["F2"].ToString() == zc["F2"].ToString())
                                    {
                                        if (zc["F18"] is DBNull || zc["F16"] is DBNull)//总仓金额为空，单价金额赋值为空
                                        {
                                            fc["F17"] = null;
                                            fc["F18"] = null;
                                        }
                                        else
                                        {
                                            if (fc["F16"] is DBNull)//分仓数量数量为空，只赋值单价
                                            {
                                                fc["F17"] = Convert.ToDecimal(zc["F18"]) / Convert.ToDecimal(zc["F16"]);
                                                fc["F18"] = null;
                                            }
                                            else
                                            {
                                                fc["F17"] = Convert.ToDecimal(zc["F18"]) / Convert.ToDecimal(zc["F16"]);
                                                fc["F18"] = Convert.ToDecimal(fc["F17"]) * Convert.ToDecimal(fc["F16"]);
                                            }
                                        }
                                        ifexist = true;
                                        break;
                                    }                                    
                                }
                                if (!ifexist)
                                {
                                    fc["F16"] = null;
                                    fc["F17"] = null;
                                    fc["F18"] = null;
                                }
                                ifexist = false;
                            
                                //
                            }
                        }
                    }

                    //汇总表

                    decimal wh = 0;
                    decimal f1 = 0;
                    decimal f2 = 0;
                    decimal f3 = 0;
                    decimal f5 = 0;
                    decimal other=0;

                    label3.Text = "正在处理汇总数据。。。。。。";
                    progressBar1.Maximum = dt_fc.Rows.Count;
                    progressBar1.Value = 0;
                    progressBar1.Step = 1;
                    

                    for (int index=3;index<dt_fc.Rows.Count;index++)
                    {
                        progressBar1.Value += progressBar1.Step;
                        DataRow dr = dt_fc.Rows[index];
                        switch (dr["FORG"].ToString())
                        {
                            case "第一事业部": f1 += (dr["F18"] is System.DBNull) ? 0 : Convert.ToDecimal(dr["F18"]); break;
                            case "第二事业部": f2 += (dr["F18"] is System.DBNull) ? 0 : Convert.ToDecimal(dr["F18"]); break;
                            case "第三事业部": f3 += (dr["F18"] is System.DBNull) ? 0 : Convert.ToDecimal(dr["F18"]); break;
                            case "第五事业部": f5 += (dr["F18"] is System.DBNull) ? 0 : Convert.ToDecimal(dr["F18"]); break;
                            case "芜湖长信科技股份有限公司":
                                wh += (dr["F18"] is System.DBNull) ? 0 : Convert.ToDecimal(dr["F18"]);
                                break;
                            default:
                                other += (dr["F18"] is System.DBNull) ? 0 : Convert.ToDecimal(dr["F18"]);
                                break;
                        }
                    }

                    label3.Text = "正在生成结果文件。。。";
                    decimal fsum = wh + f1 + f2 + f3 + f5;

                    DataTable summary = new DataTable();
                    summary.Columns.Add("FORG");
                    summary.Columns.Add("FAMT");
                    summary.Rows.Add(new object[] { "芜湖长信科技股份有限公司", wh });
                    summary.Rows.Add(new object[] { "第一事业部", f1 });
                    summary.Rows.Add(new object[] { "第二事业部", f2 });
                    summary.Rows.Add(new object[] { "第三事业部", f3 });
                    summary.Rows.Add(new object[] { "第五事业部", f5 });

                    int i = dt_fc.Rows.Count;
                    int j = dt_fc.Columns.Count;

                    string startCell = "A1";
                    string endCell = "";

                    for (char alp = 'A'; alp < 'Z'; alp++)
                    {
                        if ((int)alp==j+64)
                        {
                            endCell = alp + i.ToString();
                            break;
                        }
                    }

                    Excel.Application excel = new Excel.Application();
                    Excel.Workbook book = excel.Workbooks.Add(Missing.Value);
                    Excel.Worksheet msheet = (Excel.Worksheet)book.Worksheets.Add(Missing.Value, Missing.Value, 1, Excel.XlSheetType.xlWorksheet);
                    Excel.Worksheet sheet1 = (Excel.Worksheet)book.Worksheets[1];
                    Excel.Worksheet sheet2 = (Excel.Worksheet)book.Worksheets[2];

                    sheet1.Name = "明细";
                    sheet2.Name = "汇总";

                    Object[,] satdata = new object[dt_fc.Rows.Count + 1, dt_fc.Columns.Count];
                    object[,] sumdata = new object[10, 2];
                    int row = 1;

                    foreach (DataRow dr in dt_fc.Rows)
                    {
                        int col = 0;
                        foreach (var item in dr.ItemArray)
                        {
                            satdata[row, col] = item;
                            col++;
                        }
                        row++;
                    }
                    sumdata[1, 0] = "存货组织";
                    sumdata[1, 1] = "存货金额";
                    row = 2;
                    foreach(DataRow dr in summary.Rows)
                    {
                        int col = 0;
                        foreach (var item in dr.ItemArray)
                        {
                            sumdata[row, col] = item;
                            col++;
                        }
                        row++;
                    }
                    sumdata[row, 0] = "总仓核算金额";
                    sumdata[row, 1] = dt_zc.Rows[dt_zc.Rows.Count-1]["F18"];
                    sumdata[row+1, 0] = "合计";
                    sumdata[row+1, 1] = fsum;
                    sumdata[row + 2, 0] = "有金额无数量";
                    sumdata[row + 2, 1] = fsum - Convert.ToDecimal(dt_zc.Rows[dt_zc.Rows.Count - 1]["F18"]);

                    //格式设置
                    sheet1.Range["A:C"].ColumnWidth = 20;
                    sheet1.Range["E:E"].ColumnWidth = 20;
                    sheet1.Range["F:F"].ColumnWidth = 10;
                    sheet1.Range["G:M"].ColumnWidth = 20;

                    sheet2.Range["A:A"].ColumnWidth = 25;
                    sheet2.Range["B:B"].ColumnWidth = 20;
                    sheet2.Range["A2", "B10"].Borders.LineStyle = 1;

                    sheet1.get_Range(startCell, endCell).Value2 = satdata;
                    sheet2.get_Range("A1", "B10").Value2 = sumdata;
                    

                    

                    book.SaveAs(ret, Type.Missing, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Excel.XlSaveAsAccessMode.xlExclusive, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                    
                    book.Close(true, System.IO.Path.GetDirectoryName(fileDialog_zc.FileName), Missing.Value);
                   



                    excel.Quit();


                    Kill(excel);
                    MessageBox.Show("生成成功");
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.ToString());
                }
            
            }
            else
            {
                MessageBox.Show("请先选择文件");
            }
        }
    }
}
