using System;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Resource;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Base;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200006F RID: 111
	[Description("批量维护模拟结果列表插件")]
	public class BomBatchEditList : BaseControlList
	{
		// Token: 0x0600081B RID: 2075 RVA: 0x00060B34 File Offset: 0x0005ED34
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			object customParameter = e.Paramter.GetCustomParameter("ECNSimEffect");
			if (customParameter != null && customParameter.ToString().Equals("ECNSimEffect", StringComparison.OrdinalIgnoreCase))
			{
				this.View.SetFormTitle(new LocaleValue(ResManager.LoadKDString("模拟物料清单列表", "0151515153499000014784", 7, new object[0]), this.View.Context.UserLocale.LCID));
			}
		}

		// Token: 0x0600081C RID: 2076 RVA: 0x00060BAC File Offset: 0x0005EDAC
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			if (e.Row == 1)
			{
				(this.View.ParentFormView as IDynamicFormViewService).CustomEvents("EntityRowClick", "EntitySelect", "BtnShow");
				this.View.SendDynamicFormAction(this.View.ParentFormView);
			}
		}

		// Token: 0x0600081D RID: 2077 RVA: 0x00060C38 File Offset: 0x0005EE38
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbClearData"))
				{
					return;
				}
				this.View.ShowMessage(ResManager.LoadKDString("数据清理前，请确认没有其他用户正在使用物料清单批量维护", "0151515153499000012831", 7, new object[0]), 4, delegate(MessageBoxResult ret)
				{
					if (ret == 6)
					{
						BomBatchEditServiceHelper.ClearDatas(base.Context);
						this.View.ShowMessage(ResManager.LoadKDString("数据清理完成", "0151515153499000012832", 7, new object[0]), 0);
					}
				}, "", 0);
			}
		}
	}
}
