using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Metadata.StatusElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.Workflow.ServiceHelper;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.BomExpand.PlugIn;
using Kingdee.K3.Core.MFG.ENG.BomTree;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.ENG;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000007 RID: 7
	[Description("产品配置单据插件-界面控制")]
	public class BOMConfigEdit : BaseControlEdit
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600000F RID: 15 RVA: 0x000025B9 File Offset: 0x000007B9
		// (set) Token: 0x06000010 RID: 16 RVA: 0x000025C1 File Offset: 0x000007C1
		private TreeNode topTreeNode { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000011 RID: 17 RVA: 0x000025CA File Offset: 0x000007CA
		// (set) Token: 0x06000012 RID: 18 RVA: 0x000025D2 File Offset: 0x000007D2
		private DynamicObject CurSelectRow { get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000013 RID: 19 RVA: 0x000025DB File Offset: 0x000007DB
		// (set) Token: 0x06000014 RID: 20 RVA: 0x000025E3 File Offset: 0x000007E3
		private bool IsCfgBomOrNot { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000015 RID: 21 RVA: 0x000025EC File Offset: 0x000007EC
		// (set) Token: 0x06000016 RID: 22 RVA: 0x000025F4 File Offset: 0x000007F4
		private bool IsInSertRow { get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000017 RID: 23 RVA: 0x000025FD File Offset: 0x000007FD
		// (set) Token: 0x06000018 RID: 24 RVA: 0x00002605 File Offset: 0x00000805
		private bool IsAllExpand { get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000019 RID: 25 RVA: 0x0000260E File Offset: 0x0000080E
		// (set) Token: 0x0600001A RID: 26 RVA: 0x00002616 File Offset: 0x00000816
		private bool IsOverallCopy { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600001B RID: 27 RVA: 0x0000261F File Offset: 0x0000081F
		// (set) Token: 0x0600001C RID: 28 RVA: 0x00002627 File Offset: 0x00000827
		private bool IsCopyLock { get; set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600001D RID: 29 RVA: 0x00002630 File Offset: 0x00000830
		// (set) Token: 0x0600001E RID: 30 RVA: 0x00002638 File Offset: 0x00000838
		private TreeNode curTreeNodeForCopy { get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600001F RID: 31 RVA: 0x00002641 File Offset: 0x00000841
		// (set) Token: 0x06000020 RID: 32 RVA: 0x00002649 File Offset: 0x00000849
		protected List<DynamicObject> standBomDatas { get; set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000021 RID: 33 RVA: 0x00002652 File Offset: 0x00000852
		// (set) Token: 0x06000022 RID: 34 RVA: 0x0000265A File Offset: 0x0000085A
		private bool IsDataChangeExpand { get; set; }

		// Token: 0x06000023 RID: 35 RVA: 0x00002714 File Offset: 0x00000914
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			if (!base.View.Context.IsMultiOrg)
			{
				base.View.RuleContainer.AddPluginRule("FTreeEntity", 1, new Action<DynamicObject, object>(this.SetSupplyorg), new string[]
				{
					"FMATERIALIDCHILD"
				});
			}
			base.View.RuleContainer.AddPluginRule("FTreeEntity", 1, new Action<DynamicObject, object>(this.DoNewBomExpand), new string[]
			{
				"FEntryCfgBomId"
			});
			base.View.RuleContainer.AddPluginRule("FTreeEntity", 1, delegate(DynamicObject currentrow, dynamic dynobj)
			{
				DynamicObjectCollection source = this.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
				Dictionary<string, IGrouping<string, DynamicObject>> datas = source.GroupBy(delegate(DynamicObject x)
				{
					string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(x, "ParentRowId", null);
					if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectItemValue))
					{
						return dynamicObjectItemValue;
					}
					return "root";
				}).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
				this.SetIsSelectedEnabled(datas, currentrow);
			}, (dynamic row) => true, new string[]
			{
				"FIsSelect"
			});
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000027F0 File Offset: 0x000009F0
		private void SetIsSelectedEnabled(Dictionary<string, IGrouping<string, DynamicObject>> datas, DynamicObject parentRow)
		{
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(parentRow, "RowId", null);
			IGrouping<string, DynamicObject> grouping;
			if (datas.TryGetValue(dynamicValue, out grouping))
			{
				bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(parentRow, "IsSelect", false);
				foreach (DynamicObject dynamicObject in grouping)
				{
					base.View.StyleManager.SetEnabled("FIsSelect", dynamicObject, "parentLocked", dynamicValue2);
					this.SetIsSelectedEnabled(datas, dynamicObject);
				}
			}
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002880 File Offset: 0x00000A80
		public override void TreeNodeClick(TreeNodeArgs e)
		{
			if (this.IsOverallCopy)
			{
				this.IsOverallCopy = false;
				return;
			}
			if (this.IsDataChangeExpand)
			{
				this.IsDataChangeExpand = false;
				base.View.Model.DataChanged = false;
				return;
			}
			base.TreeNodeClick(e);
			TreeNode selectNode = this.GetSelectNode(e.NodeId);
			this.curTreeNodeForCopy = selectNode;
			this.TreeNodeOnClick(selectNode);
			if (this.IsCopyLock)
			{
				this.IsCopyLock = false;
			}
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002984 File Offset: 0x00000B84
		private void UpdateTreeEntryByPrentUnit()
		{
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "TreeEntity", null);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicValue))
			{
				return;
			}
			List<long> list = (from x in dynamicValue
			where DataEntityExtend.GetDynamicValue<long>(x, "EntryCfgBomId", 0L) > 0L
			select DataEntityExtend.GetDynamicValue<long>(x, "EntryCfgBomId", 0L)).Distinct<long>().ToList<long>();
			if (ListUtils.IsEmpty<long>(list))
			{
				return;
			}
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ENG_BOM",
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FID,FTREEENTITY_FENTRYID AS FENTRYID",
					"FMaterialId.FMASTERID as MaterialMasterId",
					"FMATERIALID",
					"FUseOrgId",
					"FUNITID",
					"FBaseUnitId"
				})
			};
			string sqlWithCardinality = StringUtils.GetSqlWithCardinality(list.Distinct<long>().Count<long>(), "@FID", 1, true);
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				FieldName = "FID",
				TableName = sqlWithCardinality,
				TableNameAs = "sp",
				ScourceKey = "FID"
			});
			List<SqlParam> list2 = new List<SqlParam>
			{
				new SqlParam("@FID", 161, list.Distinct<long>().ToArray<long>())
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, list2);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				return;
			}
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary = (from x in dynamicObjectCollection
			group x by DataEntityExtend.GetDynamicValue<long>(x, "FID", 0L)).ToDictionary((IGrouping<long, DynamicObject> x) => x.Key);
			Dictionary<long, DynamicObject> dictionary2 = (from x in dynamicObjectCollection
			where DataEntityExtend.GetDynamicValue<long>(x, "FENTRYID", 0L) > 0L
			select x).ToDictionary((DynamicObject x) => DataEntityExtend.GetDynamicValue<long>(x, "FENTRYID", 0L));
			int num = dynamicValue.Max((DynamicObject m) => DataEntityExtend.GetDynamicValue<int>(m, "BomLevel", 0));
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary3 = (from x in dynamicValue
			group x by DataEntityExtend.GetDynamicValue<string>(x, "RowId", null)).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
			Dictionary<int, IGrouping<int, DynamicObject>> dictionary4 = (from x in dynamicValue
			group x by DataEntityExtend.GetDynamicValue<int>(x, "BomLevel", 0)).ToDictionary((IGrouping<int, DynamicObject> x) => x.Key);
			string format = "{0}_{1}_{2}";
			List<string> list3 = new List<string>();
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FMATERIALID", 0L);
				long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MaterialMasterId", 0L);
				long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FUNITID", 0L);
				long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FBaseUnitId", 0L);
				string item = string.Format(format, dynamicValue2, dynamicValue3, dynamicValue4);
				if (!list3.Contains(item))
				{
					list3.Add(item);
				}
			}
			Dictionary<string, UnitConvert> unitConvertRates = MFGServiceHelper.GetUnitConvertRates(base.View.Context, list3);
			for (int i = num; i >= 0; i--)
			{
				IGrouping<int, DynamicObject> source = null;
				dictionary4.TryGetValue(i, out source);
				List<DynamicObject> list4 = source.ToList<DynamicObject>();
				foreach (DynamicObject dynamicObject2 in list4)
				{
					string dynamicValue5 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "ParentRowId", null);
					long dynamicValue6 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "CfgBomEntryId", 0L);
					int index = dynamicValue.IndexOf(dynamicObject2);
					if (dynamicValue6 == 0L && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue5))
					{
						IGrouping<string, DynamicObject> source2 = null;
						if (dictionary3.TryGetValue(dynamicValue5, out source2))
						{
							DynamicObject dynamicValue7 = DataEntityExtend.GetDynamicValue<DynamicObject>(source2.First<DynamicObject>(), "MATERIALIDCHILD", null);
							if (!ObjectUtils.IsNullOrEmpty(dynamicValue7))
							{
								long dynamicValue8 = DataEntityExtend.GetDynamicValue<long>(source2.First<DynamicObject>(), "EntryCfgBomId", 0L);
								if (dynamicValue8 != 0L)
								{
									IGrouping<long, DynamicObject> grouping = null;
									dictionary.TryGetValue(dynamicValue8, out grouping);
									if (!ListUtils.IsEmpty<DynamicObject>(grouping))
									{
										long dynamicValue9 = DataEntityExtend.GetDynamicValue<long>(dynamicValue7, "msterID", 0L);
										long dynamicValue10 = DataEntityExtend.GetDynamicValue<long>(grouping.First<DynamicObject>(), "FUNITID", 0L);
										long dynamicValue11 = DataEntityExtend.GetDynamicValue<long>(grouping.First<DynamicObject>(), "FBaseUnitId", 0L);
										string key = string.Format("{0}_{1}_{2}", dynamicValue9, dynamicValue10, dynamicValue11);
										UnitConvert unitConvert = unitConvertRates[key];
										DataEntityExtend.SetDynamicObjectItemValue(dynamicValue[index], "ParentUnitID_Id", dynamicValue10);
										DataEntityExtend.SetDynamicObjectItemValue(dynamicValue[index], "ParentBaseUnitID_Id", dynamicValue11);
										DataEntityExtend.SetDynamicObjectItemValue(dynamicValue[index], "PrentMtrlId_Id", DataEntityExtend.GetDynamicValue<long>(source2.First<DynamicObject>(), "MATERIALIDCHILD_Id", 0L));
										decimal dynamicValue12 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "DENOMINATOR", 0m);
										DataEntityExtend.SetDynamicObjectItemValue(dynamicValue[index], "BaseDenominator", unitConvert.ConvertQty(dynamicValue12, ""));
									}
								}
							}
						}
					}
					else if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue5))
					{
						DynamicObject dynamicValue13 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject2, "MATERIALIDCHILD", null);
						if (!ObjectUtils.IsNullOrEmpty(dynamicValue13) && dynamicValue6 != 0L)
						{
							DynamicObject dynamicObject3 = null;
							if (dictionary2.TryGetValue(dynamicValue6, out dynamicObject3))
							{
								long dynamicValue14 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "FUNITID", 0L);
								long dynamicValue15 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "FBaseUnitId", 0L);
								long dynamicValue16 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "FMATERIALID", 0L);
								long dynamicValue17 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "MaterialMasterId", 0L);
								DataEntityExtend.SetDynamicObjectItemValue(dynamicValue[index], "ParentUnitID_Id", dynamicValue14);
								DataEntityExtend.SetDynamicObjectItemValue(dynamicValue[index], "ParentBaseUnitID_Id", dynamicValue15);
								DataEntityExtend.SetDynamicObjectItemValue(dynamicValue[index], "PrentMtrlId_Id", dynamicValue16);
								decimal dynamicValue18 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "DENOMINATOR", 0m);
								string key2 = string.Format("{0}_{1}_{2}", dynamicValue17, dynamicValue14, dynamicValue15);
								UnitConvert unitConvert2 = unitConvertRates[key2];
								DataEntityExtend.SetDynamicObjectItemValue(dynamicValue[index], "BaseDenominator", unitConvert2.ConvertQty(dynamicValue18, ""));
							}
						}
					}
				}
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FTreeEntity");
			DBServiceHelper.LoadReferenceObject(base.View.Context, dynamicValue.ToArray<DynamicObject>(), entryEntity.DynamicObjectType, false);
			base.View.UpdateView("FParentUnitID");
			base.View.UpdateView("FParentBaseUnitID");
			base.View.UpdateView("FPrentMtrlId");
			base.View.UpdateView("FBaseDenominator");
		}

		// Token: 0x06000027 RID: 39 RVA: 0x000030FC File Offset: 0x000012FC
		private void SearchTrueParent(List<DynamicObject> lstEntryDatas, string parentRowId, Dictionary<string, IGrouping<string, DynamicObject>> dicByRowObject)
		{
			IGrouping<string, DynamicObject> source = null;
			if (!dicByRowObject.TryGetValue(parentRowId, out source))
			{
				return;
			}
			if (ObjectUtils.IsNullOrEmpty(DataEntityExtend.GetDynamicValue<DynamicObject>(source.First<DynamicObject>(), "MATERIALIDCHILD", null)))
			{
				return;
			}
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicValue<DynamicObject>(source.First<DynamicObject>(), "MATERIALIDCHILD", null), "MaterialBase", null).First<DynamicObject>(), "ErpClsID", null);
			string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(source.First<DynamicObject>(), "ParentRowId", null);
			if (!StringUtils.EqualsIgnoreCase(dynamicValue, "4"))
			{
				lstEntryDatas.Add(source.First<DynamicObject>());
				return;
			}
			this.SearchTrueParent(lstEntryDatas, dynamicValue2, dicByRowObject);
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00003194 File Offset: 0x00001394
		private void TreeNodeOnClick(TreeNode curTreeNode)
		{
			MFGCommonUtil.DoCommitNetworkCtrl(base.View.Context, this.NetworkCtrlResults);
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			bool flag = Convert.ToInt64(curTreeNode.parentid) <= 0L;
			this.IsCfgBomOrNot = flag;
			long num = flag ? Convert.ToInt64(curTreeNode.id) : Convert.ToInt64(curTreeNode.parentid);
			long bomId = flag ? 0L : Convert.ToInt64(curTreeNode.id);
			if (this.IsOverallCopy)
			{
				bomId = Convert.ToInt64(this.curTreeNodeForCopy.id);
			}
			DynamicObject bomInfo = this.GetBomInfo(num);
			DynamicObject bomInfo2 = this.GetBomInfo(bomId);
			this.BomConfigDatasMger.CfgBomId = num;
			this.BomConfigDatasMger.MainOrgId = value;
			this.BomConfigDatasMger.CfgMatrailId = DataEntityExtend.GetDynamicObjectItemValue<long>(bomInfo, "MATERIALID_Id", 0L);
			this.SetTopStandBom(bomInfo2);
			if (bomInfo2 != null)
			{
				this.SetHeadmFaceBystandBom(this.BomConfigDatasMger.TopStandBom);
			}
			else
			{
				this.SetHeadmFaceByCfgBom(bomInfo);
			}
			if (this.IsOverallCopy)
			{
				base.View.Model.SetValue("FCfgNumber", "");
				base.View.Model.SetValue("FCfgName", null);
			}
			this.ClearInitData();
			this.AddBomExpandNodes(this.BomConfigDatasMger.CfgBomId, this.BomConfigDatasMger.CfgMatrailId, this.BomConfigDatasMger.StandBomId, this.BomConfigDatasMger.StandMatrailId, "");
			base.View.Model.DataChanged = false;
			this.DoStartBomNetworkCtrl();
		}

		// Token: 0x06000029 RID: 41 RVA: 0x0000332C File Offset: 0x0000152C
		public override void CustomEvents(CustomEventsArgs e)
		{
			string eventName;
			if ((eventName = e.EventName) != null)
			{
				if (!(eventName == "ReflashBomConfigEntityData"))
				{
					if (!(eventName == "RowCollapsed") && !(eventName == "RowExpanded"))
					{
						return;
					}
				}
				else if (base.View.GetView(e.Key) != null)
				{
					int showWay;
					int.TryParse(e.EventArgs, out showWay);
					this.BomConfigDatasMger.ShowWay = showWay;
					this.BomConfigDatasMger.IsAutoCheck = MFGBillUtil.GetUserParam<bool>(base.View, "IsAutoCheck", false);
					this.ShowBomConfigChildEntity();
					long value = MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALID", -1, 0L, null);
					if (value > 0L)
					{
						this.LoadTreeViewData(value);
						this.SetCurTreeNode();
					}
					base.View.Model.DataChanged = false;
				}
			}
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003400 File Offset: 0x00001600
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			MFGCommonUtil.DoCommitNetworkCtrl(base.View.Context, this.NetworkCtrlResults);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00003420 File Offset: 0x00001620
		private void ReturnData()
		{
			MFGCommonUtil.DoCommitNetworkCtrl(base.View.Context, this.NetworkCtrlResults);
			if (!this.IsNeedReturnSoureForm())
			{
				return;
			}
			long num = (DataEntityExtend.GetDynamicObjectItemValue<string>(this.BomConfigDatasMger.TopStandBom, "DocumentStatus", null) != 'Z'.ToString()) ? this.BomConfigDatasMger.StandBomId : 0L;
			if (base.View.ParentFormView != null)
			{
				(base.View.ParentFormView as IDynamicFormViewService).CustomEvents(base.View.PageId, "ReflashBomConfigData", num.ToString());
				base.View.SendDynamicFormAction(base.View.ParentFormView);
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000034FC File Offset: 0x000016FC
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			this.IsCopyLock = (e.BarItemKey == "tbOverallCopy");
			string barItemKey;
			switch (barItemKey = e.BarItemKey)
			{
			case "tbSaveTemp":
				e.Cancel = !this.DoBachOperSave("Draft");
				return;
			case "tbSave":
			case "tbSplitSave":
				e.Cancel = !this.DoBachOperSave("Save");
				return;
			case "tbSplitSubmit":
			case "tbSubmit":
				e.Cancel = !this.DoBachOper("Submit");
				return;
			case "tbCancelAssign":
				e.Cancel = !this.DoBachOper("CancelAssign");
				return;
			case "tbSplitApprove":
			case "tbApprove":
				e.Cancel = !this.DoBachOper("Audit");
				return;
			case "tbReject":
				this.DoBachOper("UnAudit");
				return;
			case "tbClose":
				e.Cancel = !this.CheckCanClose();
				return;
			case "tbPara":
				if (base.View.Model.DataChanged)
				{
					base.View.ShowMessage(ResManager.LoadKDString("产品配置信息进行过修改，请先保存！", "015072000003204", 7, new object[0]), 0);
					e.Cancel = true;
					return;
				}
				break;
			case "tbOverallCopy":
			{
				if (ObjectUtils.IsNullOrEmpty(this.curTreeNodeForCopy))
				{
					base.View.ShowMessage(ResManager.LoadKDString("无BOM信息，不需要整体复制", "015072000017885", 7, new object[0]), 0);
					e.Cancel = true;
					return;
				}
				bool flag = Convert.ToInt64(this.curTreeNodeForCopy.parentid) <= 0L;
				if (flag)
				{
					base.View.ShowMessage(ResManager.LoadKDString("当前为配置BOM，只有标准BOM才允许复制！", "015072000017886", 7, new object[0]), 0);
					e.Cancel = true;
					return;
				}
				this.IsOverallCopy = true;
				TreeNode selectNode = this.GetSelectNode(this.curTreeNodeForCopy.parentid);
				if (selectNode != null)
				{
					base.View.GetControl<TreeView>("FConfigTreeView").Select(selectNode.id);
					this.TreeNodeOnClick(selectNode);
					this.curTreeNodeForCopy = selectNode;
					return;
				}
				break;
			}
			case "tbReturnData":
				if (base.View.Model.DataChanged)
				{
					e.Cancel = true;
					base.View.ShowMessage(ResManager.LoadKDString("内容已经修改，确认需要返回数据吗？", "015072000013255", 7, new object[0]), 4, delegate(MessageBoxResult result)
					{
						if (result == 6)
						{
							base.View.Model.DataChanged = false;
							this.ReturnData();
							base.View.Close();
						}
					}, "", 0);
					return;
				}
				this.ReturnData();
				base.View.Close();
				break;

				return;
			}
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003830 File Offset: 0x00001A30
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
			string barItemKey;
			switch (barItemKey = e.BarItemKey)
			{
			case "tbSaveTemp":
			case "tbSplitSubmit":
			case "tbSubmit":
			case "tbCancelAssign":
			case "tbSplitApprove":
			case "tbApprove":
			case "tbReject":
			{
				long value = MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALID", -1, 0L, null);
				this.LoadTreeViewData(value);
				this.SetCurTreeNode();
				return;
			}
			case "tbSplitSave":
			case "tbSave":
			{
				long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALID", -1, 0L, null);
				this.LoadTreeViewData(value2);
				this.SetCurTreeNode();
				this.TreeNodeOnClick(this.curNode);
				break;
			}

				return;
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003970 File Offset: 0x00001B70
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			if (MFGBillUtil.GetValue<long>(base.View.Model, "FBomLevel", e.Row, 0L, null) == 0L)
			{
				base.View.GetBarItem<BarItemControl>("FTreeEntity", "tbQueryStock").Enabled = false;
				return;
			}
			base.View.GetBarItem<BarItemControl>("FTreeEntity", "tbQueryStock").Enabled = true;
		}

		// Token: 0x0600002F RID: 47 RVA: 0x000039E0 File Offset: 0x00001BE0
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
			if (MFGBillUtil.GetValue<long>(base.View.Model, "FBomLevel", e.Row, 0L, null) == 0L)
			{
				base.View.GetBarItem<BarItemControl>("FTreeEntity", "tbQueryStock").Enabled = false;
				return;
			}
			base.View.GetBarItem<BarItemControl>("FTreeEntity", "tbQueryStock").Enabled = true;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003A94 File Offset: 0x00001C94
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			int num = 0;
			DynamicObject curSelectRow = null;
			this.Model.TryGetEntryCurrentRow("FTreeEntity", ref curSelectRow, ref num);
			this.CurSelectRow = curSelectRow;
			if (e.BarItemKey == "tbNewEntry" || e.BarItemKey == "tbInsertEntryRow")
			{
				if (e.BarItemKey == "tbInsertEntryRow")
				{
					string parentRowId = DataEntityExtend.GetDynamicValue<string>(this.CurSelectRow, "ParentRowId", null);
					DynamicObjectCollection source = this.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
					this.CurSelectRow = source.FirstOrDefault((DynamicObject row) => DataEntityExtend.GetDynamicValue<string>(row, "RowId", null) == parentRowId);
					if (this.CurSelectRow == null)
					{
						e.Cancel = true;
						return;
					}
				}
				Enums.Enu_BOMCateGoryCfg dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(this.CurSelectRow, "EntityBomCategory", 0);
				if (dynamicObjectItemValue != 1)
				{
					e.Cancel = true;
					base.View.ShowErrMessage(ResManager.LoadKDString("只有配置BOM才能添加子项！", "015072000002148", 7, new object[0]), "", 0);
					return;
				}
				if (DataEntityExtend.GetDynamicObjectItemValue<long>(this.CurSelectRow, "EntryCfgBomId", 0L) <= 0L)
				{
					e.Cancel = true;
					base.View.ShowErrMessage(ResManager.LoadKDString("配置物料没有对应的配置BOM，无法添加子项。请设置好配置BOM，再进行产品配置！", "015072000003339", 7, new object[0]), "", 0);
					return;
				}
				e.Cancel = !this.ValidateEntityAddDelete(this.CurSelectRow, true);
			}
			if (e.BarItemKey == "tbDeleteEntry")
			{
				int[] selectedRows = base.View.GetControl<EntryGrid>("FTreeEntity").GetSelectedRows();
				DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "TreeEntity", null);
				if (!ListUtils.IsEmpty<int>(selectedRows) && selectedRows.Count<int>() > 1)
				{
					e.Cancel = true;
					base.View.ShowMessage(ResManager.LoadKDString("不能批量删除，请逐条删除！", "015072000003201", 7, new object[0]), 0);
					return;
				}
				DynamicObject dynamicObject = this.BomConfigDatasMger.LstCfgEntityRowDatas.FirstOrDefault((DynamicObject w) => DataEntityExtend.GetDynamicObjectItemValue<string>(w, "RowId", null) == DataEntityExtend.GetDynamicObjectItemValue<string>(this.CurSelectRow, "ParentRowId", null));
				if (dynamicObject == null || DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "EntityBomCategory", 0) != 1 || DataEntityExtend.GetDynamicObjectItemValue<long>(this.CurSelectRow, "CfgBomEntryId", 0L) > 0L)
				{
					e.Cancel = true;
					base.View.ShowErrMessage(ResManager.LoadKDString("删除失败，只有手动新增的分录才能被删除！", "015072000002149", 7, new object[0]), "", 0);
					return;
				}
				e.Cancel = !this.ValidateEntityAddDelete(dynamicObject, false);
				if (e.Cancel)
				{
					return;
				}
				string dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<string>(this.CurSelectRow, "RowId", null);
				List<DynamicObject> list = this.BomConfigDatasMger.RemoveBomNodeCfgEntityDatas(dynamicObjectItemValue2, true);
				foreach (DynamicObject dynamicObject2 in list)
				{
					this.TreeRowExpand.Remove(DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject2, "Seq", 0) - 1);
				}
				for (int i = list.Count - 1; i >= 0; i--)
				{
					base.View.Model.DeleteEntryRow("FTreeEntity", dynamicValue.IndexOf(list[i]));
				}
				base.View.UpdateView("FTreeEntity");
				this.CurSelectRow = null;
			}
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00003E04 File Offset: 0x00002004
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALID", -1, 0L, null);
			if (value > 0L)
			{
				this.LoadTreeViewData(value);
				this.SetCurTreeNode();
			}
			if (base.Context.IsStandardEdition())
			{
				FormUtils.StdLockField(base.View, "FSupplyMode");
			}
			if (ObjectUtils.IsNullOrEmpty(base.View.ParentFormView) || (!ObjectUtils.IsNullOrEmpty(base.View.ParentFormView) && base.View.ParentFormView.BillBusinessInfo.GetForm().Id == "ENG_BOM") || (!ObjectUtils.IsNullOrEmpty(base.View.ParentFormView) && base.View.ParentFormView.BillBusinessInfo.GetForm().Id == "BOS_MainConsoleSutra") || (!ObjectUtils.IsNullOrEmpty(base.View.ParentFormView) && base.View.ParentFormView.BillBusinessInfo.GetForm().Id == "BOS_MainConsoleNewSutra") || (!ObjectUtils.IsNullOrEmpty(base.View.ParentFormView) && base.View.ParentFormView.BillBusinessInfo.GetForm().Id == "BOS_HtmlConsole") || (!ObjectUtils.IsNullOrEmpty(base.View.ParentFormView) && base.View.ParentFormView.BusinessInfo.GetForm().Id == "BOS_HtmlConsoleMain"))
			{
				base.View.GetMainBarItem("tbReturnData").Visible = false;
			}
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00003FDC File Offset: 0x000021DC
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			base.AfterCreateNewEntryRow(e);
			if (this.CurSelectRow == null)
			{
				return;
			}
			DynamicObject dynamicObject = null;
			int num = 0;
			this.Model.TryGetEntryCurrentRow("FTreeEntity", ref dynamicObject, ref num);
			string curParentRowId = DataEntityExtend.GetDynamicObjectItemValue<string>(this.CurSelectRow, "RowId", null);
			int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(this.CurSelectRow, "BomLevel", 0);
			string text = SequentialGuid.NewGuid().ToString("D");
			dynamicObject["ParentRowId"] = curParentRowId;
			dynamicObject["TrueParentRowId"] = curParentRowId;
			dynamicObject["BomLevel"] = dynamicObjectItemValue + 1;
			dynamicObject["RowId"] = text;
			dynamicObject["IsSelect"] = true;
			dynamicObject["IsCanEdit"] = true;
			dynamicObject["IsCanReplace"] = true;
			dynamicObject["ReplaceGroup"] = (from w in this.BomConfigDatasMger.LstCfgEntityRowDatas
			where DataEntityExtend.GetDynamicObjectItemValue<string>(w, "ParentRowId", null) == curParentRowId
			select w).Max((DynamicObject p) => DataEntityExtend.GetDynamicObjectItemValue<long>(p, "ReplaceGroup", 0L)) + 1L;
			base.View.Model.SetValue("FParentUnitID", DataEntityExtend.GetDynamicValue<long>(this.CurSelectRow, "CHILDUNITID_Id", 0L), num);
			base.View.Model.SetValue("FParentBaseUnitID", DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicValue<DynamicObject>(this.CurSelectRow, "MATERIALIDCHILD", null), "MaterialBase", null).First<DynamicObject>(), "BaseUnitId_Id", 0L), num);
			base.View.Model.SetValue("FPrentMtrlId", DataEntityExtend.GetDynamicValue<long>(this.CurSelectRow, "MATERIALIDCHILD_Id", 0L), num);
			this.BomConfigDatasMger.LstCfgEntityRowDatas.Add(dynamicObject);
			if (!ListUtils.IsEmpty<BomExpandNodeConfigMode>(this.BomConfigDatasMger.LstStandBomExpandNodes))
			{
				BomExpandNodeConfigMode bomExpandNodeConfigMode = new BomExpandNodeConfigMode(new DynamicObject(this.BomConfigDatasMger.LstStandBomExpandNodes.First<BomExpandNodeConfigMode>().DataEntity.DynamicObjectType));
				bomExpandNodeConfigMode.EntryId = text;
				bomExpandNodeConfigMode.ParentEntryId = curParentRowId;
				bomExpandNodeConfigMode.BomLevel = (long)(dynamicObjectItemValue + 1);
				this.BomConfigDatasMger.LstStandBomExpandNodes.Add(bomExpandNodeConfigMode);
			}
			MFGBillUtil.SetEffectDate(base.View, "FEFFECTDATE", e.Row, 0L);
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00004255 File Offset: 0x00002455
		public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterEntryBarItemClick(e);
			if (!StringUtils.EqualsIgnoreCase(e.BarItemKey, "tbDeleteEntry"))
			{
				this.CurSelectRow = null;
			}
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00004278 File Offset: 0x00002478
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FMATERIALTYPE"))
				{
					if (!(key == "FDOSAGETYPE"))
					{
						if (!(key == "FParentMaterialOrgId"))
						{
							return;
						}
						if (e.Value == null)
						{
							base.View.ShowErrMessage(ResManager.LoadKDString("生产组织不能被清空！", "015072000003202", 7, new object[0]), "", 0);
							e.Cancel = true;
						}
					}
					else if (3 == Convert.ToInt16(e.Value))
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("用量类型不能选择“阶梯”！", "015072000002151", 7, new object[0]), "", 0);
						e.Cancel = true;
						return;
					}
				}
				else if (3 == Convert.ToInt16(e.Value))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("子项类型不能选择“替代件”！", "015072000002150", 7, new object[0]), "", 0);
					e.Cancel = true;
					return;
				}
			}
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00004398 File Offset: 0x00002598
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			List<string> list = new List<string>
			{
				"FIsSelect",
				"FCfgNumber",
				"FCfgName"
			};
			if (list.Contains(e.Field.Key))
			{
				base.View.Model.DataChanged = true;
			}
			if (e.Field.Key == "FMATERIALID")
			{
				long num = 0L;
				if (e.NewValue != null)
				{
					long.TryParse(e.NewValue.ToString(), out num);
				}
				long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, num);
				long value = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
				IEnumerable<DynamicObject> defCfgBomsId = BOMServiceHelper.GetDefCfgBomsId(base.View.Context, materialMasterAndUserOrgId[0], value, 0L);
				DynamicObject dynamicObject = (defCfgBomsId == null) ? null : defCfgBomsId.FirstOrDefault<DynamicObject>();
				this.BomConfigDatasMger.CfgBomId = ((dynamicObject == null) ? 0L : DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L));
				this.BomConfigDatasMger.MainOrgId = value;
				this.BomConfigDatasMger.CfgMatrailId = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "MATERIALID_Id", 0L);
				this.AddBomExpandNodes(this.BomConfigDatasMger.CfgBomId, this.BomConfigDatasMger.CfgMatrailId, 0L, 0L, "");
				if (num > 0L && this.BomConfigDatasMger.CfgBomId <= 0L)
				{
					base.View.ShowWarnningMessage(ResManager.LoadKDString("物料没有对应的已审核的配置BOM", "015072000003340", 7, new object[0]), "", 0, null, 1);
				}
				this.IsDataChangeExpand = true;
				this.LoadTreeViewData(num);
				this.SetCurTreeNode();
				return;
			}
			if (StringUtils.EqualsIgnoreCase(e.Field.Key, "FAuxPropId"))
			{
				DynamicObject dynamicObject2 = e.OldValue as DynamicObject;
				if (dynamicObject2 != null)
				{
					dynamicObject2["Id"] = 0;
					DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
					DynamicObject dynamicObject3 = dynamicObjectCollection[e.Row];
					dynamicObject3["AuxPropId_Id"] = 0;
				}
			}
			List<string> list2 = new List<string>
			{
				"FMATERIALID",
				"FIsSelect",
				"FMATERIALIDCHILD",
				"FParentMaterialOrgId",
				"FParentMaterialId",
				"FBOMID"
			};
			if (!list2.Contains(e.Field.Key))
			{
				return;
			}
			BomView.TreeEntity treeEntity = null;
			DynamicObjectCollection lstCfgEntityRowDatas = this.BomConfigDatasMger.LstCfgEntityRowDatas;
			if (!ListUtils.IsEmpty<DynamicObject>(lstCfgEntityRowDatas) && e.Row > 0)
			{
				string rowId = MFGBillUtil.GetValue<string>(base.View.Model, "FRowId", e.Row, null, null);
				DynamicObject dynamicObject4 = lstCfgEntityRowDatas.FirstOrDefault((DynamicObject w) => DataEntityExtend.GetDynamicObjectItemValue<string>(w, "RowId", null) == rowId);
				treeEntity = new BomView.TreeEntity(dynamicObject4);
			}
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (key == "FIsSelect")
				{
					bool isSelect = Convert.ToBoolean(e.NewValue);
					this.SetSelectRows(lstCfgEntityRowDatas, treeEntity, isSelect);
					return;
				}
				if (!(key == "FMATERIALIDCHILD"))
				{
					if (key == "FParentMaterialOrgId")
					{
						base.View.Model.SetItemValueByNumber("FParentMaterialId", (treeEntity.MATERIALIDCHILD == null) ? null : treeEntity.MATERIALIDCHILD.Number, e.Row);
						return;
					}
					if (key == "FParentMaterialId")
					{
						this.ReSetNewBom(treeEntity);
						this.UpdateTreeEntryByPrentUnit();
						return;
					}
					if (!(key == "FBOMID"))
					{
						return;
					}
					if (!this.IsInSertRow)
					{
						DynamicObject value2 = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FBOMID", e.Row, null, null);
						if (value2 != null)
						{
							int dynamicValue = DataEntityExtend.GetDynamicValue<int>(value2, "BOMCATEGORY", 0);
							if (dynamicValue == 2)
							{
								base.View.Model.SetValue("FEntryCfgBomId", e.NewValue, e.Row);
								base.View.Model.SetValue("FNormBomId", 0, e.Row);
								return;
							}
							base.View.Model.SetValue("FNormBomId", e.NewValue, e.Row);
							base.View.Model.SetValue("FEntryCfgBomId", e.NewValue, e.Row);
							return;
						}
						else
						{
							base.View.Model.SetValue("FNormBomId", e.NewValue, e.Row);
							base.View.Model.SetValue("FEntryCfgBomId", e.NewValue, e.Row);
						}
					}
				}
				else
				{
					if (MFGBillUtil.GetValue<long>(base.View.Model, "FParentMaterialOrgId", e.Row, 0L, null) <= 0L)
					{
						base.View.Model.SetValue("FParentMaterialOrgId", (treeEntity.MATERIALIDCHILD == null) ? 0L : treeEntity.MATERIALIDCHILD.UseOrgId_Id, e.Row);
					}
					else
					{
						base.View.Model.SetItemValueByNumber("FParentMaterialId", (treeEntity.MATERIALIDCHILD == null) ? null : treeEntity.MATERIALIDCHILD.Number, e.Row);
					}
					if (this.Model.GetValue("FBOMID", e.Row) == null)
					{
						DynamicObject dynamicObject5 = this.Model.GetValue("FMATERIALIDCHILD", e.Row) as DynamicObject;
						if (ObjectUtils.IsNullOrEmpty(dynamicObject5))
						{
							return;
						}
						string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>((dynamicObject5["MaterialBase"] as DynamicObjectCollection)[0], "ErpClsID", null);
						if (dynamicValue2 != "9")
						{
							base.View.Model.SetValue("FNormBomId", 0, e.Row);
							base.View.Model.SetValue("FEntryCfgBomId", 0, e.Row);
							return;
						}
					}
					else
					{
						DynamicObject dynamicObject6 = this.Model.GetValue("FMATERIALIDCHILD", e.Row) as DynamicObject;
						if (ObjectUtils.IsNullOrEmpty(dynamicObject6))
						{
							return;
						}
						string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>((dynamicObject6["MaterialBase"] as DynamicObjectCollection)[0], "ErpClsID", null);
						if (dynamicValue3 == "9")
						{
							base.View.Model.SetValue("FNormBomId", 0, e.Row);
							return;
						}
					}
				}
			}
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00004A27 File Offset: 0x00002C27
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00004A30 File Offset: 0x00002C30
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			e.DynamicFormShowParameter.MultiSelect = false;
			string filter = e.ListFilterParameter.Filter;
			if (this.GetF7AndSetNumberEvent(e.FieldKey, e.Row, out filter))
			{
				e.Cancel = true;
			}
			else if (!string.IsNullOrWhiteSpace(filter))
			{
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, filter);
			}
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FBOMID"))
			{
				e.IsShowApproved = false;
			}
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00004ABC File Offset: 0x00002CBC
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string filter = e.Filter;
			this.GetF7AndSetNumberEvent(e.BaseDataFieldKey, e.Row, out filter);
			if (!string.IsNullOrWhiteSpace(filter))
			{
				e.Filter = base.SqlAppendAnd(e.Filter, filter);
			}
			if (StringUtils.EqualsIgnoreCase(e.BaseDataFieldKey, "FBOMID"))
			{
				e.IsShowApproved = false;
			}
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00004B20 File Offset: 0x00002D20
		protected virtual OperateOption GetSaveBomOption()
		{
			OperateOption operateOption = OperateOption.Create();
			operateOption.SetVariableValue("FNumber", MFGBillUtil.GetValue<string>(base.View.Model, "FCfgNumber", -1, null, null));
			operateOption.SetVariableValue("FName", MFGBillUtil.GetValue<ILocaleValue>(base.View.Model, "FCfgName", -1, null, null));
			operateOption.SetVariableValue("FCfgBomId", this.BomConfigDatasMger.CfgBomId);
			return operateOption;
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00004BC8 File Offset: 0x00002DC8
		private bool DoBachOperSave(string oper)
		{
			if (!this.ValidateOperaionPermission(oper) || !this.ValidateOperation(oper, this.BomConfigDatasMger.LstCfgEntityRowDatas))
			{
				return false;
			}
			FormOperation operation = this.BomConfigDatasMger.StandBomMetadata.BusinessInfo.GetForm().GetOperation(oper);
			OperateOption saveBomOption = this.GetSaveBomOption();
			DynamicObject dynamicObject = (from w in this.BomConfigDatasMger.LstCfgEntityRowDatas
			where Convert.ToInt64(w["BomLevel"]) == 0L
			select w).FirstOrDefault<DynamicObject>();
			if (dynamicObject != null)
			{
				dynamicObject["AuxPropId_Id"] = DataEntityExtend.GetDynamicValue<long>(base.View.Model.DataObject, "ParentAuxPropId_Id", 0L);
				dynamicObject["AuxPropId"] = base.View.Model.DataObject["ParentAuxPropId"];
			}
			IOperationResult operationResult = BOMServiceHelper.SaveStandBomsByCfg(base.View.Context, this.BomConfigDatasMger.LstCfgEntityRowDatas, saveBomOption, oper == "Save");
			if (operationResult == null)
			{
				return false;
			}
			if (operationResult.IsSuccess && (oper == "Save" || oper == "Draft"))
			{
				DynamicObject dynamicObject2 = operationResult.SuccessDataEnity.FirstOrDefault((DynamicObject w) => DataEntityExtend.GetDynamicObjectItemValue<long>(w, "CfgBomId", 0L) == this.BomConfigDatasMger.CfgBomId);
				if (dynamicObject2 != null)
				{
					this.standBomDatas = new List<DynamicObject>();
					this.standBomDatas.AddRange(operationResult.SuccessDataEnity.ToList<DynamicObject>());
					this.SetTopStandBom(dynamicObject2);
					this.SetHeadmFaceBystandBom(this.BomConfigDatasMger.TopStandBom);
				}
				base.View.Model.DataChanged = false;
				base.View.UpdateView("FTreeEntity");
				base.View.SendDynamicFormAction(base.View);
			}
			MFGCommonUtil.BatchWriteLog(base.View.Context, this.BomConfigDatasMger.StandBomMetadata.BusinessInfo, operationResult, operation);
			MFGBillUtil.ShowOperateResult(base.View, operationResult, operation);
			return operationResult.IsSuccess;
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00004DE8 File Offset: 0x00002FE8
		private bool DoBachOper(string oper)
		{
			if (base.View.Model.DataChanged)
			{
				base.View.ShowMessage(ResManager.LoadKDString("产品配置信息进行过修改，请先保存！", "015072000003204", 7, new object[0]), 0);
				return false;
			}
			if (!this.ValidateOperaionPermission(oper))
			{
				return false;
			}
			FormOperation operation = this.BomConfigDatasMger.StandBomMetadata.BusinessInfo.GetForm().GetOperation(oper);
			Dictionary<string, string> bachDealOper = this.BomConfigDatasMger.GetBachDealOper(oper);
			DynamicObject topStandBom = this.BomConfigDatasMger.TopStandBom;
			if (topStandBom == null || this.BomConfigDatasMger.StandBomId == 0L)
			{
				base.View.ShowMessage(string.Format(ResManager.LoadKDString("配置BOM已经审核！", "015072000014362", 7, new object[0]), operation.OperationName), 0);
				return false;
			}
			IOperationResult operationResult = null;
			if (oper != null)
			{
				if (!(oper == "Submit"))
				{
					if (!(oper == "CancelAssign") && !(oper == "Audit"))
					{
						if (!(oper == "UnAudit"))
						{
							goto IL_3DB;
						}
						List<long> list = (from s in bachDealOper.Keys
						select Convert.ToInt64(s)).ToList<long>();
						operationResult = new OperationResult();
						List<long> list2 = this.DoAuthIsSamePersonForAuditUnaudit(list);
						if (!ListUtils.IsEmpty<long>(list2))
						{
							operationResult.IsSuccess = false;
							foreach (long num in list2)
							{
								if (list.Contains(num))
								{
									list.Remove(num);
								}
								OperateResult item = new OperateResult
								{
									Message = ResManager.LoadKDString("当前用户不是审核人，不允许反审核该单据。", "015072030033292", 7, new object[0]),
									PKValue = num,
									SuccessStatus = false,
									MessageType = 0
								};
								operationResult.OperateResult.Add(item);
							}
						}
						if (!ListUtils.IsEmpty<long>(list))
						{
							List<KeyValuePair<object, object>> list3 = (from w in list
							select new KeyValuePair<object, object>(w, "")).ToList<KeyValuePair<object, object>>();
							IOperationResult operationResult2 = BusinessDataServiceHelper.SetBillStatus(base.View.Context, this.BomConfigDatasMger.StandBomMetadata.BusinessInfo, list3, new List<object>
							{
								"2"
							}, oper, null);
							operationResult.IsSuccess = operationResult2.IsSuccess;
							OperationResultExt.MergeResult(operationResult, operationResult2);
							goto IL_3DB;
						}
						goto IL_3DB;
					}
				}
				else
				{
					List<IOperationResult> list4 = MFGCommonUtil.SubmitWithWorkFlow(base.View.Context, "ENG_BOM", bachDealOper, OperateOption.Create());
					using (List<IOperationResult>.Enumerator enumerator2 = list4.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							IOperationResult operationResult3 = enumerator2.Current;
							if (ObjectUtils.IsNullOrEmpty(operationResult))
							{
								operationResult = operationResult3;
							}
							else
							{
								OperationResultExt.MergeResult(operationResult, operationResult3);
							}
						}
						goto IL_3DB;
					}
				}
				List<long> list5 = new List<long>();
				IOperationResult operationResult4 = this.ChkTargetExistWorkFlow(bachDealOper, (oper == "Audit") ? "审核" : "撤销", list5);
				List<KeyValuePair<object, object>> list6 = (from w in list5
				select new KeyValuePair<object, object>(w, "")).ToList<KeyValuePair<object, object>>();
				if (!ListUtils.IsEmpty<KeyValuePair<object, object>>(list6))
				{
					operationResult = BusinessDataServiceHelper.SetBillStatus(base.View.Context, this.BomConfigDatasMger.StandBomMetadata.BusinessInfo, list6, new List<object>
					{
						(oper == "Audit") ? "1" : "2",
						""
					}, oper, null);
				}
				if (ObjectUtils.IsNullOrEmpty(operationResult))
				{
					operationResult = operationResult4;
				}
				else
				{
					OperationResultExt.MergeResult(operationResult, operationResult4);
				}
				if (operationResult.IsSuccess)
				{
					base.View.StyleManager.SetEnabled("FParentAuxPropId", "", false);
					base.View.UpdateView("FParentAuxPropId");
				}
			}
			IL_3DB:
			if (operationResult == null)
			{
				return false;
			}
			if (operationResult.IsSuccess)
			{
				this.SetTopStandBom(this.GetBomInfo(this.BomConfigDatasMger.StandBomId));
				this.SetHeadmFaceBystandBom(this.BomConfigDatasMger.TopStandBom);
				DBServiceHelper.LoadReferenceObject(base.View.Context, this.BomConfigDatasMger.LstCfgEntityRowDatas.ToArray<DynamicObject>(), this.BomConfigDatasMger.LstCfgEntityRowDatas.First<DynamicObject>().DynamicObjectType, false);
				base.View.Model.DataChanged = false;
				base.View.UpdateView("FTreeEntity");
				base.View.SendDynamicFormAction(base.View);
				object obj = this.Model.DataObject["TreeEntity"];
				this.LockFTreeEntity(oper);
				this.DoExpandedRow();
			}
			MFGCommonUtil.BatchWriteLog(base.View.Context, this.BomConfigDatasMger.StandBomMetadata.BusinessInfo, operationResult, operation);
			MFGBillUtil.ShowOperateResult(base.View, operationResult, operation);
			return true;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000052E0 File Offset: 0x000034E0
		private void LockFTreeEntity(string oper)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FTreeEntity");
			if (oper != null)
			{
				if (oper == "Submit" || oper == "Audit")
				{
					base.View.StyleManager.SetEnabled(entryEntity, "", false);
					return;
				}
				if (!(oper == "CancelAssign") && !(oper == "UnAudit"))
				{
					return;
				}
				base.View.StyleManager.SetEnabled(entryEntity, "", true);
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x0000536C File Offset: 0x0000356C
		private IOperationResult ChkTargetExistWorkFlow(Dictionary<string, string> billDatas, string operationName, List<long> noExistPkIds)
		{
			IOperationResult operationResult = new OperationResult();
			foreach (KeyValuePair<string, string> keyValuePair in billDatas)
			{
				string key = keyValuePair.Key;
				bool flag = ProcManageServiceHelper.CheckUnCompletePrcInstExsit(base.Context, "ENG_BOM", key);
				if (flag)
				{
					string value = keyValuePair.Value;
					operationResult.IsSuccess = false;
					string message = string.Format(ResManager.LoadKDString("{0}{1}已关联工作流实例。不允许在产品配置中{2}！", "015072030041388", 7, new object[0]), this.BomConfigDatasMger.StandBomMetadata.BusinessInfo.GetForm().Name[base.Context.UserLocale.LCID], value, operationName);
					operationResult.OperateResult.Add(new OperateResult
					{
						PKValue = key,
						SuccessStatus = false,
						Message = message,
						Name = operationName
					});
				}
				else
				{
					noExistPkIds.Add(Convert.ToInt64(keyValuePair.Key));
				}
			}
			return operationResult;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000054A0 File Offset: 0x000036A0
		private List<long> DoAuthIsSamePersonForAuditUnaudit(List<long> lstBomId)
		{
			List<long> list = new List<long>();
			DynamicObject dynamicObject = SystemParameterServiceHelper.LoadBillGlobalParameter(base.Context, new string[]
			{
				"ENG_BOM"
			}).FirstOrDefault<DynamicObject>();
			bool dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<bool>(dynamicObject, "SamePersonUnaudit", false);
			if (dynamicObjectItemValue)
			{
				BusinessInfo businessInfo = this.BomConfigDatasMger.StandBomMetadata.BusinessInfo;
				BillStatusField billStatusField = businessInfo.GetBillStatusField();
				if (billStatusField != null)
				{
					StatusItem statusItem = (from t in billStatusField.StatusItems
					where t.StatusValue == "C"
					select t).FirstOrDefault<StatusItem>();
					if (statusItem == null)
					{
						return list;
					}
					UserField userField = (UserField)businessInfo.GetField(statusItem.OperationerKey);
					QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
					queryBuilderParemeter.FormId = businessInfo.GetForm().Id;
					queryBuilderParemeter.BusinessInfo = businessInfo;
					queryBuilderParemeter.SelectItems.Add(new SelectorItemInfo(businessInfo.GetForm().PkFieldName));
					queryBuilderParemeter.SelectItems.Add(new SelectorItemInfo(userField.Key));
					queryBuilderParemeter.SelectItems.Add(new SelectorItemInfo(billStatusField.Key));
					queryBuilderParemeter.FilterClauseWihtKey = string.Format("{0} in ({1})", businessInfo.GetForm().PkFieldName, string.Join<long>(",", lstBomId));
					DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.View.Context, queryBuilderParemeter, null);
					foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
					{
						string text = Convert.ToString(dynamicObject2[2]);
						if (string.IsNullOrEmpty(text) || !(text != "C"))
						{
							long num = Convert.ToInt64(dynamicObject2[1]);
							if (num != 0L)
							{
								object value = dynamicObject2[0];
								if (base.View.Context.UserId != num)
								{
									list.Add(Convert.ToInt64(value));
								}
							}
						}
					}
				}
			}
			return list;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x000056C4 File Offset: 0x000038C4
		private bool ValidateManualMaterial(ref bool isPass, DynamicObjectCollection entityDatas)
		{
			IEnumerable<DynamicObject> enumerable = from w in entityDatas
			where DataEntityExtend.GetDynamicObjectItemValue<long>(w, "MATERIALIDCHILD_Id", 0L) <= 0L
			select w;
			if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
			{
				isPass = false;
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("第{0}行分录，手动录入行子项物料必录！", "015072000003205", 7, new object[0]), string.Join<int>(",", (from w in enumerable
				select DataEntityExtend.GetDynamicObjectItemValue<int>(w, "Seq", 0)).ToList<int>())), "", 0);
			}
			return isPass;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x000057F4 File Offset: 0x000039F4
		private bool ValidateCfgMaterial(ref bool isPass, DynamicObjectCollection entityDatas)
		{
			bool flag = false;
			IEnumerable<DynamicObject> enumerable = from w in entityDatas
			where DataEntityExtend.GetDynamicObjectItemValue<int>(w, "EntityBomCategory", 0) == 1 && DataEntityExtend.GetDynamicValue<bool>(w, "IsSelect", false)
			select w;
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary = (from x in entityDatas
			where !string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(x, "ParentRowId", null))
			group x by DataEntityExtend.GetDynamicValue<string>(x, "ParentRowId", null)).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary2 = (from x in entityDatas
			where !string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(x, "TrueParentRowId", null))
			group x by string.Format("{0}||{1}", DataEntityExtend.GetDynamicValue<bool>(x, "IsSelect", false), DataEntityExtend.GetDynamicValue<string>(x, "TrueParentRowId", null))).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
			foreach (DynamicObject dynamicObject in enumerable)
			{
				IGrouping<string, DynamicObject> grouping = null;
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "RowId", null);
				if (!dictionary.TryGetValue(dynamicValue, out grouping))
				{
					isPass = false;
					base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("第{0}行分录，配置物料“{1}”必须要有子项配置信息！", "015072000003206", 7, new object[0]), DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "Seq", 0), DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>((DynamicObject)dynamicObject["MATERIALIDCHILD"], "Name", null).ToString()), "", 0);
					break;
				}
				if (DataEntityExtend.GetDynamicObjectItemValue<bool>(dynamicObject, "IsSelect", false))
				{
					IGrouping<string, DynamicObject> grouping2 = null;
					bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IsSelect", false);
					string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "RowId", null);
					string key = string.Format("{0}||{1}", dynamicValue2, dynamicObjectItemValue);
					if (!dictionary2.TryGetValue(key, out grouping2))
					{
						isPass = false;
						base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("第{0}行分录，物料“{1}”必须选中一个下级子项！", "015072000003207", 7, new object[0]), DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "Seq", 0), DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>((DynamicObject)dynamicObject["MATERIALIDCHILD"], "Name", null).ToString()), "", 0);
						break;
					}
					flag = true;
				}
			}
			if (!flag)
			{
				isPass = false;
				base.View.ShowErrMessage(ResManager.LoadKDString("没有标准BOM可以生成！", "015072000002158", 7, new object[0]), "", 0);
			}
			return isPass;
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00005B40 File Offset: 0x00003D40
		private bool ValidateFeatureMaterial(string oper, ref bool isPass, DynamicObjectCollection entityDatas)
		{
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary = null;
			IEnumerable<DynamicObject> enumerable = from w in entityDatas
			where DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsSelect", false) && DataEntityExtend.GetDynamicObjectItemValue<int>(w, "EntityBomCategory", 0) == 3
			select w;
			if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
			{
				IEnumerable<DynamicObject> source = from w in entityDatas
				where DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsSelect", false) && DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsParentFeature", false)
				select w;
				dictionary = (from g in source
				group g by DataEntityExtend.GetDynamicObjectItemValue<string>(g, "ParentRowId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
			}
			foreach (DynamicObject dynamicObject in enumerable)
			{
				string a = "1";
				DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MATERIALIDCHILD", null);
				if (!ObjectUtils.IsNullOrEmpty(dynamicValue))
				{
					a = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue, "MaterialBase", null).FirstOrDefault<DynamicObject>(), "FeatureItem", null);
				}
				IGrouping<string, DynamicObject> source2;
				if (dictionary.TryGetValue(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "RowId", null), out source2) && source2.Count<DynamicObject>() > 1 && a == "1")
				{
					isPass = false;
					base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("第{0}行分录，特征件“{1}”只能指定一个构成！", "015072000003208", 7, new object[0]), DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "Seq", 0), DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>((DynamicObject)dynamicObject["MATERIALIDCHILD"], "Name", null)), "", 0);
					break;
				}
			}
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary2 = entityDatas.GroupBy(delegate(DynamicObject x)
			{
				string dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<string>(x, "ParentRowId", null);
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectItemValue2))
				{
					return dynamicObjectItemValue2;
				}
				return "root";
			}).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
			if (oper == "Save")
			{
				foreach (DynamicObject dynamicObject2 in enumerable)
				{
					IGrouping<string, DynamicObject> grouping = null;
					string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "RowId", null);
					if (dictionary2.TryGetValue(dynamicObjectItemValue, out grouping) && !dictionary.ContainsKey(dynamicObjectItemValue))
					{
						isPass = false;
						base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("第{0}行分录，特征件“{1}”没有指定其构成！", "015072000003209", 7, new object[0]), DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject2, "Seq", 0), DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>((DynamicObject)dynamicObject2["MATERIALIDCHILD"], "Name", null)), "", 0);
						break;
					}
				}
			}
			return isPass;
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00005E0C File Offset: 0x0000400C
		private bool ValidateOperation(string oper, DynamicObjectCollection entityDatas)
		{
			bool result = true;
			if (oper != null && (oper == "Draft" || oper == "Save") && this.ValidateManualMaterial(ref result, entityDatas) && this.ValidateCfgMaterial(ref result, entityDatas))
			{
				this.ValidateFeatureMaterial(oper, ref result, entityDatas);
			}
			return result;
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00005E5C File Offset: 0x0000405C
		private bool ValidateOperaionPermission(string oper)
		{
			string empty = string.Empty;
			bool flag = false;
			if (oper != null)
			{
				if (!(oper == "Draft") && !(oper == "Save"))
				{
					if (!(oper == "Submit"))
					{
						if (!(oper == "CancelAssign"))
						{
							if (!(oper == "UnAudit"))
							{
								if (oper == "Audit")
								{
									flag = this.ValidatePermission("47afe3d45bc84016b416a1206e121d45", out empty);
								}
							}
							else
							{
								flag = this.ValidatePermission("e4d6cdd9125a4ee5a32a4c27c12dadc9", out empty);
							}
						}
						else
						{
							flag = this.ValidatePermission("4ce350fdd203407cab4939d50f0022cc", out empty);
						}
					}
					else
					{
						flag = this.ValidatePermission("dd4d4cb1f143409da5777ec417cff26b", out empty);
					}
				}
				else
				{
					flag = this.ValidatePermission("fce8b1aca2144beeb3c6655eaf78bc34", out empty);
					if (flag)
					{
						flag = this.ValidatePermission("f323992d896745fbaab4a2717c79ce2e", out empty);
					}
				}
			}
			if (!flag && !string.IsNullOrWhiteSpace(empty))
			{
				base.View.ShowMessage(empty, 0);
			}
			return flag;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00005FBC File Offset: 0x000041BC
		private bool ValidatePermission(string permission, out string permissionMessage)
		{
			permissionMessage = null;
			List<DynamicObject> list = (from w in this.BomConfigDatasMger.LstCfgEntityRowDatas
			where DataEntityExtend.GetDynamicObjectItemValue<long>(w, "EntryCfgBomId", 0L) > 0L && DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(w, "ParentMaterialOrgId", null) != null && DataEntityExtend.GetDynamicObjectItemValue<int>(w, "EntityBomCategory", 0) == 1
			select w into p
			select DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(p, "ParentMaterialOrgId", null)).Distinct<DynamicObject>().ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				return true;
			}
			List<long> passOrgs = PermissionServiceHelper.GetPermissionOrg(base.Context, new BusinessObject
			{
				Id = "ENG_BOM",
				SubSystemId = base.View.Model.SubSytemId
			}, permission);
			IEnumerable<DynamicObject> enumerable = from w in list
			where !passOrgs.Contains(DataEntityExtend.GetDynamicObjectItemValue<long>(w, "Id", 0L))
			select w;
			bool flag = ListUtils.IsEmpty<DynamicObject>(enumerable);
			if (!flag)
			{
				FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.View.Context, FormIdConst.SEC_PermissionItem, true);
				DynamicObject dynamicObject = BusinessDataServiceHelper.LoadFromCache(base.View.Context, new object[]
				{
					permission
				}, formMetadata.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<DynamicObject>();
				string arg = DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>(dynamicObject, "Name", null).ToString();
				permissionMessage = string.Format(ResManager.LoadKDString("没有“物料清单”在组织 {0} 下的“{1}”权限！", "015072000002161", 7, new object[0]), string.Join(",", from w in enumerable
				select DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>(w, "Name", null).ToString()), arg);
			}
			return flag;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00006158 File Offset: 0x00004358
		private bool CheckCanClose()
		{
			if (!this.IsNeedReturnSoureForm())
			{
				return true;
			}
			bool result2 = true;
			if (this.BomConfigDatasMger.TopStandBom != null && DataEntityExtend.GetDynamicObjectItemValue<string>(this.BomConfigDatasMger.TopStandBom, "DocumentStatus", null) == 'Z'.ToString())
			{
				result2 = false;
				base.View.ShowMessage(ResManager.LoadKDString("生成的标准BOM暂存状态，无法返回到源单，确定要退出吗？", "015072000003403", 7, new object[0]), 4, delegate(MessageBoxResult result)
				{
					if (result == 6)
					{
						base.View.Close();
					}
				}, "", 0);
			}
			return result2;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000061E4 File Offset: 0x000043E4
		private void SetCurTreeNode()
		{
			long num = (this.BomConfigDatasMger.StandBomId > 0L) ? this.BomConfigDatasMger.StandBomId : this.BomConfigDatasMger.CfgBomId;
			if (num > 0L)
			{
				this.curNode = this.GetSelectNode(num.ToString());
				if (this.curNode != null)
				{
					base.View.GetControl<TreeView>("FConfigTreeView").Select(this.curNode.id);
				}
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x0000625C File Offset: 0x0000445C
		private void SetTopStandBom(DynamicObject topStandBom)
		{
			this.BomConfigDatasMger.TopStandBom = topStandBom;
			this.BomConfigDatasMger.StandBomId = 0L;
			this.BomConfigDatasMger.StandMatrailId = 0L;
			if (topStandBom != null)
			{
				this.BomConfigDatasMger.StandBomId = DataEntityExtend.GetDynamicObjectItemValue<long>(topStandBom, "Id", 0L);
				this.BomConfigDatasMger.StandMatrailId = DataEntityExtend.GetDynamicObjectItemValue<long>(topStandBom, "MATERIALID_Id", 0L);
			}
		}

		// Token: 0x06000048 RID: 72 RVA: 0x000062F8 File Offset: 0x000044F8
		private void SetSelectRows(DynamicObjectCollection entityDatas, BomView.TreeEntity curRow, bool isSelect)
		{
			if (!ListUtils.IsEmpty<DynamicObject>(entityDatas) && curRow != null)
			{
				List<DynamicObject> lstCurClickSelectAffectRows = this.BomConfigDatasMger.BillClickSetIsSelect(entityDatas, curRow, isSelect);
				base.View.UpdateView("FIsSelect", -1);
				base.View.UpdateView("FTrueParentRowId", -1);
				IEnumerable<DynamicObject> enumerable = from w in entityDatas
				where w != curRow.DataEntity && lstCurClickSelectAffectRows.Contains(w)
				select w;
				foreach (DynamicObject dynamicObject in enumerable)
				{
					base.View.RuleContainer.RaiseDataChanged("FIsSelect", dynamicObject, new BOSActionExecuteContext(base.View));
				}
			}
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000063E4 File Offset: 0x000045E4
		private void ReSetNewBom(BomView.TreeEntity curRow)
		{
			long num = 0L;
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View, "FParentMaterialId", curRow);
			if (value != null)
			{
				long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(curRow.DataEntity, "ParentMaterialOrgId_Id", 0L);
				long dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>(curRow.DataEntity, "EntryCfgBomId", 0L);
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(curRow.DataEntity, "AuxPropId_Id", 0L);
				num = this.GetDefaultBomInfo(DataEntityExtend.GetDynamicObjectItemValue<long>(value, "MsterID", 0L), dynamicObjectItemValue2, dynamicObjectItemValue, dynamicValue);
			}
			MFGBillUtil.SetValue(base.View, "FNormBomId", curRow, 0);
			MFGBillUtil.SetValue(base.View, "FNormBomEntryId", curRow, 0);
			MFGBillUtil.SetValue(base.View, "FBOMID", curRow, 0);
			DynamicObject dynamicObject = null;
			if (num > 0L)
			{
				FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
				DynamicObjectType dynamicObjectType = formMetaData.BusinessInfo.GetSubBusinessInfo(new List<string>
				{
					"FBOMCATEGORY"
				}).GetDynamicObjectType();
				dynamicObject = BusinessDataServiceHelper.Load(base.Context, new object[]
				{
					num
				}, dynamicObjectType).FirstOrDefault<DynamicObject>();
			}
			Enums.Enu_BOMCateGoryCfg bomCateGory = this.BomConfigDatasMger.GetBomCateGory(dynamicObject, value);
			IDynamicFormView view = base.View;
			string text = "FEntityBomCategory";
			DynamicObject dynamicObject2 = curRow;
			int num2 = bomCateGory;
			MFGBillUtil.SetValue(view, text, dynamicObject2, num2.ToString());
			if (bomCateGory == 1)
			{
				MFGBillUtil.SetValue(base.View, "FEntryCfgBomId", curRow, num);
			}
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00006578 File Offset: 0x00004778
		private void DoNewBomExpand(DynamicObject curRow, dynamic obj)
		{
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(curRow, "EntryCfgBomId", 0L);
			DynamicObject dynamicObject = null;
			if (dynamicValue > 0L)
			{
				FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
				DynamicObjectType dynamicObjectType = formMetaData.BusinessInfo.GetSubBusinessInfo(new List<string>
				{
					"FBOMCATEGORY"
				}).GetDynamicObjectType();
				dynamicObject = BusinessDataServiceHelper.Load(base.Context, new object[]
				{
					dynamicValue
				}, dynamicObjectType).FirstOrDefault<DynamicObject>();
			}
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View, "FParentMaterialId", curRow);
			long materialId = (value != null) ? DataEntityExtend.GetDynamicObjectItemValue<long>(value, "Id", 0L) : 0L;
			Enums.Enu_BOMCateGoryCfg bomCateGory = this.BomConfigDatasMger.GetBomCateGory(dynamicObject, value);
			IDynamicFormView view = base.View;
			string text = "FEntityBomCategory";
			int num = bomCateGory;
			MFGBillUtil.SetValue(view, text, curRow, num.ToString());
			List<BomExpandNodeConfigMode> list = null;
			List<DynamicObject> list2 = null;
			if (dynamicValue > 0L)
			{
				list = this.DoExpand(dynamicValue, materialId, null);
				list2 = BOMServiceHelper.GetBomEntryDatas(base.View.Context, (from w in list
				select w.BomId_Id).ToList<long>(), false).ToList<DynamicObject>();
			}
			string value2 = MFGBillUtil.GetValue<string>(base.View, "FRowId", curRow);
			this.BomConfigDatasMger.AddLstStandBomExpandNodes(list, list2, value2);
			this.ShowBomConfigChildEntity();
		}

		// Token: 0x0600004B RID: 75 RVA: 0x000066D0 File Offset: 0x000048D0
		private void SetHeadmFaceBystandBom(DynamicObject standBom)
		{
			this.SetHeadFaceOtherInfoByStandBom(standBom);
			if (standBom == null)
			{
				return;
			}
			base.View.Model.SetValue("FCfgNumber", DataEntityExtend.GetDynamicObjectItemValue<string>(standBom, "Number", null));
			base.View.Model.SetValue("FCfgName", DataEntityExtend.GetDynamicObjectItemValue<ILocaleValue>(standBom, "Name", null));
			base.View.Model.SetValue("FParentAuxPropId", DataEntityExtend.GetDynamicObjectItemValue<long>(standBom, "ParentAuxPropId_Id", 0L));
			base.View.StyleManager.SetEnabled("FUseOrgId", "", false);
			base.View.StyleManager.SetEnabled("FMATERIALID", "", false);
			string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(standBom, "DocumentStatus", null);
			bool flag = dynamicObjectItemValue == 'C'.ToString() || dynamicObjectItemValue == 'B'.ToString();
			if (StringUtils.EqualsIgnoreCase(dynamicObjectItemValue, "Z") || StringUtils.EqualsIgnoreCase(dynamicObjectItemValue, "A") || StringUtils.EqualsIgnoreCase(dynamicObjectItemValue, "D") || this.IsCopyLock)
			{
				base.View.StyleManager.SetEnabled("FParentAuxPropId", "", true);
			}
			else
			{
				base.View.StyleManager.SetEnabled("FParentAuxPropId", "", false);
			}
			base.View.StyleManager.SetEnabled("FCfgNumber", "", !flag);
			base.View.StyleManager.SetEnabled("FCfgName", "", !flag);
			this.LocalMainBarItem();
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00006864 File Offset: 0x00004A64
		private void SetHeadmFaceByCfgBom(DynamicObject cfgBom)
		{
			base.View.Model.SetValue("FCfgNumber", "");
			base.View.Model.SetValue("FCfgName", null);
			if (cfgBom != null)
			{
				base.View.Model.SetValue("FParentAuxPropId", DataEntityExtend.GetDynamicValue<long>(cfgBom, "ParentAuxPropId_Id", 0L));
				base.View.StyleManager.SetEnabled("FParentAuxPropId", "", true);
			}
			base.View.StyleManager.SetEnabled("FUseOrgId", "", true);
			base.View.StyleManager.SetEnabled("FMATERIALID", "", true);
			base.View.StyleManager.SetEnabled("FCfgNumber", "", true);
			base.View.StyleManager.SetEnabled("FCfgName", "", true);
			base.View.GetMainBarItem("tbSaveTemp").Enabled = true;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00006968 File Offset: 0x00004B68
		private void SetHeadFaceOtherInfoByStandBom(DynamicObject standBom)
		{
			if (standBom != null)
			{
				base.View.Model.SetValue("FCreatorId", DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(standBom, "CreatorId", null));
				base.View.Model.SetValue("FCreateDate", DataEntityExtend.GetDynamicObjectItemValue<DateTime>(standBom, "CreateDate", default(DateTime)));
			}
			else
			{
				base.View.Model.SetValue("FCreatorId", base.View.Context.UserId);
				base.View.Model.SetValue("FCreateDate", MFGServiceHelper.GetSysDate(base.View.Context));
			}
			base.View.Model.SetValue("FModifierId", DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(standBom, "ModifierId", null));
			base.View.Model.SetValue("FModifyDate", DataEntityExtend.GetDynamicObjectItemValue<DateTime>(standBom, "ModifyDate", default(DateTime)));
			base.View.Model.SetValue("FApproverId", DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(standBom, "ApproverId", null));
			base.View.Model.SetValue("FApproveDate", DataEntityExtend.GetDynamicObjectItemValue<DateTime>(standBom, "ApproveDate", default(DateTime)));
			base.View.Model.SetValue("FDocumentStatus", DataEntityExtend.GetDynamicObjectItemValue<char>(standBom, "DocumentStatus", '\0'));
			base.View.Model.SetValue("FForbidderId", DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(standBom, "ForbidderId", null));
			base.View.Model.SetValue("FForbidDate", DataEntityExtend.GetDynamicObjectItemValue<DateTime>(standBom, "ForbidDate", default(DateTime)));
			base.View.Model.SetValue("FForbidStatus", DataEntityExtend.GetDynamicObjectItemValue<char>(standBom, "ForbidStatus", '\0'));
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00006BA4 File Offset: 0x00004DA4
		private void LocalMainBarItem()
		{
			if (base.View.OpenParameter.Status == 1)
			{
				return;
			}
			IEnumerable<DynamicObject> enumerable = this.BomConfigDatasMger.LstCfgEntityRowDatas.Where(delegate(DynamicObject w)
			{
				if (DataEntityExtend.GetDynamicObjectItemValue<int>(w, "EntityBomCategory", 0) == 1)
				{
					DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(w, "BOMID", null);
					return dynamicObjectItemValue != null && DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue, "DocumentStatus", null) != 'Z'.ToString();
				}
				return false;
			});
			base.View.GetMainBarItem("tbSaveTemp").Enabled = ListUtils.IsEmpty<DynamicObject>(enumerable);
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00006C10 File Offset: 0x00004E10
		private bool GetF7AndSetNumberEvent(string fieldKey, int eRow, out string filter)
		{
			bool flag = false;
			filter = null;
			if (fieldKey != null && (fieldKey == "FParentMaterialOrgId" || fieldKey == "FParentMaterialOrgIdMirror"))
			{
				BOSEnums.Enu_BaseDataPolicyType baseDataPolicyType = MFGServiceHelper.GetBaseDataPolicyType(base.View.Context, "BD_MATERIAL");
				if (baseDataPolicyType == 3)
				{
					flag = true;
				}
				else if (baseDataPolicyType == 2)
				{
					long value = MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALIDCHILD", eRow, 0L, null);
					List<long> sameMainMaterialUseOrgIds = MaterialServiceHelper.GetSameMainMaterialUseOrgIds(base.View.Context, value);
					if (ListUtils.IsEmpty<long>(sameMainMaterialUseOrgIds))
					{
						flag = true;
					}
					else
					{
						filter = string.Format("FOrgID in ({0})", string.Join<long>(",", sameMainMaterialUseOrgIds));
						filter = this.GetChildSupplyOrgFilter(filter, eRow);
					}
				}
			}
			if (flag)
			{
				filter = " 1 = 0 ";
			}
			return flag;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00006CD0 File Offset: 0x00004ED0
		private string GetChildSupplyOrgFilter(string filter, int row)
		{
			if (filter == null)
			{
				filter = string.Empty;
			}
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			List<long> list = new List<long>
			{
				102L,
				112L,
				101L,
				104L,
				103L,
				109L
			};
			List<long> list2 = new List<long>();
			List<long> orgByBizRelationOrgs = MFGServiceHelper.GetOrgByBizRelationOrgs(base.Context, value, list);
			if (orgByBizRelationOrgs == null || orgByBizRelationOrgs.Count < 1)
			{
				return filter;
			}
			list2.AddRange(orgByBizRelationOrgs);
			list2.Add(value);
			filter += ((filter.Length > 0) ? (" AND " + string.Format("FORGID in ({0})", string.Join<long>(",", list2))) : string.Format("FORGID in ({0})", string.Join<long>(",", list2)));
			return filter;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00006DC0 File Offset: 0x00004FC0
		private bool IsNeedReturnSoureForm()
		{
			if (base.View.ParentFormView == null)
			{
				return false;
			}
			string id = base.View.ParentFormView.BillBusinessInfo.GetForm().Id;
			return id == "PRD_MO" || id == "SUB_SUBREQORDER" || id == "SAL_SaleOrder" || id == "PLN_FORECAST";
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00006E2C File Offset: 0x0000502C
		private long GetDefaultBomInfo(long materialMsterId, long bomId, long orgId, long auxpropId)
		{
			long num = 0L;
			if (bomId > 0L)
			{
				DynamicObject bomInfo = this.GetBomInfo(bomId);
				DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(bomInfo, "MATERIALID", null);
				if (materialMsterId == DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectItemValue, "msterID", 0L))
				{
					string text = string.Format("FMASTERID ={0} AND FUseOrgId={1} AND FForbidStatus = 'A' AND FDocumentStatus = 'C' ", DataEntityExtend.GetDynamicObjectItemValue<int>(bomInfo, "msterID", 0), orgId);
					List<SelectorItemInfo> list = new List<SelectorItemInfo>
					{
						new SelectorItemInfo("FMASTERID")
					};
					DynamicObject dynamicObject = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_BOM", list, text, "").FirstOrDefault<DynamicObject>();
					num = ((dynamicObject != null) ? DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L) : 0L);
				}
			}
			if (num <= 0L)
			{
				num = BOMServiceHelper.GetHightVersionBomKey(base.Context, materialMsterId, orgId, auxpropId);
			}
			return num;
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00006EF4 File Offset: 0x000050F4
		private bool ValidateEntityAddDelete(DynamicObject row, bool isAdd)
		{
			DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(row, "BOMID", null);
			if (dynamicObjectItemValue != null)
			{
				BOSEnums.Enu_BillStatus dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<char>(dynamicObjectItemValue, "DocumentStatus", '\0');
				if (DataEntityExtend.GetDynamicValue<int>(dynamicObjectItemValue, "BOMCATEGORY", 0) == 2)
				{
					return true;
				}
				if (dynamicObjectItemValue2 == 66 || dynamicObjectItemValue2 == 67)
				{
					base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("标准BOM“{0}”是审核中或审核状态，不能{1}子项！", "015072000003210", 7, new object[0]), DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue, "Number", null), isAdd ? ResManager.LoadKDString("添加", "015072000002164", 7, new object[0]) : ResManager.LoadKDString("删除", "015072000002165", 7, new object[0])), "", 0);
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00006FBC File Offset: 0x000051BC
		private void DoExpandedRow()
		{
			TreeEntryGrid control = base.View.GetControl<TreeEntryGrid>("FTreeEntity");
			if (this.IsAllExpand)
			{
				using (IEnumerator<KeyValuePair<int, bool>> enumerator = (from w in this.TreeRowExpand
				where w.Value
				select w).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<int, bool> keyValuePair = enumerator.Current;
						control.ExpandedRow(keyValuePair.Key);
					}
					return;
				}
			}
			foreach (KeyValuePair<int, bool> keyValuePair2 in from w in this.TreeRowExpand
			where w.Value
			select w)
			{
				control.CollapsedRow(keyValuePair2.Key);
			}
		}

		// Token: 0x06000055 RID: 85 RVA: 0x000070B4 File Offset: 0x000052B4
		private void SetSupplyorg(DynamicObject dyObj, dynamic obj)
		{
			long num = Convert.ToInt64(MFGBillUtil.GetValue<int>(base.View.Model, "FUSEORGID", -1, 0, null));
			long num2 = Convert.ToInt64(dyObj["MATERIALIDCHILD_Id"]);
			int num3 = Convert.ToInt32(dyObj["Seq"]);
			if (num2 > 0L)
			{
				base.View.Model.SetValue("FSUPPLYORG", num, num3 - 1);
				return;
			}
			base.View.Model.SetValue("FSUPPLYORG", 0, num3 - 1);
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00007144 File Offset: 0x00005344
		private void HideSelBox()
		{
			int entryRowCount = base.View.Model.GetEntryRowCount("FTreeEntity");
			HiddenEntity hiddenEntity = default(HiddenEntity);
			hiddenEntity.H = true;
			hiddenEntity.M = "";
			string value = MFGBillUtil.GetValue<string>(base.View.Model, "FDOCUMENTSTATUS", -1, null, null);
			if ((!StringUtils.EqualsIgnoreCase(value, "B") && !StringUtils.EqualsIgnoreCase(value, "C")) || this.IsCfgBomOrNot)
			{
				for (int i = 0; i < entryRowCount; i++)
				{
					if (!MFGBillUtil.GetValue<bool>(base.View.Model, "FIsCanChoose", i, false, null))
					{
						base.View.GetControl<EntryGrid>("FTreeEntity").SetCellHidden("FIsSelect", hiddenEntity, i);
					}
					else if (MFGBillUtil.GetValue<bool>(base.View.Model, "FIsSelect", i, false, null))
					{
						base.View.GetControl<EntryGrid>("FTreeEntity").SetCellHidden("FIsSelect", hiddenEntity, i);
					}
				}
				this.IsCfgBomOrNot = false;
				return;
			}
			for (int j = 0; j < entryRowCount; j++)
			{
				if (MFGBillUtil.GetValue<bool>(base.View.Model, "FIsSelect", j, false, null))
				{
					HiddenEntity hiddenEntity2 = default(HiddenEntity);
					hiddenEntity2.H = false;
					hiddenEntity2.M = "";
					base.View.GetControl<EntryGrid>("FTreeEntity").SetCellHidden("FIsSelect", hiddenEntity2, j);
				}
				else
				{
					base.View.GetControl<EntryGrid>("FTreeEntity").SetCellHidden("FIsSelect", hiddenEntity, j);
				}
			}
		}

		// Token: 0x06000057 RID: 87 RVA: 0x000072C8 File Offset: 0x000054C8
		private string GetTreeNodeName(bool[] isTreeNameItemsShow, string orgStr, DynamicObject childItem)
		{
			string text = MFGBillUtil.GetUserParam<string>(base.View, "SplitCode", null);
			List<string> list = new List<string>();
			list.Add(orgStr);
			if (isTreeNameItemsShow[0] && !string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(childItem, "FName", null)))
			{
				list.Add(DataEntityExtend.GetDynamicObjectItemValue<string>(childItem, "FName", null));
			}
			if (list.Count > 1 && string.IsNullOrWhiteSpace(text))
			{
				text = "*";
			}
			if (list.Count == 1)
			{
				text = "";
			}
			return string.Format("{0}", string.Join(text, list));
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00007384 File Offset: 0x00005584
		public TreeNode ToTreeNodeObject(DynamicObjectCollection dcNodes)
		{
			TreeNode treeNode = new TreeNode
			{
				id = "0",
				cls = "parentnode",
				text = base.View.BusinessInfo.GetForm().Name
			};
			bool[] isTreeNameItemsShow = new bool[]
			{
				MFGBillUtil.GetUserParam<bool>(base.View, "BomName", false)
			};
			bool userParam = MFGBillUtil.GetUserParam<bool>(base.View, "InvisiableHistoric", false);
			List<DynamicObject> list = (from c in dcNodes
			where DataEntityExtend.GetDynamicObjectItemValue<string>(c, "FBOMCATEGORY", null) == "2"
			select c).ToList<DynamicObject>();
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary = (from o in dcNodes
			group o by DataEntityExtend.GetDynamicObjectItemValue<long>(o, "FCfgBomId", 0L)).ToDictionary((IGrouping<long, DynamicObject> o) => o.Key);
			foreach (DynamicObject dynamicObject in list)
			{
				TreeNode treeNode2 = new TreeNode();
				treeNode2.id = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "FID", null);
				treeNode2.parentid = "0";
				treeNode2.text = this.GetTreeNodeName(isTreeNameItemsShow, DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "FNumber", null), dynamicObject);
				treeNode2.icon = "images/biz/default/Common/stateApprovedV1.png";
				treeNode2.cls = "normalnode";
				treeNode.children.Add(treeNode2);
				IGrouping<long, DynamicObject> grouping;
				if (dictionary.TryGetValue(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FID", 0L), out grouping))
				{
					foreach (DynamicObject dynamicObject2 in grouping)
					{
						long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "FID", 0L);
						if (!userParam || this.BomConfigDatasMger.StandBomId == dynamicObjectItemValue)
						{
							treeNode2.cls = "parentnode";
							TreeNode treeNode3 = new TreeNode();
							treeNode3.id = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "FID", null);
							treeNode3.parentid = treeNode2.id;
							treeNode3.text = this.GetTreeNodeName(isTreeNameItemsShow, DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "FNumber", null), dynamicObject2);
							if (DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "FDocumentStatus", null) == "C")
							{
								treeNode3.icon = "images/biz/default/Common/stateApprovedV1.png";
							}
							else
							{
								treeNode3.icon = "images/biz/default/Common/stateCreatedV1.png";
							}
							treeNode3.cls = "normalnode";
							treeNode2.children.Add(treeNode3);
						}
					}
				}
			}
			return treeNode;
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00007658 File Offset: 0x00005858
		private TreeNode GetSelectNode(string nodeId)
		{
			if (this.topTreeNode.id == nodeId)
			{
				return null;
			}
			foreach (TreeNode treeNode in this.topTreeNode.children)
			{
				if (treeNode.id == nodeId)
				{
					return treeNode;
				}
				foreach (TreeNode treeNode2 in treeNode.children)
				{
					if (treeNode2.id == nodeId)
					{
						return treeNode2;
					}
				}
			}
			return null;
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00007724 File Offset: 0x00005924
		private TreeNode GetSelectNodeByText(string nodeText)
		{
			if (this.topTreeNode.text == nodeText)
			{
				return null;
			}
			foreach (TreeNode treeNode in this.topTreeNode.children)
			{
				if (treeNode.text == nodeText)
				{
					return treeNode;
				}
				foreach (TreeNode treeNode2 in treeNode.children)
				{
					if (treeNode2.text == nodeText)
					{
						return treeNode2;
					}
				}
			}
			return null;
		}

		// Token: 0x0600005B RID: 91 RVA: 0x000077F0 File Offset: 0x000059F0
		private void LoadTreeViewData(long materialId)
		{
			TreeView control = base.View.GetControl<TreeView>("FConfigTreeView");
			if (materialId <= 0L)
			{
				return;
			}
			DynamicObjectCollection configBomForConfigTreeView = BOMServiceHelper.GetConfigBomForConfigTreeView(base.Context, materialId);
			if (ObjectUtils.IsNullOrEmpty(configBomForConfigTreeView))
			{
				return;
			}
			this.topTreeNode = this.ToTreeNodeObject(configBomForConfigTreeView);
			this.TreeNodeFormat(this.topTreeNode);
			control.SetRootNode(this.topTreeNode);
			control.SetExpanded(true);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00007856 File Offset: 0x00005A56
		protected virtual void TreeNodeFormat(TreeNode treeNode)
		{
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00007858 File Offset: 0x00005A58
		// (set) Token: 0x0600005E RID: 94 RVA: 0x00007860 File Offset: 0x00005A60
		private List<NetworkCtrlResult> NetworkCtrlResults { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600005F RID: 95 RVA: 0x00007869 File Offset: 0x00005A69
		// (set) Token: 0x06000060 RID: 96 RVA: 0x00007871 File Offset: 0x00005A71
		private BomConfigViewDataManager BomConfigDatasMger { get; set; }

		// Token: 0x06000061 RID: 97 RVA: 0x0000787C File Offset: 0x00005A7C
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.IsAllExpand = MFGBillUtil.GetUserParam<bool>(base.View, "IsAllExpand", false);
			this.ClearInitData();
			this.BomConfigDatasMger = new BomConfigViewDataManager(MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM"), MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOMCONFIG"));
			this.BomConfigDatasMger.ShowWay = MFGBillUtil.GetUserParam<int>(base.View, "ShowWay", 0);
			this.BomConfigDatasMger.CfgBomId = MFGBillUtil.GetParam<long>(base.View, "pk", 0L);
			this.BomConfigDatasMger.IsAutoCheck = MFGBillUtil.GetUserParam<bool>(base.View, "IsAutoCheck", false);
			if (this.BomConfigDatasMger.CfgBomId <= 0L)
			{
				return;
			}
			DynamicObject bomInfo = this.GetBomInfo(this.BomConfigDatasMger.CfgBomId);
			long param = MFGBillUtil.GetParam<long>(base.View, "AUXPROPID", 0L);
			if (param > 0L)
			{
				base.View.Model.SetValue("FParentAuxPropId", param);
			}
			this.BomConfigDatasMger.CfgMatrailId = DataEntityExtend.GetDynamicObjectItemValue<long>(bomInfo, "MATERIALID_Id", 0L);
			this.BomConfigDatasMger.MainOrgId = DataEntityExtend.GetDynamicObjectItemValue<long>(bomInfo, "UseOrgId_Id", 0L);
			base.View.Model.BeginIniti();
			base.View.Model.SetValue("FUseOrgId", DataEntityExtend.GetDynamicValue<long>(bomInfo, "UseOrgId_Id", 0L));
			base.View.Model.EndIniti();
			DynamicObject bomInfo2 = this.GetBomInfo(MFGBillUtil.GetParam<long>(base.View, "STANDBOMID", 0L));
			this.SetTopStandBom(bomInfo2);
			this.AddBomExpandNodes(this.BomConfigDatasMger.CfgBomId, this.BomConfigDatasMger.CfgMatrailId, this.BomConfigDatasMger.StandBomId, this.BomConfigDatasMger.StandMatrailId, "");
			this.IsDataChangeExpand = true;
			this.DoStartBomNetworkCtrl();
			Form form = base.View.BillBusinessInfo.GetForm();
			string description = string.Empty;
			if (base.View.ParentFormView != null)
			{
				description = string.Format(ResManager.LoadKDString("{0}进入{1}", "015072000002166", 7, new object[0]), base.View.ParentFormView.BillBusinessInfo.GetForm().Name, form.Name);
			}
			this.Model.WriteLog(new LogObject
			{
				Description = description,
				Environment = 1,
				OperateName = ResManager.LoadKDString("进入业务对象", "015072000002167", 7, new object[0]),
				SubSystemId = form.SubsysId,
				ObjectTypeId = form.Id
			});
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00007B1C File Offset: 0x00005D1C
		public override List<TreeNode> GetTreeViewData(TreeNodeArgs e)
		{
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALID", -1, 0L, null);
			if (value > 0L)
			{
				TreeView control = base.View.GetControl<TreeView>("FConfigTreeView");
				control.SetRootNode(this.topTreeNode);
			}
			return base.GetTreeViewData(e);
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00007B6C File Offset: 0x00005D6C
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			if (this.IsNeedReturnSoureForm())
			{
				base.View.StyleManager.SetEnabled("FMATERIALID", "", false);
			}
			this.SetHeadmFaceBystandBom(this.BomConfigDatasMger.TopStandBom);
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00007BB8 File Offset: 0x00005DB8
		private void ClearInitData()
		{
			this.CurSelectRow = null;
			if (!ListUtils.IsEmpty<NetworkCtrlResult>(this.NetworkCtrlResults))
			{
				this.NetworkCtrlResults.Clear();
			}
			if (!ListUtils.IsEmpty<KeyValuePair<int, bool>>(this.TreeRowExpand))
			{
				this.TreeRowExpand.Clear();
			}
			DynamicObjectCollection firstEntityData = base.GetFirstEntityData();
			List<DynamicObject> list = (from w in firstEntityData
			orderby DataEntityExtend.GetDynamicValue<int>(w, "Seq", 0)
			select w).ToList<DynamicObject>();
			for (int i = list.Count - 1; i >= 0; i--)
			{
				base.View.Model.DeleteEntryRow("FTreeEntity", firstEntityData.IndexOf(list[i]));
			}
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00007C74 File Offset: 0x00005E74
		private void AddBomExpandNodes(long cfgBomId, long cfgMaterId, long standBomId = 0L, long standMatrailId = 0L, string parentKey = "")
		{
			if (string.IsNullOrWhiteSpace(parentKey))
			{
				if (!ListUtils.IsEmpty<DynamicObject>(this.BomConfigDatasMger.LstCfgEntityRowDatas))
				{
					this.BomConfigDatasMger.LstCfgEntityRowDatas.Clear();
				}
				if (!ListUtils.IsEmpty<BomExpandNodeConfigMode>(this.BomConfigDatasMger.LstStandBomExpandNodes))
				{
					this.BomConfigDatasMger.LstStandBomExpandNodes.Clear();
				}
			}
			Dictionary<long, long> dicBomMaps = null;
			List<BomExpandNodeConfigMode> list = null;
			List<DynamicObject> list2 = null;
			if (standBomId > 0L)
			{
				list = this.DoExpand(standBomId, standMatrailId, null);
				list2 = BOMServiceHelper.GetBomEntryDatas(base.View.Context, (from w in list
				select w.BomId_Id).ToList<long>(), false).ToList<DynamicObject>();
				dicBomMaps = this.BomConfigDatasMger.GetExpandMap(list, list2);
			}
			if (cfgBomId > 0L)
			{
				List<BomExpandNodeConfigMode> list3 = this.DoExpand(cfgBomId, cfgMaterId, dicBomMaps);
				List<DynamicObject> list4 = BOMServiceHelper.GetBomEntryDatas(base.View.Context, (from w in list3
				select w.BomId_Id).ToList<long>(), false).ToList<DynamicObject>();
				this.BomConfigDatasMger.AddLstCfgBomExpandNodes(list3, list4, parentKey);
			}
			if (!ListUtils.IsEmpty<BomExpandNodeConfigMode>(list))
			{
				this.BomConfigDatasMger.RflashLstCfgEntityRowDatas(list, list2, 3, null);
			}
			if (!ListUtils.IsEmpty<DynamicObject>(this.BomConfigDatasMger.LstCfgEntityRowDatas))
			{
				EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FTreeEntity");
				DBServiceHelper.LoadReferenceObject(base.View.Context, this.BomConfigDatasMger.LstCfgEntityRowDatas.ToArray<DynamicObject>(), entryEntity.DynamicObjectType, false);
			}
			if (ListUtils.IsEmpty<BomExpandNodeConfigMode>(list))
			{
				this.BomConfigDatasMger.InitSetIsSelect(this.BomConfigDatasMger.LstCfgEntityRowDatas);
			}
			this.ShowBomConfigChildEntity();
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00007E5C File Offset: 0x0000605C
		private void ShowBomConfigChildEntity()
		{
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection curDataEntities = this.Model.GetEntityDataObject(entryEntity);
			if (!ListUtils.IsEmpty<DynamicObject>(this.BomConfigDatasMger.LstCfgEntityRowDatas))
			{
				DBServiceHelper.LoadReferenceObject(base.View.Context, this.BomConfigDatasMger.LstCfgEntityRowDatas.ToArray<DynamicObject>(), entryEntity.DynamicObjectType, false);
			}
			this.BomConfigDatasMger.ShowWay = MFGBillUtil.GetUserParam<int>(base.View, "ShowWay", 0);
			if (this.BomConfigDatasMger.CfgBomId > 0L && this.BomConfigDatasMger.StandBomId == 0L && this.BomConfigDatasMger.ShowWay == 3)
			{
				this.BomConfigDatasMger.ShowWay = 1;
			}
			IEnumerable<DynamicObject> lstAddBomDataEntity = this.BomConfigDatasMger.GetFilterEntityDatas();
			List<DynamicObject> list = (from w in curDataEntities
			where !lstAddBomDataEntity.Contains(w)
			select w).ToList<DynamicObject>();
			if (!ListUtils.IsEmpty<DynamicObject>(list))
			{
				for (int i = list.Count<DynamicObject>() - 1; i >= 0; i--)
				{
					base.View.Model.DeleteEntryRow("FTreeEntity", curDataEntities.IndexOf(list[i]));
				}
			}
			List<DynamicObject> list2 = (from w in lstAddBomDataEntity
			where !curDataEntities.Contains(w)
			orderby DataEntityExtend.GetDynamicObjectItemValue<int>(w, "BomLevel", 0)
			select w).ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in list2)
			{
				if (this.IsOverallCopy && DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "EntityBomCategory", 0) == 1)
				{
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BOMID_Id", 0);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BOMID", null);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "NormBomId", 0);
				}
				DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BOMID_Id", 0L);
				this.IsInSertRow = true;
				if (this.IsAllExpand)
				{
					base.View.Model.CreateNewEntryRow(entryEntity, -1, dynamicObject);
				}
				else
				{
					curDataEntities.Add(dynamicObject);
				}
				this.IsInSertRow = false;
				int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "Seq", 0);
				this.TreeRowExpand[dynamicObjectItemValue] = true;
			}
			if (!this.IsAllExpand)
			{
				base.View.UpdateView("FTreeEntity");
			}
			this.UpdateTreeEntryByPrentUnit();
			this.DoExpandedRow();
		}

		// Token: 0x06000067 RID: 103 RVA: 0x000080F0 File Offset: 0x000062F0
		private DynamicObject GetBomInfo(long bomId)
		{
			if (bomId <= 0L)
			{
				return null;
			}
			string text = string.Format("{0}={1} AND FForbidStatus = 'A' ", "FID", bomId);
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FMATERIALID"),
				new SelectorItemInfo("FNumber"),
				new SelectorItemInfo("FName"),
				new SelectorItemInfo("FDocumentStatus"),
				new SelectorItemInfo("FParentAuxPropId"),
				new SelectorItemInfo("FCreatorId"),
				new SelectorItemInfo("FCreateDate"),
				new SelectorItemInfo("FModifierId"),
				new SelectorItemInfo("FModifyDate"),
				new SelectorItemInfo("FApproverId"),
				new SelectorItemInfo("FApproveDate"),
				new SelectorItemInfo("FDocumentStatus"),
				new SelectorItemInfo("FForbidderId"),
				new SelectorItemInfo("FForbidDate"),
				new SelectorItemInfo("FForbidStatus"),
				new SelectorItemInfo("FUseOrgId")
			};
			return MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_BOM", list, text, "").FirstOrDefault<DynamicObject>();
		}

		// Token: 0x06000068 RID: 104 RVA: 0x000082D0 File Offset: 0x000064D0
		private void DoStartBomNetworkCtrl()
		{
			if (this.NetworkCtrlResults == null)
			{
				this.NetworkCtrlResults = new List<NetworkCtrlResult>();
			}
			IEnumerable<DynamicObject> standBomsFromCfg = this.BomConfigDatasMger.GetStandBomsFromCfg();
			if (ListUtils.IsEmpty<DynamicObject>(standBomsFromCfg))
			{
				return;
			}
			Dictionary<object, string> dictionary = (from w in standBomsFromCfg
			where this.NetworkCtrlResults.FirstOrDefault((NetworkCtrlResult n) => n.InterID == DataEntityExtend.GetDynamicObjectItemValue<string>(w, "Id", null)) == null
			select w into g
			group g by DataEntityExtend.GetDynamicObjectItemValue<object>(g, "Id", null)).ToDictionary((IGrouping<object, DynamicObject> w) => w.Key, (IGrouping<object, DynamicObject> w) => DataEntityExtend.GetDynamicObjectItemValue<string>(w.First<DynamicObject>(), "Number", null));
			List<NetworkCtrlResult> list = MFGCommonUtil.DoStartNetworkCtrl(base.View.Context, this.BomConfigDatasMger.StandBomMetadata.BusinessInfo, dictionary);
			if (!ListUtils.IsEmpty<NetworkCtrlResult>(list))
			{
				this.NetworkCtrlResults.AddRange(list);
				MFGCommonUtil.DoChangeViewStateShowMessageByNetCtrl(base.View, from w in list
				where !w.StartSuccess
				select w);
			}
		}

		// Token: 0x06000069 RID: 105 RVA: 0x000083F0 File Offset: 0x000065F0
		private List<BomExpandNodeConfigMode> DoExpand(long bomId, long materialId, Dictionary<long, long> dicBomMaps = null)
		{
			MemBomExpandOption_ForPSV memBomExpandOption_ForPSV = this.BuildBomExpandOption(dicBomMaps);
			List<DynamicObject> list = this.BuildBomExpandSourceData(bomId, materialId);
			List<DynamicObject> bomQueryForwardResult = BomQueryServiceHelper.GetBomQueryForwardResult(base.Context, list, memBomExpandOption_ForPSV);
			return (from w in bomQueryForwardResult
			select new BomExpandNodeConfigMode(w) into x
			orderby x.BomLevel
			select x).ToList<BomExpandNodeConfigMode>();
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00008468 File Offset: 0x00006668
		private MemBomExpandOption_ForPSV BuildBomExpandOption(Dictionary<long, long> dicBomMaps = null)
		{
			MemBomExpandOption_ForPSV memBomExpandOption_ForPSV = new MemBomExpandOption_ForPSV();
			memBomExpandOption_ForPSV.ExpandLevelTo = 0;
			memBomExpandOption_ForPSV.ExpandVirtualMaterial = true;
			memBomExpandOption_ForPSV.DeleteVirtualMaterial = false;
			memBomExpandOption_ForPSV.ExpandSkipRow = true;
			memBomExpandOption_ForPSV.DeleteSkipRow = false;
			memBomExpandOption_ForPSV.IsShowOutSource = true;
			memBomExpandOption_ForPSV.CsdSubstitution = true;
			memBomExpandOption_ForPSV.BomExpandId = SequentialGuid.NewGuid().ToString();
			memBomExpandOption_ForPSV.ParentCsdYieldRate = false;
			memBomExpandOption_ForPSV.ChildCsdYieldRate = false;
			memBomExpandOption_ForPSV.BomExpandCalType = 0;
			if (dicBomMaps != null)
			{
				BomConfigExpandMemServicePlugIn bomConfigExpandMemServicePlugIn = new BomConfigExpandMemServicePlugIn();
				bomConfigExpandMemServicePlugIn.DicBomMaps = dicBomMaps;
				memBomExpandOption_ForPSV.SetBomExpandPlugIn(new List<AbstractBomExpandMemServicePlugIn>
				{
					bomConfigExpandMemServicePlugIn
				});
			}
			return memBomExpandOption_ForPSV;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00008500 File Offset: 0x00006700
		private List<DynamicObject> BuildBomExpandSourceData(long bomId, long materialId)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
			bomForwardSourceDynamicRow.MaterialId_Id = materialId;
			bomForwardSourceDynamicRow.BomId_Id = bomId;
			bomForwardSourceDynamicRow.NeedQty = 0m;
			bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
			list.Add(bomForwardSourceDynamicRow.DataEntity);
			return list;
		}

		// Token: 0x04000004 RID: 4
		private Dictionary<int, bool> TreeRowExpand = new Dictionary<int, bool>();

		// Token: 0x04000005 RID: 5
		private TreeNode curNode;
	}
}
