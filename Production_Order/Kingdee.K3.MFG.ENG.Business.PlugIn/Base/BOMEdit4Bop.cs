using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000009 RID: 9
	[Description("物料清单BOP表单插件")]
	public class BOMEdit4Bop : BaseControlEdit
	{
		// Token: 0x06000171 RID: 369 RVA: 0x000121E8 File Offset: 0x000103E8
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbAddBop")
				{
					this.DoAddBop(e);
					return;
				}
				if (!(barItemKey == "tbSyncBop"))
				{
					if (barItemKey == "tbVerify")
					{
						this.DoVerify();
						return;
					}
					if (!(barItemKey == "tbDeleteLine"))
					{
						return;
					}
					int[] selectedRows = base.View.GetControl<EntryGrid>("FBopEntity").GetSelectedRows();
					if (selectedRows.Count<int>() == 1 && selectedRows[0] == -1)
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("请选择有效数据行！", "015072000017267", 7, new object[0]), "", 0);
					}
				}
				else
				{
					DynamicObjectCollection dynamicObjectCollection = null;
					DynamicObjectCollection dynamicObjectCollection2 = null;
					this.GetEntityDatas(ref dynamicObjectCollection, ref dynamicObjectCollection2);
					if (dynamicObjectCollection == null || dynamicObjectCollection.Count == 0 || dynamicObjectCollection2 == null || dynamicObjectCollection2.Count == 0)
					{
						return;
					}
					this.DeleteInvalidEntry();
					this.SynBop(dynamicObjectCollection, dynamicObjectCollection2);
					List<DynamicObject> list = (from pl in dynamicObjectCollection2
					where Convert.ToInt64(pl["ProductLineId_Id"]) > 0L
					select (DynamicObject)pl["ProductLineId"]).Distinct<DynamicObject>().ToList<DynamicObject>();
					using (List<DynamicObject>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							BOMEdit4Bop.<>c__DisplayClassa CS$<>8__locals1 = new BOMEdit4Bop.<>c__DisplayClassa();
							CS$<>8__locals1.prdLine = enumerator.Current;
							List<long> existEntryIds = (from o in dynamicObjectCollection2
							where Convert.ToInt64(CS$<>8__locals1.prdLine["Id"]) == Convert.ToInt64(o["ProductLineId_Id"])
							select Convert.ToInt64(o["TreeEntryId"])).ToList<long>();
							DynamicObject[] entityColl = (from entry in dynamicObjectCollection
							where !existEntryIds.Contains(Convert.ToInt64(entry["Id"]))
							select entry).ToArray<DynamicObject>();
							this.addBopDataToGrid(new List<DynamicObject>
							{
								CS$<>8__locals1.prdLine
							}, entityColl, dynamicObjectCollection2);
						}
					}
					base.View.UpdateView("FBopEntity");
					this.DoVerify();
					return;
				}
			}
		}

		// Token: 0x06000172 RID: 370 RVA: 0x00012428 File Offset: 0x00010628
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (!(a == "FPRDLINELOCID"))
				{
					return;
				}
				long value = MFGBillUtil.GetValue<long>(base.View.Model, "FProductLineId", e.Row, -1L, "TreeEntity");
				if (value <= 0L)
				{
					e.Cancel = true;
					base.View.ShowErrMessage(this.str_msg1, Consts.ERROR_TITLE, 0);
					return;
				}
				string text = string.Format(" FProductLineId = {0}", value);
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
			}
		}

		// Token: 0x06000173 RID: 371 RVA: 0x000124D8 File Offset: 0x000106D8
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			if (e.BaseDataFieldKey.Equals("FPrdLineLocId"))
			{
				long value = MFGBillUtil.GetValue<long>(base.View.Model, "FProductLineId", e.Row, -1L, "TreeEntity");
				if (value <= 0L)
				{
					e.Cancel = true;
					base.View.ShowErrMessage(this.str_msg1, Consts.ERROR_TITLE, 0);
				}
				else
				{
					string text = string.Format(" FProductLineId = {0}", value);
					e.Filter = base.SqlAppendAnd(e.Filter, text);
				}
			}
			base.BeforeSetItemValueByNumber(e);
		}

		// Token: 0x06000174 RID: 372 RVA: 0x000125B4 File Offset: 0x000107B4
		public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
		{
			base.AfterDeleteRow(e);
			string entityKey;
			if ((entityKey = e.EntityKey) != null)
			{
				if (!(entityKey == "FTreeEntity"))
				{
					return;
				}
				BOMEdit4Bop.<>c__DisplayClassf CS$<>8__locals1 = new BOMEdit4Bop.<>c__DisplayClassf();
				if (e.DataEntity["Id"] == null || Convert.ToInt64(e.DataEntity["Id"]) == 0L)
				{
					return;
				}
				long treeEntryId = Convert.ToInt64(e.DataEntity["Id"]);
				this.DeleteRelatedBopEntrys(treeEntryId);
				Entity entity = base.View.Model.BusinessInfo.GetEntity("FBopEntity");
				CS$<>8__locals1.bops = base.View.Model.GetEntityDataObject(entity);
				DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(base.View.Model.BusinessInfo.GetEntity("FTreeEntity"));
				int i;
				for (i = CS$<>8__locals1.bops.Count - 1; i >= 0; i--)
				{
					DynamicObject dynamicObject = (from o in entityDataObject
					where DataEntityExtend.GetDynamicObjectItemValue<long>(o, "Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(CS$<>8__locals1.bops[i], "TreeEntryId", 0L)
					select o).FirstOrDefault<DynamicObject>();
					if (dynamicObject != null)
					{
						DataEntityExtend.SetDynamicObjectItemValue(CS$<>8__locals1.bops[i], "ReplaceGroupBop", DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "ReplaceGroup", 0L));
					}
				}
				for (int j = 0; j < CS$<>8__locals1.bops.Count; j++)
				{
					base.View.UpdateView("FReplaceGroupBop", j);
				}
			}
		}

		// Token: 0x06000175 RID: 373 RVA: 0x00012756 File Offset: 0x00010956
		private void ResetReplaceGroup(long treeEntryId)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00012760 File Offset: 0x00010960
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString()) != null)
			{
				if (!(a == "Save") && !(a == "Submit"))
				{
					return;
				}
				this.DeleteInvalidEntry();
				if (!this.DoVerify())
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000177 RID: 375 RVA: 0x000127F4 File Offset: 0x000109F4
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			base.AfterCopyData(e);
			Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			if (entityDataObject == null || entityDataObject.Count == 0)
			{
				return;
			}
			this.SetTreeEntryKeys(entityDataObject);
			Entity entity2 = base.View.BusinessInfo.GetEntity("FBopEntity");
			base.View.Model.GetEntityDataObject(entity2);
			int entryRowCount = this.Model.GetEntryRowCount("FBopEntity");
			for (int i = 0; i < entryRowCount; i++)
			{
				long ReplaceGroupBop = MFGBillUtil.GetValue<long>(base.View.Model, "FReplaceGroupBop", i, 0L, null);
				long num = (from o in entityDataObject
				where ReplaceGroupBop == Convert.ToInt64(o["ReplaceGroup"])
				select Convert.ToInt64(o["Id"])).FirstOrDefault<long>();
				if (num != 0L)
				{
					base.View.Model.SetValue("FTreeEntryId", num, i);
				}
			}
		}

		// Token: 0x06000178 RID: 376 RVA: 0x0001291C File Offset: 0x00010B1C
		public override void AfterCopyRow(AfterCopyRowEventArgs e)
		{
			base.AfterCopyRow(e);
			string entityKey;
			if ((entityKey = e.EntityKey) != null)
			{
				if (!(entityKey == "FBopEntity"))
				{
					return;
				}
				DynamicObject dynamicObject = base.View.Model.GetValue("FPrdLineLocId", e.Row) as DynamicObject;
				if (dynamicObject != null)
				{
					base.View.Model.SetItemValueByID("FPrdLineLocId", dynamicObject["Id"], e.NewRow);
				}
			}
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00012994 File Offset: 0x00010B94
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			base.AfterCreateNewEntryRow(e);
			if (e.Entity.Key.ToString().ToUpper().Equals("FTreeEntity".ToUpper()))
			{
				Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
				DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
				DynamicObject dynamicObject = entityDataObject[e.Row];
				if (Convert.ToInt64(dynamicObject["Id"]) == 0L)
				{
					long[] array = DBServiceHelper.GetSequenceInt64(base.Context, "T_ENG_BOMCHILD", 1).ToArray<long>();
					dynamicObject["Id"] = array[0];
				}
			}
		}

		// Token: 0x0600017A RID: 378 RVA: 0x00012AEC File Offset: 0x00010CEC
		private void DoAddBop(BarItemClickEventArgs e)
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityColl = base.View.Model.GetEntityDataObject(entity);
			if (entityColl == null || entityColl.Count == 0)
			{
				return;
			}
			if ((from o in entityColl
			where o["MATERIALIDCHILD"] != null
			select o).Count<DynamicObject>() == 0)
			{
				return;
			}
			Entity entity2 = base.View.BusinessInfo.GetEntity("FBopEntity");
			DynamicObjectCollection bopEntityColl = base.View.Model.GetEntityDataObject(entity2);
			List<long> value = new List<long>();
			if (bopEntityColl != null && bopEntityColl.Count > 0)
			{
				value = (from entry in bopEntityColl
				where entry["ProductLineId_Id"] != null && Convert.ToInt64(entry["ProductLineId_Id"]) > 0L
				select Convert.ToInt64(entry["ProductLineId_Id"])).Distinct<long>().ToList<long>();
			}
			base.View.Session["ProductLineIds"] = value;
			base.View.Session["UseOrgId"] = Convert.ToInt64((base.View.Model.GetValue("FUseOrgId") as DynamicObject)["Id"]);
			MFGBillUtil.ShowForm(base.View, "ENG_SelectProuctLine", null, delegate(FormResult result)
			{
				if (result.ReturnData is List<DynamicObject>)
				{
					this.addBopDataToGrid(result.ReturnData as List<DynamicObject>, entityColl.ToArray<DynamicObject>(), bopEntityColl);
					this.View.Model.DataChanged = true;
				}
			}, 0);
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00012C90 File Offset: 0x00010E90
		private bool DoVerify()
		{
			bool result = true;
			DynamicObjectCollection entityColl = null;
			DynamicObjectCollection dynamicObjectCollection = null;
			this.GetEntityDatas(ref entityColl, ref dynamicObjectCollection);
			if (dynamicObjectCollection == null || dynamicObjectCollection.Count == 0)
			{
				return result;
			}
			this.ResetBopBackGround();
			List<string> list = new List<string>();
			this.CheckLocationMustInput(dynamicObjectCollection, ref list);
			this.CheckBopBeDeleted(entityColl, dynamicObjectCollection, ref list);
			this.CheckBopBeSynched(entityColl, dynamicObjectCollection, ref list);
			this.CheckSumBopNumerator(entityColl, dynamicObjectCollection, ref list);
			this.ShowErrMsg(list);
			if (list != null && list.Count > 0)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600017C RID: 380 RVA: 0x00012D18 File Offset: 0x00010F18
		private void addBopDataToGrid(List<DynamicObject> lstPrdLine, DynamicObject[] entityColl, DynamicObjectCollection bopEntityColl)
		{
			if (lstPrdLine == null || lstPrdLine.Count == 0)
			{
				return;
			}
			Entity entity = base.View.BusinessInfo.GetEntity("FBopEntity");
			int num = 0;
			int num2 = 0;
			if (bopEntityColl.Count > 0)
			{
				num = (from bopEntry in bopEntityColl
				select Convert.ToInt32(bopEntry["Seq"])).Max();
				num2 = bopEntityColl.Count;
			}
			List<DynamicObject> filteredEntityCollection = this.GetFilteredEntityCollection(entityColl);
			if (filteredEntityCollection == null || filteredEntityCollection.Count == 0)
			{
				return;
			}
			long useOrgId = Convert.ToInt64((base.View.Model.GetValue("FUseOrgId") as DynamicObject)["Id"]);
			Dictionary<long, DynamicObject> prdLineDftFeedLocs = this.GetPrdLineDftFeedLocs(useOrgId, lstPrdLine);
			foreach (DynamicObject dynamicObject in lstPrdLine)
			{
				foreach (DynamicObject dynamicObject2 in filteredEntityCollection)
				{
					DynamicObject dynamicObject3 = new DynamicObject(entity.DynamicObjectType);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "Seq", num + 1);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ReplaceGroupBop", dynamicObject2["ReplaceGroup"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ProductLineId_Id", dynamicObject["Id"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ProductLineId", dynamicObject);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BopMaterialId_Id", dynamicObject2["MATERIALIDCHILD_Id"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BopMaterialId", dynamicObject2["MATERIALIDCHILD"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BopDosageType", dynamicObject2["DOSAGETYPE"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BopUnitId_Id", dynamicObject2["CHILDUNITID_Id"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BopUnitId", dynamicObject2["CHILDUNITID"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BopBaseUnitID_Id", dynamicObject2["ChildBaseUnitID_Id"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BopBaseUnitID", dynamicObject2["ChildBaseUnitID"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BopNumerator", dynamicObject2["NUMERATOR"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BaseBopNumerator", dynamicObject2["BaseNumerator"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BopDenominator", dynamicObject2["DENOMINATOR"]);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "BaseBopDenominator", dynamicObject2["BaseDenominator"]);
					if (prdLineDftFeedLocs != null && prdLineDftFeedLocs.Count > 0 && prdLineDftFeedLocs.ContainsKey(Convert.ToInt64(dynamicObject["Id"])))
					{
						DynamicObject dynamicObject4 = prdLineDftFeedLocs[Convert.ToInt64(dynamicObject["Id"])];
						if (dynamicObject4 != null)
						{
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "PrdLineLocId_Id", dynamicObject4["Id"]);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "PrdLineLocId", dynamicObject4);
						}
					}
					this.Model.CreateNewEntryRow(entity, num2, dynamicObject3);
					this.Model.SetValue("FTreeEntryId", dynamicObject2["Id"], num2);
					num2++;
					num++;
				}
			}
		}

		// Token: 0x0600017D RID: 381 RVA: 0x00013144 File Offset: 0x00011344
		private List<DynamicObject> GetFilteredEntityCollection(DynamicObject[] entityColl)
		{
			return (from o in entityColl
			where "1".Equals(o["MATERIALTYPE"].ToString()) && "2".Equals(o["DOSAGETYPE"].ToString()) && !"7".Equals(o["ISSUETYPE"]) && o["MATERIALIDCHILD"] != null && new string[]
			{
				"1",
				"2",
				"3",
				"4",
				"5"
			}.Contains(((o["MATERIALIDCHILD"] as DynamicObject)["MaterialBase"] as DynamicObjectCollection).FirstOrDefault<DynamicObject>()["ErpClsID"])
			select o).ToList<DynamicObject>();
		}

		// Token: 0x0600017E RID: 382 RVA: 0x00013170 File Offset: 0x00011370
		private void GetEntityDatas(ref DynamicObjectCollection entityColl, ref DynamicObjectCollection bopEntityColl)
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			entityColl = base.View.Model.GetEntityDataObject(entity);
			Entity entity2 = base.View.BusinessInfo.GetEntity("FBopEntity");
			bopEntityColl = base.View.Model.GetEntityDataObject(entity2);
		}

		// Token: 0x0600017F RID: 383 RVA: 0x00013220 File Offset: 0x00011420
		private void CheckBopBeDeleted(DynamicObjectCollection entityColl, DynamicObjectCollection bopEntityColl, ref List<string> lstErrMsg)
		{
			List<long> second = (from entry in entityColl
			where Convert.ToInt64(entry["Id"]) > 0L
			select Convert.ToInt64(entry["Id"])).ToList<long>();
			List<long> first = (from entry in bopEntityColl
			where Convert.ToInt64(entry["TreeEntryId"]) > 0L
			select Convert.ToInt64(entry["TreeEntryId"])).Distinct<long>().ToList<long>();
			List<long> list = first.Except(second).ToList<long>();
			if (list == null || list.Count == 0)
			{
				return;
			}
			lstErrMsg.Add(this.str_msg2);
			this.SetEntityColor(list);
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00013354 File Offset: 0x00011554
		private void CheckBopBeSynched(DynamicObjectCollection entityColl, DynamicObjectCollection bopEntityColl, ref List<string> lstErrMsg)
		{
			List<DynamicObject> filteredEntityCollection = this.GetFilteredEntityCollection(entityColl.ToArray<DynamicObject>());
			List<long> first = (from entry in filteredEntityCollection
			where Convert.ToInt64(entry["Id"]) > 0L
			select Convert.ToInt64(entry["Id"])).ToList<long>();
			List<string> list = new List<string>();
			List<BOMEdit4Bop.TreeEntityProdultLine> list2 = new List<BOMEdit4Bop.TreeEntityProdultLine>();
			IEnumerable<IGrouping<long, DynamicObject>> enumerable = from o in bopEntityColl
			group o by DataEntityExtend.GetDynamicObjectItemValue<long>(o, "ProductLineId_Id", 0L);
			foreach (IGrouping<long, DynamicObject> grouping in enumerable)
			{
				List<long> second = (from entry in grouping
				where Convert.ToInt64(entry["TreeEntryId"]) > 0L
				select Convert.ToInt64(entry["TreeEntryId"])).Distinct<long>().ToList<long>();
				List<long> list3 = first.Except(second).ToList<long>();
				if (list3 != null && list3.Count != 0)
				{
					list.Add(DataEntityExtend.GetDynamicObjectItemValue<string>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(grouping.FirstOrDefault<DynamicObject>(), "ProductLineId", null), "Number", null));
					foreach (long treeEntityId in list3)
					{
						list2.Add(new BOMEdit4Bop.TreeEntityProdultLine
						{
							treeEntityId = treeEntityId,
							productLineId = grouping.Key
						});
					}
				}
			}
			if (list.Count<string>() > 0)
			{
				lstErrMsg.Add(string.Format(this.str_msg3, new object[0]));
			}
			if (list2.Count > 0)
			{
				this.SetEntityColor(list2);
			}
		}

		// Token: 0x06000181 RID: 385 RVA: 0x00013B74 File Offset: 0x00011D74
		private void CheckSumBopNumerator(DynamicObjectCollection entityColl, DynamicObjectCollection bopEntityColl, ref List<string> lstErrMsg)
		{
			var enumerable = from r in bopEntityColl
			group r by new
			{
				TreeEntryId = DataEntityExtend.GetDynamicObjectItemValue<long>(r, "TreeEntryId", 0L),
				ProductLineId = DataEntityExtend.GetDynamicObjectItemValue<long>(r, "ProductLineId_Id", 0L),
				replaceGroupBop = DataEntityExtend.GetDynamicObjectItemValue<long>(r, "ReplaceGroupBop", 0L),
				productlineName = (DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(r, "ProductLineId", null)["Name"] as LocaleValue)[base.Context.UserLocale.LCID],
				productLineNumber = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(r, "ProductLineId", null)["Number"]
			} into g
			select new
			{
				treeEntryId = g.Key.TreeEntryId,
				productLineId = g.Key.ProductLineId,
				sumNumerator = g.Sum((DynamicObject o) => DataEntityExtend.GetDynamicObjectItemValue<decimal>(o, "BopNumerator", 0m)),
				replaceGroupBop = g.Key.replaceGroupBop,
				productLineName = g.Key.productlineName,
				productLineNumber = g.Key.productLineNumber
			};
			List<BOMEdit4Bop.TreeEntityProdultLine> list = new List<BOMEdit4Bop.TreeEntityProdultLine>();
			using (var enumerator = enumerable.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					<>f__AnonymousType1<long, long, decimal, long, string, object> bopgroup = enumerator.Current;
					if ((from entry in entityColl
					where Convert.ToInt64(entry["Id"]) == bopgroup.treeEntryId
					select (decimal)entry["NUMERATOR"]).FirstOrDefault<decimal>().CompareTo(bopgroup.sumNumerator) != 0)
					{
						BOMEdit4Bop.TreeEntityProdultLine item = default(BOMEdit4Bop.TreeEntityProdultLine);
						item.treeEntityId = bopgroup.treeEntryId;
						item.productLineId = bopgroup.productLineId;
						lstErrMsg.Add(string.Format(this.str_msg4, string.Concat(new object[]
						{
							bopgroup.productLineNumber,
							"（",
							bopgroup.productLineName,
							"）"
						}), bopgroup.replaceGroupBop));
						list.Add(item);
					}
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			this.SetEntityColor(list);
		}

		// Token: 0x06000182 RID: 386 RVA: 0x00013D1C File Offset: 0x00011F1C
		private void CheckLocationMustInput(DynamicObjectCollection bopEntityColl, ref List<string> lstErrMsg)
		{
			foreach (DynamicObject dynamicObject in bopEntityColl)
			{
				if (dynamicObject["PrdLineLocId"] == null)
				{
					DynamicObject dynamicObject2 = dynamicObject["ProductLineId"] as DynamicObject;
					string arg = (dynamicObject2["Name"] as LocaleValue)[base.Context.UserLocale.LCID];
					lstErrMsg.Add(string.Format(this.str_msg7, arg, Convert.ToInt64(dynamicObject["ReplaceGroupBop"])));
				}
			}
		}

		// Token: 0x06000183 RID: 387 RVA: 0x00013DCC File Offset: 0x00011FCC
		private void ShowErrMsg(List<string> lstErrMsg)
		{
			if (lstErrMsg == null || lstErrMsg.Count == 0)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string value in lstErrMsg)
			{
				num++;
				if (num > 5)
				{
					stringBuilder.AppendLine(this.str_msg5);
					stringBuilder.AppendLine(this.str_msg6);
					break;
				}
				stringBuilder.AppendLine(value);
			}
			base.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
		}

		// Token: 0x06000184 RID: 388 RVA: 0x00013E6C File Offset: 0x0001206C
		private void SetEntityColor(List<long> subKeys)
		{
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			int entryRowCount = this.Model.GetEntryRowCount("FBopEntity");
			for (int i = 0; i < entryRowCount; i++)
			{
				if (subKeys.Contains(MFGBillUtil.GetValue<long>(base.View.Model, "FTreeEntryId", i, 0L, null)))
				{
					list.Add(new KeyValuePair<int, string>(i, "#FF0000"));
				}
			}
			if (list.Count<KeyValuePair<int, string>>() > 0)
			{
				EntryGrid control = base.View.GetControl<EntryGrid>("FBopEntity");
				control.SetRowBackcolor(list);
			}
		}

		// Token: 0x06000185 RID: 389 RVA: 0x00013EF0 File Offset: 0x000120F0
		private void SetEntityColor(List<BOMEdit4Bop.TreeEntityProdultLine> lstStruct)
		{
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			int entryRowCount = this.Model.GetEntryRowCount("FBopEntity");
			for (int i = 0; i < entryRowCount; i++)
			{
				foreach (BOMEdit4Bop.TreeEntityProdultLine treeEntityProdultLine in lstStruct)
				{
					if (treeEntityProdultLine.productLineId == MFGBillUtil.GetValue<long>(base.View.Model, "FProductLineId", i, 0L, null) && treeEntityProdultLine.treeEntityId == MFGBillUtil.GetValue<long>(base.View.Model, "FTreeEntryId", i, 0L, null))
					{
						list.Add(new KeyValuePair<int, string>(i, "#FF0000"));
					}
				}
			}
			if (list.Count<KeyValuePair<int, string>>() > 0)
			{
				EntryGrid control = base.View.GetControl<EntryGrid>("FBopEntity");
				control.SetRowBackcolor(list);
			}
		}

		// Token: 0x06000186 RID: 390 RVA: 0x00013FDC File Offset: 0x000121DC
		private void ResetBopBackGround()
		{
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			int entryRowCount = this.Model.GetEntryRowCount("FBopEntity");
			for (int i = 0; i < entryRowCount; i++)
			{
				list.Add(new KeyValuePair<int, string>(i, "#FFFFFF"));
			}
			if (list.Count<KeyValuePair<int, string>>() > 0)
			{
				EntryGrid control = base.View.GetControl<EntryGrid>("FBopEntity");
				control.SetRowBackcolor(list);
			}
		}

		// Token: 0x06000187 RID: 391 RVA: 0x00014040 File Offset: 0x00012240
		private void DeleteRelatedBopEntrys(long treeEntryId)
		{
			int entryRowCount = this.Model.GetEntryRowCount("FBopEntity");
			for (int i = entryRowCount - 1; i >= 0; i--)
			{
				if (treeEntryId == MFGBillUtil.GetValue<long>(base.View.Model, "FTreeEntryId", i, 0L, null))
				{
					this.Model.DeleteEntryRow("FBopEntity", i);
				}
			}
		}

		// Token: 0x06000188 RID: 392 RVA: 0x000140B0 File Offset: 0x000122B0
		private bool IsTreeEntityChanged()
		{
			bool result = false;
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntity("FTreeEntity"));
			if (entityDataObject == null || entityDataObject.Count == 0)
			{
				return result;
			}
			int num = (from o in entityDataObject
			where Convert.ToInt64(o["Id"]) == 0L
			select o).Count<DynamicObject>();
			if (num > 0)
			{
				result = true;
			}
			return result;
		}

		// Token: 0x06000189 RID: 393 RVA: 0x00014120 File Offset: 0x00012320
		private void SetTreeEntryKeys(DynamicObjectCollection entityColl)
		{
			long[] array = DBServiceHelper.GetSequenceInt64(base.Context, "T_ENG_BOMCHILD", entityColl.Count).ToArray<long>();
			int num = 0;
			foreach (DynamicObject dynamicObject in entityColl)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Id", array[num]);
				num++;
			}
		}

		// Token: 0x0600018A RID: 394 RVA: 0x000141AC File Offset: 0x000123AC
		private Dictionary<long, DynamicObject> GetPrdLineDftFeedLocs(long useOrgId, List<DynamicObject> lstPrdLine)
		{
			Dictionary<long, DynamicObject> dictionary = new Dictionary<long, DynamicObject>();
			if (lstPrdLine == null || lstPrdLine.Count == 0)
			{
				return dictionary;
			}
			string text = string.Format(" FUseOrgId = {0} and FProductLineId in ({1}) and FDefaultPickingLocation = 1", useOrgId, string.Join<long>(",", (from o in lstPrdLine
			select Convert.ToInt64(o["ID"])).ToList<long>()));
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FId"),
				new SelectorItemInfo("FProductLineId"),
				new SelectorItemInfo("FLocationCode"),
				new SelectorItemInfo("FLocationName")
			};
			List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_PrdLineLocF8", list, text, "");
			if (baseBillInfo == null || baseBillInfo.Count == 0)
			{
				return dictionary;
			}
			foreach (DynamicObject dynamicObject in baseBillInfo)
			{
				dictionary.Add(Convert.ToInt64(dynamicObject["ProductLineId_Id"]), dynamicObject);
			}
			return dictionary;
		}

		// Token: 0x0600018B RID: 395 RVA: 0x000142FC File Offset: 0x000124FC
		private void DeleteInvalidEntry()
		{
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntity("FBopEntity"));
			DynamicObjectCollection entityDataObject2 = this.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntity("FTreeEntity"));
			if (entityDataObject == null || entityDataObject.Count == 0)
			{
				return;
			}
			for (int i = entityDataObject.Count<DynamicObject>() - 1; i >= 0; i--)
			{
				long treeEntryId = Convert.ToInt64(entityDataObject[i]["TreeEntryId"]);
				DynamicObject dynamicObject = (from entry in entityDataObject2
				where Convert.ToInt64(entry["Id"]) == treeEntryId
				select entry).FirstOrDefault<DynamicObject>();
				if (dynamicObject != null)
				{
					bool flag = false;
					DynamicObject dynamicObject2 = entityDataObject[i]["BopMaterialId"] as DynamicObject;
					if (dynamicObject2 != null && !new string[]
					{
						"1",
						"2",
						"3",
						"4",
						"5"
					}.Contains((dynamicObject2["MaterialBase"] as DynamicObjectCollection)[0]["ErpClsID"].ToString()))
					{
						flag = true;
					}
					if (string.CompareOrdinal("2", entityDataObject[i]["BopDosageType"].ToString()) != 0)
					{
						flag = true;
					}
					if (string.CompareOrdinal("1", dynamicObject["MATERIALTYPE"].ToString()) != 0)
					{
						flag = true;
					}
					if (string.CompareOrdinal("7", dynamicObject["ISSUETYPE"].ToString()) == 0)
					{
						flag = true;
					}
					if (flag)
					{
						base.View.Model.DeleteEntryRow("FBopEntity", i);
					}
				}
			}
		}

		// Token: 0x0600018C RID: 396 RVA: 0x00014530 File Offset: 0x00012730
		private void SynBop(DynamicObjectCollection entityColl, DynamicObjectCollection bopEntityColl)
		{
			foreach (DynamicObject dynamicObject in entityColl)
			{
				long treeEntryId = Convert.ToInt64(dynamicObject["Id"]);
				IEnumerable<long> enumerable = from bopEntry in bopEntityColl
				where Convert.ToInt64(bopEntry["TreeEntryId"]) == treeEntryId
				select Convert.ToInt64(bopEntry["ProductLineId_Id"]);
				if (enumerable != null && enumerable.Count<long>() != 0)
				{
					long[] array = enumerable.ToArray<long>();
					for (int i = 0; i < array.Length; i++)
					{
						long productLineId = array[i];
						IEnumerable<DynamicObject> enumerable2 = from bopEntry in bopEntityColl
						where Convert.ToInt64(bopEntry["TreeEntryId"]) == treeEntryId && Convert.ToInt64(bopEntry["ProductLineId_Id"]) == productLineId
						select bopEntry;
						if (enumerable2 != null && enumerable2.Count<DynamicObject>() != 0)
						{
							DynamicObject[] array2 = enumerable2.ToArray<DynamicObject>();
							for (int j = 0; j < array2.Length; j++)
							{
								array2[j]["BopMaterialId_Id"] = dynamicObject["MATERIALIDCHILD_Id"];
								array2[j]["BopUnitId_Id"] = dynamicObject["CHILDUNITID_Id"];
								array2[j]["BopBaseUnitID_Id"] = dynamicObject["ChildBaseUnitID_Id"];
								array2[j]["BopDosageType"] = dynamicObject["DOSAGETYPE"];
								array2[j]["BopDenominator"] = dynamicObject["DENOMINATOR"];
								array2[j]["BaseBopDenominator"] = dynamicObject["BaseDenominator"];
								if (array2.Count<DynamicObject>() == 1)
								{
									array2[j]["BopNumerator"] = dynamicObject["NUMERATOR"];
									array2[j]["BaseBopNumerator"] = dynamicObject["BaseNumerator"];
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x040000A8 RID: 168
		private string str_msg0 = ResManager.LoadKDString("请先保存单据！", "015072000017260", 7, new object[0]);

		// Token: 0x040000A9 RID: 169
		private string str_msg1 = ResManager.LoadKDString("请先录入生产线！", "015072000017261", 7, new object[0]);

		// Token: 0x040000AA RID: 170
		private string str_msg2 = ResManager.LoadKDString("BOP对应的子项明细行部分被删除，请同步BOP数据", "015072000017262", 7, new object[0]);

		// Token: 0x040000AB RID: 171
		private string str_msg3 = ResManager.LoadKDString("子项明细有新增内容，请同步BOP数据", "015072000017263", 7, new object[0]);

		// Token: 0x040000AC RID: 172
		private string str_msg4 = ResManager.LoadKDString("BOP中生产线为“{0}”、项次为“{1}”的行【用量:分子】(汇总)与子项明细对应行【用量:分子】不等，请修改。", "015072000017264", 7, new object[0]);

		// Token: 0x040000AD RID: 173
		private string str_msg5 = "......";

		// Token: 0x040000AE RID: 174
		private string str_msg6 = ResManager.LoadKDString("注：错误信息过长，其它错误行将以红色背景标注，请注意查看", "015072000017265", 7, new object[0]);

		// Token: 0x040000AF RID: 175
		private string str_msg7 = ResManager.LoadKDString("BOP中生产线为“{0}”、项次为“{1}”的行【工位/工序】为空，请录入。", "015072000017266", 7, new object[0]);

		// Token: 0x0200000A RID: 10
		internal struct TreeEntityProdultLine
		{
			// Token: 0x040000C9 RID: 201
			public long treeEntityId;

			// Token: 0x040000CA RID: 202
			public long productLineId;

			// Token: 0x040000CB RID: 203
			public string productLineName;

			// Token: 0x040000CC RID: 204
			public long ReplaceGroupBop;
		}
	}
}
