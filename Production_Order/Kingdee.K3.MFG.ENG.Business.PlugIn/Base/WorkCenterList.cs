using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCEntity.SFC.Base;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200004B RID: 75
	public class WorkCenterList : BaseControlList
	{
		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000521 RID: 1313 RVA: 0x0003E988 File Offset: 0x0003CB88
		// (set) Token: 0x06000522 RID: 1314 RVA: 0x0003E990 File Offset: 0x0003CB90
		private List<int> lstCurSelId { get; set; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000523 RID: 1315 RVA: 0x0003E999 File Offset: 0x0003CB99
		// (set) Token: 0x06000524 RID: 1316 RVA: 0x0003E9A1 File Offset: 0x0003CBA1
		private Dictionary<int, string> dctCurSelIdNum { get; set; }

		// Token: 0x06000525 RID: 1317 RVA: 0x0003E9AA File Offset: 0x0003CBAA
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.InitCustomParameters();
			if (this.workCenterType.Equals("D"))
			{
				this.View.OpenParameter.LayoutId = this.layOutId;
				this.InitFormTitle();
			}
		}

		// Token: 0x06000526 RID: 1318 RVA: 0x0003E9E7 File Offset: 0x0003CBE7
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			if (e.FilterString.Equals(""))
			{
				e.AppendQueryFilter(string.Format(" FWORKCENTERTYPE = '{0}' ", this.workCenterType));
			}
		}

		// Token: 0x06000527 RID: 1319 RVA: 0x0003EA18 File Offset: 0x0003CC18
		public override void BeforeFilterGridF7Select(BeforeFilterGridF7SelectEventArgs e)
		{
			base.BeforeFilterGridF7Select(e);
			string filter = e.ListFilterParameter.Filter;
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x0003EA30 File Offset: 0x0003CC30
		public override void FormatCellValue(FormatCellValueArgs args)
		{
			base.FormatCellValue(args);
			string a;
			if ((a = args.Header.Key.ToUpper()) != null)
			{
				if (!(a == "FRESEFFECTDATE") && !(a == "FRESEXPIREDATE"))
				{
					return;
				}
				if (args.Value != null && !string.IsNullOrWhiteSpace(args.Value.ToString()))
				{
					args.FormateValue = Convert.ToDateTime(args.Value).ToShortDateString();
				}
			}
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x0003EAA8 File Offset: 0x0003CCA8
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString().ToUpper()) != null)
			{
				if (!(a == "UNAUDIT"))
				{
					return;
				}
				if (this.firstDoOperation && !this.CanUnAudit(e))
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x0003EB00 File Offset: 0x0003CD00
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			this.firstDoOperation = true;
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBSHAREDPL"))
				{
					return;
				}
				this.ShowSharedPL(e);
			}
		}

		// Token: 0x0600052B RID: 1323 RVA: 0x0003EC0C File Offset: 0x0003CE0C
		private bool CanUnAudit(BeforeDoOperationEventArgs e)
		{
			this.firstDoOperation = false;
			bool bCheckResult = false;
			this.lstCurSelId = (from p in this.ListView.SelectedRowsInfo
			select Convert.ToInt32(p.PrimaryKeyValue)).Distinct<int>().ToList<int>();
			if (this.lstCurSelId.Count <= 0)
			{
				return bCheckResult;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (int num in this.lstCurSelId)
			{
				stringBuilder.AppendFormat("{0},", num.ToString());
			}
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			List<DynamicObject> list = ENGServiceHelper.GetUsingWCRoute(base.Context, this.lstCurSelId).ToList<DynamicObject>();
			if (list != null && list.Count > 0)
			{
				bCheckResult = (list.Count <= 0);
				string text = string.Empty;
				StringBuilder stringBuilder2 = new StringBuilder();
				var source = (from p in this.ListView.SelectedRowsInfo
				select new
				{
					Id = Convert.ToInt32(p.PrimaryKeyValue),
					Number = p.Number
				}).Distinct();
				this.dctCurSelIdNum = source.ToDictionary(p => p.Id, p => p.Number);
				using (Dictionary<int, string>.KeyCollection.Enumerator enumerator2 = this.dctCurSelIdNum.Keys.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						int iFID = enumerator2.Current;
						IEnumerable<DynamicObject> source2 = from varItem in list
						where Convert.ToInt32(varItem["FWORKCENTERID"]).Equals(iFID)
						select varItem;
						text = string.Join<object>(",", (from p in source2
						select p["FNUMBER"]).Distinct<object>().ToList<object>());
						if (text.Length > 0)
						{
							stringBuilder2.AppendFormat(ResManager.LoadKDString("工作中心[{0}]已被工艺路线[{1}]引用,", "015072000012581", 7, new object[0]), this.dctCurSelIdNum[iFID], text);
							stringBuilder2.AppendLine();
						}
					}
				}
				stringBuilder2.Append(ResManager.LoadKDString("请确认是否反审核?", "015072000001792", 7, new object[0]));
				this.View.ShowMessage(stringBuilder2.ToString(), 4, delegate(MessageBoxResult result)
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
			else
			{
				bCheckResult = true;
			}
			return bCheckResult;
		}

		// Token: 0x0600052C RID: 1324 RVA: 0x0003EF18 File Offset: 0x0003D118
		private void InitFormTitle()
		{
			LocaleValue localeValue = new LocaleValue(ResManager.LoadKDString("柔性产线列表", "015072000011018", 7, new object[0]), base.Context.UserLocale.LCID);
			this.View.SetFormTitle(localeValue);
			this.View.SetInnerTitle(localeValue);
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x0003EF6C File Offset: 0x0003D16C
		private void InitCustomParameters()
		{
			Dictionary<string, object> customParameters = this.View.OpenParameter.GetCustomParameters();
			if (customParameters.ContainsKey("WorkCenterType"))
			{
				this.workCenterType = customParameters["WorkCenterType"].ToString();
			}
			else
			{
				if (this.View.ParentFormView != null)
				{
					Dictionary<string, object> customParameters2 = this.View.ParentFormView.OpenParameter.GetCustomParameters();
					if (customParameters2.ContainsKey("WorkCenterType"))
					{
						this.workCenterType = customParameters2["WorkCenterType"].ToString();
					}
				}
				customParameters.Add("WorkCenterType", this.workCenterType);
			}
			if (this.workCenterType.Equals("D") && !customParameters.ContainsKey("LayoutId"))
			{
				customParameters.Add("LayoutId", this.layOutId);
			}
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x0003F038 File Offset: 0x0003D238
		private void ShowSharedPL(BarItemClickEventArgs e)
		{
			if ("D" != this.workCenterType)
			{
				string text = ResManager.LoadKDString("请选择柔性产线进行操作！", "015072030035037", 7, new object[0]);
				this.View.ShowErrMessage(text, "", 0);
				return;
			}
			long num = 0L;
			if (this.ListView.CurrentSelectedRowInfo != null)
			{
				if (this.ListView.SelectedRowsInfo.Count == 0 || this.ListView.SelectedRowsInfo.Count > 1)
				{
					string text2 = ResManager.LoadKDString("请选择一条柔性产线进行操作！", "015072000014888", 7, new object[0]);
					this.View.ShowErrMessage(text2, "", 0);
					return;
				}
				string primaryKeyValue = this.ListView.SelectedRowsInfo.FirstOrDefault<ListSelectedRow>().PrimaryKeyValue;
				num = SFCSharedPLEntity.Instance.GetExistSharedPL(base.Context, Convert.ToInt64(primaryKeyValue));
			}
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.FormId = "SFC_SharedPLConfig";
			billShowParameter.ParentPageId = this.View.PageId;
			if (num > 0L)
			{
				billShowParameter.Status = 2;
				billShowParameter.PKey = Convert.ToString(num);
			}
			this.View.ShowForm(billShowParameter);
		}

		// Token: 0x0400023E RID: 574
		private const string FKey_WorkCenterID = "FWORKCENTERID";

		// Token: 0x0400023F RID: 575
		private const string FKey_FNumber = "FNUMBER";

		// Token: 0x04000240 RID: 576
		private const string FKey_FID = "FID";

		// Token: 0x04000241 RID: 577
		private bool firstDoOperation = true;

		// Token: 0x04000242 RID: 578
		private string workCenterType = "A";

		// Token: 0x04000243 RID: 579
		private string layOutId = "a40c32c9-c663-4389-aedd-99bbe73792a0";
	}
}
