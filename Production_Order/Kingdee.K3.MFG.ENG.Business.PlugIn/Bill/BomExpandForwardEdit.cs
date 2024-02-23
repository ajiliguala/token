using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.MFG.Common.BusinessEntity.ENG.BomExpand;
using Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Bill
{
	// Token: 0x02000051 RID: 81
	[Description("BOM正向展开表单插件,仅用来做测试用")]
	public class BomExpandForwardEdit : BomQueryForward
	{
		// Token: 0x06000609 RID: 1545 RVA: 0x00047934 File Offset: 0x00045B34
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.InitializeBomQueryOption();
		}

		// Token: 0x0600060A RID: 1546 RVA: 0x0004793C File Offset: 0x00045B3C
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
		}

		// Token: 0x0600060B RID: 1547 RVA: 0x00047945 File Offset: 0x00045B45
		public override void OnLoad(EventArgs e)
		{
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x00047948 File Offset: 0x00045B48
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FBOMEXPAND"))
				{
					return;
				}
				this.FillBomChildData();
			}
		}

		// Token: 0x0600060D RID: 1549 RVA: 0x00047980 File Offset: 0x00045B80
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string key;
			if ((key = e.BaseDataField.Key) != null)
			{
				if (!(key == "FWorkCalId"))
				{
					return;
				}
				e.ListFilterParameter.Filter = "";
				e.IsShowApproved = false;
			}
		}

		// Token: 0x0600060E RID: 1550 RVA: 0x000479C8 File Offset: 0x00045BC8
		protected override void FillBomChildData()
		{
			this.UpdateBomQueryOption();
			this.Model.DeleteEntryData("FBomResult");
			int focusRowIndex = this.View.GetControl<EntryGrid>("FBomSource").GetFocusRowIndex();
			List<DynamicObject> list = this.BuildBomExpandSourceData(focusRowIndex);
			if (list == null || list.Count <= 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请选中要展开的BOM数据！", "015072000001945", 7, new object[0]), "", 0);
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			List<DynamicObject> bomChildData = this.GetBomChildData(list, this.memBomExpandOption);
			this.Model.SetValue("FAlgorithmCost", stopwatch.ElapsedMilliseconds);
			this.View.UpdateView("FAlgorithmCost");
			stopwatch.Restart();
			this.Model.BeginIniti();
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBomResult");
			if (bomChildData != null && bomChildData.Count > 0)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				foreach (DynamicObject item in bomChildData)
				{
					entityDataObject.Add(item);
				}
			}
			this.Model.EndIniti();
			this.View.UpdateView("FBomResult");
			this.View.SetEntityFocusRow("FBomResult", 0);
			stopwatch.Reset();
		}

		// Token: 0x0600060F RID: 1551 RVA: 0x00047B38 File Offset: 0x00045D38
		protected override List<DynamicObject> BuildBomExpandSourceData(int iFocusRow)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			int[] selectedRows = this.View.GetControl<EntryGrid>("FBomSource").GetSelectedRows();
			foreach (int num in selectedRows)
			{
				DynamicObject entityDataObject = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FBomSource"), num);
				BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
				BomExpandView.BomExpandSource bomExpandSource = entityDataObject;
				bomForwardSourceDynamicRow.MaterialId_Id = bomExpandSource.MaterialId_Id;
				bomForwardSourceDynamicRow.BomId_Id = bomExpandSource.BomId_Id;
				bomForwardSourceDynamicRow.NeedQty = bomExpandSource.NeedQty;
				bomForwardSourceDynamicRow.NeedDate = bomExpandSource.NeedDate;
				bomForwardSourceDynamicRow.WorkCalId_Id = bomExpandSource.WorkCalId_Id;
				bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
				bomForwardSourceDynamicRow.SrcBillNo = bomExpandSource.SrcBillNo;
				bomForwardSourceDynamicRow.SrcEntryId = bomExpandSource.SrcEntryId;
				bomForwardSourceDynamicRow.SrcFormId_Id = bomExpandSource.SrcFormId_Id;
				bomForwardSourceDynamicRow.SrcInterId = bomExpandSource.SrcInterId;
				bomForwardSourceDynamicRow.SrcSeqNo = bomExpandSource.SrcSeqNo;
				list.Add(bomForwardSourceDynamicRow.DataEntity);
			}
			return list;
		}

		// Token: 0x06000610 RID: 1552 RVA: 0x00047C68 File Offset: 0x00045E68
		protected override List<DynamicObject> GetBomChildData(List<DynamicObject> lstExpandSource, MemBomExpandOption_ForPSV memBomExpandOption)
		{
			memBomExpandOption.IsConvertUnitQty = true;
			BomQueryServiceHelper.GetSimpleBomQueryForwardResult(this.View.Context, lstExpandSource, memBomExpandOption);
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_BomExpandBill", true) as FormMetadata;
			EntryEntity entryEntity = formMetadata.BusinessInfo.GetEntryEntity("FBomResult");
			return BomExpandDebugServiceHelper.LoadData(this.View.Context, entryEntity.DynamicObjectType, string.Format("select {0} from {1} ", entryEntity.EntryPkFieldName, entryEntity.TableName)).ToList<DynamicObject>();
		}

		// Token: 0x040002AE RID: 686
		private const string EntityKey_FBomResult = "FBomResult";

		// Token: 0x040002AF RID: 687
		private const string EntityKey_FBomSource = "FBomSource";

		// Token: 0x040002B0 RID: 688
		private const string FieldKey_FTableLastName = "FTableLastName";

		// Token: 0x040002B1 RID: 689
		private const string FieldKey_FAlgorithmCost = "FAlgorithmCost";

		// Token: 0x040002B2 RID: 690
		private const string FieldKey_FUiCost = "FUiCost";
	}
}
