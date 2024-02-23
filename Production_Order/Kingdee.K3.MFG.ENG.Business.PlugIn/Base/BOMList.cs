using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Objects.Permission.Objects;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Common;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.BusinessCommon;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200000B RID: 11
	public class BOMList : BaseControlList
	{
		// Token: 0x17000010 RID: 16
		// (get) Token: 0x060001A8 RID: 424 RVA: 0x0001484E File Offset: 0x00012A4E
		// (set) Token: 0x060001A9 RID: 425 RVA: 0x00014856 File Offset: 0x00012A56
		private PermissionAuthResult authSynsResult { get; set; }

		// Token: 0x060001AA RID: 426 RVA: 0x00014938 File Offset: 0x00012B38
		public override void PrepareFilterParameter(FilterArgs e)
		{
			object customParameter = this.View.OpenParameter.GetCustomParameter("AssortReq");
			if (customParameter != null && customParameter.ToString() == "1")
			{
				List<ColumnField> columnInfo = this.ListModel.FilterParameter.ColumnInfo;
				List<Field> fieldList = this.ListModel.BillBusinessInfo.GetFieldList();
				ColumnField columnField = columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FUseOrgId.FName"));
				Field field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FUseOrgId"));
				if (columnField == null && field != null)
				{
					columnField = new ColumnField
					{
						Key = "FUseOrgId.FName",
						Caption = field.Name,
						ColIndex = field.ListTabIndex,
						ColType = 231,
						ColWidth = 100,
						CoreField = false,
						DefaultColWidth = 100,
						DefaultVisible = true,
						EntityCaption = field.Entity.Name,
						EntityKey = field.EntityKey,
						FieldName = "FUseOrgId_FName",
						IsHyperlink = false,
						Visible = true
					};
					this.ListModel.FilterParameter.ColumnInfo.Add(columnField);
				}
				columnField = columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FBOMCATEGORY"));
				field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FBOMCATEGORY"));
				if (columnField == null && field != null)
				{
					columnField = new ColumnField
					{
						Key = field.Key,
						Caption = field.Name,
						ColIndex = field.ListTabIndex,
						ColType = 167,
						ColWidth = 100,
						CoreField = false,
						DefaultColWidth = 100,
						DefaultVisible = true,
						EntityCaption = field.Entity.Name,
						EntityKey = field.EntityKey,
						FieldName = field.FieldName,
						IsHyperlink = false,
						Visible = true
					};
					this.ListModel.FilterParameter.ColumnInfo.Add(columnField);
				}
				columnField = columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FBOMUSE"));
				field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FBOMUSE"));
				if (columnField == null && field != null)
				{
					columnField = new ColumnField
					{
						Key = field.Key,
						Caption = field.Name,
						ColIndex = field.ListTabIndex,
						ColType = 167,
						ColWidth = 100,
						CoreField = false,
						DefaultColWidth = 100,
						DefaultVisible = true,
						EntityCaption = field.Entity.Name,
						EntityKey = field.EntityKey,
						FieldName = field.FieldName,
						IsHyperlink = false,
						Visible = true
					};
					this.ListModel.FilterParameter.ColumnInfo.Add(columnField);
				}
				columnField = columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FITEMMODEL"));
				field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FITEMMODEL"));
				if (columnField == null && field != null)
				{
					columnField = new ColumnField
					{
						Key = field.Key,
						Caption = field.Name,
						ColIndex = field.ListTabIndex,
						ColType = 167,
						ColWidth = 100,
						CoreField = false,
						DefaultColWidth = 100,
						DefaultVisible = true,
						EntityCaption = field.Entity.Name,
						EntityKey = field.EntityKey,
						FieldName = "FITEMMODEL",
						IsHyperlink = false,
						Visible = true
					};
					this.ListModel.FilterParameter.ColumnInfo.Add(columnField);
				}
				columnField = columnInfo.FirstOrDefault((ColumnField p) => StringUtils.EqualsIgnoreCase(p.Key, "FUNITID.FName"));
				field = fieldList.FirstOrDefault((Field p) => StringUtils.EqualsIgnoreCase(p.Key, "FUNITID"));
				if (columnField == null && field != null)
				{
					columnField = new ColumnField
					{
						Key = "FUNITID.FName",
						Caption = field.Name,
						ColIndex = field.ListTabIndex,
						ColType = 231,
						ColWidth = 100,
						CoreField = false,
						DefaultColWidth = 100,
						DefaultVisible = true,
						EntityCaption = field.Entity.Name,
						EntityKey = field.EntityKey,
						FieldName = "FUNITID_FName",
						IsHyperlink = false,
						Visible = true
					};
					this.ListModel.FilterParameter.ColumnInfo.Add(columnField);
				}
			}
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.FilterString) && e.FilterString.IndexOf("HighestBomVersion") >= 0)
			{
				List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(base.Context, new BusinessObject
				{
					Id = "ENG_BOM"
				}, "6e44119a58cb4a8e86f6c385e14a17ad");
				if (ListUtils.IsEmpty<long>(permissionOrg))
				{
					return;
				}
				List<long> hightVersionBomIdsByOrgIds = BOMServiceHelper.GetHightVersionBomIdsByOrgIds(base.Context, permissionOrg, true);
				if (!ListUtils.IsEmpty<long>(hightVersionBomIdsByOrgIds))
				{
					SqlParam item = new SqlParam("@PKValue", 161, hightVersionBomIdsByOrgIds.Distinct<long>().ToArray<long>());
					e.SqlParams = new List<SqlParam>();
					e.SqlParams.Add(item);
					string sqlWithCardinality = StringUtils.GetSqlWithCardinality(hightVersionBomIdsByOrgIds.Distinct<long>().Count<long>(), "@PKValue", 1, true);
					ExtJoinTableDescription extJoinTableDescription = new ExtJoinTableDescription();
					extJoinTableDescription.TableName = sqlWithCardinality;
					extJoinTableDescription.ScourceKey = "FID";
					extJoinTableDescription.FieldName = "FID";
					extJoinTableDescription.TableNameAs = "Tmp";
					e.ExtJoinTables = new List<ExtJoinTableDescription>();
					e.ExtJoinTables.Add(extJoinTableDescription);
					e.FilterString = e.FilterString.Replace("HighestBomVersion", "");
				}
			}
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.FilterString) && e.FilterString.IndexOf("HighestBomIncludeUnAudit") >= 0)
			{
				List<long> permissionOrg2 = PermissionServiceHelper.GetPermissionOrg(base.Context, new BusinessObject
				{
					Id = "ENG_BOM"
				}, "6e44119a58cb4a8e86f6c385e14a17ad");
				if (ListUtils.IsEmpty<long>(permissionOrg2))
				{
					return;
				}
				List<long> hightVersionBomIdsByOrgIds2 = BOMServiceHelper.GetHightVersionBomIdsByOrgIds(base.Context, permissionOrg2, false);
				if (!ListUtils.IsEmpty<long>(hightVersionBomIdsByOrgIds2))
				{
					SqlParam item2 = new SqlParam("@PKValue", 161, hightVersionBomIdsByOrgIds2.Distinct<long>().ToArray<long>());
					e.SqlParams = new List<SqlParam>();
					e.SqlParams.Add(item2);
					string sqlWithCardinality2 = StringUtils.GetSqlWithCardinality(hightVersionBomIdsByOrgIds2.Distinct<long>().Count<long>(), "@PKValue", 1, true);
					ExtJoinTableDescription extJoinTableDescription2 = new ExtJoinTableDescription();
					extJoinTableDescription2.TableName = sqlWithCardinality2;
					extJoinTableDescription2.ScourceKey = "FID";
					extJoinTableDescription2.FieldName = "FID";
					extJoinTableDescription2.TableNameAs = "Tmp";
					e.ExtJoinTables = new List<ExtJoinTableDescription>();
					e.ExtJoinTables.Add(extJoinTableDescription2);
					e.FilterString = e.FilterString.Replace("HighestBomIncludeUnAudit", "");
				}
			}
			bool flag = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("FromEcnOrder"));
			if (flag)
			{
				bool flag2 = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("ECNCobyEntity"));
				if (flag2)
				{
					if (ListUtils.IsEmpty<FilterEntity>(from i in e.SelectedEntities
					where StringUtils.EqualsIgnoreCase(i.Key, "FEntryBOMCOBY")
					select i))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("当前过滤方案未勾选联副产品页签，请修改过滤方案", "015072030041390", 7, new object[0]), "", 0);
						e.FilterString = "1=0";
					}
				}
				bool flag3 = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("ECNTreeEntity"));
				if (flag3)
				{
					if (ListUtils.IsEmpty<FilterEntity>(from i in e.SelectedEntities
					where StringUtils.EqualsIgnoreCase(i.Key, "FTreeEntity")
					select i))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("当前过滤方案未勾选子项明细页签，请修改过滤方案", "015072030041391", 7, new object[0]), "", 0);
						e.FilterString = "1=0";
					}
				}
			}
		}

		// Token: 0x060001AB RID: 427 RVA: 0x000152C8 File Offset: 0x000134C8
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			switch (barItemKey = e.BarItemKey)
			{
			case "tbBomTree":
				this.BomTreeViewEdit("ENG_BOMTREE");
				return;
			case "tbBomConfig":
				this.BomConfigViewEdit("ENG_BOMCONFIG");
				return;
			case "tbBomSynUpdate":
				if (MFGCommonUtil.IsOnlyQueryUser(base.Context))
				{
					this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("用户“{0}”为仅查询许可用户，不能执行 “同步更新” 操作，请联系系统管理员！", "015072030034925", 7, new object[0]), base.Context.UserName), "", 0);
					e.Cancel = true;
					return;
				}
				this.ShowListDatas("tbBomSynUpdate", null);
				return;
			case "tbViewBomCache":
			{
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
				{
					FormId = "Eng_BomVersionCache",
					PageId = Guid.NewGuid().ToString()
				};
				this.View.ShowForm(dynamicFormShowParameter);
				return;
			}
			case "tbBulkEdit":
			{
				long num2 = 0L;
				if (this.ListView.CurrentSelectedRowInfo == null || !long.TryParse(this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue, out num2) || num2 <= 0L)
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "015072000018122", 7, new object[0]), "", 0);
					return;
				}
				DynamicFormShowParameter dynamicFormShowParameter2 = new DynamicFormShowParameter();
				dynamicFormShowParameter2.FormId = "ENG_BOMBulkEdit";
				dynamicFormShowParameter2.PageId = Guid.NewGuid().ToString();
				dynamicFormShowParameter2.CustomComplexParams.Add("fPKEntryIds", this.ListView.SelectedRowsInfo.GetPKEntryIdValues());
				this.View.ShowForm(dynamicFormShowParameter2);
				return;
			}
			case "tbQueryEcn":
				this.OpenEcnList(e);
				return;
			case "tbForbid":
			{
				FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ENG_BOM_BILLPARAM", true);
				DynamicObject dynamicObject = UserParamterServiceHelper.Load(this.View.Context, formMetadata.BusinessInfo, this.View.Context.UserId, "ENG_BOM", "UserParameter");
				this.IsEnablelForbidreason = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "EnablelForbidreason", false);
				if (this.IsEnablelForbidreason)
				{
					e.Cancel = true;
					DynamicFormShowParameter dynamicFormShowParameter3 = new DynamicFormShowParameter();
					dynamicFormShowParameter3.FormId = "ENG_FORBIDREASON";
					dynamicFormShowParameter3.PageId = Guid.NewGuid().ToString();
					dynamicFormShowParameter3.OpenStyle.ShowType = 6;
					this.View.ShowForm(dynamicFormShowParameter3, delegate(FormResult x)
					{
						object returnData = x.ReturnData;
						if (StringUtils.EqualsIgnoreCase(returnData.ToString(), "FBTNCANCEL"))
						{
							e.Cancel = true;
							return;
						}
						if (ObjectUtils.IsNullOrEmpty(x.ReturnData))
						{
							this.forbidReason = null;
						}
						else
						{
							this.forbidReason = returnData.ToString();
						}
						this.View.InvokeFormOperation("Forbid");
					});
					return;
				}
				break;
			}
			case "tbBomIntCheck":
				this.BomIntegrityCheck();
				break;

				return;
			}
		}

		// Token: 0x060001AC RID: 428 RVA: 0x00015610 File Offset: 0x00013810
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string key;
			switch (key = e.Operation.FormOperation.Operation.ToUpperInvariant())
			{
			case "BATCHEDIT":
				this.ShowBomBatchEdit();
				return;
			case "RETURNDATA":
			case "RETURNINSERTDATA":
			{
				this.checkDataRule(e);
				if (e.Cancel)
				{
					return;
				}
				IDynamicFormView parentFormView = this.View.ParentFormView;
				if (parentFormView == null)
				{
					return;
				}
				if (parentFormView.OpenParameter.FormId == "ENG_BomQueryForward2")
				{
					DynamicObjectCollection mfgoptionParam = MFGServiceHelper.GetMFGOptionParam(base.Context, 1L, "QueryBomCount");
					long num2 = 50L;
					if (!ListUtils.IsEmpty<DynamicObject>(mfgoptionParam))
					{
						num2 = DataEntityExtend.GetDynamicValue<long>(mfgoptionParam.First<DynamicObject>(), "FPARAMVALUE", 0L);
					}
					if ((long)this.ListView.SelectedRowsInfo.Count > num2)
					{
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("选择的BOM不能超过{0}个！", "015072000028831", 7, new object[0]), num2), "", 0);
						e.Cancel = true;
						return;
					}
				}
				break;
			}
			case "BULKEDITAUXPTY":
				this.BomAuxPtyBulkEdit();
				e.Cancel = true;
				return;
			case "REPLACEDELETE":
				this.ReplaceDelete();
				e.Cancel = true;
				return;
			case "GETREPLACE":
				this.GetReplaceLst();
				e.Cancel = true;
				return;
			case "REGETREPLACE":
				e.Cancel = true;
				this.ReGetReplaceLst();
				return;
			case "ADDTODATACOLLECTION":
			{
				this.checkDataRule(e);
				bool cancel = e.Cancel;
				break;
			}

				return;
			}
		}

		// Token: 0x060001AD RID: 429 RVA: 0x000157FC File Offset: 0x000139FC
		protected bool GetAuthDetailDataRulePassed(List<string> pkIds)
		{
			FilterObjectByDataRuleParamenter filterObjectByDataRuleParamenter = new FilterObjectByDataRuleParamenter(this.View.BillBusinessInfo, pkIds);
			ObjectIdDetialRuleAuthedResult objectIdDetialRuleAuthedResult = PermissionServiceHelper.AuthDetailDataRule(this.View.Context, filterObjectByDataRuleParamenter);
			return objectIdDetialRuleAuthedResult == null || objectIdDetialRuleAuthedResult.Passed;
		}

		// Token: 0x060001AE RID: 430 RVA: 0x00015840 File Offset: 0x00013A40
		private void checkDataRule(BeforeDoOperationEventArgs e)
		{
			string formId = ViewUtils.GetFormId(this.View.ParentFormView);
			if (StringUtils.EqualsIgnoreCase(formId.ToUpperInvariant(), "ENG_BOMQUERYINTEGRATION") || StringUtils.EqualsIgnoreCase(formId.ToUpperInvariant(), "ENG_BOMQUERYFORWARD2"))
			{
				IListView listView = this.View as IListView;
				if (listView.SelectedRowsInfo.Count == 0)
				{
					this.View.ShowMessage(ResManager.LoadKDString("选中的行为空", "015072000026363", 7, new object[0]), 0);
					e.Cancel = true;
					return;
				}
				ListSelectedRowCollection selectedRowsInfo = listView.SelectedRowsInfo;
				List<string> pkIds = (from p in selectedRowsInfo
				select p.PrimaryKeyValue).ToList<string>();
				if (!this.GetAuthDetailDataRulePassed(pkIds))
				{
					this.View.ShowMessage(ResManager.LoadKDString("选中的行中存在权限不够的数据，不允许返回和添加备选", "015072000026364", 7, new object[0]), 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x060001AF RID: 431 RVA: 0x00015950 File Offset: 0x00013B50
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string operation;
			if ((operation = e.Operation.Operation) != null)
			{
				if (!(operation == "Audit"))
				{
					if (!(operation == "Forbid"))
					{
						if (!(operation == "Enable"))
						{
							return;
						}
						List<object> list = (from p in e.OperationResult.OperateResult
						select p.PKValue).ToList<object>();
						if (ListUtils.IsEmpty<object>(list))
						{
							return;
						}
						string[] array = (from s in list
						select Convert.ToString(s)).ToArray<string>();
						if (!ListUtils.IsEmpty<string>(array))
						{
							FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true);
							DynamicObject[] array2 = BusinessDataServiceHelper.Load(base.Context, array, formMetadata.BusinessInfo.GetDynamicObjectType());
							foreach (DynamicObject dynamicObject in array2)
							{
								if (!ListUtils.IsEmpty<string>(array) && array.Contains(DataEntityExtend.GetDynamicValue<string>(dynamicObject, "Id", null)))
								{
									DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ForbidReson", null);
								}
							}
							if (!ListUtils.IsEmpty<DynamicObject>(array2))
							{
								BusinessDataServiceHelper.Save(base.Context, array2);
							}
						}
					}
					else if (this.IsEnablelForbidreason)
					{
						List<object> list2 = (from p in e.OperationResult.OperateResult
						select p.PKValue).ToList<object>();
						if (ListUtils.IsEmpty<object>(list2))
						{
							return;
						}
						string[] array4 = (from s in list2
						select Convert.ToString(s)).ToArray<string>();
						if (!ListUtils.IsEmpty<string>(array4))
						{
							FormMetadata formMetadata2 = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true);
							DynamicObject[] array5 = BusinessDataServiceHelper.Load(base.Context, array4, formMetadata2.BusinessInfo.GetDynamicObjectType());
							foreach (DynamicObject dynamicObject2 in array5)
							{
								if (!ListUtils.IsEmpty<string>(array4) && array4.Contains(DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "Id", null)))
								{
									DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ForbidReson", this.forbidReason);
								}
							}
							if (!ListUtils.IsEmpty<DynamicObject>(array5))
							{
								BusinessDataServiceHelper.Save(base.Context, array5);
								return;
							}
						}
					}
				}
				else
				{
					DynamicObject parameterData = this.Model.ParameterData;
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(parameterData, "FUpdateType", null);
					if (dynamicValue == "1" && e.OperationResult.OperateResult.GetSuccessResult().Count > 0)
					{
						List<object> list3 = new List<object>();
						for (int k = 0; k < e.OperationResult.OperateResult.GetSuccessResult().Count; k++)
						{
							list3.Add(e.OperationResult.OperateResult.GetSuccessResult()[k].PKValue);
						}
						this.ShowListDatas("tbApprove", list3);
						return;
					}
				}
			}
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x00015C68 File Offset: 0x00013E68
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			bool flag = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("IsSynsBom"));
			if (flag)
			{
				this.View.GetMainBarItem("tbReturn").Visible = false;
				this.View.GetMainBarItem("tbClose").Visible = false;
				this.View.GetMainBarItem("tbSplitNew").Visible = false;
			}
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x00015CE0 File Offset: 0x00013EE0
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (e.EventName == "GetBomDatas")
			{
				this.SendBomDataToParent(e, "BomDatas", "IsBomDatas");
			}
			if (e.EventName.Equals("UnitGetBomDatas"))
			{
				this.SendBomDataToParent(e, "UnitBomDatas", "FormUnitBomDatas");
			}
			if (e.EventName == "RefreashData")
			{
				this.View.InvokeFormOperation("Refresh");
			}
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x00015D60 File Offset: 0x00013F60
		private bool VaildateIsHavePermission(string perItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = perItemId,
				SubSystemId = this.View.Model.SubSytemId
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			return permissionAuthResult.Passed;
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x00015DA8 File Offset: 0x00013FA8
		private void BomTreeViewEdit(string formId)
		{
			if (!this.VaildateIsHavePermission("ENG_BOMTREE"))
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有BOM“树形维护”的“查看”权限！", "015072000003453", 7, new object[0]), 0);
				return;
			}
			long num = 0L;
			if (this.ListView.CurrentSelectedRowInfo == null || !long.TryParse(this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue, out num) || num <= 0L)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有可BOM树形维护的数据，请选中要操作的物料清单！", "015072000002178", 7, new object[0]), "", 0);
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.FormId = formId;
			dynamicFormShowParameter.CustomParams["BOMID"] = num.ToString();
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x00015E78 File Offset: 0x00014078
		private void BomConfigViewEdit(string formId)
		{
			if (!this.VaildateIsHavePermission("ENG_BOMCONFIG"))
			{
				this.View.ShowMessage(string.Format(ResManager.LoadKDString("没有“产品配置”的“查看”权限！", "015072000002179", 7, new object[0]), new object[0]), 0);
				return;
			}
			long num = 0L;
			if (this.ListView.CurrentSelectedRowInfo == null || !long.TryParse(this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue, out num) || num <= 0L)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有可产品配置的数据，请选中要操作的物料清单！", "015072000002180", 7, new object[0]), "", 0);
				return;
			}
			DynamicObject dynamicObject;
			DynamicObject configBom = BOMServiceHelper.GetConfigBom(this.View.Context, num, ref dynamicObject);
			if (configBom == null)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("不存在配置BOM，不能进行产品配置！", "015072000002181", 7, new object[0]), "", 0);
				return;
			}
			if (dynamicObject != null && DataEntityExtend.GetDynamicObjectItemValue<char>(dynamicObject, "ForbidStatus", '\0') == 'B')
			{
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("标准BOM“{0}”为禁用状态，不能进行产品配置！", "015072000002182", 7, new object[0]), DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "Number", null)), "", 0);
				return;
			}
			if (!base.IsUseabled(configBom))
			{
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("配置BOM“{0}”为非审核或禁用状态，不能进行产品配置！", "015072000002183", 7, new object[0]), DataEntityExtend.GetDynamicObjectItemValue<string>(configBom, "Number", null)), "", 0);
				return;
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = "ENG_BOMCONFIG",
				SubSystemId = this.View.Model.SubSytemId
			}, "fce8b1aca2144beeb3c6655eaf78bc34");
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.OpenStyle.ShowType = 7;
			billShowParameter.FormId = formId;
			billShowParameter.PKey = DataEntityExtend.GetDynamicObjectItemValue<long>(configBom, "Id", 0L).ToString();
			if (dynamicObject != null)
			{
				billShowParameter.CustomParams["STANDBOMID"] = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L).ToString();
			}
			billShowParameter.Status = (permissionAuthResult.Passed ? 0 : 1);
			this.View.ShowForm(billShowParameter);
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x000160A4 File Offset: 0x000142A4
		private void ShowBomBatchEdit()
		{
			if (!this.VaildateIsHavePermission("ENG_BOMBATCHEDIT"))
			{
				this.View.ShowMessage(string.Format(ResManager.LoadKDString("您在【{0}】组织下没有【物料清单批量维护】的【查看】权限，请联系系统管理员", "015072000025054", 7, new object[0]), this.View.Context.CurrentOrganizationInfo.Name), 0);
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.FormId = "ENG_BOMBATCHEDIT";
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x00016140 File Offset: 0x00014340
		private void BomAuxPtyBulkEdit()
		{
			long num = 0L;
			if (this.ListView.CurrentSelectedRowInfo == null || !long.TryParse(this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue, out num) || num <= 0L)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "015072000018122", 7, new object[0]), "", 0);
				return;
			}
			long item = 0L;
			long.TryParse(this.ListView.SelectedRowsInfo.FirstOrDefault<ListSelectedRow>().EntryPrimaryKeyValue, out item);
			IEnumerable<DynamicObject> bomEntryDatas = BOMServiceHelper.GetBomEntryDatas(this.View.Context, new List<long>
			{
				item
			}, true);
			if (this.ListView.SelectedRowsInfo.Count > 1)
			{
				List<long> list = (from s in this.ListView.SelectedRowsInfo
				select Convert.ToInt64(s.EntryPrimaryKeyValue)).Distinct<long>().ToList<long>();
				IEnumerable<DynamicObject> bomEntryDatas2 = BOMServiceHelper.GetBomEntryDatas(this.View.Context, list, true);
				List<long> list2 = (from s in bomEntryDatas2
				select DataEntityExtend.GetDynamicValue<long>(s, "MATERIALIDCHILD_Id", 0L)).Distinct<long>().ToList<long>();
				if (list2.Count > 1)
				{
					this.View.ShowMessage(ResManager.LoadKDString("勾选了多个不同的子项物料，请重新选择。建议先按子项物料编码过滤，然后再选择需要批改的物料清单子项", "015072000014787", 7, new object[0]), 0);
					return;
				}
			}
			DynamicObject dynamicObject = bomEntryDatas.FirstOrDefault<DynamicObject>();
			long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "MATERIALIDCHILD_Id", 0L);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.FormId = "ENG_BOMAuxPtyBulkEdit";
			dynamicFormShowParameter.CustomParams.Add("MaterialId", Convert.ToString(dynamicObjectItemValue));
			dynamicFormShowParameter.CustomComplexParams.Add("fPKEntryIds", this.ListView.SelectedRowsInfo.GetPKEntryIdValues());
			dynamicFormShowParameter.CustomComplexParams.Add("fPKIds", this.ListView.SelectedRowsInfo.GetPrimaryKeyValues());
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x00016428 File Offset: 0x00014628
		private void ShowListDatas(string barItemName, List<object> successfpkId)
		{
			if (ObjectUtils.IsNullOrEmpty(this.authSynsResult))
			{
				this.authSynsResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
				{
					Id = "ENG_BOM"
				}, "55488307023b99");
			}
			if (!this.authSynsResult.Passed)
			{
				this.View.ShowMessage("您没有【物料清单】的【同步更新】权限，请联系系统管理员！", 0);
				return;
			}
			long num = 0L;
			if (this.ListView.CurrentSelectedRowInfo == null || !long.TryParse(this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue, out num) || num <= 0L)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "015072000018122", 7, new object[0]), "", 0);
				return;
			}
			IEnumerable<object> source = (from s in this.ListView.SelectedRowsInfo.GetPrimaryKeyValues()
			select s).Distinct<object>();
			List<DynamicObject> list = MFGServiceHelper.LoadWithCache(base.Context, source.ToArray<object>(), this.View.Model.BillBusinessInfo.GetDynamicObjectType(), false, null).ToList<DynamicObject>();
			DynamicObject parameterData = this.Model.ParameterData;
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(parameterData, "FUpdateType", null);
			if (dynamicValue == "1" && barItemName == "tbApprove")
			{
				list = MFGServiceHelper.LoadWithCache(base.Context, successfpkId.ToArray(), this.View.Model.BillBusinessInfo.GetDynamicObjectType(), false, null).ToList<DynamicObject>();
			}
			new List<DynamicObject>();
			list = (from w in list
			where DataEntityExtend.GetDynamicValue<string>(w, "DocumentStatus", null) == "C" && DataEntityExtend.GetDynamicValue<string>(w, "ForbidStatus", null) == "A"
			select w).ToList<DynamicObject>();
			if (list.Count == 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("选择的BOM数据状态不为已审核或禁用状态为禁用，请重新选择数据", "015072000018123", 7, new object[0]), "", 0);
				return;
			}
			List<long> bomIds = new List<long>();
			List<long> userOrgIds = new List<long>();
			List<long> bomMasterIds = new List<long>();
			list.ForEach(delegate(DynamicObject bomData)
			{
				bomIds.Add(DataEntityExtend.GetDynamicValue<long>(bomData, "ID", 0L));
				userOrgIds.Add(DataEntityExtend.GetDynamicValue<long>(bomData, "UseOrgId_Id", 0L));
			});
			bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(parameterData, "FPrdPPBOM", false);
			bool dynamicValue3 = DataEntityExtend.GetDynamicValue<bool>(parameterData, "FSubPPBOM", false);
			bool dynamicValue4 = DataEntityExtend.GetDynamicValue<bool>(parameterData, "PLBOM", false);
			if (dynamicValue == "1" && barItemName == "tbBomSynUpdate")
			{
				this.View.ShowMessage(ResManager.LoadKDString("用户参数选择更新方式是审核时更新", "015072000018119", 7, new object[0]), 0);
				return;
			}
			if (!dynamicValue2 && !dynamicValue3 && !dynamicValue4)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("用户参数未选择任何需要同步的订单", "015072000018120", 7, new object[0]), "", 0);
				return;
			}
			string dynamicValue5 = DataEntityExtend.GetDynamicValue<string>(parameterData, "UpdateRange", null);
			if (dynamicValue5 == "2")
			{
				bomMasterIds = (from w in list
				where DataEntityExtend.GetDynamicValue<long>(w, "UseOrgId_Id", 0L) == DataEntityExtend.GetDynamicValue<long>(w, "CreateOrgId_Id", 0L)
				select w into s
				select DataEntityExtend.GetDynamicValue<long>(s, "MsterId", 0L)).ToList<long>();
				List<DynamicObject> allocatedBOM = this.GetAllocatedBOM(bomMasterIds);
				if (!ListUtils.IsEmpty<DynamicObject>(allocatedBOM))
				{
					userOrgIds.AddRange((from s in allocatedBOM
					select DataEntityExtend.GetDynamicValue<long>(s, "UseOrgId_Id", 0L)).ToList<long>().Except(userOrgIds));
					bomIds.AddRange((from s in allocatedBOM
					select DataEntityExtend.GetDynamicValue<long>(s, "Id", 0L)).ToList<long>().Except(bomIds));
					foreach (DynamicObject dynamicObject in allocatedBOM)
					{
						long bomId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "Id", 0L);
						if (!list.Any((DynamicObject a) => DataEntityExtend.GetDynamicValue<long>(a, "Id", 0L) == bomId))
						{
							list.Add(dynamicObject);
						}
					}
				}
			}
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true);
			DBServiceHelper.LoadReferenceObject(base.Context, list.ToArray(), formMetadata.BusinessInfo.GetDynamicObjectType(), true);
			List<long> first = new List<long>();
			List<long> bomBackwardByVirtualBom = BomSyncBackwardUtil.GetBomBackwardByVirtualBom(base.Context, list, ref first);
			if (!ListUtils.IsEmpty<long>(bomBackwardByVirtualBom))
			{
				bomIds.AddRange(bomBackwardByVirtualBom.Except(bomIds));
				userOrgIds.AddRange(first.Except(userOrgIds));
				DynamicObject[] source2 = BusinessDataServiceHelper.Load(base.Context, (from i in bomIds.Distinct<long>()
				select i).ToArray<object>(), formMetadata.BusinessInfo.GetDynamicObjectType());
				list = source2.ToList<DynamicObject>();
			}
			string dynamicValue6 = DataEntityExtend.GetDynamicValue<string>(parameterData, "ConSultDate", null);
			bool dynamicValue7 = DataEntityExtend.GetDynamicValue<bool>(parameterData, "IsSkipExpand", false);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 4;
			dynamicFormShowParameter.FormId = "ENG_SYNSUPDATEPPBOM";
			dynamicFormShowParameter.CustomComplexParams.Add("BomData", list);
			dynamicFormShowParameter.CustomComplexParams.Add("BomId", bomIds);
			dynamicFormShowParameter.CustomComplexParams.Add("UserOrgId", userOrgIds);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowPrdList", dynamicValue2);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowSubList", dynamicValue3);
			dynamicFormShowParameter.CustomComplexParams.Add("ConSultDate", dynamicValue6);
			dynamicFormShowParameter.CustomComplexParams.Add("IsSkipExpand", dynamicValue7);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowPlnList", dynamicValue4);
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x00016A4C File Offset: 0x00014C4C
		private List<DynamicObject> GetAllocatedBOM(List<long> bomMasterIds)
		{
			if (ListUtils.IsEmpty<long>(bomMasterIds))
			{
				return null;
			}
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.BusinessInfo = this.View.Model.BillBusinessInfo;
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				TableName = "table(fn_StrSplit(@msterId,',',1))",
				TableNameAs = "tms",
				FieldName = "FID",
				ScourceKey = "FMASTERID"
			});
			queryBuilderParemeter.FilterClauseWihtKey = "FCreateOrgId<>FUseOrgId";
			queryBuilderParemeter.SqlParams.Add(new SqlParam("@msterId", 161, bomMasterIds.Distinct<long>().ToArray<long>()));
			return BusinessDataServiceHelper.Load(base.Context, queryBuilderParemeter.BusinessInfo.GetDynamicObjectType(), queryBuilderParemeter).ToList<DynamicObject>();
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x00016D50 File Offset: 0x00014F50
		private void ReplaceDelete()
		{
			long num = 0L;
			if (this.ListView.CurrentSelectedRowInfo == null || !long.TryParse(this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue, out num) || num <= 0L)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "015072000018122", 7, new object[0]), "", 0);
				return;
			}
			List<long> bomIds = (from s in this.ListView.SelectedRowsInfo.GetPrimaryKeyValues()
			select Convert.ToInt64(s)).ToList<long>();
			this.View.ShowMessage(ResManager.LoadKDString("您确定要对所选分录进行替代删除吗？", "015072000014915", 7, new object[0]), 1, delegate(MessageBoxResult result)
			{
				if (result == 1)
				{
					List<NetworkCtrlResult> networkCtrlResults;
					this.StartNetworkCtrl("fae2446c-66e3-4cfe-83ab-5c7c1409d177", ResManager.LoadKDString("修改", "015072000014413", 7, new object[0]), bomIds, out networkCtrlResults);
					IOperationResult operationResult;
					List<long> list = this.ValidateNetWorkCtrl(networkCtrlResults, out operationResult);
					if (list.Count == 0)
					{
						this.CommitNetworkCtrl(networkCtrlResults);
						FormUtils.ShowOperationResult(this.View, operationResult, null);
						return;
					}
					Dictionary<long, List<long>> dictionary = new Dictionary<long, List<long>>();
					foreach (KeyValuePair<object, object> keyValuePair in this.ListView.SelectedRowsInfo.GetPKEntryIdValues())
					{
						long num2 = Convert.ToInt64(keyValuePair.Key);
						if (list.Contains(num2))
						{
							List<long> list2;
							if (dictionary.TryGetValue(num2, out list2))
							{
								list2.Add(Convert.ToInt64(keyValuePair.Value));
							}
							else
							{
								dictionary.Add(num2, new List<long>
								{
									Convert.ToInt64(keyValuePair.Value)
								});
							}
						}
					}
					TaskProxyItem taskProxyItem = new TaskProxyItem();
					List<object> list3 = new List<object>
					{
						this.Context,
						taskProxyItem.TaskId,
						dictionary,
						operationResult
					};
					taskProxyItem.Parameters = list3.ToArray();
					taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.BOMService,Kingdee.K3.MFG.ENG.App.Core";
					taskProxyItem.MethodName = "ReplaceDelete";
					taskProxyItem.ProgressQueryInterval = 1;
					taskProxyItem.Title = ResManager.LoadKDString("批量修改-[物料清单]", "015072000018134", 7, new object[0]);
					FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
					{
						this.CommitNetworkCtrl(networkCtrlResults);
						if (op.IsSuccess)
						{
							this.ListView.Refresh();
						}
					});
				}
			}, "", 0);
		}

		// Token: 0x060001BA RID: 442 RVA: 0x00016E64 File Offset: 0x00015064
		private void GetReplaceLst()
		{
			long num = 0L;
			if (this.ListView.CurrentSelectedRowInfo == null || !long.TryParse(this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue, out num) || num <= 0L)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "015072000018122", 7, new object[0]), "", 0);
				return;
			}
			List<long> bomIds = (from s in this.ListView.SelectedRowsInfo.GetPrimaryKeyValues()
			select Convert.ToInt64(s)).ToList<long>();
			List<NetworkCtrlResult> networkCtrlResults;
			this.StartNetworkCtrl("fae2446c-66e3-4cfe-83ab-5c7c1409d177", ResManager.LoadKDString("修改", "015072000014413", 7, new object[0]), bomIds, out networkCtrlResults);
			IOperationResult operationResult;
			List<long> list = this.ValidateNetWorkCtrl(networkCtrlResults, out operationResult);
			if (list.Count == 0)
			{
				this.CommitNetworkCtrl(networkCtrlResults);
				FormUtils.ShowOperationResult(this.View, operationResult, null);
				return;
			}
			Dictionary<long, List<long>> dictionary = new Dictionary<long, List<long>>();
			foreach (KeyValuePair<object, object> keyValuePair in this.ListView.SelectedRowsInfo.GetPKEntryIdValues())
			{
				long num2 = Convert.ToInt64(keyValuePair.Key);
				if (list.Contains(num2))
				{
					List<long> list2;
					if (dictionary.TryGetValue(num2, out list2))
					{
						list2.Add(Convert.ToInt64(keyValuePair.Value));
					}
					else
					{
						dictionary.Add(num2, new List<long>
						{
							Convert.ToInt64(keyValuePair.Value)
						});
					}
				}
			}
			dictionary = this.IsCanGetReplace(dictionary, out operationResult);
			if (!ListUtils.IsEmpty<KeyValuePair<long, List<long>>>(dictionary))
			{
				TaskProxyItem taskProxyItem = new TaskProxyItem();
				List<object> list3 = new List<object>
				{
					base.Context,
					taskProxyItem.TaskId,
					dictionary,
					operationResult
				};
				taskProxyItem.Parameters = list3.ToArray();
				taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.BOMService,Kingdee.K3.MFG.ENG.App.Core";
				taskProxyItem.MethodName = "GetReplaceLst";
				taskProxyItem.ProgressQueryInterval = 1;
				taskProxyItem.Title = ResManager.LoadKDString("获取替代-[物料清单列表]", "015072030033295", 7, new object[0]);
				FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
				{
					this.CommitNetworkCtrl(networkCtrlResults);
					this.ListView.Refresh();
				});
				return;
			}
			this.CommitNetworkCtrl(networkCtrlResults);
			if (ListUtils.IsEmpty<OperateResult>(operationResult.OperateResult))
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有符合条件的分录需要获取替代", "015072030033294", 7, new object[0]), 0);
				return;
			}
			FormUtils.ShowOperationResult(this.View, operationResult, null);
		}

		// Token: 0x060001BB RID: 443 RVA: 0x00017138 File Offset: 0x00015338
		private void ReGetReplaceLst()
		{
			if (!MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, "ENG_BOM", "ad440ef0395e453891b47f9f6d41c3de"))
			{
				this.ListView.ShowMessage(ResManager.LoadKDString("没有物料清单的“替代设置”权限！", "015072000002174", 7, new object[0]), 0);
				return;
			}
			long num = 0L;
			if (this.ListView.CurrentSelectedRowInfo == null || !long.TryParse(this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue, out num) || num <= 0L)
			{
				this.View.ShowErrMessage("没有选择任何数据，请先选择数据！", "", 0);
				return;
			}
			List<long> bomIds = (from s in this.ListView.SelectedRowsInfo.GetPrimaryKeyValues()
			select Convert.ToInt64(s)).ToList<long>();
			IOperationResult operationResult = new OperationResult();
			List<NetworkCtrlResult> list = new List<NetworkCtrlResult>();
			this.StartNetworkCtrl("fae2446c-66e3-4cfe-83ab-5c7c1409d177", "修改", bomIds, out list);
			try
			{
				List<long> list2 = this.ValidateNetWorkCtrl(list, out operationResult);
				if (list2.Count == 0)
				{
					FormUtils.ShowOperationResult(this.View, operationResult, null);
				}
				else
				{
					Dictionary<long, List<long>> dictionary = new Dictionary<long, List<long>>();
					foreach (KeyValuePair<object, object> keyValuePair in this.ListView.SelectedRowsInfo.GetPKEntryIdValues())
					{
						long num2 = Convert.ToInt64(keyValuePair.Key);
						if (list2.Contains(num2))
						{
							List<long> list3;
							if (dictionary.TryGetValue(num2, out list3))
							{
								list3.Add(Convert.ToInt64(keyValuePair.Value));
							}
							else
							{
								dictionary.Add(num2, new List<long>
								{
									Convert.ToInt64(keyValuePair.Value)
								});
							}
						}
					}
					Dictionary<long, List<long>> dictionary2 = this.ReGetRePlaceBomInfo(dictionary, out operationResult);
					if (ListUtils.IsEmpty<KeyValuePair<long, List<long>>>(dictionary2))
					{
						if (ListUtils.IsEmpty<OperateResult>(operationResult.OperateResult))
						{
							this.ListView.ShowMessage("没有符合条件的分录需要重新获取替代", 0);
						}
						else
						{
							FormUtils.ShowOperationResult(this.ListView, operationResult, null);
						}
					}
					else
					{
						TaskProxyItem taskProxyItem = new TaskProxyItem();
						List<object> list4 = new List<object>
						{
							base.Context,
							taskProxyItem.TaskId,
							dictionary2,
							operationResult
						};
						taskProxyItem.Parameters = list4.ToArray();
						taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.BOMService,Kingdee.K3.MFG.ENG.App.Core";
						taskProxyItem.MethodName = "ReGetReplaceLst";
						taskProxyItem.Title = "重新获取替代-[物料清单列表]";
						FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
						{
							this.ListView.Refresh();
						});
					}
				}
			}
			catch (Exception ex)
			{
				this.ListView.ShowErrMessage(ex.Message, "", 0);
			}
			finally
			{
				if (!ListUtils.IsEmpty<NetworkCtrlResult>(list))
				{
					this.CommitNetworkCtrl(list);
				}
			}
		}

		// Token: 0x060001BC RID: 444 RVA: 0x00017898 File Offset: 0x00015A98
		private Dictionary<long, List<long>> ReGetRePlaceBomInfo(Dictionary<long, List<long>> dicPkEntryIds, out IOperationResult operationResult)
		{
			Dictionary<long, List<long>> dictionary = new Dictionary<long, List<long>>();
			operationResult = new OperationResult();
			if (ListUtils.IsEmpty<KeyValuePair<long, List<long>>>(dicPkEntryIds))
			{
				return dictionary;
			}
			IEnumerable<object> source = from x in dicPkEntryIds.Keys
			select x;
			List<DynamicObject> list = MFGServiceHelper.LoadWithCache(base.Context, source.ToArray<object>(), this.View.Model.BillBusinessInfo.GetDynamicObjectType(), false, null).ToList<DynamicObject>();
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true);
			DBServiceHelper.LoadReferenceObject(this.View.Context, list.ToArray(), formMetadata.BusinessInfo.GetDynamicObjectType(), false);
			Dictionary<long, Dictionary<long, DynamicObject>> dictionary2 = list.ToDictionary((DynamicObject i) => DataEntityExtend.GetDynamicValue<long>(i, "Id", 0L), (DynamicObject v) => (from be in (from vr in (from wi in (from we in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(v, "TreeEntity", null)
			where DataEntityExtend.GetDynamicValue<string>(we, "MATERIALTYPE", null) == "1" && !DataEntityExtend.GetDynamicValue<bool>(we, "IsSkip", false)
			select we into vv
			group vv by DataEntityExtend.GetDynamicValue<long>(vv, "ReplaceGroup", 0L)).ToDictionary((IGrouping<long, DynamicObject> vd) => vd.Key, (IGrouping<long, DynamicObject> vdv) => vdv.ToList<DynamicObject>())
			where wi.Value.Count<DynamicObject>() == 1
			select wi).SelectMany((KeyValuePair<long, List<DynamicObject>> si) => si.Value)
			group vr by new
			{
				HasMaterialId = (DataEntityExtend.GetDynamicValue<long>(vr, "MATERIALIDCHILD_Id", 0L) > 0L),
				IsSkip = DataEntityExtend.GetDynamicValue<bool>(vr, "IsSkip", false),
				MaterialType = DataEntityExtend.GetDynamicValue<string>(vr, "MATERIALTYPE", null)
			} into wg
			where wg.Key.HasMaterialId && !wg.Key.IsSkip && wg.Key.MaterialType == "1"
			select wg).SelectMany(s => s)
			group be by DataEntityExtend.GetDynamicValue<long>(be, "Id", 0L)).ToDictionary((IGrouping<long, DynamicObject> dbek) => dbek.Key, (IGrouping<long, DynamicObject> dbev) => dbev.FirstOrDefault<DynamicObject>()));
			foreach (DynamicObject dynamicObject in list)
			{
				long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L);
				List<long> bomEntryIds = new List<long>();
				Dictionary<long, DynamicObject> dictionary3 = new Dictionary<long, DynamicObject>();
				dictionary2.TryGetValue(dynamicObjectItemValue, out dictionary3);
				if (!ListUtils.IsEmpty<KeyValuePair<long, DynamicObject>>(dictionary3) && dicPkEntryIds.TryGetValue(dynamicObjectItemValue, out bomEntryIds))
				{
					List<DynamicObject> list2 = (from i in dictionary3
					where bomEntryIds.Contains(i.Key)
					select i.Value).ToList<DynamicObject>();
					if (!ListUtils.IsEmpty<DynamicObject>(list2))
					{
						long dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "CreateOrgId_Id", -1L);
						long dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "UseOrgId_Id", -1L);
						if (dynamicObjectItemValue2 != dynamicObjectItemValue3)
						{
							OperateResult operateResult = new OperateResult();
							operateResult.Name = "物料清单列表重新获取替代";
							operateResult.Message = string.Format("{0}暂不支持在使用组织下进行替代设置，请在创建组织进行替代设置!", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "Number", null));
							operateResult.SuccessStatus = false;
							operateResult.PKValue = dynamicObjectItemValue;
							operateResult.MessageType = -1;
							operationResult.OperateResult.Add(operateResult);
						}
						else
						{
							List<long> list3 = (from x in list2
							select DataEntityExtend.GetDynamicObjectItemValue<long>(x, "Id", 0L)).ToList<long>();
							if (!ListUtils.IsEmpty<long>(list3))
							{
								dictionary[dynamicObjectItemValue] = list3;
							}
						}
					}
				}
			}
			return dictionary;
		}

		// Token: 0x060001BD RID: 445 RVA: 0x00017BD4 File Offset: 0x00015DD4
		private Dictionary<long, List<long>> IsCanGetReplace(Dictionary<long, List<long>> dicPkEntryIds, out IOperationResult operationResult)
		{
			Dictionary<long, List<long>> dictionary = new Dictionary<long, List<long>>();
			operationResult = new OperationResult();
			if (ListUtils.IsEmpty<KeyValuePair<long, List<long>>>(dicPkEntryIds))
			{
				return dictionary;
			}
			IEnumerable<object> source = from x in dicPkEntryIds.Keys
			select x;
			List<DynamicObject> list = MFGServiceHelper.LoadWithCache(base.Context, source.ToArray<object>(), this.View.Model.BillBusinessInfo.GetDynamicObjectType(), false, null).ToList<DynamicObject>();
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true);
			DBServiceHelper.LoadReferenceObject(this.View.Context, list.ToArray(), formMetadata.BusinessInfo.GetDynamicObjectType(), false);
			foreach (DynamicObject dynamicObject in list)
			{
				long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L);
				List<long> bomEntryIds = null;
				if (dicPkEntryIds.TryGetValue(dynamicObjectItemValue, out bomEntryIds))
				{
					List<DynamicObject> source2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "TreeEntity", null).ToList<DynamicObject>();
					List<DynamicObject> list2 = (from w in source2
					where DataEntityExtend.GetDynamicValue<long>(w, "MATERIALIDCHILD_Id", 0L) > 0L && DataEntityExtend.GetDynamicValue<int>(w, "MATERIALTYPE", 0) == 1 && ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "ReplacePolicy", null)) && !DataEntityExtend.GetDynamicValue<bool>(w, "IsSkip", false) && bomEntryIds.Contains(DataEntityExtend.GetDynamicValue<long>(w, "Id", 0L))
					select w).ToList<DynamicObject>();
					if (!ListUtils.IsEmpty<DynamicObject>(list2))
					{
						long dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "CreateOrgId_Id", -1L);
						long dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "UseOrgId_Id", -1L);
						if (dynamicObjectItemValue2 != dynamicObjectItemValue3)
						{
							OperateResult operateResult = new OperateResult();
							operateResult.Name = ResManager.LoadKDString("物料清单列表获取替代", "015072000013260", 7, new object[0]);
							operateResult.Message = string.Format(ResManager.LoadKDString("{0}暂不支持在使用组织下进行替代设置，请在创建组织进行替代设置!", "015072030033297", 7, new object[0]), DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "Number", null));
							operateResult.SuccessStatus = false;
							operateResult.PKValue = dynamicObjectItemValue;
							operateResult.MessageType = -1;
							operationResult.OperateResult.Add(operateResult);
						}
						else
						{
							List<long> list3 = (from x in list2
							select DataEntityExtend.GetDynamicObjectItemValue<long>(x, "Id", 0L)).ToList<long>();
							if (!ListUtils.IsEmpty<long>(list3))
							{
								dictionary.Add(dynamicObjectItemValue, list3);
							}
						}
					}
				}
			}
			return dictionary;
		}

		// Token: 0x060001BE RID: 446 RVA: 0x00017E38 File Offset: 0x00016038
		private void StartNetworkCtrl(string operationKey, string operationName, List<long> bomIds, out List<NetworkCtrlResult> networkCtrlResults)
		{
			string text = string.Format(" FMetaObjectID = '{0}' and FoperationID = '{1}'  and ftype={2}  and FStart = '1'  ", "ENG_BOM", operationKey, 6);
			NetworkCtrlObject networkCtrlObject = NetworkCtrlServiceHelper.GetNetCtrlList(base.Context, text).FirstOrDefault<NetworkCtrlObject>();
			List<NetWorkRunTimeParam> list = new List<NetWorkRunTimeParam>();
			foreach (long num in bomIds)
			{
				list.Add(new NetWorkRunTimeParam
				{
					BillName = new LocaleValue(ResManager.LoadKDString("物料清单", "015072000018136", 7, new object[0]), 2052),
					InterID = num.ToString(),
					OperationDesc = ResManager.LoadKDString("物料清单", "015072000018136", 7, new object[0]) + "-BillNo-" + operationName,
					OperationName = new LocaleValue(operationName, 2052)
				});
			}
			networkCtrlResults = NetworkCtrlServiceHelper.BatchBeginNetCtrl(base.Context, networkCtrlObject, list, false);
		}

		// Token: 0x060001BF RID: 447 RVA: 0x00017F44 File Offset: 0x00016144
		protected void CommitNetworkCtrl(List<NetworkCtrlResult> networkCtrlResults)
		{
			NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, networkCtrlResults);
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x00017F74 File Offset: 0x00016174
		private List<long> ValidateNetWorkCtrl(List<NetworkCtrlResult> networkCtrlResults, out IOperationResult operationResult)
		{
			List<long> result = (from w in networkCtrlResults
			where w.StartSuccess
			select w into s
			select Convert.ToInt64(s.InterID)).ToList<long>();
			List<NetworkCtrlResult> list = (from w in networkCtrlResults
			where !w.StartSuccess
			select w).ToList<NetworkCtrlResult>();
			operationResult = new OperationResult();
			foreach (NetworkCtrlResult networkCtrlResult in list)
			{
				OperateResult operateResult = new OperateResult();
				operateResult.Name = networkCtrlResult.ObjectName;
				operateResult.Message = networkCtrlResult.Message;
				operateResult.SuccessStatus = false;
				operateResult.PKValue = networkCtrlResult.InterID;
				operateResult.MessageType = -1;
				operationResult.OperateResult.Add(operateResult);
			}
			return result;
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x00018084 File Offset: 0x00016284
		private void SendBomDataToParent(CustomEventsArgs e, string flag, string customEventName)
		{
			long num = 0L;
			if (this.ListView.CurrentSelectedRowInfo == null || !long.TryParse(this.ListView.CurrentSelectedRowInfo.PrimaryKeyValue, out num) || num <= 0L)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "015072000018122", 7, new object[0]), "", 0);
				return;
			}
			IEnumerable<object> source = (from s in this.ListView.SelectedRowsInfo.GetPrimaryKeyValues()
			select s).Distinct<object>();
			List<DynamicObject> list = MFGServiceHelper.LoadWithCache(base.Context, source.ToArray<object>(), this.View.Model.BillBusinessInfo.GetDynamicObjectType(), false, null).ToList<DynamicObject>();
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true);
			DBServiceHelper.LoadReferenceObject(this.View.Context, list.ToArray(), formMetadata.BusinessInfo.GetDynamicObjectType(), false);
			IDynamicFormView view = this.View.GetView(e.Key);
			view.Session.Add(flag, list);
			(view as IDynamicFormViewService).CustomEvents(this.View.PageId, customEventName, "");
			this.View.SendAynDynamicFormAction(view);
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x000181E0 File Offset: 0x000163E0
		private void OpenEcnList(BarItemClickEventArgs e)
		{
			ListSelectedRowCollection selectedRowsInfo = this.ListView.SelectedRowsInfo;
			if (ListUtils.IsEmpty<ListSelectedRow>(selectedRowsInfo))
			{
				e.Cancel = true;
				this.View.ShowMessage(ResManager.LoadKDString("请勾选物料清单进行工程变更单联查!", "015072000018101", 7, new object[0]), 0);
				return;
			}
			ListShowParameter listShowParameter = new ListShowParameter
			{
				FormId = "ENG_ECNOrder",
				PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad",
				IsLookUp = false,
				IsShowApproved = false,
				IsShowUsed = true,
				IsIsolationOrg = false
			};
			List<long> list = (from i in selectedRowsInfo
			select OtherExtend.ConvertTo<long>(i.PrimaryKeyValue, 0L)).Distinct<long>().ToList<long>();
			List<long> ecnIdByBomId = BOMServiceHelper.GetEcnIdByBomId(base.Context, list);
			if (ListUtils.IsEmpty<long>(ecnIdByBomId))
			{
				e.Cancel = true;
				this.View.ShowMessage(ResManager.LoadKDString("当前所选的物料清单没有关联的工程变更单!", "015072030038263", 7, new object[0]), 0);
				return;
			}
			string text = string.Format("exists (select 1 from {0} tm where tm.fid=t0.fid)", StringUtils.GetSqlWithCardinality(ecnIdByBomId.Count<long>(), "@fbomid", 1, true));
			listShowParameter.SqlParams.Add(new SqlParam("@fbomid", 161, ecnIdByBomId.ToArray()));
			listShowParameter.ListFilterParameter.Filter = StringUtils.JoinFilterString(listShowParameter.ListFilterParameter.Filter, text, "AND");
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x000183D0 File Offset: 0x000165D0
		private void BomIntegrityCheck()
		{
			string[] primaryKeyValues = this.ListView.SelectedRowsInfo.GetPrimaryKeyValues();
			if (ListUtils.IsEmpty<string>(primaryKeyValues))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("没有选择任何数据，请先选择数据！", "015072000018122", 7, new object[0]), "", 0);
				return;
			}
			List<long> list = (from s in primaryKeyValues
			select Convert.ToInt64(s)).Distinct<long>().ToList<long>();
			object[] array = (from s in list
			select s).ToArray<object>();
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true);
			DynamicObject[] array2 = BusinessDataServiceHelper.Load(base.Context, array, formMetadata.BusinessInfo.GetDynamicObjectType());
			List<long> list2 = new List<long>();
			List<long> list3 = array2.SelectMany((DynamicObject x) => from y in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(x, "TreeEntity", null)
			select DataEntityExtend.GetDynamicValue<long>(y, "Id", 0L)).ToList<long>();
			DynamicObjectCollection mtrlMster = BOMServiceHelper.GetMtrlMster(base.Context, list3);
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary = (from g in mtrlMster
			group g by DataEntityExtend.GetDynamicValue<long>(g, "FMASTERID", 0L) + "_" + DataEntityExtend.GetDynamicValue<long>(g, "FUSEORGID", 0L)).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
			foreach (DynamicObject dynamicObject in array2)
			{
				DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "TreeEntity", null);
				foreach (DynamicObject dynamicObject2 in dynamicValue)
				{
					if (DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "ChildSupplyOrgId_Id", 0L) > 0L)
					{
						DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject2, "MATERIALIDCHILD", null);
						string key = DataEntityExtend.GetDynamicValue<long>(dynamicObjectItemValue, "msterID", 0L) + "_" + DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "ChildSupplyOrgId_Id", 0L);
						IGrouping<string, DynamicObject> grouping = null;
						if (dictionary.TryGetValue(key, out grouping) && !ListUtils.IsEmpty<DynamicObject>(grouping))
						{
							list2.Add(DataEntityExtend.GetDynamicValue<long>(grouping.FirstOrDefault<DynamicObject>(), "FMATERIALID", 0L));
						}
					}
					else
					{
						list2.Add(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "MATERIALIDCHILD_Id", 0L));
					}
				}
			}
			list2 = list2.Distinct<long>().ToList<long>();
			this.View.ShowBomIntegrity(list, list2);
		}

		// Token: 0x040000CD RID: 205
		public const string Entry_BomChild = "FEntity";

		// Token: 0x040000CE RID: 206
		public const string FFIELD_FID = "FID";

		// Token: 0x040000CF RID: 207
		public const string FFIELD_FBOMCATEGORY = "FBOMCATEGORY";

		// Token: 0x040000D0 RID: 208
		public const string FFIELD_FBOMUSE = "FBOMUSE";

		// Token: 0x040000D1 RID: 209
		public const string FFIELD_FMATERIALID = "FMATERIALID";

		// Token: 0x040000D2 RID: 210
		public const string FFIELD_FNUMERATOR = "FNUMERATOR";

		// Token: 0x040000D3 RID: 211
		public const string FFIELD_FDENOMINATOR = "FDENOMINATOR";

		// Token: 0x040000D4 RID: 212
		public const string FKEY_ISDEFAULT = "FISDEFAULT";

		// Token: 0x040000D5 RID: 213
		private string forbidReason;

		// Token: 0x040000D6 RID: 214
		private bool IsEnablelForbidreason;
	}
}
