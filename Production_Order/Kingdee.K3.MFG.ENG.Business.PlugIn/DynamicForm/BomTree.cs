using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
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
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.BomTree;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000080 RID: 128
	[Description("BOM树形维护")]
	public class BomTree : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x17000061 RID: 97
		// (get) Token: 0x0600097B RID: 2427 RVA: 0x0006FE65 File Offset: 0x0006E065
		// (set) Token: 0x0600097C RID: 2428 RVA: 0x0006FE6D File Offset: 0x0006E06D
		private bool IsSaveSuccess { get; set; }

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x0600097D RID: 2429 RVA: 0x0006FE76 File Offset: 0x0006E076
		// (set) Token: 0x0600097E RID: 2430 RVA: 0x0006FE7E File Offset: 0x0006E07E
		private BomTreeViewDataManager BomTreeDatasMger { get; set; }

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x0600097F RID: 2431 RVA: 0x0006FE87 File Offset: 0x0006E087
		// (set) Token: 0x06000980 RID: 2432 RVA: 0x0006FE8F File Offset: 0x0006E08F
		private BomExpandNodeTreeMode CurSelNodeData { get; set; }

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x06000981 RID: 2433 RVA: 0x0006FE98 File Offset: 0x0006E098
		private TreeView CurTreeView
		{
			get
			{
				return this.View.GetControl<TreeView>("FTreeView");
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000982 RID: 2434 RVA: 0x0006FEAA File Offset: 0x0006E0AA
		private IDynamicFormView BomView
		{
			get
			{
				return this.View.GetView(this.CustomCalendarBillPageId);
			}
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000983 RID: 2435 RVA: 0x0006FEBD File Offset: 0x0006E0BD
		// (set) Token: 0x06000984 RID: 2436 RVA: 0x0006FEC5 File Offset: 0x0006E0C5
		protected string CustomCalendarBillPageId { get; set; }

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000985 RID: 2437 RVA: 0x0006FECE File Offset: 0x0006E0CE
		// (set) Token: 0x06000986 RID: 2438 RVA: 0x0006FED6 File Offset: 0x0006E0D6
		private PermissionAuthResult authSynsResult { get; set; }

		// Token: 0x06000987 RID: 2439 RVA: 0x0006FEE0 File Offset: 0x0006E0E0
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.CustomCalendarBillPageId = SequentialGuid.NewGuid().ToString();
			this.BomTreeDatasMger = new BomTreeViewDataManager();
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x0006FF18 File Offset: 0x0006E118
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.CurSelNodeData = this.BomTreeDatasMger.CreateDefCurSelNodeData();
			this.CurTreeView.SetRootVisible(false);
			long num = MFGBillUtil.GetParam<long>(this.View, "BOMID", 0L);
			long num2 = MFGBillUtil.GetParam<long>(this.View, "MATERIALID", 0L);
			if (num > 0L && num2 <= 0L)
			{
				num2 = this.GetBomMtrl(num);
			}
			else if (num <= 0L && num2 > 0L)
			{
				long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, num2);
				num = BOMServiceHelper.GetHightVersionBomKey(base.Context, materialMasterAndUserOrgId[0], materialMasterAndUserOrgId[1]);
			}
			if (num > 0L)
			{
				List<BomExpandNodeTreeMode> list = this.DoExpand(num, num2, false);
				if (!ListUtils.IsEmpty<BomExpandNodeTreeMode>(list))
				{
					this.BomTreeDatasMger.AddBomNodeMode(this.BomTreeDatasMger.RootNode.id, list);
					this.SetViewRootNode();
				}
			}
			else if (num2 > 0L)
			{
				FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "BD_MATERIAL");
				this.CurSelNodeData.MaterId = BusinessDataServiceHelper.LoadSingle(base.Context, num2, formMetaData.BusinessInfo.GetDynamicObjectType(), null);
				if (num <= 0L)
				{
					this.CurSelNodeData.SupplyerOrg = this.CurSelNodeData.MaterUseOrg;
				}
			}
			this.WriteInLog();
			this.ShowCustomBomForm(this.CurSelNodeData, false);
		}

		// Token: 0x06000989 RID: 2441 RVA: 0x00070058 File Offset: 0x0006E258
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (base.Context.IsStandardEdition())
			{
				this.View.GetMainBarItem("tbAllocates").Visible = false;
				this.View.GetMainBarItem("tbAllocates").Enabled = false;
				this.View.GetMainBarItem("tbCancelAllocates").Visible = false;
				this.View.GetMainBarItem("tbCancelAllocates").Enabled = false;
				this.View.GetMainBarItem("tbBatchAlloc").Visible = false;
				this.View.GetMainBarItem("tbBatchAlloc").Enabled = false;
				this.View.GetMainBarItem("tbAllocateInquires").Visible = false;
				this.View.GetMainBarItem("tbAllocateInquires").Enabled = false;
			}
		}

		// Token: 0x0600098A RID: 2442 RVA: 0x0007012C File Offset: 0x0006E32C
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			bool flag = this.IsDisableOrgThenCancel(e);
			if (flag)
			{
				return;
			}
			string barItemKey;
			switch (barItemKey = e.BarItemKey)
			{
			case "tbSplitNew":
			case "tbNew":
				this.ShowBomTreeForm();
				return;
			case "tbAllSubmit":
				this.DoBachOper("Submit");
				return;
			case "tbAllApprove":
				this.DoBachOper("Audit");
				return;
			case "tbPara":
			case "tbParaList":
			case "tbClose":
				return;
			case "tbSelectBom":
				this.ReloadFromBom();
				return;
			case "tbBatchAlloc":
			{
				if (!this.ValidatePermission("b53a6f0e0fe549daa1458ca49576ed70"))
				{
					e.Cancel = true;
					this.View.ShowMessage("您没有【物料清单】的【分配】权限，请联系系统管理员！", 0);
					return;
				}
				if (this.BomTreeDatasMger.RootNode == null || ListUtils.IsEmpty<TreeNode>(this.BomTreeDatasMger.RootNode.children))
				{
					this.View.ShowMessage(ResManager.LoadKDString("没有可分配的BOM.", "015072000012297", 7, new object[0]), 0);
					return;
				}
				string id = this.BomTreeDatasMger.RootNode.children.First<TreeNode>().id;
				BomExpandNodeTreeMode bomExpandNodeTreeMode = this.BomTreeDatasMger.FindBomNodeMode(id);
				long bomId_Id = bomExpandNodeTreeMode.BomId_Id;
				if (bomId_Id == 0L)
				{
					this.View.ShowMessage(ResManager.LoadKDString("没有可分配的BOM.", "015072000012297", 7, new object[0]), 0);
					return;
				}
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(bomExpandNodeTreeMode.BomId, "DocumentStatus", null);
				if (dynamicValue != "C")
				{
					this.View.ShowMessage(ResManager.LoadKDString("所选BOM版本未经过审核，不能分配.", "015072000012298", 7, new object[0]), 0);
					return;
				}
				if (DataEntityExtend.GetDynamicValue<long>(bomExpandNodeTreeMode.BomId, "CreateOrgId_Id", 0L) != DataEntityExtend.GetDynamicValue<long>(bomExpandNodeTreeMode.BomId, "UseOrgId_Id", 0L))
				{
					this.View.ShowErrMessage("", ResManager.LoadKDString("创建组织和使用组织不同，不能执行批量分配!", "015072000012299", 7, new object[0]), 0);
					return;
				}
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
				{
					FormId = "ENG_BOMBatchAllocate",
					PageId = Guid.NewGuid().ToString()
				};
				dynamicFormShowParameter.OpenStyle.ShowType = 7;
				dynamicFormShowParameter.CustomParams.Add("CurrentBomId", bomExpandNodeTreeMode.BomId_Id.ToString());
				this.View.ShowForm(dynamicFormShowParameter);
				return;
			}
			case "tbBomSynUpdate":
				if (!this.ValidatePermission("55488307023b99"))
				{
					e.Cancel = true;
					this.View.ShowMessage("您没有【物料清单】的【同步更新】权限，请联系系统管理员！", 0);
					return;
				}
				this.ShowListDatas();
				return;
			case "tbTreeCopy":
				this.TreeCopy();
				return;
			}
			this.DoChildOper(e.BarItemKey);
		}

		// Token: 0x0600098B RID: 2443 RVA: 0x00070470 File Offset: 0x0006E670
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
			if (e.BarItemKey != "tbSplitSave" && e.BarItemKey != "tbSave" && e.BarItemKey != "tbSaveTemp")
			{
				return;
			}
			if (!this.IsSaveSuccess)
			{
				if (this.BomView != null)
				{
					this.BomView.Model.DataChanged = false;
				}
				return;
			}
			this.ReExpandBom();
		}

		// Token: 0x0600098C RID: 2444 RVA: 0x000704E4 File Offset: 0x0006E6E4
		private void ReExpandBom()
		{
			foreach (TreeNode treeNode in this.CurSelNodeData.CurTreeNode.children)
			{
				this.CurTreeView.RemoveNode(treeNode.id);
			}
			DynamicObject dataObject = this.BomView.Model.DataObject;
			long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dataObject, "Id", 0L);
			long dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>(dataObject, "MATERIALID_Id", 0L);
			if (dynamicObjectItemValue <= 0L || dynamicObjectItemValue2 <= 0L)
			{
				return;
			}
			List<BomExpandNodeTreeMode> list = this.DoExpand(dynamicObjectItemValue, dynamicObjectItemValue2, false);
			if (ListUtils.IsEmpty<BomExpandNodeTreeMode>(list))
			{
				return;
			}
			if (this.BomTreeDatasMger.IsLstBomNodeModeEmpty())
			{
				this.BomTreeDatasMger.AddBomNodeMode(this.BomTreeDatasMger.RootNode.id, list);
				this.SetViewRootNode();
				this.View.Session["FormInputParam"] = this.CurSelNodeData;
				return;
			}
			this.BomTreeDatasMger.AddBomNodeMode(this.CurSelNodeData.CurTreeNode.id, list);
			this.BomTreeDatasMger.ToTreeNodeObject(this.CurSelNodeData.CurTreeNode);
			this.CurTreeView.AddNodes(this.CurSelNodeData.CurTreeNode.id, this.CurSelNodeData.CurTreeNode.children);
			this.CurTreeView.RefreshNode(this.CurSelNodeData.CurTreeNode.id, this.CurSelNodeData.CurTreeNode);
			foreach (BomExpandNodeTreeMode bomExpandNodeTreeMode in this.BomTreeDatasMger.LstBomNodeMode)
			{
				bomExpandNodeTreeMode.CurTreeNode.cls = (ListUtils.IsEmpty<TreeNode>(bomExpandNodeTreeMode.CurTreeNode.children) ? null : "parentnode");
			}
		}

		// Token: 0x0600098D RID: 2445 RVA: 0x00070724 File Offset: 0x0006E924
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			if (this.BomView != null && this.BomView.Model.DataChanged)
			{
				e.Cancel = true;
				this.BomView.ShowMessage(ResManager.LoadKDString("内容已经修改，是否保存？", "015072000002230", 7, new object[0]), 3, delegate(MessageBoxResult result)
				{
					if (result == 6)
					{
						this.DoChildOper("tbSave");
					}
					else if (result == 7)
					{
						this.BomView.Model.DataChanged = false;
						this.View.Close();
					}
					this.BomView.SendDynamicFormAction(this.View);
				}, "", 0);
				this.View.SendDynamicFormAction(this.BomView);
			}
		}

		// Token: 0x0600098E RID: 2446 RVA: 0x0007081C File Offset: 0x0006EA1C
		public override void TreeNodeClick(TreeNodeArgs e)
		{
			base.TreeNodeClick(e);
			if (this.BomView != null && this.BomView.Model.DataChanged)
			{
				e.Cancel = true;
				this.BomView.ShowMessage(ResManager.LoadKDString("内容已经修改，是否保存？", "015072000002230", 7, new object[0]), 3, delegate(MessageBoxResult result)
				{
					if (result == 6)
					{
						this.DoChildOper("tbSave");
						if (this.IsSaveSuccess)
						{
							this.ReExpandBom();
						}
						if (this.CurSelNodeData != null)
						{
							this.CurTreeView.Select(this.CurSelNodeData.EntryId);
						}
					}
					else if (result == 7)
					{
						this.BomView.Model.DataChanged = false;
						return;
					}
					this.BomView.SendDynamicFormAction(this.View);
				}, "", 0);
				this.View.SendDynamicFormAction(this.BomView);
			}
			BomExpandNodeTreeMode bomExpandNodeTreeMode = this.BomTreeDatasMger.FindBomNodeMode(e.NodeId);
			if (bomExpandNodeTreeMode != null && bomExpandNodeTreeMode.EntryId != this.CurSelNodeData.EntryId)
			{
				if (bomExpandNodeTreeMode.BomId_Id <= 0L && !this.ValidatePermission("fce8b1aca2144beeb3c6655eaf78bc34"))
				{
					e.Cancel = true;
					this.View.ShowMessage(string.Format(ResManager.LoadKDString("没有“物料清单”的“新增”权限！", "015072000002209", 7, new object[0]), new object[0]), 0);
				}
				if (!e.Cancel)
				{
					if (bomExpandNodeTreeMode.BomId_Id > 0L && this.CheckBOMIsDeleted(bomExpandNodeTreeMode.BomId_Id))
					{
						e.Cancel = true;
						this.View.ShowMessage(string.Format(ResManager.LoadKDString("您要读取的数据在系统中不存在，可能已经被删除！[ID={0},Type=ENG_BOM]", "015072000018150", 7, new object[0]), bomExpandNodeTreeMode.BomId_Id), 4);
					}
					else
					{
						e.Cancel = !this.ShowCustomBomForm(bomExpandNodeTreeMode, false);
					}
				}
				if (!e.Cancel)
				{
					this.CurSelNodeData = bomExpandNodeTreeMode;
					return;
				}
				this.CurTreeView.Select(this.CurSelNodeData.EntryId);
			}
		}

		// Token: 0x0600098F RID: 2447 RVA: 0x000709B0 File Offset: 0x0006EBB0
		private bool CheckBOMIsDeleted(long bomId)
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true) as FormMetadata;
			DynamicObjectType dynamicObjectType = formMetadata.BusinessInfo.GetDynamicObjectType();
			DynamicObject[] array = BusinessDataServiceHelper.LoadFromCache(base.Context, new object[]
			{
				bomId
			}, dynamicObjectType);
			return ListUtils.IsEmpty<DynamicObject>(array);
		}

		// Token: 0x06000990 RID: 2448 RVA: 0x00070A04 File Offset: 0x0006EC04
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (e.EventName == "ReflashBomData" && e.EventArgs == "1")
			{
				this.BomTreeDatasMger.ReSetBomData(this.CurTreeView, this.BomView.Model.DataObject, this.CurSelNodeData.CurTreeNode.id);
				this.IsSaveSuccess = true;
			}
			if (e.EventName == "ChangeTreeBySupplyOrg")
			{
				this.DeleteChildNote(e.EventArgs);
			}
			if (e.EventName == "BOMTREETOBOM")
			{
				this.View.Close();
			}
		}

		// Token: 0x06000991 RID: 2449 RVA: 0x00070AB0 File Offset: 0x0006ECB0
		private void SetViewRootNode()
		{
			this.BomTreeDatasMger.ToTreeNodeObject(null);
			this.BomTreeDataFormat(this.BomTreeDatasMger);
			string id = this.BomTreeDatasMger.RootNode.children.First<TreeNode>().id;
			this.CurSelNodeData = this.BomTreeDatasMger.FindBomNodeMode(id);
			this.CurTreeView.SetRootNode(this.BomTreeDatasMger.RootNode);
			this.CurTreeView.SetRootVisible(false);
			this.CurTreeView.Select(id);
			bool userParam = MFGBillUtil.GetUserParam<bool>(this.View, "IsExpandTree", false);
			if (userParam)
			{
				TreeNode treeNodeLst = this.BomTreeDatasMger.RootNode.children.First<TreeNode>();
				this.WalkNode(this.CurTreeView, treeNodeLst);
				this.CurTreeView.InvokeControlMethod("ExpandTree", new object[0]);
			}
			foreach (BomExpandNodeTreeMode bomExpandNodeTreeMode in this.BomTreeDatasMger.LstBomNodeMode)
			{
				bomExpandNodeTreeMode.CurTreeNode.cls = (ListUtils.IsEmpty<TreeNode>(bomExpandNodeTreeMode.CurTreeNode.children) ? null : "parentnode");
			}
		}

		// Token: 0x06000992 RID: 2450 RVA: 0x00070BEC File Offset: 0x0006EDEC
		protected virtual void BomTreeDataFormat(BomTreeViewDataManager bomTreeViewData)
		{
		}

		// Token: 0x06000993 RID: 2451 RVA: 0x00070BF0 File Offset: 0x0006EDF0
		private void WalkNode(TreeView CurTreeView, TreeNode treeNodeLst)
		{
			if (treeNodeLst.children.Count > 0)
			{
				foreach (TreeNode treeNode in treeNodeLst.children)
				{
					CurTreeView.InvokeControlMethod("ExpandNode", new object[]
					{
						treeNode.id
					});
					this.WalkNode(CurTreeView, treeNode);
				}
			}
		}

		// Token: 0x06000994 RID: 2452 RVA: 0x00070C70 File Offset: 0x0006EE70
		private void DeleteChildNote(string materialId)
		{
			BomExpandNodeTreeMode bomExpandNodeTreeMode = this.BomTreeDatasMger.FindBomNodeModeByMaterail(Convert.ToInt64(materialId));
			if (bomExpandNodeTreeMode == null)
			{
				return;
			}
			foreach (TreeNode treeNode in bomExpandNodeTreeMode.CurTreeNode.children)
			{
				this.CurTreeView.RemoveNode(treeNode.id);
			}
		}

		// Token: 0x06000995 RID: 2453 RVA: 0x00070CE8 File Offset: 0x0006EEE8
		private List<BomExpandNodeTreeMode> DoExpand(long bomId_Id, long materialId_Id, bool SelectBom = false)
		{
			MemBomExpandOption_ForPSV memBomExpandOption_ForPSV = this.BuildBomExpandOption();
			List<DynamicObject> list = this.BuildBomExpandSourceData(bomId_Id, materialId_Id);
			List<DynamicObject> bomQueryForwardResult = BomQueryServiceHelper.GetBomQueryForwardResult(base.Context, list, memBomExpandOption_ForPSV);
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "BD_MATERIAL");
			FormMetadata formMetaData2 = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
			ComboField comboField = formMetaData.BusinessInfo.GetField("FErpClsID") as ComboField;
			ComboField comboField2 = formMetaData2.BusinessInfo.GetField("FDOSAGETYPE") as ComboField;
			bool[] array = new bool[]
			{
				MFGBillUtil.GetUserParam<bool>(this.View, "BomId", false),
				MFGBillUtil.GetUserParam<bool>(this.View, "Number", false),
				MFGBillUtil.GetUserParam<bool>(this.View, "Name", false),
				MFGBillUtil.GetUserParam<bool>(this.View, "Specification", false),
				MFGBillUtil.GetUserParam<bool>(this.View, "ErpClsID", false),
				MFGBillUtil.GetUserParam<bool>(this.View, "DosageType", false),
				MFGBillUtil.GetUserParam<bool>(this.View, "ChildUnitId", false),
				MFGBillUtil.GetUserParam<bool>(this.View, "Numerator", false),
				MFGBillUtil.GetUserParam<bool>(this.View, "Denominator", false),
				MFGBillUtil.GetUserParam<bool>(this.View, "FixScrapQty", false),
				MFGBillUtil.GetUserParam<bool>(this.View, "ScrapRate", false)
			};
			string userParam = MFGBillUtil.GetUserParam<string>(this.View, "SplitCode", null);
			bool userParam2 = MFGBillUtil.GetUserParam<bool>(this.View, "OnlyShowIsMainPrd", false);
			bool userParam3 = MFGBillUtil.GetUserParam<bool>(this.View, "IsShowSubMtrl", false);
			List<BomExpandNodeTreeMode> list2 = new List<BomExpandNodeTreeMode>();
			foreach (DynamicObject dynamicObject in bomQueryForwardResult)
			{
				BomExpandNodeTreeMode bomExpandNodeTreeMode = new BomExpandNodeTreeMode(userParam2, comboField, comboField2, array, userParam, userParam3, dynamicObject);
				if (Convert.ToInt64(dynamicObject["BomLevel"]) == 0L && this.CurSelNodeData != null && !SelectBom)
				{
					bomExpandNodeTreeMode.ParentBomEntryId = this.CurSelNodeData.ParentBomEntryId;
				}
				list2.Add(bomExpandNodeTreeMode);
			}
			return list2;
		}

		// Token: 0x06000996 RID: 2454 RVA: 0x00070F2C File Offset: 0x0006F12C
		private MemBomExpandOption_ForPSV BuildBomExpandOption()
		{
			MemBomExpandOption_ForPSV memBomExpandOption_ForPSV = new MemBomExpandOption_ForPSV();
			memBomExpandOption_ForPSV.ExpandLevelTo = 0;
			memBomExpandOption_ForPSV.ExpandVirtualMaterial = true;
			memBomExpandOption_ForPSV.DeleteVirtualMaterial = false;
			memBomExpandOption_ForPSV.ExpandSkipRow = true;
			memBomExpandOption_ForPSV.DeleteSkipRow = false;
			memBomExpandOption_ForPSV.IsShowOutSource = true;
			memBomExpandOption_ForPSV.BomExpandId = SequentialGuid.NewGuid().ToString();
			memBomExpandOption_ForPSV.ParentCsdYieldRate = false;
			memBomExpandOption_ForPSV.ChildCsdYieldRate = false;
			memBomExpandOption_ForPSV.Mode = 1;
			bool userParam = MFGBillUtil.GetUserParam<bool>(this.View, "IsShowSubMtrl", false);
			memBomExpandOption_ForPSV.CsdSubstitution = userParam;
			memBomExpandOption_ForPSV.BomExpandCalType = 0;
			memBomExpandOption_ForPSV.Option.SetVariableValue("requireDataPermission", true);
			return memBomExpandOption_ForPSV;
		}

		// Token: 0x06000997 RID: 2455 RVA: 0x00070FD0 File Offset: 0x0006F1D0
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

		// Token: 0x06000998 RID: 2456 RVA: 0x00071024 File Offset: 0x0006F224
		private bool ShowCustomBomForm(BomExpandNodeTreeMode bomFormParam, bool isFromSelectBill = false)
		{
			if (!this.ValidatePermission("6e44119a58cb4a8e86f6c385e14a17ad"))
			{
				this.View.ShowMessage(string.Format(ResManager.LoadKDString("没有“物料清单”的“查看”权限！", "015072000002210", 7, new object[0]), new object[0]), 0);
				return false;
			}
			OperationStatus status = 1;
			if (bomFormParam.BomId_Id > 0L && this.ValidatePermission("f323992d896745fbaab4a2717c79ce2e"))
			{
				status = 2;
			}
			if (bomFormParam.BomId_Id <= 0L)
			{
				if (!this.ValidatePermission("fce8b1aca2144beeb3c6655eaf78bc34"))
				{
					this.View.ShowMessage(string.Format(ResManager.LoadKDString("没有“物料清单”的“新增”权限！", "015072000002209", 7, new object[0]), new object[0]), 0);
					return false;
				}
				status = 0;
			}
			IDynamicFormView view = this.View.GetView(this.CustomCalendarBillPageId);
			if (view == null)
			{
				BillShowParameter billShowParameter = new BillShowParameter();
				billShowParameter.OpenStyle.ShowType = 3;
				billShowParameter.OpenStyle.TagetKey = "FPanel";
				billShowParameter.FormId = "ENG_BOM";
				billShowParameter.ParentPageId = this.View.PageId;
				billShowParameter.PageId = this.CustomCalendarBillPageId;
				billShowParameter.PKey = bomFormParam.BomId_Id.ToString();
				billShowParameter.Status = status;
				billShowParameter.AddBillOptionParameter("FSaveAndNew", false);
				billShowParameter.AddBillOptionParameter("FSaveAndSubmit", false);
				billShowParameter.CustomParams["ShowConfirmDialogWhenChangeOrg"] = "false";
				this.ShowBillForm(this.View, billShowParameter, bomFormParam, null);
			}
			else
			{
				this.View.Session["FormInputParam"] = bomFormParam;
				BillOpenParameter billOpenParameter = (BillOpenParameter)view.OpenParameter;
				billOpenParameter.PkValue = bomFormParam.BomId_Id.ToString();
				billOpenParameter.Status = status;
				billOpenParameter.CreateFrom = 0;
				billOpenParameter.SetCustomParameter("ShowConfirmDialogWhenChangeOrg", "false");
				if (isFromSelectBill)
				{
					billOpenParameter.SetCustomParameter("IsFromSelectBill", "true");
				}
				view.Refresh();
				this.View.SendDynamicFormAction(view);
			}
			return true;
		}

		// Token: 0x06000999 RID: 2457 RVA: 0x00071250 File Offset: 0x0006F450
		public void ShowBillForm(IDynamicFormView view, BillShowParameter billShowPara, object inputParam = null, Action<FormResult> action = null)
		{
			if (view == null || billShowPara == null)
			{
				return;
			}
			if (inputParam != null)
			{
				view.Session["FormInputParam"] = inputParam;
			}
			if (string.IsNullOrWhiteSpace(billShowPara.ParentPageId))
			{
				billShowPara.ParentPageId = view.PageId;
			}
			view.ShowForm(billShowPara, delegate(FormResult result)
			{
				if (action != null)
				{
					action(result);
				}
				if (inputParam != null)
				{
					view.Session["FormInputParam"] = null;
				}
			});
		}

		// Token: 0x0600099A RID: 2458 RVA: 0x000712E0 File Offset: 0x0006F4E0
		private bool DoStartParentBomNetworkCtrl(string entityId, out List<NetworkCtrlResult> networkCtrlResults)
		{
			DynamicObject item = null;
			if (!string.IsNullOrWhiteSpace(entityId))
			{
				item = this.BomTreeDatasMger.FindBomNodeMode(entityId).ParentBomId;
			}
			return this.DoStartBomNetworkCtrl(new List<DynamicObject>
			{
				item
			}, out networkCtrlResults);
		}

		// Token: 0x0600099B RID: 2459 RVA: 0x00071334 File Offset: 0x0006F534
		private bool DoStartParentBomNetworkCtrl(List<DynamicObject> lstBomId, out List<NetworkCtrlResult> networkCtrlResults)
		{
			if (this.CurSelNodeData.BomId != null && lstBomId.Contains(this.CurSelNodeData.BomId) && this.BomView.OpenParameter.Status == 1)
			{
				return this.DoStartBomNetworkCtrl(new List<DynamicObject>
				{
					this.CurSelNodeData.BomId
				}, out networkCtrlResults);
			}
			return this.DoStartBomNetworkCtrl((from w in lstBomId
			where w != this.CurSelNodeData.BomId
			select w).ToList<DynamicObject>(), out networkCtrlResults);
		}

		// Token: 0x0600099C RID: 2460 RVA: 0x000713E4 File Offset: 0x0006F5E4
		private bool DoStartBomNetworkCtrl(List<DynamicObject> lstBomId, out List<NetworkCtrlResult> networkCtrlResults)
		{
			networkCtrlResults = new List<NetworkCtrlResult>();
			Dictionary<object, string> dictionary = (from w in lstBomId.Distinct<DynamicObject>()
			where w != null
			select w).ToDictionary((DynamicObject w) => DataEntityExtend.GetDynamicObjectItemValue<object>(w, "Id", null), (DynamicObject w) => DataEntityExtend.GetDynamicObjectItemValue<string>(w, "Number", null));
			if (ListUtils.IsEmpty<KeyValuePair<object, string>>(dictionary))
			{
				return true;
			}
			List<NetworkCtrlResult> list = MFGCommonUtil.DoStartNetworkCtrl(this.View.Context, this.BomView.BusinessInfo, dictionary);
			if (!ListUtils.IsEmpty<NetworkCtrlResult>(list))
			{
				networkCtrlResults.AddRange(list);
				IEnumerable<NetworkCtrlResult> enumerable = from w in list
				where !w.StartSuccess
				select w;
				if (!ListUtils.IsEmpty<NetworkCtrlResult>(enumerable))
				{
					this.BomView.ShowMessage(enumerable.First<NetworkCtrlResult>().Message, 0);
					this.View.SendDynamicFormAction(this.BomView);
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600099D RID: 2461 RVA: 0x000714F0 File Offset: 0x0006F6F0
		private void WriteInLog()
		{
			if (this.View.ParentFormView == null)
			{
				return;
			}
			Form form = this.View.ParentFormView.BillBusinessInfo.GetForm();
			Form form2 = this.View.BillBusinessInfo.GetForm();
			if (form.Id == "BD_MATERIAL" || form.Id == "ENG_BOM" || form.Id == "ENG_BOMTREE")
			{
				this.Model.WriteLog(new LogObject
				{
					Description = string.Format(ResManager.LoadKDString("{0}进入{1}", "015072000002166", 7, new object[0]), form.Name, form2.Name),
					Environment = 1,
					OperateName = ResManager.LoadKDString("进入业务对象", "015072000002167", 7, new object[0]),
					SubSystemId = form2.SubsysId,
					ObjectTypeId = form2.Id
				});
			}
		}

		// Token: 0x0600099E RID: 2462 RVA: 0x000715E4 File Offset: 0x0006F7E4
		private long GetBomMtrl(long bomId)
		{
			string text = string.Format("{0}={1}", "FID", bomId);
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FMATERIALID")
			};
			List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_BOM", list, text, "");
			if (!ListUtils.IsEmpty<DynamicObject>(baseBillInfo))
			{
				return DataEntityExtend.GetDynamicObjectItemValue<long>(baseBillInfo.First<DynamicObject>(), "MATERIALID_Id", 0L);
			}
			return 0L;
		}

		// Token: 0x0600099F RID: 2463 RVA: 0x000716B8 File Offset: 0x0006F8B8
		private void DoBachOper(string oper)
		{
			FormOperation operation = this.BomView.BillBusinessInfo.GetForm().GetOperation(oper);
			if (!this.VaildatePermission(operation))
			{
				return;
			}
			List<long> list;
			List<DynamicObject> bachDealOper = this.BomTreeDatasMger.GetBachDealOper(oper, ref list);
			if (ListUtils.IsEmpty<DynamicObject>(bachDealOper))
			{
				this.BomView.ShowMessage(string.Format(ResManager.LoadKDString("没有可{0}的数据！", "015072000002154", 7, new object[0]), operation.OperationName), 0);
				this.View.SendDynamicFormAction(this.BomView);
				return;
			}
			List<DynamicObject> list2 = new List<DynamicObject>();
			List<NetworkCtrlResult> list3 = null;
			try
			{
				if (this.DoStartParentBomNetworkCtrl(bachDealOper, out list3))
				{
					IOperationResult operationResult = null;
					List<KeyValuePair<object, object>> list4 = null;
					if (oper != null)
					{
						if (!(oper == "Submit"))
						{
							if (!(oper == "Audit"))
							{
								goto IL_3D6;
							}
						}
						else
						{
							if (this.BomView.Model.DataChanged)
							{
								operationResult = BusinessDataServiceHelper.Save(this.View.Context, this.BomView.BillBusinessInfo, new DynamicObject[]
								{
									this.BomView.Model.DataObject
								}, OperateOption.Create(), "Save");
								if (!operationResult.IsSuccess)
								{
									goto IL_3D6;
								}
							}
							Dictionary<string, string> dictionary = (from g in bachDealOper
							group g by DataEntityExtend.GetDynamicObjectItemValue<string>(g, "Id", null)).ToDictionary((IGrouping<string, DynamicObject> k) => k.Key, (IGrouping<string, DynamicObject> v) => DataEntityExtend.GetDynamicObjectItemValue<string>(v.FirstOrDefault<DynamicObject>(), "Number", null));
							List<IOperationResult> list5 = MFGCommonUtil.SubmitWithWorkFlow(base.Context, "ENG_BOM", dictionary, OperateOption.Create());
							using (List<IOperationResult>.Enumerator enumerator = list5.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									IOperationResult operationResult2 = enumerator.Current;
									if (!ListUtils.IsEmpty<DynamicObject>(operationResult2.SuccessDataEnity))
									{
										list2.AddRange(operationResult2.SuccessDataEnity);
									}
									if (ObjectUtils.IsNullOrEmpty(operationResult))
									{
										operationResult = operationResult2;
									}
									else
									{
										OperationResultExt.MergeResult(operationResult, operationResult2);
									}
								}
								goto IL_3D6;
							}
						}
						List<long> list6 = new List<long>();
						Dictionary<string, string> billDatas = (from g in bachDealOper
						group g by DataEntityExtend.GetDynamicObjectItemValue<string>(g, "Id", null)).ToDictionary((IGrouping<string, DynamicObject> k) => k.Key, (IGrouping<string, DynamicObject> v) => DataEntityExtend.GetDynamicObjectItemValue<string>(v.FirstOrDefault<DynamicObject>(), "Number", null));
						IOperationResult operationResult3 = this.ChkTargetExistWorkFlow(billDatas, "全部审核", list6);
						List<long> list7 = new List<long>();
						list4 = (from w in list6
						select new KeyValuePair<object, object>(w, "")).ToList<KeyValuePair<object, object>>();
						foreach (BomExpandNodeTreeMode bomExpandNodeTreeMode in this.BomTreeDatasMger.LstBomNodeMode)
						{
							if (list6.Contains(bomExpandNodeTreeMode.BomId_Id) && list.Contains(bomExpandNodeTreeMode.ParentBomEntryId))
							{
								list7.Add(bomExpandNodeTreeMode.ParentBomEntryId);
							}
						}
						if (!ListUtils.IsEmpty<KeyValuePair<object, object>>(list4))
						{
							List<object> list8 = new List<object>
							{
								"1",
								""
							};
							OperateOption operateOption = OperateOption.Create();
							operateOption.SetVariableValue("lstParentBomEntryId", list7);
							operationResult = BusinessDataServiceHelper.SetBillStatus(this.View.Context, this.BomView.BillBusinessInfo, list4, list8, oper, operateOption);
						}
						if (!ObjectUtils.IsNullOrEmpty(operationResult) && !ListUtils.IsEmpty<DynamicObject>(operationResult.SuccessDataEnity))
						{
							list2.AddRange(operationResult.SuccessDataEnity);
						}
						if (ObjectUtils.IsNullOrEmpty(operationResult))
						{
							operationResult = operationResult3;
						}
						else
						{
							OperationResultExt.MergeResult(operationResult, operationResult3);
						}
						if (!ListUtils.IsEmpty<DynamicObject>(operationResult3.SuccessDataEnity))
						{
							list2.AddRange(operationResult3.SuccessDataEnity);
						}
					}
					IL_3D6:
					if (operationResult != null)
					{
						MFGCommonUtil.BatchWriteLog(this.View.Context, this.BomView.BillBusinessInfo, operationResult, operation);
						MFGBillUtil.ShowOperateResult(this.BomView, operationResult, operation);
						this.View.SendDynamicFormAction(this.BomView);
						if (!ListUtils.IsEmpty<DynamicObject>(list2))
						{
							this.ShowCustomBomForm(this.CurSelNodeData, false);
							this.BomTreeDatasMger.ReSetBomData(this.CurTreeView, list2.ToList<DynamicObject>());
						}
					}
				}
			}
			finally
			{
				MFGCommonUtil.DoCommitNetworkCtrl(this.View.Context, list3);
			}
		}

		// Token: 0x060009A0 RID: 2464 RVA: 0x00071B6C File Offset: 0x0006FD6C
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
					string message = string.Format("物料清单{0}已关联工作流实例。不允许在树形维护中批量审核！", value);
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

		// Token: 0x060009A1 RID: 2465 RVA: 0x00071C48 File Offset: 0x0006FE48
		private void DoChildOper(string barItemKey)
		{
			List<NetworkCtrlResult> list = null;
			try
			{
				if (!(barItemKey == "tbSaveTemp") || this.ValidateSaveTemp())
				{
					if ((!(barItemKey == "tbSaveTemp") && !(barItemKey == "tbSave") && !(barItemKey == "tbSplitSave")) || this.DoStartParentBomNetworkCtrl(this.CurSelNodeData.EntryId, out list))
					{
						if (barItemKey == "tbSaveTemp" || barItemKey == "tbSave" || barItemKey == "tbSplitSave")
						{
							this.IsSaveSuccess = false;
						}
						BarItem childBarItem = this.GetChildBarItem(barItemKey);
						if (!ObjectUtils.IsNullOrEmpty(childBarItem))
						{
							foreach (FormBusinessService formBusinessService in childBarItem.ClickActions)
							{
								this.BomView.InvokeFormOperation(formBusinessService.GetJsonParameters().First<object>().ToString());
							}
							this.View.SendDynamicFormAction(this.BomView);
						}
					}
				}
			}
			finally
			{
				MFGCommonUtil.DoCommitNetworkCtrl(this.View.Context, list);
			}
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x00071D7C File Offset: 0x0006FF7C
		private bool ValidateSaveTemp()
		{
			if (MFGBillUtil.GetValue<long>(this.BomView.Model, "FMATERIALID", -1, 0L, null) <= 0L)
			{
				this.BomView.ShowMessage(ResManager.LoadKDString("当前物料不可为主产品，无法暂存BOM！", "015072000003342", 7, new object[0]), 0);
				this.View.SendDynamicFormAction(this.BomView);
				return false;
			}
			return true;
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x00071DDC File Offset: 0x0006FFDC
		private bool ValidatePermission(string permission)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = "ENG_BOM",
				SubSystemId = this.View.Model.SubSytemId
			}, permission);
			return permissionAuthResult.Passed;
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x00071E24 File Offset: 0x00070024
		private bool VaildatePermission(FormOperation operation)
		{
			bool flag = this.ValidatePermission(operation.PermissionItemId);
			if (!flag)
			{
				this.BomView.ShowMessage(string.Format(ResManager.LoadKDString("没有“物料清单”的“{0}”权限！", "015072000002211", 7, new object[0]), operation.OperationName), 0);
				this.View.SendDynamicFormAction(this.BomView);
			}
			return flag;
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x00071E80 File Offset: 0x00070080
		private BarItem GetChildBarItem(string barItemKey)
		{
			Dictionary<string, BarItem> allBarItems = this.BomView.LayoutInfo.GetFormAppearance().Menu.GetAllBarItems();
			BarItem result = null;
			allBarItems.TryGetValue(barItemKey, out result);
			return result;
		}

		// Token: 0x060009A6 RID: 2470 RVA: 0x00071EB8 File Offset: 0x000700B8
		private void ShowBomTreeForm()
		{
			Form form = this.View.BusinessInfo.GetForm();
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.FormId = form.Id;
			dynamicFormShowParameter.Caption = form.Name;
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060009A7 RID: 2471 RVA: 0x00071FB0 File Offset: 0x000701B0
		private void ReloadFromBom()
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_BOM";
			listShowParameter.PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad";
			long num = Convert.ToInt64(MFGBillUtil.GetValue<long>(this.View.GetView(this.CustomCalendarBillPageId).Model, "FUseOrgId", -1, 0L, null));
			if (num == 0L)
			{
				num = base.Context.CurrentOrganizationInfo.ID;
			}
			string filter = string.Empty;
			filter = string.Format(" FUseOrgId = '{0}'", num);
			listShowParameter.ListFilterParameter.Filter = filter;
			listShowParameter.IsIsolationOrg = false;
			listShowParameter.ListType = 2;
			listShowParameter.IsShowApproved = false;
			listShowParameter.IsShowUsed = true;
			listShowParameter.MultiSelect = false;
			listShowParameter.IsLookUp = true;
			this.View.ShowForm(listShowParameter, delegate(FormResult result)
			{
				object returnData = result.ReturnData;
				if (returnData is ListSelectedRowCollection)
				{
					ListSelectedRowCollection listSelectedRowCollection = (ListSelectedRowCollection)returnData;
					if (listSelectedRowCollection.Count == 0)
					{
						return;
					}
					long num2 = Convert.ToInt64(listSelectedRowCollection[0].FieldValues["FBillHead"]);
					if (num2 > 0L)
					{
						long bomMtrl = this.GetBomMtrl(num2);
						List<BomExpandNodeTreeMode> list = this.DoExpand(num2, bomMtrl, true);
						if (!ListUtils.IsEmpty<BomExpandNodeTreeMode>(list))
						{
							this.BomTreeDatasMger.AddBomNodeMode(this.BomTreeDatasMger.RootNode.id, list);
							this.SetViewRootNode();
							this.ShowCustomBomForm(this.CurSelNodeData, false);
						}
					}
				}
			});
		}

		// Token: 0x060009A8 RID: 2472 RVA: 0x00072140 File Offset: 0x00070340
		private void ShowListDatas()
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
				return;
			}
			if (this.BomTreeDatasMger.RootNode == null || ListUtils.IsEmpty<TreeNode>(this.BomTreeDatasMger.RootNode.children))
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有可同步更新的BOM", "015072000033346", 7, new object[0]), 0);
				return;
			}
			List<long> list = new List<long>();
			List<long> list2 = new List<long>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			List<BomExpandNodeTreeMode> lstBomNodeMode = this.BomTreeDatasMger.LstBomNodeMode;
			foreach (BomExpandNodeTreeMode bomExpandNodeTreeMode in lstBomNodeMode)
			{
				if (bomExpandNodeTreeMode.BomId != null)
				{
					list.Add(bomExpandNodeTreeMode.BomId_Id);
					long dynamicValue = DataEntityExtend.GetDynamicValue<long>(bomExpandNodeTreeMode.BomId, "UseOrgId_Id", 0L);
					list2.Add(dynamicValue);
					list3.Add(bomExpandNodeTreeMode.BomId);
				}
			}
			if (ListUtils.IsEmpty<long>(list))
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有可同步更新的BOM", "015072000033346", 7, new object[0]), 0);
				return;
			}
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ENG_BOM", true);
			list3 = BusinessDataServiceHelper.LoadFromCache(this.View.Context, (from s in list
			select s).ToArray<object>(), formMetadata.BusinessInfo.GetDynamicObjectType()).ToList<DynamicObject>();
			list3 = (from w in list3
			where DataEntityExtend.GetDynamicValue<string>(w, "DocumentStatus", null) == "C" && DataEntityExtend.GetDynamicValue<string>(w, "ForbidStatus", null) == "A"
			select w).ToList<DynamicObject>();
			if (list3.Count == 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("选择的BOM数据状态不为已审核或禁用状态为禁用，请重新选择数据", "015072000018123", 7, new object[0]), "", 0);
				return;
			}
			list = (from x in list3
			select DataEntityExtend.GetDynamicValue<long>(x, "Id", 0L)).Distinct<long>().ToList<long>();
			list2 = (from x in list3
			select DataEntityExtend.GetDynamicValue<long>(x, "UseOrgId_Id", 0L)).Distinct<long>().ToList<long>();
			FormMetadata formMetadata2 = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ENG_BOM_BILLPARAM", true);
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(this.View.Context, formMetadata2.BusinessInfo, this.View.Context.UserId, "ENG_BOM", "UserParameter");
			string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "UpdateRange", null);
			string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ConSultDate", null);
			bool dynamicValue4 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IsSkipExpand", false);
			List<long> bomMasterIds = new List<long>();
			if (dynamicValue2 == "2")
			{
				bomMasterIds = (from w in list3
				where DataEntityExtend.GetDynamicValue<long>(w, "UseOrgId_Id", 0L) == DataEntityExtend.GetDynamicValue<long>(w, "CreateOrgId_Id", 0L)
				select w into s
				select DataEntityExtend.GetDynamicValue<long>(s, "MsterId", 0L)).ToList<long>();
				List<DynamicObject> allocatedBOM = this.GetAllocatedBOM(bomMasterIds);
				if (!ListUtils.IsEmpty<DynamicObject>(allocatedBOM))
				{
					list2.AddRange((from s in allocatedBOM
					select DataEntityExtend.GetDynamicValue<long>(s, "UseOrgId_Id", 0L)).ToList<long>().Except(list2));
					list.AddRange((from s in allocatedBOM
					select DataEntityExtend.GetDynamicValue<long>(s, "Id", 0L)).ToList<long>().Except(list));
					foreach (DynamicObject dynamicObject2 in allocatedBOM)
					{
						long bomId = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "Id", 0L);
						if (!list3.Any((DynamicObject a) => DataEntityExtend.GetDynamicValue<long>(a, "Id", 0L) == bomId))
						{
							list3.Add(dynamicObject2);
						}
					}
				}
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 4;
			dynamicFormShowParameter.FormId = "ENG_SYNSUPDATEPPBOM";
			dynamicFormShowParameter.CustomComplexParams.Add("BomData", list3);
			dynamicFormShowParameter.CustomComplexParams.Add("BomId", list);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowPrdList", true);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowSubList", true);
			dynamicFormShowParameter.CustomComplexParams.Add("UserOrgId", list2);
			dynamicFormShowParameter.CustomComplexParams.Add("ConSultDate", dynamicValue3);
			dynamicFormShowParameter.CustomComplexParams.Add("IsSkipExpand", dynamicValue4);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowPlnList", true);
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060009A9 RID: 2473 RVA: 0x00072668 File Offset: 0x00070868
		private List<DynamicObject> GetAllocatedBOM(List<long> bomMasterIds)
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ENG_BOM", true);
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.BusinessInfo = formMetadata.BusinessInfo;
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				TableName = "table(fn_StrSplit(@msterId,',',1))",
				TableNameAs = "tms",
				FieldName = "FID",
				ScourceKey = "FMASTERID"
			});
			queryBuilderParemeter.FilterClauseWihtKey = "FCreateOrgId<>FUseOrgId";
			queryBuilderParemeter.SqlParams.Add(new SqlParam("@msterId", 161, bomMasterIds.Distinct<long>().ToArray<long>()));
			return BusinessDataServiceHelper.Load(this.View.Context, queryBuilderParemeter.BusinessInfo.GetDynamicObjectType(), queryBuilderParemeter).ToList<DynamicObject>();
		}

		// Token: 0x060009AA RID: 2474 RVA: 0x00072734 File Offset: 0x00070934
		private void TreeCopy()
		{
			List<BomExpandNodeTreeMode> lstBomNodeMode = this.BomTreeDatasMger.LstBomNodeMode;
			if (ListUtils.IsEmpty<BomExpandNodeTreeMode>(lstBomNodeMode))
			{
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
			{
				FormId = "ENG_BOMTREECOPY",
				PageId = Guid.NewGuid().ToString()
			};
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.CustomComplexParams.Add("treeNodeDatas", lstBomNodeMode);
			this.View.ShowForm(dynamicFormShowParameter);
			this.View.Close();
		}

		// Token: 0x060009AB RID: 2475 RVA: 0x000727B8 File Offset: 0x000709B8
		private bool IsDisableOrgThenCancel(BarItemClickEventArgs e)
		{
			if (!this.needCheckDisableOrgBaritems.Contains(e.BarItemKey))
			{
				return false;
			}
			if (this.BomView == null)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("在当前组织下没有权限或者组织已禁用", "015072000022393", 7, new object[0]), "", 0);
				e.Cancel = true;
				return true;
			}
			return false;
		}

		// Token: 0x0400047F RID: 1151
		private const string PanelKey = "FPanel";

		// Token: 0x04000480 RID: 1152
		private const string TreeViewKey = "FTreeView";

		// Token: 0x04000481 RID: 1153
		private string[] needCheckDisableOrgBaritems = new string[]
		{
			"tbSplitSave",
			"tbSplitSubmit",
			"tbSplitApprove",
			"tbSelectBom",
			"tbPrint",
			"tbPreview",
			"tbNoteTemplateSetting",
			"tbAccessory"
		};
	}
}
