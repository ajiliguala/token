using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.Calendar;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000B9 RID: 185
	[Description("工厂日历设置插件")]
	public class WorkCalSetup : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000DAD RID: 3501 RVA: 0x000A002C File Offset: 0x0009E22C
		public WorkCalSetup()
		{
			this.CustomCalendarBill_PageId = SequentialGuid.NewGuid().ToString();
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x06000DAE RID: 3502 RVA: 0x000A0058 File Offset: 0x0009E258
		// (set) Token: 0x06000DAF RID: 3503 RVA: 0x000A0060 File Offset: 0x0009E260
		protected string CustomCalendarBill_PageId { get; set; }

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x06000DB0 RID: 3504 RVA: 0x000A0069 File Offset: 0x0009E269
		// (set) Token: 0x06000DB1 RID: 3505 RVA: 0x000A0071 File Offset: 0x0009E271
		private protected TreeNode TopNode { protected get; private set; }

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x06000DB2 RID: 3506 RVA: 0x000A007A File Offset: 0x0009E27A
		// (set) Token: 0x06000DB3 RID: 3507 RVA: 0x000A0082 File Offset: 0x0009E282
		private protected AbstractBaseDataView CurSelNodeData { protected get; private set; }

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x06000DB4 RID: 3508 RVA: 0x000A008B File Offset: 0x0009E28B
		// (set) Token: 0x06000DB5 RID: 3509 RVA: 0x000A0093 File Offset: 0x0009E293
		private protected AbstractBaseDataView CurSelOrgNodeData { protected get; private set; }

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x06000DB6 RID: 3510 RVA: 0x000A009C File Offset: 0x0009E29C
		// (set) Token: 0x06000DB7 RID: 3511 RVA: 0x000A00A4 File Offset: 0x0009E2A4
		private protected AbstractBaseDataView CurSelDeptNodeData { protected get; private set; }

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x06000DB8 RID: 3512 RVA: 0x000A00AD File Offset: 0x0009E2AD
		// (set) Token: 0x06000DB9 RID: 3513 RVA: 0x000A00B5 File Offset: 0x0009E2B5
		private protected AbstractBaseDataView CurSelWorkCenterNodeData { protected get; private set; }

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x06000DBA RID: 3514 RVA: 0x000A00BE File Offset: 0x0009E2BE
		// (set) Token: 0x06000DBB RID: 3515 RVA: 0x000A00C6 File Offset: 0x0009E2C6
		private protected AbstractBaseDataView CurSelResNodeData { protected get; private set; }

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x06000DBC RID: 3516 RVA: 0x000A00CF File Offset: 0x0009E2CF
		// (set) Token: 0x06000DBD RID: 3517 RVA: 0x000A00D7 File Offset: 0x0009E2D7
		private protected AbstractBaseDataView CurSelEquNodeData { protected get; private set; }

		// Token: 0x06000DBE RID: 3518 RVA: 0x000A00E0 File Offset: 0x0009E2E0
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			if (this.View.ParentFormView != null && (this.View.ParentFormView.OpenParameter.FormId.Equals("ENG_WorkCenter") || this.View.ParentFormView.OpenParameter.FormId.Equals("ENG_Resource") || this.View.ParentFormView.OpenParameter.FormId.Equals("BD_Department") || this.View.ParentFormView.OpenParameter.FormId.Equals("ENG_FlowProductLine") || this.View.ParentFormView.OpenParameter.FormId.Equals("ENG_RepetitiveProductLine") || this.View.ParentFormView.OpenParameter.FormId.Equals("REM_MainPLScheduleParam")) && this.View.ParentFormView.Session != null && this.View.ParentFormView.Session.Count<KeyValuePair<string, object>>() > 0 && this.View.ParentFormView.Session["FormInputParam"] != null)
			{
				this.treeNodeParam = MFGBillUtil.GetParentFormSession<T_WorkCalSetupFormParam>(this.View, "FormInputParam");
			}
		}

		// Token: 0x06000DBF RID: 3519 RVA: 0x000A022C File Offset: 0x0009E42C
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.BindTreeViewData();
			if (this.treeNodeParam == null || (this.treeNodeParam != null && this.workCalTreeData.FindObjectByTreeNode(this.treeNodeParam.treeNodeId) == null))
			{
				this.InitCurSelNodeInfo(this.workCalTreeData.FindObjectByTreeNode(this.TopNode.children.FirstOrDefault<TreeNode>()));
			}
			else
			{
				this.InitCurSelNodeInfo(this.workCalTreeData.FindObjectByTreeNode(this.treeNodeParam.treeNodeId));
			}
			this.ShowCustomCalendarForm(1);
		}

		// Token: 0x06000DC0 RID: 3520 RVA: 0x000A02B4 File Offset: 0x0009E4B4
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			TreeView control = this.View.GetControl<TreeView>("FWorkCalSetupTreeView");
			control.SetRootVisible(false);
			if (this.treeNodeParam != null && this.workCalTreeData.FindObjectByTreeNode(this.treeNodeParam.treeNodeId) != null)
			{
				control.Select(this.treeNodeParam.treeNodeId);
			}
		}

		// Token: 0x06000DC1 RID: 3521 RVA: 0x000A0390 File Offset: 0x0009E590
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			IDynamicFormView childView = this.View.GetView(this.CustomCalendarBill_PageId);
			if (childView != null && childView.Model.DataChanged)
			{
				e.Cancel = true;
				this.View.ShowMessage(ResManager.LoadKDString("内容已经修改，是否保存？", "015072000002230", 7, new object[0]), 3, delegate(MessageBoxResult result)
				{
					if (result == 6)
					{
						if (this.View is IDynamicFormViewService)
						{
							(this.View as IDynamicFormViewService).MainBarItemClick("BTN_SAVE");
							return;
						}
					}
					else if (result == 7)
					{
						CalendarServiceHelper.ClearPrivateShiftNoDiffCal(this.Context);
						childView.Model.DataChanged = false;
						this.View.Close();
					}
				}, "", 0);
			}
		}

		// Token: 0x06000DC2 RID: 3522 RVA: 0x000A0424 File Offset: 0x0009E624
		public override List<TreeNode> GetTreeViewData(TreeNodeArgs e)
		{
			TreeView control = this.View.GetControl<TreeView>("FWorkCalSetupTreeView");
			control.SetRootNode(this.TopNode);
			return base.GetTreeViewData(e);
		}

		// Token: 0x06000DC3 RID: 3523 RVA: 0x000A0458 File Offset: 0x0009E658
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "BTN_VIEW"))
				{
					if (!(a == "BTN_MODIFY"))
					{
						if (!(a == "BTN_DELETE"))
						{
							if (!(a == "BTN_SAVE"))
							{
								return;
							}
							if (this.VaildatePermission("fce8b1aca2144beeb3c6655eaf78bc34"))
							{
								IDynamicFormView view = this.View.GetView(this.CustomCalendarBill_PageId);
								if (view != null)
								{
									view.InvokeFormOperation(5);
									this.View.SendDynamicFormAction(view);
								}
							}
						}
						else if (this.VaildatePermission("24f64c0dbfa945f78a6be123197a63f5"))
						{
							IDynamicFormView view = this.View.GetView(this.CustomCalendarBill_PageId);
							if (view != null)
							{
								view.InvokeFormOperation("RemoveWorkCalMAP");
								this.View.SendDynamicFormAction(view);
								return;
							}
						}
					}
					else if (this.VaildatePermission("f323992d896745fbaab4a2717c79ce2e"))
					{
						this.ShowCustomCalendarForm(2);
						return;
					}
				}
				else
				{
					IDynamicFormView view = this.View.GetView(this.CustomCalendarBill_PageId);
					if (view != null)
					{
						this.ShowCustomCalendarForm(1);
						return;
					}
				}
			}
		}

		// Token: 0x06000DC4 RID: 3524 RVA: 0x000A06BC File Offset: 0x0009E8BC
		public override void TreeNodeClick(TreeNodeArgs e)
		{
			base.TreeNodeClick(e);
			IDynamicFormView childView = this.View.GetView(this.CustomCalendarBill_PageId);
			if (childView != null && childView.Model.DataChanged)
			{
				e.Cancel = true;
				this.View.ShowMessage(ResManager.LoadKDString("内容已经修改，是否保存？", "015072000002230", 7, new object[0]), 3, delegate(MessageBoxResult result)
				{
					if (result == 6)
					{
						if (this.View is IDynamicFormViewService)
						{
							(this.View as IDynamicFormViewService).MainBarItemClick("BTN_SAVE");
						}
						if (this.CurSelNodeData != null)
						{
							TreeView control = this.View.GetControl<TreeView>("FWorkCalSetupTreeView");
							control.Select(new DiffCalendarOption.NodeInfo(this.CurSelNodeData.ParentNodeType, this.CurSelNodeData.Id).FullNodeId);
							return;
						}
					}
					else
					{
						if (result == 7)
						{
							childView.Model.DataChanged = false;
							DiffCalendarOption.NodeInfo nodeInfo2;
							nodeInfo2..ctor(e.NodeId);
							AbstractBaseDataView curSelNodeData = this.workCalTreeData.FindObjectItem(nodeInfo2.NodeId, Convert.ToInt32(nodeInfo2.NodeType));
							this.InitCurSelNodeInfo(curSelNodeData);
							this.ShowCustomCalendarForm(1);
							return;
						}
						if (result == 2 && this.CurSelNodeData != null)
						{
							TreeView control2 = this.View.GetControl<TreeView>("FWorkCalSetupTreeView");
							control2.Select(new DiffCalendarOption.NodeInfo(this.CurSelNodeData.ParentNodeType, this.CurSelNodeData.Id).FullNodeId);
						}
					}
				}, "", 0);
				return;
			}
			DiffCalendarOption.NodeInfo nodeInfo;
			nodeInfo..ctor(e.NodeId);
			AbstractBaseDataView abstractBaseDataView = this.workCalTreeData.FindObjectItem(nodeInfo.NodeId, Convert.ToInt32(nodeInfo.NodeType));
			if (abstractBaseDataView != null)
			{
				this.InitCurSelNodeInfo(abstractBaseDataView);
				this.ShowCustomCalendarForm(1);
				return;
			}
			this.View.ShowMessage(ResManager.LoadKDString("请选择合法的数据结点!", "015072000002231", 7, new object[0]), 0);
		}

		// Token: 0x06000DC5 RID: 3525 RVA: 0x000A07CC File Offset: 0x0009E9CC
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (e.Key == "ENG_WorkCalCustomTree" && e.EventName == "AfterDoOperation")
			{
				string eventArgs = e.EventArgs;
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(eventArgs))
				{
					string[] array = eventArgs.Split(new char[]
					{
						'_'
					}).ToArray<string>();
					this.workCalTreeData = CalendarServiceHelper.GetWorkCalendarSetupTreeData(base.Context, true);
					AbstractBaseDataView abstractBaseDataView = this.workCalTreeData.FindObjectItem(Convert.ToInt64(array[0]), Convert.ToInt32(array[1]));
					if (abstractBaseDataView != null)
					{
						this.InitCurSelNodeInfo(abstractBaseDataView);
						this.ShowCustomCalendarForm(1);
					}
				}
			}
		}

		// Token: 0x06000DC6 RID: 3526 RVA: 0x000A0880 File Offset: 0x0009EA80
		private void BindTreeViewData()
		{
			this.workCalTreeData = CalendarServiceHelper.GetWorkCalendarSetupTreeData(base.Context, true);
			if (this.treeNodeParam != null && this.treeNodeParam.useOrg != 0L)
			{
				OrganizationData organizationData = (from w in this.workCalTreeData.Organizations
				where w.Id == this.treeNodeParam.useOrg
				select w).FirstOrDefault<OrganizationData>();
				if (organizationData != null)
				{
					WorkCalTreeData workCalTreeData = new WorkCalTreeData();
					workCalTreeData.Organizations.Add(organizationData);
					this.TopNode = workCalTreeData.ToTreeNodeObject();
				}
			}
			if (this.treeNodeParam == null || this.TopNode == null)
			{
				this.TopNode = this.workCalTreeData.ToTreeNodeObject();
			}
		}

		// Token: 0x06000DC7 RID: 3527 RVA: 0x000A0924 File Offset: 0x0009EB24
		private void InitCurSelNodeInfo(AbstractBaseDataView curSelNodeData)
		{
			this.CurSelOrgNodeData = null;
			this.CurSelDeptNodeData = null;
			this.CurSelWorkCenterNodeData = null;
			this.CurSelResNodeData = null;
			this.CurSelEquNodeData = null;
			this.CurSelNodeData = curSelNodeData;
			switch (this.CurSelNodeData.ParentNodeType)
			{
			case 0:
				this.CurSelOrgNodeData = this.CurSelNodeData;
				return;
			case 1:
				this.CurSelOrgNodeData = this.CurSelNodeData.Parent;
				this.CurSelDeptNodeData = this.CurSelNodeData;
				return;
			case 2:
				this.CurSelOrgNodeData = this.CurSelNodeData.Parent.Parent;
				this.CurSelDeptNodeData = this.CurSelNodeData.Parent;
				this.CurSelWorkCenterNodeData = this.CurSelNodeData;
				return;
			case 3:
				this.CurSelOrgNodeData = this.CurSelNodeData.Parent.Parent.Parent;
				this.CurSelDeptNodeData = this.CurSelNodeData.Parent.Parent;
				this.CurSelWorkCenterNodeData = this.CurSelNodeData.Parent;
				this.CurSelResNodeData = this.CurSelNodeData;
				return;
			case 4:
				this.CurSelOrgNodeData = this.CurSelNodeData.Parent.Parent.Parent.Parent;
				this.CurSelDeptNodeData = this.CurSelNodeData.Parent.Parent.Parent;
				this.CurSelWorkCenterNodeData = this.CurSelNodeData.Parent.Parent;
				this.CurSelResNodeData = this.CurSelNodeData.Parent;
				this.CurSelEquNodeData = this.CurSelNodeData;
				return;
			default:
				return;
			}
		}

		// Token: 0x06000DC8 RID: 3528 RVA: 0x000A0AA0 File Offset: 0x0009ECA0
		private void ShowCustomCalendarForm(OperationStatus status = 1)
		{
			new object();
			IDynamicFormView view = this.View.GetView(this.CustomCalendarBill_PageId);
			if (view == null && !this._isShow)
			{
				BillShowParameter billShowParameter = new BillShowParameter();
				billShowParameter.OpenStyle.ShowType = 3;
				billShowParameter.OpenStyle.TagetKey = "FContainer";
				billShowParameter.FormId = "ENG_WorkCalCustom";
				billShowParameter.ParentPageId = this.View.PageId;
				billShowParameter.PageId = this.CustomCalendarBill_PageId;
				billShowParameter.PKey = this.CurSelNodeData.CalId.ToString();
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_WorkCalCustom", true) as FormMetadata;
				foreach (KeyValuePair<string, BarItem> keyValuePair in formMetadata.GetLayoutInfo().GetFormAppearance().Menu.GetAllBarItems())
				{
					billShowParameter.InitControlStates.Add(new ControlState
					{
						Key = keyValuePair.Key,
						Visible = false
					});
				}
				if (this.CurSelNodeData.CalId > 0L)
				{
					billShowParameter.Status = status;
				}
				this.ShowBillForm(this.View, billShowParameter, this.CurSelNodeData, null);
				bool flag = this.CurSelNodeData.CalId > 0L;
				this.View.GetMainBarItem("btn_view").Enabled = flag;
				this.View.GetMainBarItem("btn_modify").Enabled = flag;
				this.View.GetMainBarItem("btn_save").Enabled = !flag;
				this._isShow = true;
				return;
			}
			if (view != null)
			{
				this.View.Session["FormInputParam"] = this.CurSelNodeData;
				((BillOpenParameter)view.OpenParameter).PkValue = this.CurSelNodeData.CalId.ToString();
				if (this.CurSelNodeData.CalId <= 0L)
				{
					((BillOpenParameter)view.OpenParameter).Status = 0;
				}
				else
				{
					((BillOpenParameter)view.OpenParameter).Status = status;
				}
				view.Refresh();
				Entity entity = view.BusinessInfo.GetEntity("FEntity");
				DynamicObjectCollection entityDataObject = view.Model.GetEntityDataObject(entity);
				if (!ListUtils.IsEmpty<DynamicObject>(entityDataObject))
				{
					DateTime sysDate = MFGServiceHelper.GetSysDate(base.Context);
					int num = Convert.ToInt32(sysDate.DayOfWeek);
					DateTime dateTime = sysDate.Date.AddDays((double)(-1 * ((num == 0) ? 6 : (num - 1))));
					int num2 = 0;
					foreach (DynamicObject dynamicObject in entityDataObject)
					{
						if (dateTime.CompareTo(DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject, "Day", default(DateTime))) <= 0)
						{
							view.SetEntityFocusRow("FEntity", num2);
							break;
						}
						num2++;
					}
				}
				this.View.SendDynamicFormAction(view);
			}
		}

		// Token: 0x06000DC9 RID: 3529 RVA: 0x000A0DF8 File Offset: 0x0009EFF8
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

		// Token: 0x06000DCA RID: 3530 RVA: 0x000A0E88 File Offset: 0x0009F088
		private bool VaildatePermission(string strPerItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = "ENG_WorkCalSetup",
				SubSystemId = this.View.Model.SubSytemId
			}, strPerItemId);
			if (!permissionAuthResult.Passed && strPerItemId != null)
			{
				if (!(strPerItemId == "fce8b1aca2144beeb3c6655eaf78bc34"))
				{
					if (!(strPerItemId == "f323992d896745fbaab4a2717c79ce2e"))
					{
						if (strPerItemId == "24f64c0dbfa945f78a6be123197a63f5")
						{
							this.View.ShowErrMessage(ResManager.LoadKDString("需要配置工作日历设置的删除权限", "015072000013267", 7, new object[0]), ResManager.LoadKDString("没有删除权限", "015072000013268", 7, new object[0]), 0);
						}
					}
					else
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("需要配置工作日历设置的修改权限", "015072000001963", 7, new object[0]), ResManager.LoadKDString("没有修改权限", "015072000001921", 7, new object[0]), 0);
					}
				}
				else
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("需要配置工作日历设置的新增权限", "015072000001960", 7, new object[0]), ResManager.LoadKDString("没有新增权限", "015072000001915", 7, new object[0]), 0);
				}
			}
			return permissionAuthResult.Passed;
		}

		// Token: 0x04000655 RID: 1621
		private const string FIELDKEY_TREEVIEW = "FWorkCalSetupTreeView";

		// Token: 0x04000656 RID: 1622
		private const string SPANKEY = "FContainer";

		// Token: 0x04000657 RID: 1623
		private bool _isShow;

		// Token: 0x04000658 RID: 1624
		private WorkCalTreeData workCalTreeData;

		// Token: 0x04000659 RID: 1625
		private T_WorkCalSetupFormParam treeNodeParam;
	}
}
