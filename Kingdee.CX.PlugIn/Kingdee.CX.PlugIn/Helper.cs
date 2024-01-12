using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000003 RID: 3
	public class Helper
	{
		// Token: 0x06000006 RID: 6 RVA: 0x00002BEC File Offset: 0x00000DEC
		public static double ToDouble(object obj)
		{
			bool flag = obj == null;
			double result;
			if (flag)
			{
				result = 0.0;
			}
			else
			{
				double num = 0.0;
				double.TryParse(obj.ToString(), out num);
				result = num;
			}
			return result;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002C2C File Offset: 0x00000E2C
		public static long ToLong(object obj)
		{
			bool flag = obj == null;
			long result;
			if (flag)
			{
				result = 0L;
			}
			else
			{
				long num = 0L;
				long.TryParse(obj.ToString(), out num);
				result = num;
			}
			return result;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002C60 File Offset: 0x00000E60
		public static DateTime ToDateTime(object obj)
		{
			bool flag = obj == null;
			DateTime result;
			if (flag)
			{
				result = DateTime.MinValue;
			}
			else
			{
				DateTime minValue = DateTime.MinValue;
				DateTime.TryParse(obj.ToString(), out minValue);
				result = minValue;
			}
			return result;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002C98 File Offset: 0x00000E98
		public static DateTime ToExcelDateTime(object obj)
		{
			bool flag = obj == null;
			DateTime result;
			if (flag)
			{
				result = DateTime.MinValue;
			}
			else
			{
				bool flag2 = !obj.ToString().Contains("/");
				if (flag2)
				{
					result = DateTime.FromOADate(Helper.ToDouble(obj));
				}
				else
				{
					DateTime minValue = DateTime.MinValue;
					DateTime.TryParse(obj.ToString(), out minValue);
					result = minValue;
				}
			}
			return result;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002CF8 File Offset: 0x00000EF8
		public static int CompanyDate(string dateStr1, string dateStr2)
		{
			DateTime t = Convert.ToDateTime(dateStr1);
			DateTime t2 = Convert.ToDateTime(dateStr2);
			return DateTime.Compare(t, t2);
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002D24 File Offset: 0x00000F24
		public static int ToInt(object obj)
		{
			bool flag = obj == null;
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				int num = 0;
				int.TryParse(obj.ToString(), out num);
				result = num;
			}
			return result;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002D54 File Offset: 0x00000F54
		public static string ToStr(object obj, int type = 0)
		{
			bool flag = obj == null;
			string result;
			if (flag)
			{
				result = "";
			}
			else
			{
				bool flag2 = type == 1;
				if (flag2)
				{
					string text = obj.ToString();
					result = ((text == "0") ? "" : text);
				}
				else
				{
					result = obj.ToString().Trim();
				}
			}
			return result;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002DAC File Offset: 0x00000FAC
		public static string ToXML(object obj)
		{
			bool flag = obj == null;
			string result;
			if (flag)
			{
				result = "";
			}
			else
			{
				string text = SecurityElement.Escape(obj.ToString().Trim());
				result = text;
			}
			return result;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002DE4 File Offset: 0x00000FE4
		public static string ListToStr(List<string> list)
		{
			string text = "";
			foreach (string str in list)
			{
				text = text + str + ",";
			}
			return text.TrimEnd(new char[]
			{
				','
			});
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002E58 File Offset: 0x00001058
		public static string ToBool(object obj)
		{
			bool flag = obj == null;
			string result;
			if (flag)
			{
				result = "false";
			}
			else
			{
				string a = obj.ToString().ToLower();
				bool flag2 = a == "1" || a == "true";
				if (flag2)
				{
					result = "true";
				}
				else
				{
					result = "false";
				}
			}
			return result;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002EB8 File Offset: 0x000010B8
		public static string DateToString(object date, int type = 0)
		{
			bool flag = date == null;
			string result;
			if (flag)
			{
				result = "";
			}
			else
			{
				DateTime dateTime = Convert.ToDateTime(date);
				bool flag2 = type == 1;
				if (flag2)
				{
					bool flag3 = dateTime.Month < 10;
					if (flag3)
					{
						result = dateTime.Year.ToString() + "-0" + dateTime.Month.ToString();
					}
					else
					{
						result = dateTime.Year.ToString() + "-" + dateTime.Month.ToString();
					}
				}
				else
				{
					bool flag4 = type == 2;
					if (flag4)
					{
						result = dateTime.Year.ToString();
					}
					else
					{
						bool flag5 = type == 3;
						if (flag5)
						{
							result = dateTime.Month.ToString();
						}
						else
						{
							bool flag6 = type == 4;
							if (flag6)
							{
								result = dateTime.Day.ToString();
							}
							else
							{
								result = string.Concat(new string[]
								{
									dateTime.Year.ToString(),
									"-",
									dateTime.Month.ToString(),
									"-",
									dateTime.Day.ToString()
								});
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00003014 File Offset: 0x00001214
		public static string DateToNY(object date, int addMonth)
		{
			bool flag = date == null;
			string result;
			if (flag)
			{
				result = "";
			}
			else
			{
				DateTime dateTime = Convert.ToDateTime(date).AddMonths(addMonth);
				bool flag2 = dateTime.Month < 10;
				if (flag2)
				{
					result = dateTime.Year.ToString() + "-0" + dateTime.Month.ToString();
				}
				else
				{
					result = dateTime.Year.ToString() + "-" + dateTime.Month.ToString();
				}
			}
			return result;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x000030B4 File Offset: 0x000012B4
		public static string GetNY(object rq)
		{
			bool flag = rq == null;
			string result;
			if (flag)
			{
				result = "";
			}
			else
			{
				DateTime dateTime = Convert.ToDateTime(rq);
				string str = dateTime.Year.ToString();
				int month = dateTime.Month;
				int day = dateTime.Day;
				bool flag2 = day > 25;
				if (flag2)
				{
					DateTime dateTime2 = dateTime.AddMonths(1);
					str = dateTime2.Year.ToString();
					month = dateTime2.Month;
				}
				bool flag3 = month < 10;
				if (flag3)
				{
					result = str + "-0" + month.ToString();
				}
				else
				{
					result = str + "-" + month.ToString();
				}
			}
			return result;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x0000316C File Offset: 0x0000136C
		public static void GetListByRandom(List<string> list, int num, List<string> list2)
		{
			Random r = new Random();
			IEnumerable<string> enumerable = (from x in list
			orderby r.Next()
			select x).Take(num);
			foreach (string item in enumerable)
			{
				bool flag = !list2.Contains(item);
				if (flag)
				{
					list2.Add(item);
				}
			}
			bool flag2 = list2.Count < num;
			if (flag2)
			{
				Helper.GetListByRandom(list.Except(list2).ToList<string>(), num - list2.Count, list2);
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00003224 File Offset: 0x00001424
		public static void WriteLog(string method, string requestJson)
		{
			try
			{
				Task.Run(delegate()
				{
					string text = AppDomain.CurrentDomain.BaseDirectory + "\\ERPLog\\" + DateTime.Now.ToString("yyyyMM") + "\\";
					bool flag = !Directory.Exists(text);
					if (flag)
					{
						Directory.CreateDirectory(text);
					}
					string path = string.Concat(new string[]
					{
						text,
						method,
						"_",
						DateTime.Now.ToString("yyyyMMddHHmmssffff"),
						".txt"
					});
					using (StreamWriter streamWriter = File.AppendText(path))
					{
						streamWriter.WriteLine("------------" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
						streamWriter.WriteLine(requestJson);
					}
				});
			}
			catch
			{
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00003274 File Offset: 0x00001474
		public static string BuildApiUrl(string host, string method)
		{
			bool flag = host.EndsWith("/");
			string result;
			if (flag)
			{
				result = host + method;
			}
			else
			{
				result = host + "/" + method;
			}
			return result;
		}
	}
}
