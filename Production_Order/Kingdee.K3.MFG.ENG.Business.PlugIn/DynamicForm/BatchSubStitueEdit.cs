using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Common;
using Kingdee.K3.Core.MFG.ENG.BatchSubStitue;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000057 RID: 87
	public class BatchSubStitueEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000653 RID: 1619 RVA: 0x0004B608 File Offset: 0x00049808
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.InitData(false);
		}

		// Token: 0x06000654 RID: 1620 RVA: 0x0004B65C File Offset: 0x0004985C
		private void InitData(bool isFilter)
		{
			if (!isFilter)
			{
				this.subStitueOption = MFGBillUtil.GetParam<BatchSubStitueOption>(this.View, "BatchSubStitueOption", null);
			}
			List<DynamicObject> subStitueBomData = this.GetSubStitueBomData(this.subStitueOption);
			if (isFilter && ListUtils.IsEmpty<DynamicObject>(subStitueBomData))
			{
				Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FSubEntity");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				entityDataObject.Clear();
				this.View.UpdateView("FSubEntity");
				return;
			}
			if (ListUtils.IsEmpty<DynamicObject>(subStitueBomData))
			{
				return;
			}
			if (this.subStitueOption.MainReplaceMaterialRows.Count > 1)
			{
				this.DeLCombomReplace(subStitueBomData);
			}
			List<long> bomIds = (from o in subStitueBomData
			select DataEntityExtend.GetDynamicValue<long>(o, "Id", 0L)).Distinct<long>().ToList<long>();
			if (!ListUtils.IsEmpty<DynamicObject>(subStitueBomData))
			{
				this.groupBatchReplaceBomItem = (from x in subStitueBomData
				group x by DataEntityExtend.GetDynamicValue<long>(x, "Id", 0L)).ToList<IGrouping<long, DynamicObject>>();
				this.resultReplaceBomDict = this.groupBatchReplaceBomItem.ToDictionary((IGrouping<long, DynamicObject> k) => k.Key);
			}
			this.networkCtrlResults = this.StartNetworkCtrl(bomIds, "Edit");
			List<long> unSuccessBomIds = new List<long>();
			this.ShowNetworkCtrl(this.networkCtrlResults, ref unSuccessBomIds);
			if (!ListUtils.IsEmpty<long>(unSuccessBomIds))
			{
				this.groupBatchReplaceBomItem = (from k in this.groupBatchReplaceBomItem
				where !unSuccessBomIds.Contains(k.Key)
				select k).ToList<IGrouping<long, DynamicObject>>();
			}
			this.BindChildEntitys();
		}

		// Token: 0x06000655 RID: 1621 RVA: 0x0004BB38 File Offset: 0x00049D38
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			if (e.BarItemKey == "tbRefresh")
			{
				this.StopNetworkCtrl(null);
				this.InitData(false);
				this.DisableEntityRowByParam();
			}
			if (e.BarItemKey == "tbFilter")
			{
				Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FSubEntity");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
				{
					this.View.ShowMessage(ResManager.LoadKDString("替代关系表体没有数据，无需过滤，请点击刷新操作", "015072000037253", 7, new object[0]), 0);
					return;
				}
				List<long> item = (from w in entityDataObject
				where Convert.ToInt64(w["BomId_Id"]) != 0L
				select w into s
				select Convert.ToInt64(s["BomId_Id"])).ToList<long>();
				List<long> item2 = (from w in entityDataObject
				where Convert.ToInt64(w["MaterialId_Id"]) != 0L
				select w into s
				select Convert.ToInt64(s["MaterialId_Id"])).ToList<long>();
				List<long> item3 = (from w in entityDataObject
				where Convert.ToInt64(w["CreatorId_Id"]) != 0L
				select w into s
				select Convert.ToInt64(s["CreatorId_Id"])).ToList<long>();
				List<long> item4 = (from w in entityDataObject
				where Convert.ToInt64(w["ApproverId_Id"]) != 0L
				select w into s
				select Convert.ToInt64(s["ApproverId_Id"])).ToList<long>();
				Tuple<long, List<long>, List<long>, List<long>, List<long>> tuple = new Tuple<long, List<long>, List<long>, List<long>, List<long>>(this.subStitueOption.UseOrgId, item, item2, item3, item4);
				MFGBillUtil.ShowFilterForm(this.View, "ENG_SubBatchSet", tuple, delegate(FormResult filterResult)
				{
					if (filterResult.ReturnData is FilterParameter)
					{
						FilterParameter filterParameter = filterResult.ReturnData as FilterParameter;
						DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(filterParameter.CustomFilter, "BomId", null);
						if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue))
						{
							List<long> dynamicObjectColumnValues = DataEntityExtend.GetDynamicObjectColumnValues<long>(dynamicValue, "BomId_Id");
							this.subStitueOption.BomIds = dynamicObjectColumnValues;
						}
						DateTime dynamicValue2 = DataEntityExtend.GetDynamicValue<DateTime>(filterParameter.CustomFilter, "BeginCreateDate", default(DateTime));
						if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue2))
						{
							this.subStitueOption.BeginCreateDate = dynamicValue2;
						}
						DateTime dynamicValue3 = DataEntityExtend.GetDynamicValue<DateTime>(filterParameter.CustomFilter, "EndCreateDate", default(DateTime));
						if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue3))
						{
							this.subStitueOption.EndCreateDate = dynamicValue3;
						}
						DynamicObjectCollection dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(filterParameter.CustomFilter, "CreatorId", null);
						if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue4))
						{
							this.subStitueOption.CreatorIds = DataEntityExtend.GetDynamicObjectColumnValues<long>(dynamicValue4, "CreatorId_Id");
						}
						DynamicObjectCollection dynamicValue5 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(filterParameter.CustomFilter, "ApproverId", null);
						if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue5))
						{
							this.subStitueOption.ApproverIds = DataEntityExtend.GetDynamicObjectColumnValues<long>(dynamicValue5, "ApproverId_Id");
						}
						DynamicObjectCollection dynamicValue6 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(filterParameter.CustomFilter, "ParentMtrl", null);
						if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue6))
						{
							List<long> parentMtrlIds = (from i in dynamicValue6
							select DataEntityExtend.GetDynamicValue<long>(i, "ParentMtrl_Id", 0L)).ToList<long>();
							this.subStitueOption.ParentMtrlIds = parentMtrlIds;
						}
						DynamicObject dynamicValue7 = DataEntityExtend.GetDynamicValue<DynamicObject>(filterParameter.CustomFilter, "MtrlFrom", null);
						if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue7))
						{
							string dynamicValue8 = DataEntityExtend.GetDynamicValue<string>(dynamicValue7, "Number", null);
							this.subStitueOption.MtrlIdFrom = dynamicValue8;
						}
						DynamicObject dynamicValue9 = DataEntityExtend.GetDynamicValue<DynamicObject>(filterParameter.CustomFilter, "MtrlTo", null);
						if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue9))
						{
							string dynamicValue10 = DataEntityExtend.GetDynamicValue<string>(dynamicValue9, "Number", null);
							this.subStitueOption.MtrlIdTo = dynamicValue10;
						}
						this.StopNetworkCtrl(null);
						this.InitData(true);
						this.DisableEntityRowByParam();
						this.subStitueOption.BomIds = new List<long>();
						this.subStitueOption.ParentMtrlIds = new List<long>();
						this.subStitueOption.MtrlIdFrom = string.Empty;
						this.subStitueOption.MtrlIdTo = string.Empty;
						this.subStitueOption.BeginCreateDate = DateTime.MinValue;
						this.subStitueOption.EndCreateDate = DateTime.MinValue;
						this.subStitueOption.CreatorIds = new List<long>();
						this.subStitueOption.ApproverIds = new List<long>();
					}
				}, "ENG_SubBatchSetFilter", 0);
			}
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x0004BD4C File Offset: 0x00049F4C
		protected List<NetworkCtrlResult> StartNetworkCtrl(List<long> bomIds, string OperationNumber)
		{
			string text = string.Format(" FMetaObjectID = '{0}' and FNumber = '{1}'  and ftype={2}  and FStart = '1'  ", "ENG_BOM", OperationNumber, 6);
			NetworkCtrlObject networkCtrlObject = NetworkCtrlServiceHelper.GetNetCtrlList(base.Context, text).FirstOrDefault<NetworkCtrlObject>();
			List<NetWorkRunTimeParam> list = new List<NetWorkRunTimeParam>();
			string text2 = networkCtrlObject.NetCtrlName.ToString();
			text2.Substring(text2.IndexOf('-') + 1);
			foreach (long num in bomIds)
			{
				string empty = string.Empty;
				list.Add(new NetWorkRunTimeParam
				{
					BillName = new LocaleValue(ResManager.LoadKDString("替代关系批量设置", "015072000018131", 7, new object[0]), base.Context.UserLocale.LCID),
					InterID = num.ToString(),
					OperationName = networkCtrlObject.NetCtrlName
				});
			}
			return NetworkCtrlServiceHelper.BatchBeginNetCtrl(base.Context, networkCtrlObject, list, false);
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x0004BE64 File Offset: 0x0004A064
		public virtual void ShowNetworkCtrl(List<NetworkCtrlResult> networkCtrlResults, ref List<long> unSuccessBomIds)
		{
			IGrouping<long, DynamicObject> source = null;
			if (!ListUtils.IsEmpty<NetworkCtrlResult>(networkCtrlResults))
			{
				List<NetworkCtrlResult> list = (from w in networkCtrlResults
				where !w.StartSuccess
				select w).ToList<NetworkCtrlResult>();
				networkCtrlResults = networkCtrlResults.Except(list).ToList<NetworkCtrlResult>();
				IOperationResult operationResult = new OperationResult();
				foreach (NetworkCtrlResult networkCtrlResult in list)
				{
					OperateResult operateResult = new OperateResult();
					if (this.resultReplaceBomDict.TryGetValue(Convert.ToInt64(networkCtrlResult.InterID), out source))
					{
						operateResult.Name = source.ToList<DynamicObject>().FirstOrDefault<DynamicObject>()["number_Id"].ToString();
					}
					unSuccessBomIds.Add(Convert.ToInt64(networkCtrlResult.InterID));
					operateResult.Message = networkCtrlResult.Message;
					operateResult.SuccessStatus = false;
					operateResult.PKValue = networkCtrlResult.InterID;
					operateResult.MessageType = 1;
					operationResult.OperateResult.Add(operateResult);
				}
				if (operationResult != null && operationResult.OperateResult.Count > 0)
				{
					FormUtils.ShowOperationResult(this.View, operationResult, null);
				}
			}
		}

		// Token: 0x06000658 RID: 1624 RVA: 0x0004BFC4 File Offset: 0x0004A1C4
		protected void StopNetworkCtrl(IEnumerable<object> pkIds = null)
		{
			List<NetworkCtrlResult> list;
			if (!ListUtils.IsEmpty<object>(pkIds))
			{
				list = (from o in this.networkCtrlResults
				where pkIds.Contains(o.InterID)
				select o).ToList<NetworkCtrlResult>();
			}
			else
			{
				list = this.networkCtrlResults;
			}
			NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, list);
		}

		// Token: 0x06000659 RID: 1625 RVA: 0x0004C027 File Offset: 0x0004A227
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			this.StopNetworkCtrl(null);
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x0004C04C File Offset: 0x0004A24C
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			if (!StringUtils.EqualsIgnoreCase(e.Key, "FBTNOK"))
			{
				this.StopNetworkCtrl(null);
				this.View.Close();
				return;
			}
			List<long> list = new List<long>();
			List<long> list2 = new List<long>();
			this.GetSelectedItemsByStId(ref list, ref list2);
			if (list.Count == 0 && list2.Count == 0)
			{
				e.Cancel = true;
				this.View.ShowMessage(ResManager.LoadKDString("没有数据可进行替代关系批量设置！", "015072000018958", 7, new object[0]), 4);
				return;
			}
			IOperationResult item = new OperationResult();
			TaskProxyItem taskProxyItem = new TaskProxyItem();
			bool userParam = MFGBillUtil.GetUserParam<bool>(this.View, "ISUpdateVersion", false);
			string userParam2 = MFGBillUtil.GetUserParam<string>(this.View, "NewBomDStatus", null);
			this.subStitueOption.IsUpdateVersion = userParam;
			this.subStitueOption.NewBomStatus = userParam2;
			this.subStitueOption.ComputeId = taskProxyItem.TaskId;
			List<object> list3 = new List<object>
			{
				base.Context,
				list,
				list2,
				this.subStitueOption,
				item
			};
			taskProxyItem.Parameters = list3.ToArray();
			taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.BatchSubStitueService,Kingdee.K3.MFG.ENG.App.Core";
			taskProxyItem.MethodName = "UpdateBomReplaceMaterialForNew";
			taskProxyItem.ProgressQueryInterval = 1;
			taskProxyItem.Title = ResManager.LoadKDString("替代关系批量设置-[物料清单]", "015072000018132", 7, new object[0]);
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
			{
				this.StopNetworkCtrl(null);
				this.View.Refresh();
			});
		}

		// Token: 0x0600065B RID: 1627 RVA: 0x0004C218 File Offset: 0x0004A418
		protected List<long> GetSelectedItems()
		{
			List<long> result = new List<long>();
			DynamicObjectCollection source = this.View.Model.DataObject["SubEntity"] as DynamicObjectCollection;
			result = (from p in source
			where DataEntityExtend.GetDynamicValue<bool>(p, "IsSelect", true)
			select p into o
			select DataEntityExtend.GetDynamicValue<long>(o, "Id", 0L)).ToList<long>();
			this.subStitueOption.ReplaceGroup = (from p in source
			where DataEntityExtend.GetDynamicValue<bool>(p, "IsSelect", true)
			select p into o
			select DataEntityExtend.GetDynamicValue<long>(o, "ReplaceGroup", 0L)).ToList<long>();
			return result;
		}

		// Token: 0x0600065C RID: 1628 RVA: 0x0004C3B4 File Offset: 0x0004A5B4
		protected void GetSelectedItemsByStId(ref List<long> selectForOld, ref List<long> selectForNew)
		{
			DynamicObjectCollection source = this.View.Model.DataObject["SubEntity"] as DynamicObjectCollection;
			selectForOld = (from p in source
			where DataEntityExtend.GetDynamicValue<bool>(p, "IsSelect", true) && (DataEntityExtend.GetDynamicObjectItemValue<long>(p, "STEntryId", 0L) == 0L || DataEntityExtend.GetDynamicObjectItemValue<long>(p, "substitutionid", 0L) != this.subStitueOption.SubstitutionId)
			select p into o
			select DataEntityExtend.GetDynamicValue<long>(o, "Id", 0L)).ToList<long>();
			selectForNew = (from p in source
			where DataEntityExtend.GetDynamicValue<bool>(p, "IsSelect", true) && DataEntityExtend.GetDynamicObjectItemValue<long>(p, "STEntryId", 0L) > 0L && DataEntityExtend.GetDynamicObjectItemValue<long>(p, "substitutionid", 0L) == this.subStitueOption.SubstitutionId
			select p into o
			select DataEntityExtend.GetDynamicValue<long>(o, "Id", 0L)).ToList<long>();
			this.subStitueOption.ReplaceGroup = (from p in source
			where DataEntityExtend.GetDynamicValue<bool>(p, "IsSelect", true)
			select p into o
			select DataEntityExtend.GetDynamicValue<long>(o, "ReplaceGroup", 0L)).ToList<long>();
		}

		// Token: 0x0600065D RID: 1629 RVA: 0x0004C4AC File Offset: 0x0004A6AC
		protected virtual void BindChildEntitys()
		{
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FSubEntity");
			this.Model.BeginIniti();
			if (!ListUtils.IsEmpty<IGrouping<long, DynamicObject>>(this.groupBatchReplaceBomItem))
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				entityDataObject.Clear();
				foreach (IGrouping<long, DynamicObject> source in this.groupBatchReplaceBomItem)
				{
					List<DynamicObject> groupItems = source.ToList<DynamicObject>();
					this.UpdateEntityData(groupItems, entryEntity, ref entityDataObject);
				}
				this.ModifyDataObjectByParam(ref entityDataObject);
				int index = 0;
				for (int i = 1; i < entityDataObject.Count<DynamicObject>(); i++)
				{
					if (entityDataObject[index]["BomId_Id"].Equals(entityDataObject[i]["BomId_Id"]))
					{
						entityDataObject[i]["MATERIALID_ID"] = 0;
						entityDataObject[i]["BomId_Id"] = 0;
						entityDataObject[i]["DOCUMENTSTATUS"] = "";
					}
					else
					{
						index = i;
					}
				}
				DBServiceHelper.LoadReferenceObject(base.Context, entityDataObject.ToArray<DynamicObject>(), entryEntity.DynamicObjectType, false);
			}
			this.Model.EndIniti();
			this.View.UpdateView("FSubEntity");
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x0004C790 File Offset: 0x0004A990
		protected virtual void ModifyDataObjectByParam(ref DynamicObjectCollection detailDataEntities)
		{
			IEnumerable<DynamicObject> source = from p in detailDataEntities
			where DataEntityExtend.GetDynamicValue<long>(p, "MATERIALCHILDID_ID", 0L) == this.subStitueOption.mainMaterialId && DataEntityExtend.GetDynamicValue<int>(p, "MATERIALTYPE", 0) == 1
			select p;
			bool userParam = MFGBillUtil.GetUserParam<bool>(this.View, "ISShowAllReplace", false);
			bool userParam2 = MFGBillUtil.GetUserParam<bool>(this.View, "ISShowTheWholeBom", false);
			List<DynamicObject> list = new List<DynamicObject>();
			if (userParam)
			{
				DynamicObject mainReplaceMaterialFirst = (from w in this.subStitueOption.MainReplaceMaterialRows
				where DataEntityExtend.GetDynamicValue<bool>(w, "IsKeyItem", false)
				select w).FirstOrDefault<DynamicObject>();
				this.mainItemRowIds = (from p in source
				where DataEntityExtend.GetDynamicValue<decimal>(p, "Denominator", 0m) == DataEntityExtend.GetDynamicValue<decimal>(mainReplaceMaterialFirst, "Denominator", 0m) && DataEntityExtend.GetDynamicValue<decimal>(p, "Numerator", 0m) == DataEntityExtend.GetDynamicValue<decimal>(mainReplaceMaterialFirst, "Numerator", 0m) && DataEntityExtend.GetDynamicValue<long>(p, "UnitID_Id", 0L) == DataEntityExtend.GetDynamicValue<long>(mainReplaceMaterialFirst, "UnitID_Id", 0L)
				select p into s
				select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "SUBROWID", null)).ToList<string>();
				if (userParam2)
				{
					using (IEnumerator<IGrouping<long, DynamicObject>> enumerator = (from g in detailDataEntities
					group g by Convert.ToInt64(g["Id"])).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							IGrouping<long, DynamicObject> source2 = enumerator.Current;
							if (source2.Any((DynamicObject p) => this.mainItemRowIds.Contains(DataEntityExtend.GetDynamicValue<string>(p, "SUBROWID", null)) || this.mainItemRowIds.Contains(DataEntityExtend.GetDynamicValue<string>(p, "PARENTROWID", null))))
							{
								list.AddRange(source2.ToList<DynamicObject>());
							}
						}
						goto IL_165;
					}
				}
				list = (from p in detailDataEntities
				where this.mainItemRowIds.Contains(DataEntityExtend.GetDynamicValue<string>(p, "SUBROWID", null)) || this.mainItemRowIds.Contains(DataEntityExtend.GetDynamicValue<string>(p, "PARENTROWID", null))
				select p).ToList<DynamicObject>();
				IL_165:
				detailDataEntities.Clear();
				if (ListUtils.IsEmpty<DynamicObject>(list))
				{
					return;
				}
				using (List<DynamicObject>.Enumerator enumerator2 = list.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						DynamicObject item = enumerator2.Current;
						detailDataEntities.Add(item);
					}
					return;
				}
			}
			this.mainItemRowIds = (from s in source
			select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "SUBROWID", null)).ToList<string>();
			HashSet<string> hashSet = new HashSet<string>(from s in source
			select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "SUBROWID", null));
			if (!userParam2)
			{
				for (int i = detailDataEntities.Count - 1; i >= 0; i--)
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(detailDataEntities[i], "SUBROWID", null);
					if (!hashSet.Contains(dynamicValue))
					{
						dynamicValue = DataEntityExtend.GetDynamicValue<string>(detailDataEntities[i], "PARENTROWID", null);
						if (!hashSet.Contains(dynamicValue))
						{
							detailDataEntities.RemoveAt(i);
						}
					}
				}
			}
		}

		// Token: 0x0600065F RID: 1631 RVA: 0x0004CA28 File Offset: 0x0004AC28
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.DisableEntityRowByParam();
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x0004CA38 File Offset: 0x0004AC38
		protected virtual void DisableEntityRowByParam()
		{
			if (ListUtils.IsEmpty<IGrouping<long, DynamicObject>>(this.groupBatchReplaceBomItem))
			{
				return;
			}
			DynamicObject dataObject = this.View.Model.DataObject;
			DynamicObjectCollection dynamicObjectCollection = dataObject["SubEntity"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (!this.mainItemRowIds.Contains(DataEntityExtend.GetDynamicValue<string>(dynamicObject, "SUBROWID", null)) && !this.mainItemRowIds.Contains(DataEntityExtend.GetDynamicValue<string>(dynamicObject, "PARENTROWID", null)))
				{
					MFGBillUtil.SetEnabled(this.View, "FIsSelect", dynamicObject, false, "Disable");
					this.View.UpdateView("FIsSelect");
				}
			}
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x0004CB58 File Offset: 0x0004AD58
		protected virtual void UpdateEntityData(List<DynamicObject> groupItems, Entity entity, ref DynamicObjectCollection detailDataEntities)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			List<string> list = (from p in groupItems
			where !string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(p, "PARENTROWID", null))
			select p into s
			select DataEntityExtend.GetDynamicValue<string>(s, "PARENTROWID", null)).Distinct<string>().ToList<string>();
			foreach (string key in list)
			{
				string value = SequentialGuid.NewGuid().ToString();
				dictionary.Add(key, value);
			}
			if (ListUtils.IsEmpty<DynamicObject>(groupItems))
			{
				return;
			}
			MFGBillUtil.GetUserParam<bool>(this.View, "ISShowAllReplace", false);
			if (!MFGBillUtil.GetUserParam<bool>(this.View, "ISShowTheWholeBom", false))
			{
				(from p in groupItems
				where DataEntityExtend.GetDynamicValue<long>(p, "MATERIALCHILDID_ID", 0L) == this.subStitueOption.mainMaterialId && DataEntityExtend.GetDynamicValue<int>(p, "MATERIALTYPE", 0) == 1
				select p).ToList<DynamicObject>();
			}
			int num = detailDataEntities.Count;
			foreach (DynamicObject dynamicObject in groupItems)
			{
				num++;
				DynamicObject dynamicObject2 = new DynamicObject(entity.DynamicObjectType);
				dynamicObject2["MATERIALID_ID"] = dynamicObject["MATERIALID_ID"];
				dynamicObject2["BomId_Id"] = dynamicObject["Id"];
				dynamicObject2["DOCUMENTSTATUS"] = dynamicObject["DOCUMENTSTATUS"];
				dynamicObject2["Id"] = dynamicObject["Id"];
				dynamicObject2["Seq"] = num;
				dynamicObject2["MATERIALCHILDID_ID"] = dynamicObject["MATERIALCHILDID_ID"];
				dynamicObject2["UNITID_ID"] = dynamicObject["UNITID_ID"];
				dynamicObject2["NUMERATOR"] = dynamicObject["NUMERATOR"];
				dynamicObject2["DENOMINATOR"] = dynamicObject["DENOMINATOR"];
				dynamicObject2["SCRAPRATE"] = dynamicObject["SCRAPRATE"];
				dynamicObject2["AUXPROPID_ID"] = dynamicObject["AUXPROPID_ID"];
				string empty = string.Empty;
				if (dictionary.TryGetValue(Convert.ToString(dynamicObject["PARENTROWID"]), out empty))
				{
					dynamicObject2["PARENTROWID"] = empty;
				}
				else
				{
					dynamicObject2["PARENTROWID"] = dynamicObject["PARENTROWID"];
				}
				if (dictionary.TryGetValue(Convert.ToString(dynamicObject["SUBROWID"]), out empty))
				{
					dynamicObject2["SUBROWID"] = empty;
				}
				else
				{
					dynamicObject2["SUBROWID"] = SequentialGuid.NewGuid().ToString();
				}
				dynamicObject2["ROWEXPANDTYPE"] = dynamicObject["ROWEXPANDTYPE"];
				dynamicObject2["REPLACEPOLICY"] = dynamicObject["REPLACEPOLICY"];
				dynamicObject2["REPLACETYPE"] = dynamicObject["REPLACETYPE"];
				dynamicObject2["REPLACEPRIORITY"] = dynamicObject["REPLACEPRIORITY"];
				dynamicObject2["REPLACEGROUP"] = dynamicObject["REPLACEGROUP"];
				dynamicObject2["FIXSCRAPQTY"] = dynamicObject["FIXSCRAPQTY"];
				dynamicObject2["MATERIALTYPE"] = dynamicObject["MATERIALTYPE"];
				dynamicObject2["ISKEYITEM"] = ((dynamicObject["ISKEYITEM"].ToString() == "0") ? "false" : "true");
				dynamicObject2["CREATORID_ID"] = dynamicObject["CREATORID_ID"];
				dynamicObject2["APPROVERID_ID"] = dynamicObject["APPROVERID_ID"];
				dynamicObject2["SUBSTITUTIONID"] = dynamicObject["SUBSTITUTIONID"];
				dynamicObject2["STENTRYID"] = dynamicObject["STENTRYID"];
				detailDataEntities.Add(dynamicObject2);
			}
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x0004CFB8 File Offset: 0x0004B1B8
		protected virtual List<DynamicObject> GetSubStitueBomData(BatchSubStitueOption subStitueOption)
		{
			return BatchSubStitueServiceHelper.GetSubStituteBomResult(base.Context, subStitueOption);
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x0004D138 File Offset: 0x0004B338
		private void DeLCombomReplace(List<DynamicObject> batchReplaceBomItem)
		{
			this.subStitueOption.bomEntryIds.Clear();
			this.subStitueOption.RepalceBomEntryIds.Clear();
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary = (from x in batchReplaceBomItem
			group x by DataEntityExtend.GetDynamicValue<long>(x, "Id", 0L)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
			List<DynamicObject> list = new List<DynamicObject>();
			List<long> bomIds = new List<long>();
			DynamicObject dynamicObject = (from w in this.subStitueOption.MainReplaceMaterialRows
			where DataEntityExtend.GetDynamicValue<bool>(w, "IsKeyItem", false)
			select w).FirstOrDefault<DynamicObject>();
			long mtrlId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MaterialID_Id", 0L);
			long bomId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L);
			long auxProId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "AuxPropID_Id", 0L);
			List<DynamicObject> noKeyMainItem = (from w in this.subStitueOption.MainReplaceMaterialRows
			where !DataEntityExtend.GetDynamicValue<bool>(w, "IsKeyItem", false)
			select w).ToList<DynamicObject>();
			foreach (KeyValuePair<long, IGrouping<long, DynamicObject>> keyValuePair in dictionary)
			{
				List<DynamicObject> list2 = new List<DynamicObject>();
				List<DynamicObject> source = keyValuePair.Value.ToList<DynamicObject>();
				Dictionary<int, IGrouping<int, DynamicObject>> dictionary2 = (from w in source
				where DataEntityExtend.GetDynamicValue<string>(w, "MATERIALTYPE", null) == "1"
				select w into g
				group g by DataEntityExtend.GetDynamicValue<int>(g, "REPLACEGROUP", 0)).ToDictionary((IGrouping<int, DynamicObject> d) => d.Key);
				List<long> entryIds = new List<long>();
				foreach (KeyValuePair<int, IGrouping<int, DynamicObject>> keyValuePair2 in dictionary2)
				{
					List<DynamicObject> list3 = keyValuePair2.Value.ToList<DynamicObject>();
					if (list3.Count<DynamicObject>() == 1)
					{
						string dynamicValue = DataEntityExtend.GetDynamicValue<string>(list3.FirstOrDefault<DynamicObject>(), "ReplaceType", null);
						if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue))
						{
							list2.Add(list3.FirstOrDefault<DynamicObject>());
						}
					}
					else if (list3.Count<DynamicObject>() == this.subStitueOption.MainReplaceMaterialRows.Count<DynamicObject>())
					{
						DynamicObject dynamicObject2 = (from w in list3
						where DataEntityExtend.GetDynamicValue<string>(w, "ISKEYITEM", null) == "1" && DataEntityExtend.GetDynamicValue<long>(w, "MATERIALCHILDID_ID", 0L) == mtrlId && DataEntityExtend.GetDynamicValue<long>(w, "BOMID_ID", 0L) == bomId && DataEntityExtend.GetDynamicValue<long>(w, "AUXPROPID_ID", 0L) == auxProId
						select w).FirstOrDefault<DynamicObject>();
						if (!ObjectUtils.IsNullOrEmpty(dynamicObject2))
						{
							List<long> list4 = new List<long>();
							bool flag = this.isValidateRate(entryIds, dynamicObject2, list3, dynamicObject, noKeyMainItem, ref list4);
							if (flag)
							{
								int replaceGroup = keyValuePair2.Key;
								List<long> list5 = (from w in source
								where DataEntityExtend.GetDynamicValue<int>(w, "REPLACEGROUP", 0) == replaceGroup
								select w into s
								select DataEntityExtend.GetDynamicValue<long>(s, "ENTRYID", 0L)).ToList<long>();
								string key = Convert.ToString(keyValuePair.Key) + "&" + Convert.ToString(replaceGroup);
								this.subStitueOption.RepalceBomEntryIds.Add(key, list5);
								this.subStitueOption.bomEntryIds.AddRange(list5);
								bomIds.Add(keyValuePair.Key);
							}
						}
					}
				}
				List<DynamicObject> list6 = (from w in list2
				where DataEntityExtend.GetDynamicValue<long>(w, "MATERIALCHILDID_ID", 0L) == mtrlId && DataEntityExtend.GetDynamicValue<long>(w, "BOMID_ID", 0L) == bomId && DataEntityExtend.GetDynamicValue<long>(w, "AUXPROPID_ID", 0L) == auxProId
				select w).ToList<DynamicObject>();
				foreach (DynamicObject dynamicObject3 in list6)
				{
					int dynamicValue2 = DataEntityExtend.GetDynamicValue<int>(dynamicObject3, "ReplaceGroup", 0);
					List<long> list7 = new List<long>();
					new List<string>();
					bool flag2 = this.isValidateRate(entryIds, dynamicObject3, list2, dynamicObject, noKeyMainItem, ref list7);
					if (flag2)
					{
						long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "ENTRYID", 0L);
						list7.Add(dynamicValue3);
						string key2 = Convert.ToString(keyValuePair.Key) + "&" + Convert.ToString(dynamicValue2);
						this.subStitueOption.RepalceBomEntryIds.Add(key2, list7);
						this.subStitueOption.bomEntryIds.AddRange(list7);
						bomIds.Add(keyValuePair.Key);
					}
				}
			}
			list = (from w in batchReplaceBomItem
			where !bomIds.Distinct<long>().Contains(DataEntityExtend.GetDynamicValue<long>(w, "Id", 0L))
			select w).ToList<DynamicObject>();
			foreach (DynamicObject item in list)
			{
				batchReplaceBomItem.Remove(item);
			}
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x0004D6FC File Offset: 0x0004B8FC
		private bool isValidateRate(List<long> entryIds, DynamicObject keyBomEntryData, List<DynamicObject> rEntryDatas, DynamicObject keyMainItem, List<DynamicObject> noKeyMainItem, ref List<long> sEntryIds)
		{
			bool flag = true;
			foreach (DynamicObject dynamicObject in noKeyMainItem)
			{
				long noKeyMtrlId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MaterialID_Id", 0L);
				long noKeyBomId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L);
				long noKeyAuxProId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "AuxPropID_Id", 0L);
				List<DynamicObject> list = (from w in rEntryDatas
				where !entryIds.Contains(DataEntityExtend.GetDynamicValue<long>(w, "ENTRYID", 0L)) && DataEntityExtend.GetDynamicValue<long>(w, "MATERIALCHILDID_ID", 0L) == noKeyMtrlId && DataEntityExtend.GetDynamicValue<long>(w, "BOMID_ID", 0L) == noKeyBomId && DataEntityExtend.GetDynamicValue<long>(w, "AUXPROPID_ID", 0L) == noKeyAuxProId
				select w).ToList<DynamicObject>();
				if (ListUtils.IsEmpty<DynamicObject>(list))
				{
					flag = false;
					break;
				}
				foreach (DynamicObject dynamicObject2 in list)
				{
					decimal dynamicValue = DataEntityExtend.GetDynamicValue<decimal>(keyBomEntryData, "BASENUMERATOR", 0m);
					decimal dynamicValue2 = DataEntityExtend.GetDynamicValue<decimal>(keyBomEntryData, "DENOMINATOR", 0m);
					decimal dynamicValue3 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "BASENUMERATOR", 0m);
					decimal dynamicValue4 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "DENOMINATOR", 0m);
					decimal d = MathUtil.Round(dynamicValue * dynamicValue3 / (dynamicValue2 * dynamicValue4), 10, 0);
					decimal dynamicValue5 = DataEntityExtend.GetDynamicValue<decimal>(keyMainItem, "BaseNumerator", 0m);
					decimal dynamicValue6 = DataEntityExtend.GetDynamicValue<decimal>(keyMainItem, "Denominator", 0m);
					decimal dynamicValue7 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "BaseNumerator", 0m);
					decimal dynamicValue8 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Denominator", 0m);
					decimal d2 = MathUtil.Round(dynamicValue5 * dynamicValue7 / (dynamicValue6 * dynamicValue8), 10, 0);
					if (!(d != d2))
					{
						flag = true;
						long dynamicValue9 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "ENTRYID", 0L);
						entryIds.Add(dynamicValue9);
						sEntryIds.Add(dynamicValue9);
						break;
					}
					flag = false;
				}
				if (!flag)
				{
					break;
				}
			}
			return flag;
		}

		// Token: 0x040002C8 RID: 712
		private BatchSubStitueOption subStitueOption;

		// Token: 0x040002C9 RID: 713
		private Dictionary<long, IGrouping<long, DynamicObject>> resultReplaceBomDict;

		// Token: 0x040002CA RID: 714
		private List<IGrouping<long, DynamicObject>> groupBatchReplaceBomItem;

		// Token: 0x040002CB RID: 715
		protected List<NetworkCtrlResult> networkCtrlResults = new List<NetworkCtrlResult>();

		// Token: 0x040002CC RID: 716
		private List<string> mainItemRowIds = new List<string>();
	}
}
