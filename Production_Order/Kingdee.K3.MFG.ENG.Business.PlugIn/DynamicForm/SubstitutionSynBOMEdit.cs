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
using Kingdee.K3.Core.MFG.ENG.SynBom;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000B4 RID: 180
	public class SubstitutionSynBOMEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000D03 RID: 3331 RVA: 0x00099EE9 File Offset: 0x000980E9
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.InitData(false);
		}

		// Token: 0x06000D04 RID: 3332 RVA: 0x00099EF9 File Offset: 0x000980F9
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			this.StopNetworkCtrl(null);
		}

		// Token: 0x06000D05 RID: 3333 RVA: 0x0009A248 File Offset: 0x00098448
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
					this.View.ShowMessage(ResManager.LoadKDString("更新BOM表体没有数据，无需过滤，请点击刷新操作", "0151515153499000014053", 7, new object[0]), 0);
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
				MFGBillUtil.ShowFilterForm(this.View, "ENG_SynBom", tuple, delegate(FormResult filterResult)
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
				}, "ENG_SynBOMFilter", 0);
			}
		}

		// Token: 0x06000D06 RID: 3334 RVA: 0x0009A464 File Offset: 0x00098664
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			if (!StringUtils.EqualsIgnoreCase(e.Key, "FBTNOK"))
			{
				this.StopNetworkCtrl(null);
				this.View.Close();
				return;
			}
			List<long> selectedItems = this.GetSelectedItems();
			if (selectedItems.Count == 0)
			{
				e.Cancel = true;
				this.View.ShowMessage(ResManager.LoadKDString("请选择数据进行同步更新！", "0151515153499000014054", 7, new object[0]), 4);
				return;
			}
			IOperationResult item = new OperationResult();
			TaskProxyItem taskProxyItem = new TaskProxyItem();
			this.subStitueOption.ComputeId = taskProxyItem.TaskId;
			List<object> list = new List<object>
			{
				base.Context,
				selectedItems,
				this.subStitueOption,
				item
			};
			taskProxyItem.Parameters = list.ToArray();
			taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.SynBOM.SubstitutionSynBomService,Kingdee.K3.MFG.ENG.App.Core";
			taskProxyItem.MethodName = "UpdateBomServiceForSynBom";
			taskProxyItem.ProgressQueryInterval = 1;
			taskProxyItem.Title = ResManager.LoadKDString("替代方案同步更新-[物料清单]", "0151515153499000014055", 7, new object[0]);
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
			{
				this.StopNetworkCtrl(null);
			});
		}

		// Token: 0x06000D07 RID: 3335 RVA: 0x0009A594 File Offset: 0x00098794
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.DisableEntityRowByParam();
		}

		// Token: 0x06000D08 RID: 3336 RVA: 0x0009A5E8 File Offset: 0x000987E8
		private void InitData(bool isFilter)
		{
			if (!isFilter)
			{
				this.subStitueOption = MFGBillUtil.GetParam<SynBOMOption>(this.View, "SynBomOption", null);
				this.subStitueOption.SynBOMEntity = this.Model.BillBusinessInfo.GetEntryEntity("FSubEntity");
			}
			this.EntityBomItems = new List<DynamicObject>();
			IOperationResult subStitueBomData = this.GetSubStitueBomData(this.subStitueOption);
			List<DynamicObject> list = (List<DynamicObject>)subStitueBomData.FuncResult;
			List<IGrouping<long, DynamicObject>> list2 = new List<IGrouping<long, DynamicObject>>();
			if (isFilter && ListUtils.IsEmpty<DynamicObject>(list))
			{
				Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FSubEntity");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				entityDataObject.Clear();
				this.View.UpdateView("FSubEntity");
				return;
			}
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				return;
			}
			List<long> bomIds = (from o in list
			select DataEntityExtend.GetDynamicValue<long>(o, "BomId_Id", 0L)).Distinct<long>().ToList<long>();
			if (!ListUtils.IsEmpty<DynamicObject>(list))
			{
				list2 = (from x in list
				group x by DataEntityExtend.GetDynamicValue<long>(x, "BomId_Id", 0L)).ToList<IGrouping<long, DynamicObject>>();
				this.resultReplaceBomDict = list2.ToDictionary((IGrouping<long, DynamicObject> k) => k.Key);
			}
			this.networkCtrlResults = this.StartNetworkCtrl(bomIds, "Edit");
			List<long> unSuccessBomIds = new List<long>();
			this.ShowNetworkCtrl(this.networkCtrlResults, ref unSuccessBomIds);
			if (!ListUtils.IsEmpty<long>(unSuccessBomIds))
			{
				list2 = (from k in list2
				where !unSuccessBomIds.Contains(k.Key)
				select k).ToList<IGrouping<long, DynamicObject>>();
			}
			foreach (IGrouping<long, DynamicObject> collection in list2)
			{
				this.EntityBomItems.AddRange(collection);
			}
			this.EntityDataUpdate();
		}

		// Token: 0x06000D09 RID: 3337 RVA: 0x0009A80C File Offset: 0x00098A0C
		private void EntityDataUpdate()
		{
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FSubEntity");
			this.Model.BeginIniti();
			if (!ListUtils.IsEmpty<DynamicObject>(this.EntityBomItems))
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				entityDataObject.Clear();
				List<DynamicObject> list = this.RemoveUnMachMainItems(this.EntityBomItems.ToList<DynamicObject>());
				list = (from x in list
				orderby x["BomId_Id"], x["ReplaceGroup"], x["MATERIALTYPE"]
				select x).ToList<DynamicObject>();
				if (!ListUtils.IsEmpty<DynamicObject>(list))
				{
					foreach (DynamicObject item in list)
					{
						entityDataObject.Add(item);
					}
				}
				int index = 0;
				for (int i = 1; i < entityDataObject.Count<DynamicObject>(); i++)
				{
					if (entityDataObject[index]["BomId_Id"].Equals(entityDataObject[i]["BomId_Id"]) && DataEntityExtend.GetDynamicObjectItemValue<string>(entityDataObject[i], "MATERIALTYPE", null) != "1")
					{
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject[i], "DocumentStatus", "");
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject[i], "MaterialId", null);
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject[i], "MaterialId_Id", 0);
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject[i], "BomId", null);
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject[i], "BomId_Id", 0);
					}
					else
					{
						index = i;
					}
				}
			}
			this.Model.EndIniti();
			this.View.UpdateView("FSubEntity");
		}

		// Token: 0x06000D0A RID: 3338 RVA: 0x0009AA20 File Offset: 0x00098C20
		protected virtual void DisableEntityRowByParam()
		{
			DynamicObject dataObject = this.View.Model.DataObject;
			DynamicObjectCollection dynamicObjectCollection = dataObject["SubEntity"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "MATERIALTYPE", 0) != 1 || !DataEntityExtend.GetDynamicObjectItemValue<bool>(dynamicObject, "IskeyItem", false))
				{
					MFGBillUtil.SetEnabled(this.View, "FIsSelect", dynamicObject, false, "Disable");
					this.View.UpdateView("FIsSelect");
				}
			}
		}

		// Token: 0x06000D0B RID: 3339 RVA: 0x0009AAC8 File Offset: 0x00098CC8
		protected virtual IOperationResult GetSubStitueBomData(SynBOMOption synBomOption)
		{
			return SubstitutionSynBOMServiceHelper.GetSubStituteBomResult(base.Context, synBomOption);
		}

		// Token: 0x06000D0C RID: 3340 RVA: 0x0009AB10 File Offset: 0x00098D10
		protected List<long> GetSelectedItems()
		{
			List<long> result = new List<long>();
			DynamicObjectCollection source = this.View.Model.DataObject["SubEntity"] as DynamicObjectCollection;
			result = (from p in source
			where DataEntityExtend.GetDynamicValue<bool>(p, "IsSelect", true)
			select p into o
			select DataEntityExtend.GetDynamicValue<long>(o, "BomId_Id", 0L)).ToList<long>();
			this.subStitueOption.ReplaceGroup = (from p in source
			where DataEntityExtend.GetDynamicValue<bool>(p, "IsSelect", true)
			select p into o
			select DataEntityExtend.GetDynamicValue<long>(o, "ReplaceGroup", 0L)).ToList<long>();
			return result;
		}

		// Token: 0x06000D0D RID: 3341 RVA: 0x0009ABE4 File Offset: 0x00098DE4
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
					BillName = new LocaleValue(ResManager.LoadKDString("替代方案更新BOM", "0151515153499000014056", 7, new object[0]), base.Context.UserLocale.LCID),
					InterID = num.ToString(),
					OperationName = networkCtrlObject.NetCtrlName
				});
			}
			return NetworkCtrlServiceHelper.BatchBeginNetCtrl(base.Context, networkCtrlObject, list, false);
		}

		// Token: 0x06000D0E RID: 3342 RVA: 0x0009ACFC File Offset: 0x00098EFC
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
						operateResult.Name = DataEntityExtend.GetDynamicObjectItemValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(source.ToList<DynamicObject>().FirstOrDefault<DynamicObject>(), "BomId", null), "Number", null);
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

		// Token: 0x06000D0F RID: 3343 RVA: 0x0009AE60 File Offset: 0x00099060
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

		// Token: 0x06000D10 RID: 3344 RVA: 0x0009AF74 File Offset: 0x00099174
		private List<DynamicObject> RemoveUnMachMainItems(List<DynamicObject> synBomEntities)
		{
			List<IGrouping<long, DynamicObject>> list = (from x in synBomEntities
			group x by DataEntityExtend.GetDynamicObjectItemValue<long>(x, "BomId_Id", 0L)).ToList<IGrouping<long, DynamicObject>>();
			List<DynamicObject> list2 = new List<DynamicObject>();
			using (List<IGrouping<long, DynamicObject>>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IGrouping<long, DynamicObject> bomItem = enumerator.Current;
					List<IGrouping<int, DynamicObject>> list3 = (from x in bomItem
					where DataEntityExtend.GetDynamicObjectItemValue<int>(x, "MATERIALTYPE", 0) == 1
					group x by DataEntityExtend.GetDynamicObjectItemValue<int>(x, "ReplaceGroup", 0)).ToList<IGrouping<int, DynamicObject>>();
					using (List<IGrouping<int, DynamicObject>>.Enumerator enumerator2 = list3.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							IGrouping<int, DynamicObject> bomItemMinkey = enumerator2.Current;
							if (bomItemMinkey.Count<DynamicObject>() != this.subStitueOption.MainReplaceMaterialRows.Count<DynamicObject>())
							{
								list2.AddRange((from x in synBomEntities
								where DataEntityExtend.GetDynamicObjectItemValue<long>(x, "BomId_Id", 0L) == bomItem.Key && DataEntityExtend.GetDynamicObjectItemValue<int>(x, "ReplaceGroup", 0) == bomItemMinkey.Key
								select x).ToList<DynamicObject>());
							}
						}
					}
				}
			}
			list2.ForEach(delegate(DynamicObject x)
			{
				synBomEntities.Remove(x);
			});
			list2 = new List<DynamicObject>();
			List<DynamicObject> list4 = (from x in synBomEntities
			where DataEntityExtend.GetDynamicObjectItemValue<int>(x, "MATERIALTYPE", 0) == 1
			select x).ToList<DynamicObject>();
			List<string> list5 = new List<string>();
			foreach (DynamicObject dynamicObject in list4)
			{
				if (!this.subStitueOption.mainMaterialIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "MATERIALCHILDID_ID", 0L)) || !this.subStitueOption.mainEntryIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "STENTRYID", 0L)) || DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "SUBSTITUTIONID", 0L) != this.subStitueOption.SubstitutionId)
				{
					string item = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "BomId_Id", 0L) + "_" + DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "ReplaceGroup", 0);
					list5.Add(item);
				}
			}
			foreach (DynamicObject dynamicObject2 in synBomEntities)
			{
				string item2 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "BomId_Id", 0L) + "_" + DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject2, "ReplaceGroup", 0);
				if (list5.Contains(item2))
				{
					list2.Add(dynamicObject2);
				}
			}
			list2.ForEach(delegate(DynamicObject x)
			{
				synBomEntities.Remove(x);
			});
			return synBomEntities;
		}

		// Token: 0x040005ED RID: 1517
		private SynBOMOption subStitueOption;

		// Token: 0x040005EE RID: 1518
		private Dictionary<long, IGrouping<long, DynamicObject>> resultReplaceBomDict;

		// Token: 0x040005EF RID: 1519
		private List<DynamicObject> EntityBomItems;

		// Token: 0x040005F0 RID: 1520
		protected List<NetworkCtrlResult> networkCtrlResults = new List<NetworkCtrlResult>();

		// Token: 0x040005F1 RID: 1521
		private List<string> mainItemRowIds = new List<string>();
	}
}
