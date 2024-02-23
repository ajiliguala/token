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

namespace Kingdee.K3.MFG.ENG.Business.PlugIn
{
	// Token: 0x020000BA RID: 186
	[Description("父线消耗子线物料查询")]
	public class MainPrdConsumeMaterialEdit : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000DCC RID: 3532 RVA: 0x000A0FBC File Offset: 0x0009F1BC
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
					long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>((DynamicObject)customParameters[text], "FUSEORGID", 0L);
					if (!this.orgIds.Contains(dynamicObjectItemValue))
					{
						this.orgIds.Add(dynamicObjectItemValue);
					}
					string item = string.Format("{0}", DataEntityExtend.GetDynamicObjectItemValue<string>((DynamicObject)customParameters[text], "FNUMBER", null));
					if (!this.bufferNos.Contains(item))
					{
						this.bufferNos.Add(item);
					}
				}
			}
		}

		// Token: 0x06000DCD RID: 3533 RVA: 0x000A10BC File Offset: 0x0009F2BC
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

		// Token: 0x06000DCE RID: 3534 RVA: 0x000A1170 File Offset: 0x0009F370
		private void InitData()
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			Dictionary<string, long> dictionary = new Dictionary<string, long>();
			string text = "select t1.FNUMBER,t3.FProductLineId from T_ENG_BUFFER t1\r\n                                 inner join Table(fn_StrSplit(@bufferNo,',',2)) t2 on t2.FId=t1.FNUMBER\r\n                                 inner join T_ENG_LineRelationship t3 on t1.FID=t3.FID\r\n                              where t3.FLINEREALATIONSHIP='A'";
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, text, new List<SqlParam>
			{
				new SqlParam("@bufferNo", DbType.String, this.bufferNos.ToArray())
			}))
			{
				while (dataReader.Read())
				{
					if (!dictionary.Keys.Contains(dataReader[0].ToString()))
					{
						dictionary.Add(dataReader[0].ToString(), Convert.ToInt64(dataReader[1]));
					}
				}
			}
			Dictionary<long, List<long>> dictionary2 = new Dictionary<long, List<long>>();
			text = "select t1.FPRODUCTLINEID,t1.FMATERIALID from T_ENG_REPEATLINERELATEBOM t1\r\n                            inner join Table(fn_StrSplit(@FID,',',1)) t2 on t2.FId=t1.FPRODUCTLINEID and t1.FDOCUMENTSTATUS='C' \r\n                            union all\r\n                       select t3.FPRODUCTLINEID,t3.FMATERIALID from T_ENG_FLOWLINERELATEBOM t3\r\n                            inner join Table(fn_StrSplit(@FID,',',1)) t2 on t2.FId=t3.FPRODUCTLINEID and t3.FDOCUMENTSTATUS='C'";
			using (IDataReader dataReader2 = DBServiceHelper.ExecuteReader(base.Context, text, new List<SqlParam>
			{
				new SqlParam("@FID", 161, dictionary.Values.ToArray<long>())
			}))
			{
				while (dataReader2.Read())
				{
					long num = Convert.ToInt64(dataReader2[0]);
					if (!dictionary2.Keys.Contains(num))
					{
						dictionary2.Add(num, new List<long>
						{
							Convert.ToInt64(dataReader2[1])
						});
					}
					else
					{
						dictionary2[num].Add(Convert.ToInt64(dataReader2[1]));
					}
				}
			}
			Dictionary<string, List<long>> dictionary3 = new Dictionary<string, List<long>>();
			foreach (string key in dictionary.Keys)
			{
				long num2 = dictionary[key];
				if (dictionary2.Keys.Contains(num2))
				{
					dictionary3.Add(key, dictionary2[num2]);
				}
			}
			if (dictionary3.Count<KeyValuePair<string, List<long>>>() > 0)
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
					new SelectorItemInfo("FMATERIALID")
				};
				List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_BOM", list, text2, "");
				int num3 = 0;
				foreach (DynamicObject dynamicObject in baseBillInfo)
				{
					DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "UseOrgId", null);
					DynamicObjectCollection dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "BopEntity", null);
					DynamicObject dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "MATERIALID", null);
					string dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue3, "Number", null);
					using (List<DynamicObject>.Enumerator enumerator3 = this.list.GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							DynamicObject item = enumerator3.Current;
							if (dictionary3.Keys.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(item, "FNUMBER", null)) && DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectItemValue, "Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(item, "FUSEORGID", 0L))
							{
								var enumerable = from bop in dynamicObjectItemValue2
								where DataEntityExtend.GetDynamicObjectItemValue<long>(bop, "ProductLineId_Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(item, "FPRODUCTLINEID", 0L) && DataEntityExtend.GetDynamicObjectItemValue<bool>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(bop, "BopMaterialId", null), "MaterialProduce", null)[0], "IsProductLine", false)
								select new
								{
									P1 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(bop, "BopMaterialId", null),
									P2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(bop, "ProductLineId", null)
								};
								if (enumerable.Count() > 0)
								{
									LocaleValue localeValue = dynamicObjectItemValue["Name"] as LocaleValue;
									string text3 = localeValue[base.Context.UserLocale.LCID];
									string dynamicObjectItemValue5 = DataEntityExtend.GetDynamicObjectItemValue<string>(item, "FNUMBER", null);
									string dynamicObjectItemValue6 = DataEntityExtend.GetDynamicObjectItemValue<string>(item, "FNAME", null);
									foreach (var <>f__AnonymousType in enumerable)
									{
										if (dictionary3[dynamicObjectItemValue5].Contains(DataEntityExtend.GetDynamicObjectItemValue<long>(<>f__AnonymousType.P1, "Id", 0L)))
										{
											DynamicObject dynamicObject2 = new DynamicObject(entryEntity.DynamicObjectType);
											DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "UseOrgName", text3);
											DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "BufferNumber", dynamicObjectItemValue5);
											DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "BufferName", dynamicObjectItemValue6);
											localeValue = (<>f__AnonymousType.P2["Name"] as LocaleValue);
											DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "PrdLineNumber", DataEntityExtend.GetDynamicObjectItemValue<string>(<>f__AnonymousType.P2, "Number", null));
											DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "PrdLineName", localeValue[base.Context.UserLocale.LCID]);
											DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ParentMaterialNumber", dynamicObjectItemValue4);
											localeValue = (dynamicObjectItemValue3["Name"] as LocaleValue);
											DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ParentMaterialName", localeValue[base.Context.UserLocale.LCID]);
											DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ChildMaterialNumber", DataEntityExtend.GetDynamicObjectItemValue<string>(<>f__AnonymousType.P1, "Number", null));
											localeValue = (<>f__AnonymousType.P1["Name"] as LocaleValue);
											DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ChildMaterialName", localeValue[base.Context.UserLocale.LCID]);
											this.Model.CreateNewEntryRow(entryEntity, num3, dynamicObject2);
											num3++;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000DCF RID: 3535 RVA: 0x000A17F0 File Offset: 0x0009F9F0
		private void SortData()
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			int num = 1;
			IOrderedEnumerable<DynamicObject> orderedEnumerable = from item in entityDataObject
			orderby DataEntityExtend.GetDynamicObjectItemValue<string>(item, "UseOrgName", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "BufferNumber", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "PrdLineNumber", null)
			select item;
			foreach (DynamicObject dynamicObject in orderedEnumerable)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", num);
				num++;
			}
		}

		// Token: 0x04000662 RID: 1634
		private List<DynamicObject> list = new List<DynamicObject>();

		// Token: 0x04000663 RID: 1635
		private List<long> orgIds = new List<long>();

		// Token: 0x04000664 RID: 1636
		private List<string> bufferNos = new List<string>();
	}
}
