using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.VerificationHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000043 RID: 67
	public class ProcessEdit : BaseControlEdit
	{
		// Token: 0x06000497 RID: 1175 RVA: 0x00038A00 File Offset: 0x00036C00
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString().ToUpper()) != null)
			{
				if (!(a == "SAVE") && !(a == "SUBMIT"))
				{
					return;
				}
				foreach (DynamicObject dynamicObject in ((DynamicObjectCollection)base.View.Model.DataObject["CostResourceEntity"]))
				{
					long num = Convert.ToInt64(dynamicObject["Id"]);
					decimal d = Convert.ToDecimal(dynamicObject["UseRate"]);
					if (this.curentUseRate.Keys.Contains(num) && this.curentUseRate[num] != d)
					{
						dynamicObject["UseRateRecent"] = this.curentUseRate[num];
					}
				}
			}
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x00038B0C File Offset: 0x00036D0C
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			this.firstDoOperation = true;
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbSyncRoute"))
				{
					return;
				}
				LicenseVerifier.CheckViewOnlyOperation(base.Context, ResManager.LoadKDString("同步工艺路线", "015072030034331", 7, new object[0]));
				DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.Model.DataObject["CostResourceEntity"];
				if (dynamicObjectCollection != null && dynamicObjectCollection.Count > 0)
				{
					DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
					dynamicFormShowParameter.ParentPageId = base.View.PageId;
					dynamicFormShowParameter.CustomComplexParams.Add("ProcessId", base.View.Model.DataObject["Id"]);
					dynamicFormShowParameter.CustomComplexParams.Add("UseOrgId", base.View.Model.DataObject["UseOrgId_Id"]);
					dynamicFormShowParameter.FormId = "SFC_FlexibleRouteList";
					dynamicFormShowParameter.CustomComplexParams.Add("CostInfo", dynamicObjectCollection);
					base.View.ShowForm(dynamicFormShowParameter);
					return;
				}
				base.View.ShowErrMessage(ResManager.LoadKDString("没有成本资源需要同步！", "015072030033569", 7, new object[0]), "", 0);
			}
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x00038C50 File Offset: 0x00036E50
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string operation;
			if ((operation = e.Operation.Operation) != null)
			{
				if (!(operation == "Save") && !(operation == "Submit"))
				{
					return;
				}
				this.HandleRateRecent();
				base.View.UpdateView();
			}
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x00038D04 File Offset: 0x00036F04
		private bool CanUnAudit(BeforeDoOperationEventArgs e)
		{
			this.firstDoOperation = false;
			bool bCheckResult = true;
			string text = string.Format("{0}={1}", "FPROCESSID", Convert.ToInt32(this.Model.DataObject["Id"]).ToString());
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FID"),
				new SelectorItemInfo("FNUMBER"),
				new SelectorItemInfo("FPROCESSID")
			};
			List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_Route", list, text, "");
			if (baseBillInfo != null)
			{
				bCheckResult = (baseBillInfo.Count <= 0);
				if (baseBillInfo.Count > 0)
				{
					string arg = string.Join<object>(",", from p in baseBillInfo
					select p["Number"]);
					string text2 = string.Format(ResManager.LoadKDString("当前作业已被工艺路线[{0}]引用,请确认是否反审核?", "015072000001786", 7, new object[0]), arg);
					base.View.ShowMessage(text2, 4, delegate(MessageBoxResult result)
					{
						if (result == 6)
						{
							this.View.InvokeFormOperation(e.Operation.FormOperation.Operation.ToString());
							bCheckResult = true;
							return;
						}
						this.firstDoOperation = true;
					}, "", 0);
				}
			}
			return bCheckResult;
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x00038E64 File Offset: 0x00037064
		public override void AfterBindData(EventArgs e)
		{
			IDynamicFormView parentFormView = base.View.ParentFormView;
			if (parentFormView != null)
			{
				parentFormView.Model.SetValue("FOrgId", "0");
			}
			Dictionary<string, string> billType = this.GetBillType();
			object value = base.View.Model.GetValue("FFeatureFlag");
			if (billType != null && billType.Count<KeyValuePair<string, string>>() > 0)
			{
				this.BindBillTypeCombox(billType);
				if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
				{
					string text = string.Join(",", billType.Keys);
					base.View.Model.SetValue("FFeatureFlag", text);
					base.View.UpdateView("FFeatureFlag");
				}
			}
			if (parentFormView != null)
			{
				base.View.SendAynDynamicFormAction(parentFormView);
			}
			this.HandleRateRecent();
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x00038F24 File Offset: 0x00037124
		private void BindBillTypeCombox(Dictionary<string, string> lstBillInfos)
		{
			List<EnumItem> list = new List<EnumItem>();
			if (lstBillInfos != null && lstBillInfos.Count > 0)
			{
				foreach (string text in lstBillInfos.Keys)
				{
					list.Add(new EnumItem
					{
						EnumId = text,
						Value = text,
						Caption = new LocaleValue(Convert.ToString(lstBillInfos[text]), base.Context.UserLocale.LCID)
					});
				}
			}
			ComboFieldEditor control = base.View.GetControl<ComboFieldEditor>("FFeatureFlag");
			control.SetComboItems(list);
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x00038FE0 File Offset: 0x000371E0
		private Dictionary<string, string> GetBillType()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("A", ResManager.LoadKDString("普通作业", "015072000018031", 7, new object[0]));
			dictionary.Add("B", ResManager.LoadKDString("包装作业", "015072000018032", 7, new object[0]));
			if (!base.Context.IsStandardEdition())
			{
				dictionary.Add("C", ResManager.LoadKDString("SMT贴片", "015072000018033", 7, new object[0]));
			}
			return dictionary;
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x00039064 File Offset: 0x00037264
		private void HandleRateRecent()
		{
			this.curentUseRate.Clear();
			bool userParam = MFGBillUtil.GetUserParam<bool>(base.View, "IsShowCostResource", false);
			base.View.GetMainBarItem("tbSyncRoute").Visible = userParam;
			base.View.GetControl("FTabCostResource").Visible = userParam;
			if (userParam && base.View.OpenParameter.Status == 2)
			{
				DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.Model.DataObject["CostResourceEntity"];
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					if (!this.curentUseRate.ContainsKey(Convert.ToInt64(dynamicObject["Id"])))
					{
						this.curentUseRate.Add(Convert.ToInt64(dynamicObject["Id"]), Convert.ToDecimal(dynamicObject["UseRate"]));
					}
				}
			}
		}

		// Token: 0x040001FA RID: 506
		private const string FKey_FProcessID = "FPROCESSID";

		// Token: 0x040001FB RID: 507
		private const string FKey_FNumber = "FNUMBER";

		// Token: 0x040001FC RID: 508
		private const string FKey_FID = "FID";

		// Token: 0x040001FD RID: 509
		private bool firstDoOperation = true;

		// Token: 0x040001FE RID: 510
		private Dictionary<long, decimal> curentUseRate = new Dictionary<long, decimal>();
	}
}
