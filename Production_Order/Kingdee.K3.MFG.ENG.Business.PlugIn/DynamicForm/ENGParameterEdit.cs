using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000093 RID: 147
	[Description("工程数据参数插件")]
	public class ENGParameterEdit : BaseMFGSysParamEditPlugIn
	{
		// Token: 0x06000ABF RID: 2751 RVA: 0x0007BD23 File Offset: 0x00079F23
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this._orgId = Convert.ToInt64(e.Paramter.GetCustomParameter("OrgId"));
		}

		// Token: 0x06000AC0 RID: 2752 RVA: 0x0007BD48 File Offset: 0x00079F48
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			object value = MFGServiceHelper.GetSystemProfile<long>(base.Context, 0L, "MFG_EngParameter", "DateScope", 0L);
			if (Convert.ToInt64(value) == 0L)
			{
				this.View.Model.SetValue("FDateScope", 180);
			}
		}

		// Token: 0x06000AC1 RID: 2753 RVA: 0x0007BDA4 File Offset: 0x00079FA4
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.ShowOrHideEffectDateField();
			long num = Convert.ToInt64(SystemParameterServiceHelper.GetParamter(base.Context, this._orgId, 0L, "MFG_EngParameter", "DateScope", 0L));
			string text = Convert.ToString(SystemParameterServiceHelper.GetParamter(base.Context, this._orgId, 0L, "MFG_EngParameter", "SourceBill", 0L));
			string text2 = Convert.ToString(SystemParameterServiceHelper.GetParamter(base.Context, this._orgId, 0L, "MFG_EngParameter", "PriceField", 0L));
			string text3 = Convert.ToString(SystemParameterServiceHelper.GetParamter(base.Context, this._orgId, 0L, "MFG_EngParameter", "SourceRate", 0L));
			string text4 = Convert.ToString(SystemParameterServiceHelper.GetParamter(base.Context, this._orgId, 0L, "MFG_EngParameter", "IsDiffBomVersion", 0L));
			string systemProfile = MFGServiceHelper.GetSystemProfile<string>(base.Context, this._orgId, "MFG_EngParameter", "CostQtyParam", null);
			long num2 = (num != 0L) ? num : Convert.ToInt64(SystemParameterServiceHelper.GetParamter(base.Context, 0L, 0L, "MFG_EngParameter", "DateScope", 0L));
			string text5 = (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text)) ? text : Convert.ToString(SystemParameterServiceHelper.GetParamter(base.Context, 0L, 0L, "MFG_EngParameter", "SourceBill", 0L));
			string text6 = (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2)) ? text2 : Convert.ToString(SystemParameterServiceHelper.GetParamter(base.Context, 0L, 0L, "MFG_EngParameter", "PriceField", 0L));
			string text7 = (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text3)) ? text3 : Convert.ToString(SystemParameterServiceHelper.GetParamter(base.Context, 0L, 0L, "MFG_EngParameter", "SourceRate", 0L));
			string text8 = (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text4)) ? text4 : Convert.ToString(SystemParameterServiceHelper.GetParamter(base.Context, 0L, 0L, "MFG_EngParameter", "IsDiffBomVersion", 0L));
			string text9 = (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(systemProfile)) ? systemProfile : MFGServiceHelper.GetSystemProfile<string>(base.Context, 0L, "MFG_EngParameter", "CostQtyParam", "1");
			this.View.Model.SetValue("FDateScope", num2);
			this.View.Model.SetValue("FIsDiffBomVersion", text8);
			this.View.Model.SetValue("FCostQtyParam", text9);
			this.View.Model.SetValue("FSourceBill", text5);
			this.View.Model.SetValue("FPriceField", text6);
			this.View.Model.SetValue("FSourceRate", text7);
			this.View.UpdateView("FDateScope");
			this.View.UpdateView("FSourceBill");
			this.View.UpdateView("FPriceField");
			this.View.UpdateView("FSourceRate");
			this.View.UpdateView("FIsDiffBomVersion");
			this.View.UpdateView("FCostQtyParam");
			if (base.Context.IsStandardEdition())
			{
				FormUtils.StdHideField(this.View, "FIsCheckCurBOMDistinct");
			}
		}

		// Token: 0x06000AC2 RID: 2754 RVA: 0x0007C0AD File Offset: 0x0007A2AD
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			if (StringUtils.EqualsIgnoreCase(e.Field.Key, "FEffectDateType"))
			{
				this.ShowOrHideEffectDateField();
			}
		}

		// Token: 0x06000AC3 RID: 2755 RVA: 0x0007C0D4 File Offset: 0x0007A2D4
		private void ShowOrHideEffectDateField()
		{
			bool visible = false;
			string a = Convert.ToString(this.View.Model.GetValue("FEffectDateType"));
			if (a == "1")
			{
				visible = true;
			}
			this.View.GetControl("FEffectDate").Visible = visible;
		}

		// Token: 0x0400050F RID: 1295
		private long _orgId;
	}
}
