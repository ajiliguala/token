using System;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000017 RID: 23
	[HotUpdate]
	[Description("工艺参数表单插件")]
	public class TecParameterEdit : BaseControlEdit
	{
		// Token: 0x06000258 RID: 600 RVA: 0x0001C9C8 File Offset: 0x0001ABC8
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.routeCurrentEntryRow = (base.View.OpenParameter.GetCustomParameter("currentRow") as DynamicObject);
			this.documentStatus = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(base.View.OpenParameter.GetCustomParameter("documentStatus")) ? "" : Convert.ToString(base.View.OpenParameter.GetCustomParameter("documentStatus")));
			if (this.routeCurrentEntryRow != null)
			{
				this.prdOrgId = DataEntityExtend.GetDynamicObjectItemValue<long>(this.routeCurrentEntryRow, "ProOrgId_Id", 0L);
			}
			if (this.prdOrgId == 0L && this.routeCurrentEntryRow != null)
			{
				this.prdOrgId = DataEntityExtend.GetDynamicObjectItemValue<long>((this.routeCurrentEntryRow.Parent as DynamicObject).Parent as DynamicObject, "UseOrgId_Id", 0L);
			}
		}

		// Token: 0x06000259 RID: 601 RVA: 0x0001CAA0 File Offset: 0x0001ACA0
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (base.View.OpenParameter.Status == 1)
			{
				base.View.GetMainBarItem("tbDeleteBill").Enabled = false;
				base.View.GetBarItem<BarItemControl>("FEntity", "tbNewEntry").Enabled = false;
				base.View.GetBarItem<BarItemControl>("FEntity", "tbDeleteEntry").Enabled = false;
				base.View.GetBarItem<BarItemControl>("FEntity", "tbCopyEntry").Enabled = false;
				base.View.UpdateView("tbDeleteBill");
			}
			this.SetBillHeadFieldData();
			if (this.documentStatus.Equals("B") || this.documentStatus.Equals("C"))
			{
				base.View.GetMainBarItem("tbDeleteBill").Enabled = false;
				base.View.GetBarItem<BarItemControl>("FEntity", "tbNewEntry").Enabled = false;
				base.View.GetBarItem<BarItemControl>("FEntity", "tbDeleteEntry").Enabled = false;
				base.View.GetBarItem<BarItemControl>("FEntity", "tbCopyEntry").Enabled = false;
				base.View.UpdateView("tbDeleteBill");
			}
		}

		// Token: 0x0600025A RID: 602 RVA: 0x0001CC44 File Offset: 0x0001AE44
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBDELETEBILL"))
				{
					return;
				}
				base.View.ShowMessage(ResManager.LoadKDString("确定要删除本单据吗？", "015649000011023", 7, new object[0]), 4, delegate(MessageBoxResult result)
				{
					if (result == 6)
					{
						this.View.Model.Delete(null);
						this.View.Model.DataChanged = false;
						this.View.Close();
						return;
					}
					e.Cancel = true;
				}, "", 0);
			}
		}

		// Token: 0x0600025B RID: 603 RVA: 0x0001CCCE File Offset: 0x0001AECE
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
		}

		// Token: 0x0600025C RID: 604 RVA: 0x0001CCD7 File Offset: 0x0001AED7
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			base.AfterCreateNewEntryRow(e);
			this.CreateEntryRowIdFieldValue(e);
		}

		// Token: 0x0600025D RID: 605 RVA: 0x0001CCE8 File Offset: 0x0001AEE8
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBINPUTPARAMVALUE"))
				{
					return;
				}
				DynamicObject entryCurrentRow = this.GetEntryCurrentRow();
				this.OpenJsonForm(entryCurrentRow);
			}
		}

		// Token: 0x0600025E RID: 606 RVA: 0x0001CD27 File Offset: 0x0001AF27
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
		}

		// Token: 0x0600025F RID: 607 RVA: 0x0001CD30 File Offset: 0x0001AF30
		public override void AfterF7Select(AfterF7SelectEventArgs e)
		{
			base.AfterF7Select(e);
		}

		// Token: 0x06000260 RID: 608 RVA: 0x0001CD39 File Offset: 0x0001AF39
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
		}

		// Token: 0x06000261 RID: 609 RVA: 0x0001CD42 File Offset: 0x0001AF42
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
		}

		// Token: 0x06000262 RID: 610 RVA: 0x0001CD4B File Offset: 0x0001AF4B
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
		}

		// Token: 0x06000263 RID: 611 RVA: 0x0001CDA4 File Offset: 0x0001AFA4
		private void OpenJsonForm(DynamicObject currentRow)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.FormId = "ENG_TecParamJsonInput";
			dynamicFormShowParameter.CustomComplexParams.Add("currentRow", currentRow);
			dynamicFormShowParameter.CustomComplexParams.Add("documentStatus", this.documentStatus);
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult returnValue)
			{
				if (returnValue.ReturnData != null)
				{
					string text = returnValue.ReturnData.ToString();
					currentRow["ParamValue"] = text;
					this.View.UpdateView("FEntity");
				}
			});
		}

		// Token: 0x06000264 RID: 612 RVA: 0x0001CE37 File Offset: 0x0001B037
		private void CreateEntryRowIdFieldValue(CreateNewEntryEventArgs e)
		{
		}

		// Token: 0x06000265 RID: 613 RVA: 0x0001CE3C File Offset: 0x0001B03C
		private void SetBillHeadFieldData()
		{
			if (this.prdOrgId != 0L)
			{
				base.View.Model.SetValue("FOrgId", this.prdOrgId);
			}
			if (this.routeCurrentEntryRow != null)
			{
				base.View.Model.SetValue("FRouteEntryId", DataEntityExtend.GetDynamicObjectItemValue<long>(this.routeCurrentEntryRow, "Id", 0L));
				base.View.Model.SetValue("FSeqNumber", DataEntityExtend.GetDynamicObjectItemValue<string>(this.routeCurrentEntryRow.Parent as DynamicObject, "SeqNumber", null));
				base.View.Model.SetValue("FOperNumber", DataEntityExtend.GetDynamicObjectItemValue<string>(this.routeCurrentEntryRow, "OperNumber", null));
				base.View.Model.SetValue("FProcessId", DataEntityExtend.GetDynamicObjectItemValue<long>(this.routeCurrentEntryRow, "ProcessId_Id", 0L));
				DynamicObject dynamicObject = (this.routeCurrentEntryRow.Parent as DynamicObject).Parent as DynamicObject;
				base.View.Model.SetValue("FRouteNo", Convert.ToString(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "Number", null)));
				LocaleValue dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>(dynamicObject, "Name", null);
				base.View.Model.SetValue("FRouteName", Convert.ToString(dynamicObjectItemValue.GetString(base.Context.UserLocale.LCID)));
			}
		}

		// Token: 0x06000266 RID: 614 RVA: 0x0001CFAC File Offset: 0x0001B1AC
		private DynamicObject GetEntryCurrentRow()
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FEntity");
			return this.Model.GetEntityDataObject(entryEntity, entryCurrentRowIndex);
		}

		// Token: 0x06000267 RID: 615 RVA: 0x0001CFF0 File Offset: 0x0001B1F0
		private DynamicObject GetIndexEntryRow(int rowIndex)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			return this.Model.GetEntityDataObject(entryEntity, rowIndex);
		}

		// Token: 0x06000268 RID: 616 RVA: 0x0001D024 File Offset: 0x0001B224
		private bool CheckFirstRow()
		{
			return this.Model.GetEntryCurrentRowIndex("FEntity") == 0;
		}

		// Token: 0x04000134 RID: 308
		private DynamicObject routeCurrentEntryRow;

		// Token: 0x04000135 RID: 309
		private long prdOrgId;

		// Token: 0x04000136 RID: 310
		private string documentStatus;

		// Token: 0x04000137 RID: 311
		private bool IsNeedReload;
	}
}
