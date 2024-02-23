using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000AF RID: 175
	[Description("缓冲区物料查询")]
	public class QueryMaterialFromBufferEdit : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000CD3 RID: 3283 RVA: 0x000980E0 File Offset: 0x000962E0
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
					this.prdList.Add(DataEntityExtend.GetDynamicObjectItemValue<string>((DynamicObject)customParameters[text], "FPRODUCTLINEID", null));
				}
			}
		}

		// Token: 0x06000CD4 RID: 3284 RVA: 0x00098184 File Offset: 0x00096384
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

		// Token: 0x06000CD5 RID: 3285 RVA: 0x00098268 File Offset: 0x00096468
		private void InitData()
		{
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			if (this.prdList.Count <= 0)
			{
				return;
			}
			string text = string.Format("FPRODUCTLINEID in ({0})", string.Join(",", this.prdList.ToArray()));
			List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_FlowLineRelateBom", null, text, "");
			List<DynamicObject> baseBillInfo2 = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_RepeatLineRelateBom", null, text, "");
			int num = 0;
			using (List<DynamicObject>.Enumerator enumerator = this.list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DynamicObject item = enumerator.Current;
					IEnumerable<DynamicObject> enumerable = from a in baseBillInfo
					where DataEntityExtend.GetDynamicObjectItemValue<long>(a, "UseOrgId_Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(item, "FUSEORGID", 0L) && DataEntityExtend.GetDynamicObjectItemValue<long>(a, "ProductLineId_Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(item, "FPRODUCTLINEID", 0L)
					select a;
					if (enumerable.Count<DynamicObject>() > 0)
					{
						using (IEnumerator<DynamicObject> enumerator2 = enumerable.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								DynamicObject dynamicObject = enumerator2.Current;
								DynamicObject dynamicObject2 = new DynamicObject(entryEntity.DynamicObjectType);
								DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "UseOrgId", null);
								LocaleValue localeValue = dynamicObjectItemValue["Name"] as LocaleValue;
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "UseOrgName", localeValue[base.Context.UserLocale.LCID]);
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "BufferNumber", DataEntityExtend.GetDynamicObjectItemValue<string>(item, "FNUMBER", null));
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "BufferName", DataEntityExtend.GetDynamicObjectItemValue<string>(item, "FNAME", null));
								DynamicObject dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "MaterialId", null);
								localeValue = (dynamicObjectItemValue2["Name"] as LocaleValue);
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "MaterialNumber", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue2, "Number", null));
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "MaterialName", localeValue[base.Context.UserLocale.LCID]);
								DynamicObject dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "ProductLineId", null);
								localeValue = (dynamicObjectItemValue3["Name"] as LocaleValue);
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "PrdLineNumber", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue3, "Number", null));
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "PrdLineName", localeValue[base.Context.UserLocale.LCID]);
								this.Model.CreateNewEntryRow(entryEntity, num, dynamicObject2);
								num++;
							}
							continue;
						}
					}
					enumerable = from a in baseBillInfo2
					where DataEntityExtend.GetDynamicObjectItemValue<long>(a, "UseOrgId_Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(item, "FUSEORGID", 0L) && DataEntityExtend.GetDynamicObjectItemValue<long>(a, "ProductLineId_Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(item, "FPRODUCTLINEID", 0L)
					select a;
					if (enumerable.Count<DynamicObject>() > 0)
					{
						foreach (DynamicObject dynamicObject3 in enumerable)
						{
							DynamicObject dynamicObject4 = new DynamicObject(entryEntity.DynamicObjectType);
							DynamicObject dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject3, "UseOrgId", null);
							LocaleValue localeValue2 = dynamicObjectItemValue4["Name"] as LocaleValue;
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject4, "UseOrgName", localeValue2[base.Context.UserLocale.LCID]);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject4, "BufferNumber", DataEntityExtend.GetDynamicObjectItemValue<string>(item, "FNUMBER", null));
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject4, "BufferName", DataEntityExtend.GetDynamicObjectItemValue<string>(item, "FNAME", null));
							DynamicObject dynamicObjectItemValue5 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject3, "MaterialId", null);
							localeValue2 = (dynamicObjectItemValue5["Name"] as LocaleValue);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject4, "MaterialNumber", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue5, "Number", null));
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject4, "MaterialName", localeValue2[base.Context.UserLocale.LCID]);
							DynamicObject dynamicObjectItemValue6 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject3, "ProductLineId", null);
							localeValue2 = (dynamicObjectItemValue6["Name"] as LocaleValue);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject4, "PrdLineNumber", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue6, "Number", null));
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject4, "PrdLineName", localeValue2[base.Context.UserLocale.LCID]);
							this.Model.CreateNewEntryRow(entryEntity, num, dynamicObject4);
							num++;
						}
					}
				}
			}
		}

		// Token: 0x06000CD6 RID: 3286 RVA: 0x00098728 File Offset: 0x00096928
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

		// Token: 0x040005DE RID: 1502
		private List<DynamicObject> list = new List<DynamicObject>();

		// Token: 0x040005DF RID: 1503
		private List<string> prdList = new List<string>();
	}
}
