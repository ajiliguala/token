using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.ECN;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200008A RID: 138
	[Description("ECN清理日志-批量删除")]
	public class ECNClearLogBatchDelete : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000A64 RID: 2660 RVA: 0x00078B58 File Offset: 0x00076D58
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.changeOrgId = MFGBillUtil.GetParam<long>(this.View, "FChangeOrgId", 0L);
			this.clearDateFrom = MFGBillUtil.GetParam<DateTime>(this.View, "FClearDateFrom", default(DateTime));
			this.clearDateTo = MFGBillUtil.GetParam<DateTime>(this.View, "FClearDateTo", default(DateTime));
		}

		// Token: 0x06000A65 RID: 2661 RVA: 0x00078BC2 File Offset: 0x00076DC2
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			this.InitData();
		}

		// Token: 0x06000A66 RID: 2662 RVA: 0x00078BD4 File Offset: 0x00076DD4
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbRefresh")
				{
					this.InitData();
					e.Cancel = true;
					return;
				}
				if (!(barItemKey == "tbDelete"))
				{
					return;
				}
				this.DeleteLog();
				e.Cancel = true;
			}
		}

		// Token: 0x06000A67 RID: 2663 RVA: 0x00078C28 File Offset: 0x00076E28
		private void InitData()
		{
			ECNClearLogQueryOption ecnclearLogQueryOption = new ECNClearLogQueryOption
			{
				ChangeOrgId = this.changeOrgId,
				ClearBeginDate = this.clearDateFrom,
				ClearEndDate = this.clearDateTo
			};
			this.ecnClearLogData = ECNClearOldMtrlServiceHelper.GetECNClearLogInfo(base.Context, ecnclearLogQueryOption);
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			int num = 0;
			this.View.Model.BeginIniti();
			this.View.Model.DeleteEntryData("FEntity");
			this.View.Model.BatchCreateNewEntryRow("FEntity", this.ecnClearLogData.Count);
			foreach (DynamicObject dynamicObject in this.ecnClearLogData)
			{
				MFGCommonUtil.SetDyFormViewFieldsValue(this.View, entity, dynamicObject, num);
				num++;
			}
			this.View.Model.EndIniti();
			this.View.UpdateView("FEntity");
		}

		// Token: 0x06000A68 RID: 2664 RVA: 0x00078D64 File Offset: 0x00076F64
		private void DeleteLog()
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			List<DynamicObject> list = (from w in entityDataObject
			where DataEntityExtend.GetDynamicValue<bool>(w, "CheckBox", false)
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请先选择需要删除的日志！", "015072000012083", 7, new object[0]), "", 0);
				return;
			}
			List<string> list2 = (from s in list
			select DataEntityExtend.GetDynamicValue<string>(s, "ID", null)).ToList<string>();
			ECNClearOldMtrlServiceHelper.DeleteLog(base.Context, list2);
			this.InitData();
		}

		// Token: 0x040004F8 RID: 1272
		private long changeOrgId;

		// Token: 0x040004F9 RID: 1273
		private DateTime clearDateFrom;

		// Token: 0x040004FA RID: 1274
		private DateTime clearDateTo;

		// Token: 0x040004FB RID: 1275
		private DynamicObjectCollection ecnClearLogData;
	}
}
