using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000084 RID: 132
	[Description("子线物料供给父线查询--生产线与物料关系")]
	public class ChildPrdOfferMaterialEdit : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000A26 RID: 2598 RVA: 0x00076064 File Offset: 0x00074264
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			Dictionary<string, object> customParameters = e.Paramter.GetCustomParameters();
			foreach (string text in customParameters.Keys)
			{
				int num = 0;
				if (int.TryParse(text, out num) && num != 0)
				{
					this.list.Add((DynamicObject)customParameters[text]);
					long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>((DynamicObject)customParameters[text], "USEORGID_Id", 0L);
					if (!this.orgIds.Contains(dynamicObjectItemValue))
					{
						this.orgIds.Add(dynamicObjectItemValue);
					}
					long dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>((DynamicObject)customParameters[text], "ProductLineId_Id", 0L);
					if (!this.prdLineIds.Contains(dynamicObjectItemValue2))
					{
						this.prdLineIds.Add(dynamicObjectItemValue2);
					}
				}
			}
		}

		// Token: 0x06000A27 RID: 2599 RVA: 0x00076160 File Offset: 0x00074360
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

		// Token: 0x06000A28 RID: 2600 RVA: 0x000762D0 File Offset: 0x000744D0
		private void InitData()
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			Dictionary<long, List<long>> dictionary = new Dictionary<long, List<long>>();
			if (this.prdLineIds.Count<long>() > 0)
			{
				string text = "select t4.FPRODUCTLINEID,t1.FPRODUCTLINEID from T_ENG_LineRelationship t1\r\n                                      inner join \r\n                                      (select t2.FID,t2.FPRODUCTLINEID from T_ENG_LineRelationship t2 inner join Table(fn_StrSplit(@FPrdLineId,',',1)) t3 on t3.FId=t2.FPRODUCTLINEID) t4 on t1.FID=t4.FID \r\n                                  where t1.FLINEREALATIONSHIP='B'";
				using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, text, new List<SqlParam>
				{
					new SqlParam("@FPrdLineId", 161, this.prdLineIds.ToArray())
				}))
				{
					while (dataReader.Read())
					{
						long num = Convert.ToInt64(dataReader[0]);
						if (!dictionary.Keys.Contains(num))
						{
							dictionary[num] = new List<long>
							{
								Convert.ToInt64(dataReader[1])
							};
						}
						else
						{
							dictionary[num].Add(Convert.ToInt64(dataReader[1]));
						}
					}
				}
			}
			if (dictionary.Values.Count<List<long>>() > 0)
			{
				string text2 = string.Empty;
				if (this.orgIds.Count<long>() <= 0)
				{
					return;
				}
				text2 = string.Format("FUseOrgId in ({0}) and FDocumentStatus='C'", string.Join<long>(",", this.orgIds.ToArray()));
				List<SelectorItemInfo> list = new List<SelectorItemInfo>
				{
					new SelectorItemInfo("FUseOrgId"),
					new SelectorItemInfo("FBopMaterialId"),
					new SelectorItemInfo("FProductLineId"),
					new SelectorItemInfo("FNumber")
				};
				List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_BOM", list, text2, "");
				int num2 = 0;
				foreach (DynamicObject dynamicObject in baseBillInfo)
				{
					DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "UseOrgId", null);
					DynamicObjectCollection dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "BopEntity", null);
					string text3 = string.Empty;
					foreach (DynamicObject dynamicObject2 in this.list)
					{
						if (DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectItemValue, "Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "USEORGID_Id", 0L))
						{
							var enumerable = from bop in dynamicObjectItemValue2
							select new
							{
								P1 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(bop, "BopMaterialId", null),
								P2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(bop, "ProductLineId", null)
							};
							foreach (var <>f__AnonymousType in enumerable)
							{
								long dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "ProductLineId_Id", 0L);
								if (dictionary.Keys.Contains(dynamicObjectItemValue3) && dictionary[dynamicObjectItemValue3].Contains(DataEntityExtend.GetDynamicObjectItemValue<long>(<>f__AnonymousType.P2, "Id", 0L)) && DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "MATERIALID_Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(<>f__AnonymousType.P1, "Id", 0L))
								{
									if (string.IsNullOrEmpty(text3))
									{
										text3 = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "Number", null);
									}
									DynamicObject dynamicObject3 = new DynamicObject(entryEntity.DynamicObjectType);
									LocaleValue localeValue = dynamicObjectItemValue["Name"] as LocaleValue;
									DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "UseOrgName", localeValue[base.Context.UserLocale.LCID]);
									DynamicObject dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject2, "ProductLineId", null);
									string dynamicObjectItemValue5 = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue4, "Number", null);
									localeValue = (dynamicObjectItemValue4["Name"] as LocaleValue);
									DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ChildPrdLineNumber", dynamicObjectItemValue5);
									DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ChildPrdLineName", localeValue[base.Context.UserLocale.LCID]);
									string dynamicObjectItemValue6 = DataEntityExtend.GetDynamicObjectItemValue<string>(<>f__AnonymousType.P1, "Number", null);
									localeValue = (<>f__AnonymousType.P1["Name"] as LocaleValue);
									DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ChildMaterialNumber", dynamicObjectItemValue6);
									DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ChildMaterialName", localeValue[base.Context.UserLocale.LCID]);
									string dynamicObjectItemValue7 = DataEntityExtend.GetDynamicObjectItemValue<string>(<>f__AnonymousType.P2, "Number", null);
									localeValue = (<>f__AnonymousType.P2["Name"] as LocaleValue);
									DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ParentPrdLineNumber", dynamicObjectItemValue7);
									DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ParentPrdLineName", localeValue[base.Context.UserLocale.LCID]);
									DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BomNumber", text3);
									this.Model.CreateNewEntryRow(entryEntity, num2, dynamicObject3);
									num2++;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000A29 RID: 2601 RVA: 0x00076838 File Offset: 0x00074A38
		private void SortData()
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			int num = 1;
			IOrderedEnumerable<DynamicObject> orderedEnumerable = from item in entityDataObject
			orderby DataEntityExtend.GetDynamicObjectItemValue<string>(item, "UseOrgName", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "ChildPrdLineNumber", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "ChildMaterialNumber", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "BomNumber", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "ParentPrdLineNumber", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "ParentMaterialNumber", null)
			select item;
			foreach (DynamicObject dynamicObject in orderedEnumerable)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", num);
				num++;
			}
		}

		// Token: 0x040004D4 RID: 1236
		private List<DynamicObject> list = new List<DynamicObject>();

		// Token: 0x040004D5 RID: 1237
		private List<long> orgIds = new List<long>();

		// Token: 0x040004D6 RID: 1238
		private List<long> prdLineIds = new List<long>();
	}
}
