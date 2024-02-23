using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200005B RID: 91
	[Description("生成下级订单中间单据空白对象插件")]
	public class BillMiddleEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x060006B1 RID: 1713 RVA: 0x0004EED8 File Offset: 0x0004D0D8
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			string text = Convert.ToString(this.View.OpenParameter.GetCustomParameter("fromId", true));
			DynamicObject[] source = this.View.OpenParameter.GetCustomParameter("billDatas", true) as DynamicObject[];
			if (StringUtils.EqualsIgnoreCase(text, "PRD_MO"))
			{
				this.View.SetFormTitle(new LocaleValue(ResManager.LoadKDString("生成下级订单-生产订单", "0151515153499000016547", 7, new object[0]), this.View.Context.UserLocale.LCID));
			}
			else
			{
				this.View.SetFormTitle(new LocaleValue(ResManager.LoadKDString("生成下级订单-委外订单", "0151515153499000016548", 7, new object[0]), this.View.Context.UserLocale.LCID));
			}
			List<DynamicObject> list = source.ToList<DynamicObject>();
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.ParentPageId = this.View.PageId;
			if (list.Count == 1)
			{
				billShowParameter.Status = 0;
				billShowParameter.CreateFrom = 1;
				string key = "_ConvertSessionKey";
				string text2 = "ConverOneResult";
				billShowParameter.CustomParams.Add(key, text2);
				this.View.Session[text2] = list[0];
				billShowParameter.FormId = text;
			}
			else
			{
				if (list.Count <= 1)
				{
					return;
				}
				billShowParameter.FormId = "BOS_ConvertResultForm";
				string key2 = "ConvertResults";
				this.View.Session[key2] = list.ToArray();
				billShowParameter.CustomParams.Add("_ConvertResultFormId", text);
			}
			if (this.View.Context.UserToken.ToLowerInvariant().Equals("bosidetest"))
			{
				billShowParameter.OpenStyle.ShowType = 0;
			}
			else
			{
				billShowParameter.OpenStyle.TagetKey = "FPanel";
				billShowParameter.OpenStyle.ShowType = 3;
			}
			this.View.ShowForm(billShowParameter);
		}
	}
}
