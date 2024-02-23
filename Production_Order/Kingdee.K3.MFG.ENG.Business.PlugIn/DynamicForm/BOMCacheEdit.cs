using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000065 RID: 101
	public class BOMCacheEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000746 RID: 1862 RVA: 0x00055513 File Offset: 0x00053713
		public override void BeforeBindData(EventArgs e)
		{
			this.BindData();
		}

		// Token: 0x06000747 RID: 1863 RVA: 0x0005551B File Offset: 0x0005371B
		public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
		{
			e.IsIsolationOrg = false;
			e.IsAuthPermission = false;
		}

		// Token: 0x06000748 RID: 1864 RVA: 0x0005552B File Offset: 0x0005372B
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			e.IsShowApproved = false;
			e.IsShowUsed = false;
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x0005553B File Offset: 0x0005373B
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			if (StringUtils.EqualsIgnoreCase(e.BarItemKey, "tbRefresh"))
			{
				this.View.Model.DeleteEntryData("FEntity");
				this.BindData();
			}
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x00055574 File Offset: 0x00053774
		private void BindData()
		{
			Dictionary<string, ConcurrentDictionary<string, List<long>>> bomcache = MFGServiceHelper.GetBOMCache(base.Context);
			int num = 0;
			foreach (KeyValuePair<string, ConcurrentDictionary<string, List<long>>> keyValuePair in bomcache)
			{
				foreach (KeyValuePair<string, List<long>> keyValuePair2 in keyValuePair.Value)
				{
					string[] array = keyValuePair2.Key.Split(new string[]
					{
						"_"
					}, StringSplitOptions.RemoveEmptyEntries);
					long num2 = OtherExtend.ConvertTo<long>(array[0], 0L);
					long num3 = OtherExtend.ConvertTo<long>(array[1], 0L);
					foreach (long num4 in keyValuePair2.Value)
					{
						this.Model.CreateNewEntryRow("FEntity");
						this.Model.SetValue("FOrgId", num3, num);
						this.Model.SetValue("FBOMID", num4, num);
						this.Model.SetValue("FMaterialId", num2, num);
						this.Model.SetValue("FSite", Dns.GetHostName(), num);
						this.Model.SetValue("FCacheKey", keyValuePair.Key, num);
						this.Model.SetValue("FCacheType", this.GetBomCacheType(keyValuePair.Key), num);
						num++;
					}
				}
			}
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x00055760 File Offset: 0x00053960
		private string GetBomCacheType(string cacheKey)
		{
			if (cacheKey != null)
			{
				if (cacheKey == "EB83085E-756B-412E-BEDF-0B2D7E4F17C3")
				{
					return ResManager.LoadKDString("默认可用BOM信息", "0151515153499000016549", 7, new object[0]);
				}
				if (cacheKey == "131AF148-1F92-44AB-BD57-00C893ED428D")
				{
					return ResManager.LoadKDString("默认可用标准BOM信息", "0151515153499000016550", 7, new object[0]);
				}
				if (cacheKey == "8AB61FBD-268D-476A-A447-D587CF0E5BE3")
				{
					return ResManager.LoadKDString("默认已审核BOM信息", "0151515153499000016551", 7, new object[0]);
				}
			}
			return string.Empty;
		}
	}
}
