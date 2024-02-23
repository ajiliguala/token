using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200009A RID: 154
	[Description("根据BOP生成生产线工位物料参数")]
	public class GenerateParaFromBopEdit : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000B04 RID: 2820 RVA: 0x0007E134 File Offset: 0x0007C334
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			OrgField orgField = this.View.BusinessInfo.GetField("FOrgId") as OrgField;
			ENGBillUtil.SetPrdOrgField(this.View.Context, orgField, "ENG_LineLocationBomPara", "fce8b1aca2144beeb3c6655eaf78bc34");
		}

		// Token: 0x06000B05 RID: 2821 RVA: 0x0007E17E File Offset: 0x0007C37E
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.InitProductLines();
			this.View.UpdateView("FEntity");
		}

		// Token: 0x06000B06 RID: 2822 RVA: 0x0007E1A0 File Offset: 0x0007C3A0
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FCONFIRM"))
				{
					if (!(a == "FCANCEL"))
					{
						return;
					}
					this.View.Close();
				}
				else if (this.ValidataData())
				{
					this.GetParaFromBop();
					return;
				}
			}
		}

		// Token: 0x06000B07 RID: 2823 RVA: 0x0007E1F8 File Offset: 0x0007C3F8
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			if (e.Field.Key.ToUpper().Equals("FORGID"))
			{
				this.Model.DeleteEntryData("FEntity");
				long value = MFGBillUtil.GetValue<long>(this.Model, "FOrgId", -1, 0L, null);
				if (value != 0L)
				{
					this.InitProductLines();
				}
				this.View.UpdateView("FEntity");
			}
		}

		// Token: 0x06000B08 RID: 2824 RVA: 0x0007E268 File Offset: 0x0007C468
		public override void EntryCellFocued(EntryCellFocuedEventArgs e)
		{
			base.EntryCellFocued(e);
			if (e.NewRow < 0)
			{
				return;
			}
			if (!StringUtils.EqualsIgnoreCase(e.NewFieldKey.ToString(), "FSelected"))
			{
				for (int i = 0; i < this.listProductLine.Count; i++)
				{
					this.Model.SetValue("FSelected", false, i);
				}
				this.Model.SetValue("FSelected", true, e.NewRow);
			}
		}

		// Token: 0x06000B09 RID: 2825 RVA: 0x0007E340 File Offset: 0x0007C540
		private void InitProductLines()
		{
			string value = MFGBillUtil.GetValue<string>(this.Model, "FOrgId", -1, null, null);
			this.listProductLine = this.GetProductLineList(value);
			if (this.listProductLine == null || this.listProductLine.Count == 0)
			{
				return;
			}
			this.listProductLine = (from prodLine in this.listProductLine
			where DataEntityExtend.GetDynamicObjectItemValue<string>(prodLine, "DOCUMENTSTATUS", null) == "C" && DataEntityExtend.GetDynamicObjectItemValue<string>(prodLine, "FORBIDSTATUS", null) == "A"
			orderby DataEntityExtend.GetDynamicObjectItemValue<string>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(prodLine, "DeptID", null), "Number", null), DataEntityExtend.GetDynamicObjectItemValue<string>(prodLine, "Number", null)
			select prodLine).ToList<DynamicObject>();
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			int num = 0;
			foreach (DynamicObject dynamicObject in this.listProductLine)
			{
				DynamicObject dynamicObject2 = new DynamicObject(entity.DynamicObjectType);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "Seq", num + 1);
				DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "DeptID", null);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "DepartId", Convert.ToInt64(dynamicObjectItemValue["Id"]));
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "DepartNumber", dynamicObjectItemValue["Number"]);
				LocaleValue localeValue = dynamicObjectItemValue["Name"] as LocaleValue;
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "DepartName", localeValue[base.Context.UserLocale.LCID]);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ProductLineId", Convert.ToInt64(dynamicObject["Id"]));
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ProductLineNumber", dynamicObject["Number"].ToString());
				localeValue = (dynamicObject["Name"] as LocaleValue);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ProductLineName", localeValue[base.Context.UserLocale.LCID]);
				this.Model.CreateNewEntryRow(entity, num, dynamicObject2);
				num++;
			}
			for (int i = 0; i < this.listProductLine.Count; i++)
			{
				this.Model.SetValue("FSelected", false, i);
			}
		}

		// Token: 0x06000B0A RID: 2826 RVA: 0x0007E5BC File Offset: 0x0007C7BC
		private List<DynamicObject> GetProductLineList(string useOrgId)
		{
			string text = string.Format(" FUseOrgId = {0} and FWorkCenterType != 'A'", useOrgId);
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FDeptID"),
				new SelectorItemInfo("FId"),
				new SelectorItemInfo("FNumber"),
				new SelectorItemInfo("FName"),
				new SelectorItemInfo("FDOCUMENTSTATUS"),
				new SelectorItemInfo("FFORBIDSTATUS")
			};
			return MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_WorkCenterBase", list, text, "");
		}

		// Token: 0x06000B0B RID: 2827 RVA: 0x0007E664 File Offset: 0x0007C864
		private bool ValidataData()
		{
			DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(this.View.Model.DataObject, "Entity", null);
			if (dynamicObjectItemValue.FirstOrDefault((DynamicObject o) => DataEntityExtend.GetDynamicObjectItemValue<bool>(o, "Selected", false)) == null)
			{
				this.View.ShowErrMessage("", this.CONSTMSG, 0);
				return false;
			}
			return true;
		}

		// Token: 0x06000B0C RID: 2828 RVA: 0x0007E6DC File Offset: 0x0007C8DC
		private void GetParaFromBop()
		{
			DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(this.View.Model.DataObject, "Entity", null);
			List<DynamicObject> listProLineSelected = (from o in dynamicObjectItemValue
			where DataEntityExtend.GetDynamicObjectItemValue<bool>(o, "Selected", false)
			select o).ToList<DynamicObject>();
			List<DynamicObject> bopFromBom = this.GetBopFromBom(listProLineSelected);
			List<DynamicObject> list = this.AbandonTheSameBop(bopFromBom);
			int num = bopFromBom.Count - list.Count;
			this.InsertBopIntoTable(list, ref num);
		}

		// Token: 0x06000B0D RID: 2829 RVA: 0x0007E790 File Offset: 0x0007C990
		private List<DynamicObject> GetBopFromBom(List<DynamicObject> listProLineSelected)
		{
			string text = string.Format(" t0.FUseOrgId = {0} and t0.FDocumentStatus = 'C' ", MFGBillUtil.GetValue<long>(this.Model, "FOrgId", -1, 0L, null));
			List<long> productLineIds = (from c in listProLineSelected
			select Convert.ToInt64(c["ProductLineId"])).ToList<long>();
			List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_BOM", null, text, "");
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
			EntryEntity entryEntity = formMetaData.BusinessInfo.GetEntryEntity("FBopEntity");
			DynamicObjectCollection dynamicObjectCollection = new DynamicObjectCollection(entryEntity.DynamicObjectType, null);
			for (int i = baseBillInfo.Count - 1; i >= 0; i--)
			{
				DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(baseBillInfo[i], "TreeEntity", null);
				DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(baseBillInfo[i], "BopEntity", null);
				long num = (long)baseBillInfo[i]["UseOrgId_Id"];
				if (dynamicObjectItemValue.Count > 0)
				{
					DynamicObject bopExpandData = this.GetBopExpandData(baseBillInfo[i], dynamicObjectItemValue.ToList<DynamicObject>());
					DynamicObjectCollection dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(bopExpandData, "BopExpandResult", null);
					foreach (DynamicObject dynamicObject in dynamicObjectItemValue2)
					{
						if ((long)dynamicObject["BomLevel"] != 0L && !(bool)dynamicObject["IsProductLine"])
						{
							DynamicObject dynamicObject2 = new DynamicObject(entryEntity.DynamicObjectType);
							dynamicObject2["ProductLineId_Id"] = dynamicObject["ResultProductLineId_Id"];
							dynamicObject2["PrdLineLocId_Id"] = dynamicObject["ResultProductLocId_Id"];
							dynamicObject2["BopMaterialId_Id"] = dynamicObject["MaterialId_Id"];
							dynamicObject2["BopBaseUnitID_Id"] = dynamicObject["BaseUnitId_Id"];
							dynamicObjectCollection.Add(dynamicObject2);
						}
					}
				}
			}
			IEnumerable<DynamicObject> source = (from bop in dynamicObjectCollection
			where productLineIds.Contains((long)bop["ProductLineId_Id"])
			select bop).Distinct(new MyComparer());
			return source.ToList<DynamicObject>();
		}

		// Token: 0x06000B0E RID: 2830 RVA: 0x0007E9DC File Offset: 0x0007CBDC
		private List<DynamicObject> AbandonTheSameBop(List<DynamicObject> bopData)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FUSEORGID"),
				new SelectorItemInfo("FPRODUCTLINEID"),
				new SelectorItemInfo("FLINELOCATION"),
				new SelectorItemInfo("FMATERIALID")
			};
			foreach (DynamicObject dynamicObject in bopData)
			{
				string text = string.Format(" t0.FUSEORGID={0} and t0.FPRODUCTLINEID={1} and t0.FLINELOCATION={2} and t0.FMATERIALID={3} ", new object[]
				{
					MFGBillUtil.GetValue<long>(this.Model, "FOrgId", -1, 0L, null),
					DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "ProductLineId_Id", 0L),
					DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "PrdLineLocId_Id", 0L),
					DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "BopMaterialId_Id", 0L)
				});
				List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_LineLocationBomPara", list2, text, "");
				if (baseBillInfo.Count == 0 || baseBillInfo == null)
				{
					list.Add(dynamicObject);
				}
			}
			return list;
		}

		// Token: 0x06000B0F RID: 2831 RVA: 0x0007EB20 File Offset: 0x0007CD20
		private void InsertBopIntoTable(List<DynamicObject> lastBopData, ref int number)
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, "ENG_LineLocationBomPara", true) as FormMetadata;
			DynamicObjectType dynamicObjectType = formMetadata.BusinessInfo.GetDynamicObjectType();
			List<DynamicObject> list = new List<DynamicObject>();
			foreach (DynamicObject dynamicObject in lastBopData)
			{
				DynamicObject dynamicObject2 = new DynamicObject(dynamicObjectType);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "CreateOrgId_Id", MFGBillUtil.GetValue<long>(this.Model, "FOrgId", -1, 0L, null));
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "UseOrgId_Id", MFGBillUtil.GetValue<long>(this.Model, "FOrgId", -1, 0L, null));
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ProductLineId_Id", dynamicObject["ProductLineId_Id"]);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "LineLocation_Id", dynamicObject["PrdLineLocId_Id"]);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "MaterialId_Id", dynamicObject["BopMaterialId_Id"]);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "UnitID_Id", dynamicObject["BopBaseUnitID_Id"]);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "DataSource", 'B');
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "PickingNumber", 'A');
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "SendInterval", 1);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "StorageRation", 0);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "IntegerMultiple", 0);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "DocumentStatus", 'A');
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ForbidStatus", 'A');
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "CreatorId_Id", base.Context.UserId);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "CreateDate", TimeServiceHelper.GetSystemDateTime(base.Context));
				list.Add(dynamicObject2);
			}
			if (list.Count > 0)
			{
				BusinessDataServiceHelper.Save(base.Context, list.ToArray());
			}
			this.View.ShowMessage(string.Format(ResManager.LoadKDString("生成成功，共生成{0}条数据，忽略{1}条数据！", "015072000017276", 7, new object[0]), lastBopData.Count, number), 0);
		}

		// Token: 0x06000B10 RID: 2832 RVA: 0x0007ED6C File Offset: 0x0007CF6C
		private DynamicObject GetBopExpandData(DynamicObject bomData, List<DynamicObject> bopDatas)
		{
			BopExpandOption bopExpandOption = new BopExpandOption();
			bopExpandOption.BopExpandId = SequentialGuid.NewGuid().ToString();
			bopExpandOption.ExpandVirtualMaterial = true;
			bopExpandOption.DeleteVirtualMaterial = false;
			bopExpandOption.DeleteBomIdAffectPlan = false;
			bopExpandOption.Mode = 1;
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BopExpandBill");
			EntryEntity entryEntity = formMetaData.BusinessInfo.GetEntryEntity("FBopSource");
			List<DynamicObject> list = new List<DynamicObject>();
			DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(this.View.Context);
			foreach (DynamicObject dynamicObject in bopDatas)
			{
				DynamicObject dynamicObject2 = new DynamicObject(entryEntity.DynamicObjectType);
				dynamicObject2["RowId"] = SequentialGuid.NewGuid().ToString();
				dynamicObject2["SrcInterId"] = 0;
				dynamicObject2["SrcEntryId"] = 0;
				dynamicObject2["MaterialId_Id"] = bomData["MATERIALID_Id"];
				dynamicObject2["BomId_Id"] = bomData["Id"];
				dynamicObject2["ProductLineId_Id"] = dynamicObject["ProductLineId_Id"];
				dynamicObject2["ProductLocId_Id"] = dynamicObject["PrdLineLocId_Id"];
				dynamicObject2["UnitId_Id"] = dynamicObject["BopUnitId_Id"];
				dynamicObject2["BaseUnitId_Id"] = dynamicObject["BopBaseUnitID_Id"];
				dynamicObject2["NeedQty"] = 0;
				dynamicObject2["NeedDate"] = systemDateTime.Date;
				list.Add(dynamicObject2);
			}
			return BopExpandServiceHelper.ExpandBopForward(base.Context, list, bopExpandOption);
		}

		// Token: 0x04000528 RID: 1320
		private string CONSTMSG = ResManager.LoadKDString("必须至少选择一条生产线!", "015072000017275", 7, new object[0]);

		// Token: 0x04000529 RID: 1321
		private List<DynamicObject> listProductLine = new List<DynamicObject>();
	}
}
