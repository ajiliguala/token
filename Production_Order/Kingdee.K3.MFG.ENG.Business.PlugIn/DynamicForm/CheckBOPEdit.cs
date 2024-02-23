using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000083 RID: 131
	[Description("BOP检查")]
	public class CheckBOPEdit : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000A1D RID: 2589 RVA: 0x0007599C File Offset: 0x00073B9C
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			Dictionary<string, object> customParameters = e.Paramter.GetCustomParameters();
			foreach (string text in customParameters.Keys)
			{
				int num = 0;
				if (int.TryParse(text, out num))
				{
					this.list.Add((DynamicObject)customParameters[text]);
					long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>((DynamicObject)customParameters[text], "USEORGID_Id", 0L);
					if (!this.orgIds.Contains(dynamicObjectItemValue))
					{
						this.orgIds.Add(dynamicObjectItemValue);
					}
					long dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>((DynamicObject)customParameters[text], "MATERIALID_Id", 0L);
					if (!this.materialIds.Contains(dynamicObjectItemValue2))
					{
						this.materialIds.Add(dynamicObjectItemValue2);
					}
				}
			}
		}

		// Token: 0x06000A1E RID: 2590 RVA: 0x00075A94 File Offset: 0x00073C94
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (this.list.Count<DynamicObject>() > 0)
			{
				this.InitData();
				this.SortData();
				this.View.UpdateView("FEntity");
			}
		}

		// Token: 0x06000A1F RID: 2591 RVA: 0x00075AEC File Offset: 0x00073CEC
		private void InitData()
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.BusinessInfo = formMetaData.BusinessInfo;
			if (this.orgIds.Count<long>() > 0)
			{
				queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
				{
					TableName = "TABLE(fn_StrSplit(@OrgIds,',',1))",
					TableNameAs = "SPO",
					FieldName = "FID",
					ScourceKey = "FUseOrgId"
				});
				queryBuilderParemeter.SqlParams.Add(new SqlParam("@OrgIds", 161, this.orgIds.Distinct<long>().ToArray<long>()));
				queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
				{
					TableName = "TABLE(fn_StrSplit(@MtrlIds,',',1))",
					TableNameAs = "SPM",
					FieldName = "FID",
					ScourceKey = "FMATERIALID"
				});
				queryBuilderParemeter.SqlParams.Add(new SqlParam("@MtrlIds", 161, this.materialIds.Distinct<long>().ToArray<long>()));
				queryBuilderParemeter.FilterClauseWihtKey = "FDocumentStatus = 'C'";
				List<string> list = new List<string>
				{
					"FID",
					"FUseOrgId",
					"FProductLineId",
					"FMATERIALID",
					"FBopMaterialId",
					"FNumber"
				};
				DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, formMetaData.BusinessInfo.GetSubBusinessInfo(list).GetDynamicObjectType(), queryBuilderParemeter);
				int num = 0;
				foreach (DynamicObject dynamicObject in this.list)
				{
					int num2 = 0;
					foreach (DynamicObject dynamicObject2 in array)
					{
						DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject2, "UseOrgId", null);
						if (DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectItemValue, "Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "USEORGID_Id", 0L) && DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "MATERIALID_Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "MATERIALID_Id", 0L))
						{
							long prdLineId = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "PRODUCTLINEID_Id", 0L);
							IEnumerable<int> source = from b in DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject2, "BopEntity", null)
							where DataEntityExtend.GetDynamicObjectItemValue<long>(b, "ProductLineId_Id", 0L) == prdLineId
							select 1;
							if (source.Count<int>() > 0)
							{
								num2++;
								break;
							}
						}
					}
					DynamicObject dynamicObject3 = new DynamicObject(entryEntity.DynamicObjectType);
					DynamicObject dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "USEORGID", null);
					LocaleValue localeValue = dynamicObjectItemValue2["Name"] as LocaleValue;
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "UseOrgName", localeValue[base.Context.UserLocale.LCID]);
					DynamicObject dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "MATERIALID", null);
					string dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue3, "Number", null);
					localeValue = (dynamicObjectItemValue3["Name"] as LocaleValue);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "MaterialNumber", dynamicObjectItemValue4);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "MaterialName", localeValue[base.Context.UserLocale.LCID]);
					DynamicObject dynamicObjectItemValue5 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "ProductLineId", null);
					string dynamicObjectItemValue6 = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue5, "Number", null);
					localeValue = (dynamicObjectItemValue5["Name"] as LocaleValue);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "PrdLineNumber", dynamicObjectItemValue6);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "PrdLineName", localeValue[base.Context.UserLocale.LCID]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "IsDefineBOP", num2 > 0);
					this.Model.CreateNewEntryRow(entryEntity, num, dynamicObject3);
					num++;
				}
				return;
			}
		}

		// Token: 0x06000A20 RID: 2592 RVA: 0x00075F48 File Offset: 0x00074148
		private void SortData()
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			int num = 1;
			IOrderedEnumerable<DynamicObject> orderedEnumerable = from item in entityDataObject
			orderby DataEntityExtend.GetDynamicObjectItemValue<string>(item, "UseOrgName", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "PrdLineNumber", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "MaterialNumber", null)
			select item;
			foreach (DynamicObject dynamicObject in orderedEnumerable)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", num);
				num++;
			}
		}

		// Token: 0x040004CD RID: 1229
		private List<DynamicObject> list = new List<DynamicObject>();

		// Token: 0x040004CE RID: 1230
		private List<long> orgIds = new List<long>();

		// Token: 0x040004CF RID: 1231
		private List<long> materialIds = new List<long>();
	}
}
