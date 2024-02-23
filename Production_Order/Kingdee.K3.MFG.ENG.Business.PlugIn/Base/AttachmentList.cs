using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000004 RID: 4
	[Description("附件明细列表插件")]
	public class AttachmentList : BaseControlList
	{
		// Token: 0x06000003 RID: 3 RVA: 0x00002070 File Offset: 0x00000270
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbbtnDel"))
				{
					return;
				}
				List<int> source = (from p in this.ListView.SelectedRowsInfo
				select Convert.ToInt32(p.PrimaryKeyValue)).ToList<int>();
				try
				{
					MFGServiceHelper.ExecuteTempFile(base.Context, new List<string>
					{
						source.FirstOrDefault<int>().ToString()
					}, "SFC", false, "");
				}
				catch (Exception ex)
				{
					Logger.Error("MFG", ResManager.LoadKDString("技术文档下载出错", "015072000028829", 7, new object[0]), ex);
				}
			}
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000213C File Offset: 0x0000033C
		private void DeleteFile(List<string> attID)
		{
			new List<SelectorItemInfo>();
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "BOS_Attachment", true) as FormMetadata;
			DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, attID.ToArray(), formMetadata.BusinessInfo.GetDynamicObjectType());
			if (array != null && array.Count<DynamicObject>() > 0)
			{
				try
				{
					try
					{
						if (Interlocked.Exchange(ref AttachmentList.inTimer, 1) == 0)
						{
							Thread thread = new Thread(new ParameterizedThreadStart(this.DownloadFile));
							string item = HttpContext.Current.Request.PhysicalApplicationPath + KeyConst.TEMPFILEPATH;
							thread.Start(new Tuple<DynamicObject[], string>(array, item));
						}
					}
					catch (Exception)
					{
					}
					return;
				}
				finally
				{
					Interlocked.Exchange(ref AttachmentList.inTimer, 0);
				}
			}
			throw new Exception(ResManager.LoadKDString("数据错误", "015072000028830", 7, new object[0]));
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002224 File Offset: 0x00000424
		private void DownloadFile(object data)
		{
			try
			{
				Tuple<DynamicObject[], string> tuple = data as Tuple<DynamicObject[], string>;
				DynamicObject[] item = tuple.Item1;
				string item2 = tuple.Item2;
				foreach (DynamicObject dynamicObject in item)
				{
					if (!Directory.Exists(item2))
					{
						Directory.CreateDirectory(item2);
					}
					string text = dynamicObject["AttachmentName"].ToString();
					text = text.Replace(" ", "");
					text = text.Replace("%", "");
					text = text.Replace("#", "");
					text = text.Replace("%", "");
					text = text.Replace("^", "");
					text = text.Replace("&", "");
					text = text.Replace("+", "");
					string path = Path.Combine(item2, string.Concat(new string[]
					{
						base.Context.DBId,
						"_",
						dynamicObject["Id"].ToString(),
						"_",
						text
					}));
					if (File.Exists(path))
					{
						File.Delete(path);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("MFG", ResManager.LoadKDString("技术文档下载出错", "015072000028829", 7, new object[0]), ex);
			}
		}

		// Token: 0x04000001 RID: 1
		private static int inTimer;
	}
}
