using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.PreInsertData;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Serialization;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.ECN;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000089 RID: 137
	[Description("ECN清理日志")]
	public class ECNClearLog : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000069 RID: 105
		// (set) Token: 0x06000A4D RID: 2637 RVA: 0x000781C9 File Offset: 0x000763C9
		private long ChangeOrgId
		{
			set
			{
				if (value == 0L && base.Context.CurrentOrganizationInfo.FunctionIds.Contains(104L))
				{
					this.changeOrgId = base.Context.CurrentOrganizationInfo.ID;
					return;
				}
				this.changeOrgId = value;
			}
		}

		// Token: 0x1700006A RID: 106
		// (set) Token: 0x06000A4E RID: 2638 RVA: 0x00078208 File Offset: 0x00076408
		private DateTime ClearDateFrom
		{
			set
			{
				this.clearDateFrom = value.Date;
			}
		}

		// Token: 0x1700006B RID: 107
		// (set) Token: 0x06000A4F RID: 2639 RVA: 0x00078218 File Offset: 0x00076418
		private DateTime ClearDateTo
		{
			set
			{
				this.clearDateTo = value.Date.AddDays(1.0);
			}
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000A50 RID: 2640 RVA: 0x00078243 File Offset: 0x00076443
		private TreeView CurTreeView
		{
			get
			{
				return this.View.GetControl<TreeView>("FTreeView");
			}
		}

		// Token: 0x06000A51 RID: 2641 RVA: 0x00078258 File Offset: 0x00076458
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			DynamicObject directInScheme = this.GetDirectInScheme();
			object obj = null;
			object obj2 = null;
			if (directInScheme != null)
			{
				this.ChangeOrgId = DataEntityExtend.GetDynamicValue<long>(directInScheme, "ChangeOrgId_Id", 0L);
				obj = directInScheme["ClearDateFrom"];
				obj2 = directInScheme["ClearDateTo"];
			}
			this.currentFormId = this.View.BillBusinessInfo.GetForm().Id;
			this.ClearDateFrom = ((obj == null) ? DateTime.Now : Convert.ToDateTime(obj));
			this.ClearDateTo = ((obj2 == null) ? DateTime.Now : Convert.ToDateTime(obj2));
		}

		// Token: 0x06000A52 RID: 2642 RVA: 0x000782EC File Offset: 0x000764EC
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			this.InitData();
		}

		// Token: 0x06000A53 RID: 2643 RVA: 0x00078394 File Offset: 0x00076594
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbRefresh")
				{
					this.InitData();
					e.Cancel = true;
					return;
				}
				if (!(barItemKey == "tbFilter"))
				{
					return;
				}
				MFGBillUtil.ShowFilterForm(this.View, "ENG_ECNClearLog", null, delegate(FormResult filterResult)
				{
					if (filterResult.ReturnData is FilterParameter)
					{
						this.filterParam = (filterResult.ReturnData as FilterParameter);
						this.ChangeOrgId = DataEntityExtend.GetDynamicValue<long>(this.filterParam.CustomFilter, "ChangeOrgId_Id", 0L);
						this.ClearDateFrom = DataEntityExtend.GetDynamicValue<DateTime>(this.filterParam.CustomFilter, "ClearDateFrom", default(DateTime));
						this.ClearDateTo = DataEntityExtend.GetDynamicValue<DateTime>(this.filterParam.CustomFilter, "ClearDateTo", default(DateTime));
						this.InitData();
					}
				}, "ENG_ECNClearLogFilter", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x06000A54 RID: 2644 RVA: 0x0007840C File Offset: 0x0007660C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if ((e.Operation.FormOperation.IsEqualOperation("Delete") || e.Operation.FormOperation.IsEqualOperation("BatchDelete")) && !MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BillBusinessInfo.GetForm().Id, e.Operation.FormOperation.PermissionItemId))
			{
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("没有ECN清理日志查询的{0}权限", "015072000012080", 7, new object[0]), e.Operation.FormOperation.OperationName[base.Context.UserLocale.LCID]), "", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x06000A55 RID: 2645 RVA: 0x000784DD File Offset: 0x000766DD
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			if (e.Operation.IsEqualOperation("Delete"))
			{
				this.DeleteLog();
				return;
			}
			if (e.Operation.IsEqualOperation("BatchDelete"))
			{
				this.ShowBatchDeleteLogView();
			}
		}

		// Token: 0x06000A56 RID: 2646 RVA: 0x00078517 File Offset: 0x00076717
		public override void TreeNodeClick(TreeNodeArgs e)
		{
			base.TreeNodeClick(e);
			this.selectedNodeId = e.NodeId;
			this.FillClearLogEntity(e.NodeId);
		}

		// Token: 0x06000A57 RID: 2647 RVA: 0x00078540 File Offset: 0x00076740
		private void ShowBatchDeleteLogView()
		{
			if (ListUtils.IsEmpty<DynamicObject>(this.ecnClearLogData))
			{
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.FormId = "ENG_ECNClearLogBatchDelete";
			dynamicFormShowParameter.CustomParams.Add("FChangeOrgId", Convert.ToString(this.changeOrgId));
			dynamicFormShowParameter.CustomParams.Add("FClearDateFrom", this.clearDateFrom.ToString());
			dynamicFormShowParameter.CustomParams.Add("FClearDateTo", this.clearDateTo.ToString());
			this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult formResult)
			{
				this.InitData();
			});
		}

		// Token: 0x06000A58 RID: 2648 RVA: 0x000785FC File Offset: 0x000767FC
		private void DeleteLog()
		{
			if (this.selectedNodeId == null || this.ecnClearLogDataDic == null)
			{
				return;
			}
			IGrouping<string, DynamicObject> source;
			if (this.ecnClearLogDataDic.TryGetValue(this.selectedNodeId, out source))
			{
				List<string> list = (from s in source
				select DataEntityExtend.GetDynamicValue<string>(s, "ID", null)).ToList<string>();
				if (ECNClearOldMtrlServiceHelper.DeleteLog(base.Context, list))
				{
					this.CurTreeView.RemoveNode(this.selectedNodeId);
				}
			}
		}

		// Token: 0x06000A59 RID: 2649 RVA: 0x000786B0 File Offset: 0x000768B0
		private void InitData()
		{
			ECNClearLogQueryOption ecnclearLogQueryOption = new ECNClearLogQueryOption
			{
				ChangeOrgId = this.changeOrgId,
				ClearBeginDate = this.clearDateFrom,
				ClearEndDate = this.clearDateTo
			};
			this.ecnClearLogData = ECNClearOldMtrlServiceHelper.GetECNClearLogInfo(base.Context, ecnclearLogQueryOption);
			this.ecnClearLogDataDic = (from g in this.ecnClearLogData
			group g by DataEntityExtend.GetDynamicValue<DateTime>(g, "ClearDate", default(DateTime)).ToString("yyyy-MM-dd HH:mm:ss")).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
			this.CreateTree();
		}

		// Token: 0x06000A5A RID: 2650 RVA: 0x00078778 File Offset: 0x00076978
		private void CreateTree()
		{
			TreeNode treeNode = new TreeNode();
			treeNode.text = ResManager.LoadKDString("清理日志", "015072000012081", 7, new object[0]);
			treeNode.id = "0";
			List<DateTime> list = (from s in this.ecnClearLogData
			select DataEntityExtend.GetDynamicValue<DateTime>(s, "ClearDate", default(DateTime))).Distinct<DateTime>().ToList<DateTime>();
			if (ListUtils.IsEmpty<DateTime>(list))
			{
				this.CurTreeView.SetRootNode(treeNode);
				this.View.Model.DeleteEntryData("FEntity");
				this.View.UpdateView("FEntity");
				this.View.ShowErrMessage(ResManager.LoadKDString("没有查找到对应的数据！", "015078000002388", 7, new object[0]), "", 0);
				return;
			}
			foreach (DateTime dateTime in list)
			{
				TreeNode treeNode2 = new TreeNode();
				treeNode2.id = dateTime.ToString();
				treeNode2.text = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
				treeNode.children.Add(treeNode2);
			}
			this.CurTreeView.SetRootNode(treeNode);
			this.CurTreeView.Select(treeNode.children.First<TreeNode>().id);
			this.CurTreeView.InvokeControlMethod("ExpandTree", new object[0]);
		}

		// Token: 0x06000A5B RID: 2651 RVA: 0x000788F8 File Offset: 0x00076AF8
		private void FillClearLogEntity(string nodeId)
		{
			IGrouping<string, DynamicObject> grouping;
			if (nodeId == "0" || !this.ecnClearLogDataDic.TryGetValue(nodeId, out grouping))
			{
				this.View.Model.DeleteEntryData("FEntity");
				this.View.UpdateView("FEntity");
				return;
			}
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			int num = 0;
			this.View.Model.BeginIniti();
			this.View.Model.DeleteEntryData("FEntity");
			this.View.Model.BatchCreateNewEntryRow("FEntity", grouping.Count<DynamicObject>());
			foreach (DynamicObject dynamicObject in grouping)
			{
				dynamicObject["Seq"] = num + 1;
				MFGCommonUtil.SetDyFormViewFieldsValue(this.View, entity, dynamicObject, num);
				num++;
			}
			this.View.Model.EndIniti();
			this.View.UpdateView("FEntity");
		}

		// Token: 0x06000A5C RID: 2652 RVA: 0x00078A20 File Offset: 0x00076C20
		protected DynamicObject GetDirectInScheme()
		{
			DynamicObject result = null;
			string nextEntrySchemeId = UserParamterServiceHelper.GetNextEntrySchemeId(base.Context, "ENG_ECNClearLog");
			if (!ObjectUtils.IsNullOrEmpty(nextEntrySchemeId))
			{
				FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "BOS_FilterScheme");
				FormMetadata formMetaData2 = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_ECNClearLogFilter");
				DynamicObject dynamicObject = BusinessDataServiceHelper.Load(base.Context, new object[]
				{
					nextEntrySchemeId
				}, formMetaData.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<DynamicObject>();
				if (dynamicObject != null)
				{
					FilterScheme filterScheme = new FilterScheme(dynamicObject);
					if (filterScheme.Scheme != null && !string.IsNullOrEmpty(filterScheme.Scheme))
					{
						SchemeEntity schemeEntity = (SchemeEntity)new DcxmlSerializer(new PreInsertDataDcxmlBinder()).DeserializeFromString(filterScheme.Scheme, null);
						DcxmlBinder dcxmlBinder = new DynamicObjectDcxmlBinder(formMetaData2.BusinessInfo);
						dcxmlBinder.OnlyDbProperty = false;
						CultureInfo culture = new CultureInfo(2052);
						dcxmlBinder.Culture = culture;
						DcxmlSerializer dcxmlSerializer = new DcxmlSerializer(dcxmlBinder);
						DynamicObject dynamicObject2 = (DynamicObject)dcxmlSerializer.DeserializeFromString(schemeEntity.CustomFilterSetting, null);
						if (!ObjectUtils.IsNullOrEmpty(dynamicObject2))
						{
							result = dynamicObject2;
						}
					}
					else
					{
						result = new DynamicObject(this.Model.BusinessInfo.GetDynamicObjectType());
					}
				}
			}
			return result;
		}

		// Token: 0x040004EC RID: 1260
		private FilterParameter filterParam;

		// Token: 0x040004ED RID: 1261
		private string currentFormId;

		// Token: 0x040004EE RID: 1262
		private long changeOrgId;

		// Token: 0x040004EF RID: 1263
		private DateTime clearDateFrom;

		// Token: 0x040004F0 RID: 1264
		private DateTime clearDateTo;

		// Token: 0x040004F1 RID: 1265
		private DynamicObjectCollection ecnClearLogData;

		// Token: 0x040004F2 RID: 1266
		private Dictionary<string, IGrouping<string, DynamicObject>> ecnClearLogDataDic;

		// Token: 0x040004F3 RID: 1267
		private string selectedNodeId;
	}
}
