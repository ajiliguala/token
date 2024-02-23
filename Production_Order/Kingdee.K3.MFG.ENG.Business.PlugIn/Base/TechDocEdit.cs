using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Attachment;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.Web.List;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCEntity;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.SFS;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000037 RID: 55
	[Description("技术文档表单插件")]
	[HotUpdate]
	public class TechDocEdit : BaseControlEdit
	{
		// Token: 0x06000407 RID: 1031 RVA: 0x000325BE File Offset: 0x000307BE
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			base.View.GetControl<TabControl>("FTabHead").SetFireSelChanged(true);
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x000325E0 File Offset: 0x000307E0
		public override void AfterBindData(EventArgs e)
		{
			IDynamicFormView view = base.View.GetView(base.View.PageId + "_ENG_Attachment_F7");
			if (base.View.Model.GetPKValue() == null || MFGBillUtil.GetValue<string>(base.View.Model, "FDocumentStatus", -1, null, null).Equals("Z"))
			{
				if (view != null)
				{
					view.Close();
					base.View.SendDynamicFormAction(view);
				}
			}
			else if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(view))
			{
				AttachmentKey attachmentKey = this.GetAttachmentKey();
				view.OpenParameter.SetCustomParameter("AttachmentKey", AttachmentKey.ConvertToString(attachmentKey));
				string filterString = string.Format("FBILLTYPE='{0}' and FINTERID='{1}' and FENTRYKEY='{2}' and FENTRYINTERID='{3}'", new object[]
				{
					attachmentKey.BillType,
					attachmentKey.BillInterID,
					attachmentKey.EntryKey,
					attachmentKey.EntryInterID
				});
				(view as IListView).OpenParameter.FilterParameter.FilterString = filterString;
				view.RefreshByFilter();
				base.View.SendDynamicFormAction(view);
			}
			else
			{
				this.ShowListForm_Att();
			}
			this.LoadPLMFileInfo();
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x000326F4 File Offset: 0x000308F4
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			base.AfterCopyData(e);
			base.View.Model.DeleteEntryData("FEntityM");
			base.View.Model.DeleteEntryData("FEntityMP");
			base.View.Model.DeleteEntryData("FEntityR");
			base.View.Model.DeleteEntryData("FEntityPLM");
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x0003275C File Offset: 0x0003095C
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(base.View.ParentFormView))
			{
				if (base.View.ParentFormView.Session != null && base.View.ParentFormView.Session.ContainsKey("tbCopy") && base.View.ParentFormView.Session["tbCopy"] != null)
				{
					base.View.Model.SetValue("FApplyScope", "F");
					base.View.GetControl("FApplyScope").Enabled = false;
					base.View.ParentFormView.Session.Remove("tbCopy");
				}
				if (base.View.ParentFormView.Session != null && base.View.ParentFormView.Session.ContainsKey("Edit") && base.View.ParentFormView.Session["Edit"] != null)
				{
					base.View.Model.SetValue("FApplyScope", "F");
					base.View.GetControl("FApplyScope").Enabled = false;
					base.View.ParentFormView.Session.Remove("Edit");
				}
				if (base.View.ParentFormView.Session != null && base.View.ParentFormView.Session.ContainsKey("MoObjects") && base.View.ParentFormView.Session["MoObjects"] != null)
				{
					Dictionary<long, Tuple<DynamicObject, DynamicObject>> dictionary = base.View.ParentFormView.Session["MoObjects"] as Dictionary<long, Tuple<DynamicObject, DynamicObject>>;
					int num = 0;
					base.View.Model.SetValue("FApplyScope", "F");
					base.View.GetControl("FApplyScope").Enabled = false;
					EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMO");
					DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
					foreach (KeyValuePair<long, Tuple<DynamicObject, DynamicObject>> keyValuePair in dictionary)
					{
						DynamicObject item = entryEntity.DynamicObjectType.CreateInstance() as DynamicObject;
						entityDataObject.Add(item);
						base.View.Model.SetValue("FMoNumber", Convert.ToString(keyValuePair.Value.Item1["BillNo"]), num);
						base.View.Model.SetValue("FMOEntryId", Convert.ToInt64(keyValuePair.Value.Item2["Id"]), num);
						base.View.Model.SetValue("FMoRowNumber", Convert.ToString(keyValuePair.Value.Item2["Seq"]), num);
						base.View.Model.SetValue("FMaterialId", Convert.ToInt64(keyValuePair.Value.Item2["MaterialId_Id"]), num);
						base.View.Model.SetValue("FRoutingId", Convert.ToInt64(keyValuePair.Value.Item2["RoutingId_Id"]), num);
						base.View.Model.SetValue("FWorkShopID", Convert.ToInt64(keyValuePair.Value.Item2["WorkShopID_Id"]), num);
						base.View.Model.SetValue("FUnitId", Convert.ToInt64(keyValuePair.Value.Item2["UnitId_Id"]), num);
						base.View.Model.SetValue("FBaseUnitId", Convert.ToInt64(keyValuePair.Value.Item2["BaseUnitId_Id"]), num);
						base.View.Model.SetValue("FQty", Convert.ToDecimal(keyValuePair.Value.Item2["Qty"]), num);
						base.View.Model.SetValue("FBaseUnitQty", Convert.ToDecimal(keyValuePair.Value.Item2["BaseUnitQty"]), num);
						base.View.Model.SetValue("FREMWorkShopId", Convert.ToInt64(keyValuePair.Value.Item2["REMWorkShopId_Id"]), num);
						base.View.Model.SetValue("FLot", Convert.ToInt64(keyValuePair.Value.Item2["Lot_Id"]), num);
						base.View.Model.SetValue("FBomId", Convert.ToInt64(keyValuePair.Value.Item2["BomId_Id"]), num);
						base.View.Model.SetValue("FCreatorIdMO", Convert.ToInt64(base.Context.UserId), num);
						DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(base.Context);
						base.View.Model.SetValue("FCreateDateMO", systemDateTime, num);
						num++;
					}
					base.View.ParentFormView.Session.Remove("MoObjects");
				}
			}
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x00032D10 File Offset: 0x00030F10
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(this.Model.DataObject, "ApplyScope", null);
			if (dynamicObjectItemValue.Equals("A"))
			{
				base.View.Model.DeleteEntryData("FEntityM");
				base.View.Model.DeleteEntryData("FEntityMP");
				base.View.Model.DeleteEntryData("FEntityR");
				return;
			}
			if (dynamicObjectItemValue.Equals("B"))
			{
				base.View.Model.DeleteEntryData("FEntityMP");
				base.View.Model.DeleteEntryData("FEntityR");
				return;
			}
			if (dynamicObjectItemValue.Equals("C"))
			{
				base.View.Model.DeleteEntryData("FEntityM");
				base.View.Model.DeleteEntryData("FEntityR");
				return;
			}
			if (dynamicObjectItemValue.Equals("D"))
			{
				base.View.Model.DeleteEntryData("FEntityM");
				base.View.Model.DeleteEntryData("FEntityMP");
			}
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x00032E58 File Offset: 0x00031058
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SAVE") && !(a == "SUBMIT"))
				{
					return;
				}
				string value = MFGBillUtil.GetValue<string>(base.View.Model, "FApplyScope", -1, null, null);
				if (value.Equals("C"))
				{
					Entity entity = base.View.Model.BusinessInfo.GetEntity("FEntityMP");
					List<DynamicObject> list = (from o in base.View.Model.GetEntityDataObject(entity)
					where DataEntityExtend.GetDynamicObjectItemValue<long>(o, "MPMaterialId_Id", 0L) == 0L && DataEntityExtend.GetDynamicObjectItemValue<long>(o, "MPProcessId_Id", 0L) == 0L
					select o).ToList<DynamicObject>();
					foreach (DynamicObject item in list)
					{
						base.View.Model.GetEntityDataObject(entity).Remove(item);
					}
					list = base.View.Model.GetEntityDataObject(entity).ToList<DynamicObject>();
					int num = 1;
					foreach (DynamicObject dynamicObject in list)
					{
						dynamicObject["SEQ"] = num++;
					}
					base.View.UpdateView("FEntityMP");
				}
			}
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x00032FEC File Offset: 0x000311EC
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SAVE"))
				{
					return;
				}
				IDynamicFormView view = base.View.GetView(base.View.PageId + "_ENG_Attachment_F7");
				if (base.View.Model.GetPKValue() != null && ObjectUtils.IsNullOrEmptyOrWhiteSpace(view))
				{
					this.ShowListForm_Att();
					base.View.GetControl<TabControl>("FTabHead").SelectedIndex = 0;
				}
			}
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x0003307C File Offset: 0x0003127C
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (key == "FDocumentStatus")
				{
					this.SetAttBtnEnable();
					return;
				}
				if (!(key == "FApplyScope"))
				{
					return;
				}
				this.DeletePLMEntry();
			}
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x000330C8 File Offset: 0x000312C8
		private void DeletePLMEntry()
		{
			string value = Convert.ToString(base.View.Model.GetValue("FApplyScope"));
			if ("C".Equals(value) || "D".Equals(value))
			{
				return;
			}
			base.View.Model.DeleteEntryData("FEntityPLM");
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x00033120 File Offset: 0x00031320
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (fieldKey == "FRouteId")
				{
					e.CustomParams.Add("ProduceType", "F");
					return;
				}
				if (fieldKey == "FIssuedDep")
				{
					OtherExtend.ConvertTo<ListShowParameter>(e.DynamicFormShowParameter, null).IsIsolationOrg = false;
					return;
				}
				if (fieldKey == "FMBBomId")
				{
					this.SetBomFilter(e);
					return;
				}
				if (!(fieldKey == "FMoNumber"))
				{
					return;
				}
				this.ShowMoListForm(e.Row);
			}
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x00033204 File Offset: 0x00031404
		private void ShowMoListForm(int row)
		{
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			if (value <= 0L)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请先选择生产组织", "015649000015809", 7, new object[0]), 0);
				return;
			}
			if (!this.ValidatePermission())
			{
				return;
			}
			DynamicObject dataObject = this.Model.DataObject;
			DynamicObjectCollection dynamicObjectCollection = dataObject["EntityMO"] as DynamicObjectCollection;
			List<long> list = new List<long>();
			if (dynamicObjectCollection.Count > 0)
			{
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					list.Add(Convert.ToInt64(dynamicObject["MOEntryId"]));
				}
			}
			ListSelBillShowParameter listSelBillShowParameter = new ListSelBillShowParameter
			{
				FormId = "PRD_MO",
				ParentPageId = base.View.PageId,
				IsShowApproved = true,
				IsLookUp = true,
				UseOrgId = value,
				ListFilterParameter = new ListRegularFilterParameter
				{
					SelectEntitys = new List<string>
					{
						"FBillHead",
						"FTreeEntity"
					},
					Filter = string.Format(" FProductType='1' AND FDocumentStatus = '{0}' AND FCancelStatus != '{1}' AND FIsSuspend!='1' AND FIsEntrust!='1' AND FStatus in ('{2}','{3}','{4}') AND FPRODUCTTYPE = '1'  {5} ", new object[]
					{
						'C',
						'B',
						4,
						5,
						3,
						(list.Distinct<long>().ToList<long>().Count > 0) ? string.Format(" AND {0}_{1} not in ( {2}) ", "FTreeEntity", "FEntryId", string.Join<long>(",", list.Distinct<long>().ToList<long>())) : string.Empty
					}),
					OrderBy = " FBILLNO DESC "
				}
			};
			base.View.ShowForm(listSelBillShowParameter, delegate(FormResult result)
			{
				if (result != null && result.ReturnData != null && (result.ReturnData as ListSelectedRowCollection).Count > 0)
				{
					ListSelectedRowCollection rows = (ListSelectedRowCollection)result.ReturnData;
					this.SetReturnValue(rows, row);
				}
			});
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x00033438 File Offset: 0x00031638
		private void SetReturnValue(ListSelectedRowCollection rows, int row)
		{
			long num = 0L;
			long num2 = 0L;
			if (!long.TryParse(rows[0].PrimaryKeyValue, out num) || num <= 0L)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请选择生产订单", "015649000015811", 7, new object[0]), 0);
				return;
			}
			if (!long.TryParse(rows[0].EntryPrimaryKeyValue, out num2) || num2 <= 0L)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请选择生产订单的一条分录", "015078000002333", 7, new object[0]), 0);
				return;
			}
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary = this.LoadMobillReferenceData((from x in rows
			select OtherExtend.ConvertTo<long>(x.EntryPrimaryKeyValue, 0L)).ToList<long>());
			if (dictionary == null)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("生产订单分录信息加载失败", "015649000015813", 7, new object[0]), "", 0);
				return;
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMO");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			for (int i = 0; i < rows.Count; i++)
			{
				if (i == 0)
				{
					base.View.Model.SetValue("FMoNumber", rows[i].BillNo, row);
					base.View.Model.SetValue("FMOEntryId", rows[i].EntryPrimaryKeyValue, row);
					object obj = ((DynamicObjectDataRow)rows[i].DataRow).DynamicObject["t1_FSeq"];
					base.View.Model.SetValue("FMoRowNumber", obj, row);
					base.View.Model.SetValue("FMaterialId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FMaterialId"], row);
					base.View.Model.SetValue("FRoutingId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FRoutingId"], row);
					base.View.Model.SetValue("FWorkShopID", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FWorkShopID"], row);
					base.View.Model.SetValue("FUnitId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FUnitId"], row);
					base.View.Model.SetValue("FBaseUnitId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FBaseUnitId"], row);
					base.View.Model.SetValue("FQty", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FQty"], row);
					base.View.Model.SetValue("FBaseUnitQty", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FBaseUnitQty"], row);
					base.View.Model.SetValue("FREMWorkShopId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FREMWorkShopId"], row);
					base.View.Model.SetValue("FLot", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FLot"], row);
					base.View.Model.SetValue("FBomId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FBomId"], row);
					DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(base.Context);
					base.View.Model.SetValue("FCreateDateMO", systemDateTime, row);
				}
				else
				{
					DynamicObject item = entryEntity.DynamicObjectType.CreateInstance() as DynamicObject;
					entityDataObject.Insert(row, item);
					base.View.Model.SetValue("FMoNumber", rows[i].BillNo, row);
					base.View.Model.SetValue("FMOEntryId", rows[i].EntryPrimaryKeyValue, row);
					object obj2 = ((DynamicObjectDataRow)rows[i].DataRow).DynamicObject["t1_FSeq"];
					base.View.Model.SetValue("FMoRowNumber", obj2, row);
					base.View.Model.SetValue("FMaterialId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FMaterialId"], row);
					base.View.Model.SetValue("FRoutingId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FRoutingId"], row);
					base.View.Model.SetValue("FWorkShopID", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FWorkShopID"], row);
					base.View.Model.SetValue("FUnitId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FUnitId"], row);
					base.View.Model.SetValue("FBaseUnitId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FBaseUnitId"], row);
					base.View.Model.SetValue("FQty", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FQty"], row);
					base.View.Model.SetValue("FBaseUnitQty", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FBaseUnitQty"], row);
					base.View.Model.SetValue("FREMWorkShopId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FREMWorkShopId"], row);
					base.View.Model.SetValue("FLot", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FLot"], row);
					base.View.Model.SetValue("FBomId", dictionary[OtherExtend.ConvertTo<long>(rows[i].EntryPrimaryKeyValue, 0L)].First<DynamicObject>()["FBomId"], row);
					DateTime systemDateTime2 = TimeServiceHelper.GetSystemDateTime(base.Context);
					base.View.Model.SetValue("FCreateDateMO", systemDateTime2, row);
				}
				row++;
			}
			base.View.UpdateView("FEntityMO");
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x00033BE0 File Offset: 0x00031DE0
		private bool ValidatePermission()
		{
			if (this.IsValidatedPermission == null)
			{
				this.IsValidatedPermission = new bool?(MFGCommonUtil.AuthPermissionBeforeShowF7Form(base.View, "PRD_MO", "6e44119a58cb4a8e86f6c385e14a17ad"));
			}
			bool flag = Convert.ToBoolean(this.IsValidatedPermission);
			if (!flag)
			{
				base.View.ShowMessage(ResManager.LoadKDString("没有目标单据的“查看”权限！", "015649000015810", 7, new object[0]), 0);
			}
			return flag;
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x00033C70 File Offset: 0x00031E70
		private Dictionary<long, IGrouping<long, DynamicObject>> LoadMobillReferenceData(List<long> moEntryId)
		{
			List<string> list = new List<string>();
			list.Add("FTreeEntity_FEntryId");
			list.Add("FWorkShopID");
			list.Add("FProjectNo");
			list.Add("FStockInOrgId");
			list.Add("FInStockOwnerTypeId");
			list.Add("FInStockOwnerId");
			list.Add("FInStockType");
			list.Add("FBFLowId");
			list.Add("FMTONO");
			list.Add("FRoutingId");
			list.Add("FStockId");
			list.Add("FStockLocId");
			list.Add("FMaterialId");
			list.Add("FUnitId");
			list.Add("FBaseUnitId");
			list.Add("FQty");
			list.Add("FBaseUnitQty");
			list.Add("FREMWorkShopId");
			list.Add("FLot");
			list.Add("FBomId");
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "PRD_MO",
				SelectItems = SelectorItemInfo.CreateItems(list.ToArray())
			};
			ExtJoinTableDescription item = new ExtJoinTableDescription
			{
				TableName = "table(fn_StrSplit(@PKValue,',',1))",
				TableNameAs = "sp",
				FieldName = "FId",
				ScourceKey = "FTreeEntity_FEntryId"
			};
			SqlParam item2 = new SqlParam("@PKValue", 161, moEntryId.Distinct<long>().ToArray<long>());
			queryBuilderParemeter.ExtJoinTables.Add(item);
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, new List<SqlParam>
			{
				item2
			});
			if (dynamicObjectCollection.Count > 0)
			{
				return (from x in dynamicObjectCollection
				group x by OtherExtend.ConvertTo<long>(x["FTreeEntity_FEntryId"], 0L)).ToDictionary((IGrouping<long, DynamicObject> x) => x.Key);
			}
			return null;
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x00033E5C File Offset: 0x0003205C
		private void SetBomFilter(BeforeF7SelectEventArgs e)
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["EntityMB"] as DynamicObjectCollection;
			long num = (dynamicObjectCollection[e.Row]["MBMaterialId_Id"] != null) ? Convert.ToInt64(dynamicObjectCollection[e.Row]["MBMaterialId_Id"]) : 0L;
			string text = string.Format(" FMATERIALID={0} and FDocumentStatus='C' and FForbidStatus='A' ", num);
			e.ListFilterParameter.Filter = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? text : string.Format("{0} AND {1}", e.ListFilterParameter.Filter, text));
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x00033F08 File Offset: 0x00032108
		public override void TabItemSelectedChange(TabItemSelectedChangeEventArgs e)
		{
			base.TabItemSelectedChange(e);
			string a;
			if ((a = e.TabKey.ToUpper()) != null)
			{
				if (!(a == "FTABHEAD_ATTLIST"))
				{
					if (!(a == "FTABHEAD_P"))
					{
						return;
					}
					if (base.View.Model.GetPKValue() == null || MFGBillUtil.GetValue<string>(base.View.Model, "FDocumentStatus", -1, null, null).Equals("Z"))
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("保存后才能使用PLM文档功能，请先保存单据！", "015072000017376", 7, new object[0]), "", 0);
					}
				}
				else
				{
					if (base.View.Model.GetPKValue() == null || MFGBillUtil.GetValue<string>(base.View.Model, "FDocumentStatus", -1, null, null).Equals("Z"))
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("保存后才能使用附件功能，请先保存单据！", "015072000012059", 7, new object[0]), "", 0);
						return;
					}
					this.SetAttBtnEnable();
					return;
				}
			}
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x0003400C File Offset: 0x0003220C
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBBUTTON_PLMADD")
				{
					this.OpenPLMFileSelectForm();
					return;
				}
				if (a == "TBBUTTON_PLMUPDATE")
				{
					this.UpdateSelectRowInfo();
					return;
				}
				if (!(a == "TBBUTTON_PLMPREVIEW"))
				{
					return;
				}
				this.BrowsePLMFile();
			}
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x00034070 File Offset: 0x00032270
		private void BrowsePLMFile()
		{
			List<int> list = (from i in base.View.GetControl<EntryGrid>("FEntityPLM").GetSelectedRows()
			orderby i
			select i).ToList<int>();
			if (list == null || list.Count != 1)
			{
				string text = ResManager.LoadKDString("请选择一个文件浏览", "015072000017377", 7, new object[0]);
				base.View.ShowErrMessage(text, "", 0);
				return;
			}
			DynamicObject[] array = (base.View.Model.DataObject["EntityPLM"] as DynamicObjectCollection).ToArray<DynamicObject>();
			string text2 = Convert.ToString(array[list[0]]["PLMFileId"]);
			SFSPLMPreviewServiceHelper.DoSFSBrowseFile(base.Context, base.View, text2);
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x00034144 File Offset: 0x00032344
		private void UpdateSelectRowInfo()
		{
			base.View.GetControl<EntryGrid>("FEntityPLM").GetSelectedRows();
			List<int> list = (from i in base.View.GetControl<EntryGrid>("FEntityPLM").GetSelectedRows()
			orderby i
			select i).ToList<int>();
			DynamicObject[] array = (base.View.Model.DataObject["EntityPLM"] as DynamicObjectCollection).ToArray<DynamicObject>();
			List<long> list2 = new List<long>();
			foreach (int num in list)
			{
				DynamicObject dynamicObject = array[num];
				string value = Convert.ToString(dynamicObject["PLMFileStatus"]);
				if (ResManager.LoadKDString("有效", "015072000017378", 7, new object[0]).Equals(value))
				{
					string text = ResManager.LoadKDString("有效状态的文件不需要更新", "015072000017379", 7, new object[0]);
					base.View.ShowErrMessage(text, "", 0);
					return;
				}
				list2.Add(Convert.ToInt64(dynamicObject["PLMFID"]));
			}
			SFCTechDocEntity.Instance.UpdatePLMFileInfo(base.Context, list2);
			this.LoadPLMFileInfo();
			string text2 = ResManager.LoadKDString("更新完成", "015072000017380", 7, new object[0]);
			base.View.ShowMessage(text2, 0);
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x000342F0 File Offset: 0x000324F0
		private void OpenPLMFileSelectForm()
		{
			string value = MFGBillUtil.GetValue<string>(base.View.Model, "FAPPLYSCOPE", -1, null, null);
			Dictionary<long, string> dictionary = new Dictionary<long, string>();
			if ("C".Equals(value))
			{
				EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMP");
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				using (IEnumerator<DynamicObject> enumerator = entityDataObject.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DynamicObject dynamicObject = enumerator.Current;
						DynamicObject dynamicObject2 = dynamicObject["MPMaterialId"] as DynamicObject;
						long key = Convert.ToInt64(dynamicObject2["Id"]);
						if (!dictionary.ContainsKey(key))
						{
							dictionary.Add(key, Convert.ToString(dynamicObject2["Number"]));
						}
					}
					goto IL_176;
				}
			}
			if ("D".Equals(value))
			{
				EntryEntity entryEntity2 = base.View.BusinessInfo.GetEntryEntity("FEntityR");
				DynamicObjectCollection entityDataObject2 = this.Model.GetEntityDataObject(entryEntity2);
				foreach (DynamicObject dynamicObject3 in entityDataObject2)
				{
					DynamicObject dynamicObject4 = dynamicObject3["RMaterialId"] as DynamicObject;
					if (dynamicObject4 != null)
					{
						long key2 = Convert.ToInt64(dynamicObject4["Id"]);
						if (!dictionary.ContainsKey(key2))
						{
							dictionary.Add(key2, Convert.ToString(dynamicObject4["Number"]));
						}
					}
				}
			}
			IL_176:
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.FormId = "ENG_PLMFileSelect";
			dynamicFormShowParameter.CustomComplexParams.Add("dicMaterialId", dictionary);
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult returnValue)
			{
				if (returnValue.ReturnData != null)
				{
					List<DynamicObject> lstFile = returnValue.ReturnData as List<DynamicObject>;
					this.BindDataToDevEntry(lstFile);
				}
			});
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x00034520 File Offset: 0x00032720
		private void BindDataToDevEntry(List<DynamicObject> lstFile)
		{
			if (lstFile == null)
			{
				return;
			}
			DynamicObject dynamicObject;
			int num;
			base.View.Model.TryGetEntryCurrentRow("FEntityPLM", ref dynamicObject, ref num);
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["EntityPLM"] as DynamicObjectCollection;
			DynamicObjectType dynamicCollectionItemPropertyType = dynamicObjectCollection.DynamicCollectionItemPropertyType;
			using (List<DynamicObject>.Enumerator enumerator = lstFile.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DynamicObject item = enumerator.Current;
					IEnumerable<DynamicObject> enumerable = from o in dynamicObjectCollection
					where Convert.ToInt64(o["PLMFID"]) == Convert.ToInt64(item["PLMFID"])
					select o;
					if (enumerable == null || enumerable.Count<DynamicObject>() <= 0)
					{
						DynamicObject dynamicObject2 = new DynamicObject(dynamicCollectionItemPropertyType);
						dynamicObject2["PLMFID"] = item["PLMFID"];
						dynamicObject2["PLMFileId"] = item["PLMFileId"];
						dynamicObject2["PLMFileName"] = Convert.ToString(item["PLMFileName"]);
						dynamicObject2["PLMVerNO"] = Convert.ToString(item["PLMVerNO"]);
						dynamicObject2["PLMBuildVer"] = Convert.ToString(item["PLMBuildVer"]);
						dynamicObject2["PLMMaxVer"] = Convert.ToString(item["PLMMaxVer"]);
						dynamicObject2["PLMMinVer"] = Convert.ToString(item["PLMMinVer"]);
						dynamicObject2["PLMCreatorName"] = Convert.ToString(item["PLMCreatorName"]);
						dynamicObject2["PLMModifyTime"] = item["PLMModifyTime"];
						dynamicObject2["PLMImportTime"] = MFGServiceHelper.GetSysDate(base.Context);
						dynamicObject2["PLMFileStatus"] = ResManager.LoadKDString("有效", "015072000017378", 7, new object[0]);
						dynamicObject2["PLMErpMaterialId_Id"] = Convert.ToInt64(item["MaterialId_Id"]);
						dynamicObject2["PLMErpMaterialId"] = item["MaterialId"];
						dynamicObjectCollection.Add(dynamicObject2);
					}
				}
			}
			if (num == -1)
			{
				num = 0;
			}
			base.View.UpdateView("FEntityPLM");
			this.Model.SetEntryCurrentRowIndex("FEntityPLM", num);
			base.View.SetEntityFocusRow("FEntityPLM", num);
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x00034810 File Offset: 0x00032A10
		private void LoadPLMFileInfo()
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["EntityPLM"] as DynamicObjectCollection;
			if (dynamicObjectCollection == null)
			{
				return;
			}
			List<long> list = new List<long>();
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				long item = Convert.ToInt64(dynamicObject["PLMFID"]);
				list.Add(item);
			}
			if (list.Count <= 0)
			{
				return;
			}
			List<DynamicObject> fileInfoByFileObjectIds4Pdf = SFCTechDocEntity.Instance.GetFileInfoByFileObjectIds4Pdf(base.Context, list);
			if (ListUtils.IsEmpty<DynamicObject>(fileInfoByFileObjectIds4Pdf))
			{
				return;
			}
			foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
			{
				long plmFid = Convert.ToInt64(dynamicObject2["PLMFID"]);
				DynamicObject dynamicObject3 = fileInfoByFileObjectIds4Pdf.FirstOrDefault((DynamicObject o) => Convert.ToInt64(o["FID"]) == plmFid);
				if (dynamicObject3 != null)
				{
					dynamicObject2["PLMFileName"] = Convert.ToString(dynamicObject3["FFILENAME"]);
					dynamicObject2["PLMVerNO"] = Convert.ToString(dynamicObject3["FVerNO"]);
					dynamicObject2["PLMBuildVer"] = Convert.ToString(dynamicObject3["FBuildVer"]);
					dynamicObject2["PLMMaxVer"] = Convert.ToString(dynamicObject3["FMaxVer"]);
					dynamicObject2["PLMMinVer"] = Convert.ToString(dynamicObject3["FMinVer"]);
					dynamicObject2["PLMCreatorName"] = Convert.ToString(dynamicObject3["FUSERNAME"]);
					dynamicObject2["PLMModifyTime"] = dynamicObject3["FMODIFYDATE"];
					string b = Convert.ToString(dynamicObject3["FFILEID"]);
					string a = Convert.ToString(dynamicObject2["PLMFileId"]);
					if (a == b)
					{
						dynamicObject2["PLMFileStatus"] = ResManager.LoadKDString("有效", "015072000017378", 7, new object[0]);
					}
					else
					{
						dynamicObject2["PLMFileStatus"] = ResManager.LoadKDString("无效", "015072000017381", 7, new object[0]);
					}
				}
				else
				{
					dynamicObject2["PLMFileStatus"] = ResManager.LoadKDString("无效", "015072000017381", 7, new object[0]);
				}
			}
			base.View.UpdateView("FEntityPLM");
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x00034AC0 File Offset: 0x00032CC0
		private void ShowListForm_Att()
		{
			AttachmentKey attachmentKey = this.GetAttachmentKey();
			string filter = string.Format("FBILLTYPE='{0}' and FINTERID='{1}' and FENTRYKEY='{2}' and FENTRYINTERID='{3}'", new object[]
			{
				attachmentKey.BillType,
				attachmentKey.BillInterID,
				attachmentKey.EntryKey,
				attachmentKey.EntryInterID
			});
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.IsLookUp = false;
			listShowParameter.CustomParams.Add("AttachmentKey", AttachmentKey.ConvertToString(attachmentKey));
			listShowParameter.OpenStyle.ShowType = 3;
			listShowParameter.OpenStyle.TagetKey = "FPanel_AttList";
			listShowParameter.Caption = ResManager.LoadKDString("附件管理", "015072000012060", 7, new object[0]);
			listShowParameter.FormId = "ENG_Attachment";
			listShowParameter.MultiSelect = false;
			listShowParameter.PageId = string.Format("{0}_{1}_F7", base.View.PageId, listShowParameter.FormId);
			listShowParameter.ListFilterParameter.Filter = filter;
			listShowParameter.IsShowQuickFilter = false;
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x00034BB8 File Offset: 0x00032DB8
		private AttachmentKey GetAttachmentKey()
		{
			return new AttachmentKey
			{
				BillType = base.View.BillBusinessInfo.GetForm().Id,
				BillNo = Convert.ToString(base.View.Model.DataObject["Number"]),
				BillInterID = Convert.ToString(base.View.Model.DataObject["Id"]),
				OperationStatus = base.View.OpenParameter.Status,
				EntryKey = " ",
				EntryInterID = "-1",
				RowIndex = 0
			};
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x00034C64 File Offset: 0x00032E64
		private void SetAttBtnEnable()
		{
			if (base.View.PageId != null)
			{
				string text = string.Format("{0}_{1}_F7", base.View.PageId, "ENG_Attachment");
				IDynamicFormView view = base.View.GetView(text);
				if (view != null)
				{
					ListView listView = (ListView)view;
					BarDataManager listMenu = listView.BillLayoutInfo.GetFormAppearance().ListMenu;
					listMenu.GetBarItem("tbbtnClose").Visible = 0;
					listMenu.GetBarItem("tbBatchNew").Visible = 1;
					if (MFGBillUtil.GetValue<string>(base.View.Model, "FDocumentStatus", -1, null, null).Equals("A") || MFGBillUtil.GetValue<string>(base.View.Model, "FDocumentStatus", -1, null, null).Equals("D"))
					{
						listMenu.GetBarItem("tbBatchNew").Enabled = 1;
						listMenu.GetBarItem("tbbtnDel").Enabled = 1;
						listMenu.GetBarItem("tbbtnEdit").Enabled = 1;
					}
					else
					{
						listMenu.GetBarItem("tbBatchNew").Enabled = 0;
						listMenu.GetBarItem("tbbtnDel").Enabled = 0;
						listMenu.GetBarItem("tbbtnEdit").Enabled = 0;
					}
					AttachmentKey attachmentKey = this.GetAttachmentKey();
					view.OpenParameter.SetCustomParameter("AttachmentKey", AttachmentKey.ConvertToString(attachmentKey));
					view.Refresh();
					base.View.SendDynamicFormAction(view);
				}
			}
		}

		// Token: 0x040001C3 RID: 451
		private string listPageId = SequentialGuid.NewGuid().ToString();

		// Token: 0x040001C4 RID: 452
		private bool? IsValidatedPermission = null;
	}
}
