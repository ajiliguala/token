using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Common;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200007F RID: 127
	[Description("BOM更新用料清单")]
	public class BomSynsUpdatePpBom : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000055 RID: 85
		// (get) Token: 0x0600095B RID: 2395 RVA: 0x0006F4C8 File Offset: 0x0006D6C8
		// (set) Token: 0x0600095C RID: 2396 RVA: 0x0006F4D0 File Offset: 0x0006D6D0
		private string PrdPageId { get; set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x0600095D RID: 2397 RVA: 0x0006F4D9 File Offset: 0x0006D6D9
		// (set) Token: 0x0600095E RID: 2398 RVA: 0x0006F4E1 File Offset: 0x0006D6E1
		private string SubPageId { get; set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x0600095F RID: 2399 RVA: 0x0006F4EA File Offset: 0x0006D6EA
		// (set) Token: 0x06000960 RID: 2400 RVA: 0x0006F4F2 File Offset: 0x0006D6F2
		private string PlnPagetId { get; set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x06000961 RID: 2401 RVA: 0x0006F4FB File Offset: 0x0006D6FB
		// (set) Token: 0x06000962 RID: 2402 RVA: 0x0006F503 File Offset: 0x0006D703
		protected List<DynamicObject> moDatas { get; set; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000963 RID: 2403 RVA: 0x0006F50C File Offset: 0x0006D70C
		// (set) Token: 0x06000964 RID: 2404 RVA: 0x0006F514 File Offset: 0x0006D714
		protected List<DynamicObject> subDatas { get; set; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000965 RID: 2405 RVA: 0x0006F51D File Offset: 0x0006D71D
		// (set) Token: 0x06000966 RID: 2406 RVA: 0x0006F525 File Offset: 0x0006D725
		protected List<DynamicObject> plnDatas { get; set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000967 RID: 2407 RVA: 0x0006F52E File Offset: 0x0006D72E
		// (set) Token: 0x06000968 RID: 2408 RVA: 0x0006F536 File Offset: 0x0006D736
		private List<DynamicObject> BomDatas { get; set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000969 RID: 2409 RVA: 0x0006F53F File Offset: 0x0006D73F
		// (set) Token: 0x0600096A RID: 2410 RVA: 0x0006F547 File Offset: 0x0006D747
		private PermissionAuthResult authPrdResult { get; set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x0600096B RID: 2411 RVA: 0x0006F550 File Offset: 0x0006D750
		// (set) Token: 0x0600096C RID: 2412 RVA: 0x0006F558 File Offset: 0x0006D758
		private PermissionAuthResult authSubResult { get; set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x0600096D RID: 2413 RVA: 0x0006F561 File Offset: 0x0006D761
		// (set) Token: 0x0600096E RID: 2414 RVA: 0x0006F569 File Offset: 0x0006D769
		private PermissionAuthResult authPlnResult { get; set; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x0600096F RID: 2415 RVA: 0x0006F572 File Offset: 0x0006D772
		// (set) Token: 0x06000970 RID: 2416 RVA: 0x0006F57A File Offset: 0x0006D77A
		private string consultDate { get; set; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000971 RID: 2417 RVA: 0x0006F583 File Offset: 0x0006D783
		// (set) Token: 0x06000972 RID: 2418 RVA: 0x0006F58B File Offset: 0x0006D78B
		private bool isSkipExpand { get; set; }

		// Token: 0x06000973 RID: 2419 RVA: 0x0006F594 File Offset: 0x0006D794
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.ShowDynamicTabForm();
		}

		// Token: 0x06000974 RID: 2420 RVA: 0x0006F5A4 File Offset: 0x0006D7A4
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string key;
			if ((key = e.Key) != null)
			{
				if (key == "FBTNOK")
				{
					this.SynsUpdatePPbom();
					return;
				}
				if (!(key == "FBTNCANCEL"))
				{
					return;
				}
				this.View.Close();
			}
		}

		// Token: 0x06000975 RID: 2421 RVA: 0x0006F5F0 File Offset: 0x0006D7F0
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			object obj;
			if (e.EventName == "IsPrdDatas" && this.View.Session.TryGetValue("MoDatas", out obj))
			{
				this.moDatas = (obj as List<DynamicObject>);
			}
			object obj2;
			if (e.EventName == "IsSubDatas" && this.View.Session.TryGetValue("SubDatas", out obj2))
			{
				this.subDatas = (obj2 as List<DynamicObject>);
			}
			object obj3;
			if (e.EventName == "IsPlnDatas" && this.View.Session.TryGetValue("PlnDatas", out obj3))
			{
				this.plnDatas = (obj3 as List<DynamicObject>);
			}
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x0006F6AC File Offset: 0x0006D8AC
		private void ShowDynamicTabForm()
		{
			bool flag = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("IsShowPrdList"));
			bool flag2 = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("IsShowSubList"));
			bool flag3 = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("IsShowPlnList"));
			List<long> value = this.View.OpenParameter.GetCustomParameter("BomId") as List<long>;
			List<long> value2 = this.View.OpenParameter.GetCustomParameter("UserOrgId") as List<long>;
			this.consultDate = Convert.ToString(this.View.OpenParameter.GetCustomParameter("ConSultDate"));
			this.isSkipExpand = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("IsSkipExpand"));
			List<DynamicObject> list = new List<DynamicObject>();
			list = (this.View.OpenParameter.GetCustomParameter("BomData") as List<DynamicObject>);
			this.BomDatas = list;
			if (ObjectUtils.IsNullOrEmpty(this.authPrdResult))
			{
				this.authPrdResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
				{
					Id = "PRD_PPBOM"
				}, "f323992d896745fbaab4a2717c79ce2e");
			}
			if (ObjectUtils.IsNullOrEmpty(this.authSubResult))
			{
				this.authSubResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
				{
					Id = "SUB_PPBOM"
				}, "f323992d896745fbaab4a2717c79ce2e");
			}
			if (ObjectUtils.IsNullOrEmpty(this.authPlnResult))
			{
				this.authPlnResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
				{
					Id = "SUB_PPBOM"
				}, "f323992d896745fbaab4a2717c79ce2e");
			}
			if (!this.authPrdResult.Passed && !this.authSubResult.Passed && !this.authPlnResult.Passed)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("用户没有计划订单、生产、委外用料清单的修改权限", "015072000018034", 7, new object[0]), "", 0);
				return;
			}
			if (flag3 && this.authPrdResult.Passed)
			{
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = "PLN_OrderListShow";
				dynamicFormShowParameter.ParentPageId = this.View.PageId;
				string text = SequentialGuid.NewGuid().ToString();
				this.PlnPagetId = text;
				dynamicFormShowParameter.PageId = text;
				dynamicFormShowParameter.OpenStyle.TagetKey = "FAddTab";
				dynamicFormShowParameter.OpenStyle.ShowType = 1;
				dynamicFormShowParameter.CustomComplexParams.Add("BomData", list);
				dynamicFormShowParameter.CustomComplexParams.Add("BomId", value);
				dynamicFormShowParameter.CustomComplexParams.Add("UserOrgId", value2);
				this.View.ShowForm(dynamicFormShowParameter);
			}
			if (flag && this.authPrdResult.Passed)
			{
				DynamicFormShowParameter dynamicFormShowParameter2 = new DynamicFormShowParameter();
				dynamicFormShowParameter2.FormId = "PRD_MoListShow";
				dynamicFormShowParameter2.ParentPageId = this.View.PageId;
				string text2 = SequentialGuid.NewGuid().ToString();
				this.PrdPageId = text2;
				dynamicFormShowParameter2.PageId = text2;
				dynamicFormShowParameter2.OpenStyle.TagetKey = "FAddTab";
				dynamicFormShowParameter2.OpenStyle.ShowType = 1;
				dynamicFormShowParameter2.CustomComplexParams.Add("BomData", list);
				dynamicFormShowParameter2.CustomComplexParams.Add("BomId", value);
				dynamicFormShowParameter2.CustomComplexParams.Add("UserOrgId", value2);
				if (this.View.OpenParameter.ParentPageId == "PRD_MO")
				{
					List<long> value3 = this.View.OpenParameter.GetCustomParameter("EntryIds") as List<long>;
					dynamicFormShowParameter2.CustomComplexParams.Add("EntryIds", value3);
					dynamicFormShowParameter2.ParentPageId = "PRD_MO";
				}
				this.View.ShowForm(dynamicFormShowParameter2);
			}
			if (flag2 && this.authSubResult.Passed)
			{
				DynamicFormShowParameter dynamicFormShowParameter3 = new DynamicFormShowParameter();
				dynamicFormShowParameter3.FormId = "SUB_ReqListShow";
				dynamicFormShowParameter3.ParentPageId = this.View.PageId;
				string text3 = SequentialGuid.NewGuid().ToString();
				this.SubPageId = text3;
				dynamicFormShowParameter3.PageId = text3;
				dynamicFormShowParameter3.OpenStyle.TagetKey = "FAddTab";
				dynamicFormShowParameter3.OpenStyle.ShowType = 1;
				dynamicFormShowParameter3.CustomComplexParams.Add("BomData", list);
				dynamicFormShowParameter3.CustomComplexParams.Add("BomId", value);
				dynamicFormShowParameter3.CustomComplexParams.Add("UserOrgId", value2);
				if (this.View.OpenParameter.ParentPageId == "SUB_SUBREQORDER")
				{
					List<long> value4 = this.View.OpenParameter.GetCustomParameter("EntryIds") as List<long>;
					dynamicFormShowParameter3.CustomComplexParams.Add("EntryIds", value4);
					dynamicFormShowParameter3.ParentPageId = "SUB_SUBREQORDER";
				}
				this.View.ShowForm(dynamicFormShowParameter3);
			}
			this.View.GetControl<TabControl>("FAddTab").SelectedIndex = 0;
		}

		// Token: 0x06000977 RID: 2423 RVA: 0x0006FBD0 File Offset: 0x0006DDD0
		private void SynsUpdatePPbom()
		{
			this.View.Session.Clear();
			if (!StringUtils.IsEmpty(this.PrdPageId))
			{
				IDynamicFormView view = this.View.GetView(this.PrdPageId);
				(view as IDynamicFormViewService).CustomEvents(this.View.PageId, "GetMoDatas", "");
				this.View.SendAynDynamicFormAction(view);
			}
			if (!StringUtils.IsEmpty(this.SubPageId))
			{
				IDynamicFormView view2 = this.View.GetView(this.SubPageId);
				(view2 as IDynamicFormViewService).CustomEvents(this.View.PageId, "GetSubDatas", "");
				this.View.SendAynDynamicFormAction(view2);
			}
			if (!StringUtils.IsEmpty(this.PlnPagetId))
			{
				IDynamicFormView view3 = this.View.GetView(this.PlnPagetId);
				(view3 as IDynamicFormViewService).CustomEvents(this.View.PageId, "GetPlnDatas", "");
				this.View.SendAynDynamicFormAction(view3);
			}
			if (ListUtils.IsEmpty<DynamicObject>(this.plnDatas) && ListUtils.IsEmpty<DynamicObject>(this.moDatas) && ListUtils.IsEmpty<DynamicObject>(this.subDatas))
			{
				return;
			}
			bool value = MFGBillUtil.GetValue<bool>(this.View.Model, "FIsNewOrder", -1, false, null);
			TaskProxyItem taskProxyItem = new TaskProxyItem();
			List<object> list = new List<object>
			{
				base.Context,
				this.BomDatas,
				ListUtils.IsEmpty<DynamicObject>(this.plnDatas) ? new List<DynamicObject>() : this.plnDatas,
				ListUtils.IsEmpty<DynamicObject>(this.moDatas) ? new List<DynamicObject>() : this.moDatas,
				ListUtils.IsEmpty<DynamicObject>(this.subDatas) ? new List<DynamicObject>() : this.subDatas,
				this.consultDate,
				this.isSkipExpand,
				value,
				taskProxyItem.TaskId
			};
			taskProxyItem.Parameters = list.ToArray();
			taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.BomSynUpdatePPBom.BOMSynUpdateService,Kingdee.K3.MFG.ENG.App.Core";
			taskProxyItem.MethodName = "SynsUpdatePrdOrSubPpBom";
			taskProxyItem.ProgressQueryInterval = 1;
			taskProxyItem.Title = ResManager.LoadKDString("同步更新计划BOM/用料清单", "015072000018035", 7, new object[0]);
			taskProxyItem.Title = ResManager.LoadKDString("同步更新用料清单", "015072000018149", 7, new object[0]);
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
			{
				this.DoEmptyFunction();
			});
		}

		// Token: 0x06000978 RID: 2424 RVA: 0x0006FE5B File Offset: 0x0006E05B
		protected virtual void DoEmptyFunction()
		{
		}
	}
}
