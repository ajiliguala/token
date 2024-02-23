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
	// Token: 0x02000085 RID: 133
	[Description("子线供给父线物料查询--缓冲区")]
	public class ChildPrdSupplyMaterialEdit : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000A32 RID: 2610 RVA: 0x000769BC File Offset: 0x00074BBC
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
					string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>((DynamicObject)customParameters[text], "FPRODUCTLINEID", null);
					string dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<string>((DynamicObject)customParameters[text], "FUSEORGID", null);
					if (!this.prdList.Contains(dynamicObjectItemValue))
					{
						this.prdList.Add(dynamicObjectItemValue);
					}
					if (!this.useOrgList.Contains(dynamicObjectItemValue2))
					{
						this.useOrgList.Add(dynamicObjectItemValue2);
					}
					string item = string.Format("{0}", DataEntityExtend.GetDynamicObjectItemValue<string>((DynamicObject)customParameters[text], "FNUMBER", null));
					if (!this.bufferNos.Contains(item))
					{
						this.bufferNos.Add(item);
					}
				}
			}
		}

		// Token: 0x06000A33 RID: 2611 RVA: 0x00076AF0 File Offset: 0x00074CF0
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

		// Token: 0x06000A34 RID: 2612 RVA: 0x00076B84 File Offset: 0x00074D84
		private void InitData()
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			int num = 0;
			if (this.prdList.Count<string>() <= 0)
			{
				return;
			}
			Dictionary<string, List<long>> dictionary = new Dictionary<string, List<long>>();
			string text = "select t1.FNUMBER,t3.FProductLineId from T_ENG_BUFFER t1\r\n                                 inner join Table(fn_StrSplit(@bufferNo,',',2)) t2 on t2.FId=t1.FNUMBER\r\n                                 inner join T_ENG_LineRelationship t3 on t1.FID=t3.FID\r\n                              where t3.FLINEREALATIONSHIP='B'";
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, text, new List<SqlParam>
			{
				new SqlParam("@bufferNo", DbType.String, this.bufferNos.ToArray())
			}))
			{
				while (dataReader.Read())
				{
					if (!dictionary.Keys.Contains(dataReader[0].ToString()))
					{
						dictionary.Add(dataReader[0].ToString(), new List<long>
						{
							Convert.ToInt64(dataReader[1])
						});
					}
					else
					{
						dictionary[dataReader[0].ToString()].Add(Convert.ToInt64(dataReader[1]));
					}
				}
			}
			string text2 = string.Format("FUSEORGID in ({0}) and FPRODUCTLINEID in ({1})", string.Join(",", this.useOrgList.ToArray()), string.Join(",", this.prdList.ToArray()));
			List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_RepeatLineRelateBom", null, text2, "");
			List<DynamicObject> baseBillInfo2 = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_FlowLineRelateBom", null, text2, "");
			var first = from o in baseBillInfo
			select new
			{
				P1 = DataEntityExtend.GetDynamicObjectItemValue<long>(o, "MaterialId_Id", 0L),
				P2 = DataEntityExtend.GetDynamicObjectItemValue<long>(o, "UseOrgId_Id", 0L)
			};
			var second = from o in baseBillInfo2
			select new
			{
				P1 = DataEntityExtend.GetDynamicObjectItemValue<long>(o, "MaterialId_Id", 0L),
				P2 = DataEntityExtend.GetDynamicObjectItemValue<long>(o, "UseOrgId_Id", 0L)
			};
			var source = first.Concat(second);
			string text3 = string.Empty;
			if (this.useOrgList.Count<string>() > 0)
			{
				text3 = string.Format("FUseOrgId in ({0}) and FDocumentStatus='C'", string.Join(",", this.useOrgList.ToArray()));
				List<SelectorItemInfo> list = new List<SelectorItemInfo>
				{
					new SelectorItemInfo("FUseOrgId"),
					new SelectorItemInfo("FBopMaterialId"),
					new SelectorItemInfo("FProductLineId"),
					new SelectorItemInfo("FNumber"),
					new SelectorItemInfo("FMATERIALID")
				};
				List<DynamicObject> baseBillInfo3 = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_BOM", list, text3, "");
				foreach (DynamicObject dynamicObject in baseBillInfo3)
				{
					DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "UseOrgId", null);
					IEnumerable<DynamicObject> source2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "BopEntity", null).AsEnumerable<DynamicObject>();
					string dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "Number", null);
					foreach (DynamicObject dynamicObject2 in this.list)
					{
						if (DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectItemValue, "Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "FUSEORGID", 0L))
						{
							var enumerable = from bop in source2
							select new
							{
								P1 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(bop, "BopMaterialId", null),
								P2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(bop, "ProductLineId", null)
							};
							LocaleValue localeValue = dynamicObjectItemValue["Name"] as LocaleValue;
							string text4 = localeValue[base.Context.UserLocale.LCID];
							string dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "FNUMBER", null);
							string dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "FNAME", null);
							foreach (var <>f__AnonymousType in enumerable)
							{
								if (dictionary.Keys.Contains(dynamicObjectItemValue3))
								{
									List<long> list2 = dictionary[dynamicObjectItemValue3];
									if (list2.Contains(DataEntityExtend.GetDynamicObjectItemValue<long>(<>f__AnonymousType.P2, "Id", 0L)) && source.Contains(new
									{
										P1 = DataEntityExtend.GetDynamicObjectItemValue<long>(<>f__AnonymousType.P1, "Id", 0L),
										P2 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "FUSEORGID", 0L)
									}))
									{
										DynamicObject dynamicObject3 = new DynamicObject(entryEntity.DynamicObjectType);
										DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "UseOrgName", text4);
										DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BufferNumber", dynamicObjectItemValue3);
										DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BufferName", dynamicObjectItemValue4);
										string dynamicObjectItemValue5 = DataEntityExtend.GetDynamicObjectItemValue<string>(<>f__AnonymousType.P1, "Number", null);
										localeValue = (<>f__AnonymousType.P1["Name"] as LocaleValue);
										DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ChildMaterialNumber", dynamicObjectItemValue5);
										DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ChildMaterialName", localeValue[base.Context.UserLocale.LCID]);
										string dynamicObjectItemValue6 = DataEntityExtend.GetDynamicObjectItemValue<string>(<>f__AnonymousType.P2, "Number", null);
										localeValue = (<>f__AnonymousType.P2["Name"] as LocaleValue);
										DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ParentPrdLineNumber", dynamicObjectItemValue6);
										DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ParentPrdLineName", localeValue[base.Context.UserLocale.LCID]);
										DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BOMNumber", dynamicObjectItemValue2);
										this.Model.CreateNewEntryRow(entryEntity, num, dynamicObject3);
										num++;
									}
								}
							}
						}
					}
				}
				return;
			}
		}

		// Token: 0x06000A35 RID: 2613 RVA: 0x00077174 File Offset: 0x00075374
		private void SortData()
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			int num = 1;
			IOrderedEnumerable<DynamicObject> orderedEnumerable = from item in entityDataObject
			orderby DataEntityExtend.GetDynamicObjectItemValue<string>(item, "UseOrgName", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "BufferNumber", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "BOMNumber", null), DataEntityExtend.GetDynamicObjectItemValue<string>(item, "ChildMaterialNumber", null)
			select item;
			foreach (DynamicObject dynamicObject in orderedEnumerable)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", num);
				num++;
			}
		}

		// Token: 0x040004DE RID: 1246
		private List<DynamicObject> list = new List<DynamicObject>();

		// Token: 0x040004DF RID: 1247
		private List<string> prdList = new List<string>();

		// Token: 0x040004E0 RID: 1248
		private List<string> useOrgList = new List<string>();

		// Token: 0x040004E1 RID: 1249
		private List<string> bufferNos = new List<string>();
	}
}
