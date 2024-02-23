using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.MaterialTree;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000A4 RID: 164
	[Description("物料树形控件插件")]
	public class MaterialTree : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000B97 RID: 2967 RVA: 0x00085C13 File Offset: 0x00083E13
		// (set) Token: 0x06000B98 RID: 2968 RVA: 0x00085C1B File Offset: 0x00083E1B
		private TreeParameters Params { get; set; }

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000B99 RID: 2969 RVA: 0x00085C24 File Offset: 0x00083E24
		// (set) Token: 0x06000B9A RID: 2970 RVA: 0x00085C2C File Offset: 0x00083E2C
		private string currentNodeId { get; set; }

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000B9B RID: 2971 RVA: 0x00085C35 File Offset: 0x00083E35
		// (set) Token: 0x06000B9C RID: 2972 RVA: 0x00085C3D File Offset: 0x00083E3D
		private TreeNode topTreeNode { get; set; }

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000B9D RID: 2973 RVA: 0x00085C46 File Offset: 0x00083E46
		// (set) Token: 0x06000B9E RID: 2974 RVA: 0x00085C61 File Offset: 0x00083E61
		private List<object> ShowMaterial
		{
			get
			{
				if (this.showmtrl == null)
				{
					this.showmtrl = new List<object>();
				}
				return this.showmtrl;
			}
			set
			{
				this.showmtrl = value;
			}
		}

		// Token: 0x06000B9F RID: 2975 RVA: 0x00085C6C File Offset: 0x00083E6C
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Dictionary<string, object> customParameters = this.View.OpenParameter.GetCustomParameters();
			object obj = null;
			object obj2 = null;
			customParameters.TryGetValue("ShowParam", out obj);
			customParameters.TryGetValue("ShowObject", out obj2);
			string text = (obj == null) ? string.Empty : obj.ToString();
			string text2 = (obj2 == null) ? string.Empty : obj2.ToString();
			if (!string.IsNullOrWhiteSpace(text))
			{
				object obj3 = null;
				if (this.View.ParentFormView != null && this.View.ParentFormView.Session.TryGetValue(text, out obj3) && this.View.ParentFormView.Session[text] != null)
				{
					this.Params = (TreeParameters)this.View.ParentFormView.Session[text];
				}
			}
			if (!string.IsNullOrWhiteSpace(text2))
			{
				object obj4 = null;
				if (this.View.ParentFormView != null && this.View.ParentFormView.Session.TryGetValue(text2, out obj4) && this.View.ParentFormView.Session[text2] != null)
				{
					this.ShowMaterial = (List<object>)this.View.ParentFormView.Session[text2];
				}
			}
			if (this.showmtrl != null)
			{
				this.InitalTree();
			}
		}

		// Token: 0x06000BA0 RID: 2976 RVA: 0x00085DBC File Offset: 0x00083FBC
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (e.EventName.Equals("TreeViewDataSourceChanged"))
			{
				TreeNode topTreeNode = this.topTreeNode;
				if (topTreeNode != null && topTreeNode.children.Count > 0)
				{
					TreeView treeView = (TreeView)this.View.GetControl("FTreeView");
					treeView.Select(topTreeNode.children.First<TreeNode>().id);
				}
			}
		}

		// Token: 0x06000BA1 RID: 2977 RVA: 0x00085E28 File Offset: 0x00084028
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			if (e.Key.ToUpperInvariant().Equals("FSEARCH") && this.View.Model.GetValue("FFilterStr") != null)
			{
				string text = this.View.Model.GetValue("FFilterStr").ToString();
				if (string.IsNullOrWhiteSpace(text))
				{
					return;
				}
				this.SearchResult(text);
			}
		}

		// Token: 0x06000BA2 RID: 2978 RVA: 0x00085E98 File Offset: 0x00084098
		public override void BeforeF1Click(F1ClickArgs e)
		{
			if (this.View.ParentFormView != null)
			{
				Form form = this.View.ParentFormView.BillBusinessInfo.GetForm();
				string helpContextId = string.IsNullOrEmpty(form.HelpContextId) ? form.Id : form.HelpContextId;
				e.HelpContextId = helpContextId;
			}
		}

		// Token: 0x06000BA3 RID: 2979 RVA: 0x00085EEC File Offset: 0x000840EC
		private void SearchResult(string textValue)
		{
			TreeView control = this.View.GetControl<TreeView>("FTreeView");
			bool flag = false;
			if (string.IsNullOrWhiteSpace(this.currentNodeId))
			{
				flag = true;
			}
			TreeNode treeNode = this.FindNodeByName(this.topTreeNode, this.currentNodeId, textValue, ref flag);
			TreeNode treeNode2;
			if (treeNode != null)
			{
				treeNode2 = treeNode;
			}
			else
			{
				flag = false;
				this.AddAllChildNode(this.currentNodeId, 0);
				treeNode2 = this.FindNodeByName(this.topTreeNode, this.currentNodeId, textValue, ref flag);
			}
			if (treeNode2 != null)
			{
				control.Select(treeNode2.id);
				return;
			}
			this.View.ShowMessage(ResManager.LoadKDString("没有找到想要的结果！", "015072000002220", 7, new object[0]), 0);
		}

		// Token: 0x06000BA4 RID: 2980 RVA: 0x00085F90 File Offset: 0x00084190
		private TreeNode FindNodeByName(TreeNode parentNode, string curId, string name, ref bool isFind)
		{
			if (parentNode == null)
			{
				return null;
			}
			if (parentNode.text.Contains(name) && isFind)
			{
				return parentNode;
			}
			if (parentNode.id.Equals(curId))
			{
				isFind = !isFind;
			}
			foreach (TreeNode parentNode2 in parentNode.children)
			{
				TreeNode treeNode = this.FindNodeByName(parentNode2, curId, name, ref isFind);
				if (treeNode != null)
				{
					return treeNode;
				}
			}
			return null;
		}

		// Token: 0x06000BA5 RID: 2981 RVA: 0x00086024 File Offset: 0x00084224
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
		}

		// Token: 0x06000BA6 RID: 2982 RVA: 0x00086030 File Offset: 0x00084230
		public override void TreeNodeClick(TreeNodeArgs e)
		{
			base.TreeNodeClick(e);
			this.currentNodeId = e.NodeId;
			this.AddChildNode(e.NodeId);
			if (this.View.ParentFormView != null)
			{
				(this.View.ParentFormView as IDynamicFormViewService).CustomEvents("MFG_MaterialTree", "TreeNodeClick", e.NodeId);
				this.View.SendDynamicFormAction(this.View.ParentFormView);
			}
		}

		// Token: 0x06000BA7 RID: 2983 RVA: 0x000860B8 File Offset: 0x000842B8
		private void AddChildNode(string parentNodeId)
		{
			if (parentNodeId.Contains("m"))
			{
				return;
			}
			TreeView treeView = (TreeView)this.View.GetControl("FTreeView");
			IGrouping<string, TreeNode> source = null;
			TreeNode treeNode = this.AllNodeDic[parentNodeId];
			if (treeNode.children.Any((TreeNode p) => p.id.Contains("m")))
			{
				return;
			}
			if (this.MaterialNodeDic.TryGetValue(parentNodeId, out source))
			{
				List<TreeNode> list = source.ToList<TreeNode>();
				treeNode.children.AddRange(list);
				treeView.AddNodes(parentNodeId, list);
			}
		}

		// Token: 0x06000BA8 RID: 2984 RVA: 0x00086168 File Offset: 0x00084368
		private void AddAllChildNode(string parentNodeId, int currLevel = 0)
		{
			if (currLevel >= 29)
			{
				return;
			}
			currLevel++;
			if (parentNodeId.Contains("m"))
			{
				return;
			}
			TreeView treeView = (TreeView)this.View.GetControl("FTreeView");
			IGrouping<string, TreeNode> source = null;
			TreeNode treeNode = this.AllNodeDic[parentNodeId];
			List<string> list = (from s in this.topTreeNode.children
			select s.id).ToList<string>();
			if (!list.Contains(parentNodeId))
			{
				if (treeNode.children.Any((TreeNode p) => p.id.Contains("m")))
				{
					return;
				}
				if (this.MaterialNodeDic.TryGetValue(parentNodeId, out source))
				{
					List<TreeNode> list2 = source.ToList<TreeNode>();
					treeNode.children.AddRange(list2);
					treeView.AddNodes(parentNodeId, list2);
				}
			}
			foreach (TreeNode treeNode2 in treeNode.children)
			{
				this.AddAllChildNode(treeNode2.id, currLevel);
			}
		}

		// Token: 0x06000BA9 RID: 2985 RVA: 0x000862C0 File Offset: 0x000844C0
		private void InitalTree()
		{
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.View.Model.ParameterData))
			{
				FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ENG_BOMTREEPARAM", true);
				DynamicObject parameterData = UserParamterServiceHelper.Load(this.View.Context, formMetadata.BusinessInfo, this.View.Context.UserId, "ENG_BOMTREE", "UserParameter");
				this.View.Model.ParameterData = parameterData;
			}
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			if (MFGBillUtil.GetUserParam<bool>(this.View, "Number", false))
			{
				dictionary.Add("FNumber", MFGBillUtil.GetUserParam<bool>(this.View, "Number", false));
			}
			if (MFGBillUtil.GetUserParam<bool>(this.View, "Name", false))
			{
				dictionary.Add("FName", MFGBillUtil.GetUserParam<bool>(this.View, "Name", false));
			}
			if (MFGBillUtil.GetUserParam<bool>(this.View, "Specification", false))
			{
				dictionary.Add("FSpecification", MFGBillUtil.GetUserParam<bool>(this.View, "Name", false));
			}
			string userParam = MFGBillUtil.GetUserParam<string>(this.View, "SplitCode", null);
			TreeView treeView = (TreeView)this.View.GetControl("FTreeView");
			MaterialTreeObject materialTreeObject = new MaterialTreeObject();
			if (this.ShowMaterial != null)
			{
				if (this.ShowMaterial.Count > 0)
				{
					materialTreeObject = MaterialTreeServiceHelper.GetMaterialTree(base.Context, this.ShowMaterial, dictionary, userParam);
					this.MaterialNodeDic = (from p in materialTreeObject.MaterialNodes
					group p by p.parentid).ToDictionary((IGrouping<string, TreeNode> d) => d.Key);
					this.groupNodeDic = (from p in materialTreeObject.GroupNodes
					group p by p.parentid).ToDictionary((IGrouping<string, TreeNode> d) => d.Key);
					List<TreeNode> list = new List<TreeNode>();
					list.AddRange(materialTreeObject.OrgNodes);
					list.AddRange(this.GetAllNode(materialTreeObject.GroupNodes));
					list.AddRange(materialTreeObject.MaterialNodes);
					this.AllNodeDic = list.ToDictionary((TreeNode p) => p.id);
				}
				if (this.Params.IsShowOrg)
				{
					TreeNode treeNode = new TreeNode
					{
						text = ResManager.LoadKDString("物料", "015072000002221", 7, new object[0]),
						id = "0",
						parentid = "0",
						cls = "parentnode"
					};
					treeNode.children.AddRange(materialTreeObject.OrgNodes);
					foreach (TreeNode treeNode2 in materialTreeObject.OrgNodes)
					{
						foreach (TreeNode treeNode3 in materialTreeObject.GroupNodes)
						{
							if (treeNode3.parentid == treeNode2.id)
							{
								treeNode2.children.Add(treeNode3);
							}
						}
					}
					this.topTreeNode = treeNode;
					treeView.SetStyle(1);
					treeView.SetRootNode(treeNode);
				}
			}
		}

		// Token: 0x06000BAA RID: 2986 RVA: 0x00086684 File Offset: 0x00084884
		private void AddMtrl(TreeNode treenode, List<TreeNode> mtrlNode)
		{
			List<TreeNode> list = (from e in mtrlNode
			where e.parentid == treenode.id
			select e).ToList<TreeNode>();
			foreach (TreeNode treenode2 in treenode.children)
			{
				this.AddMtrl(treenode2, mtrlNode);
			}
			if (list.Count > 0)
			{
				treenode.children.AddRange(list);
			}
		}

		// Token: 0x06000BAB RID: 2987 RVA: 0x0008671C File Offset: 0x0008491C
		private List<TreeNode> GetAllNode(List<TreeNode> nodes)
		{
			List<TreeNode> list = new List<TreeNode>();
			if (ListUtils.IsEmpty<TreeNode>(nodes))
			{
				return list;
			}
			list.AddRange(nodes);
			foreach (TreeNode treeNode in nodes)
			{
				if (!ListUtils.IsEmpty<TreeNode>(treeNode.children))
				{
					list.AddRange(this.GetAllNode(treeNode.children));
				}
			}
			return list;
		}

		// Token: 0x0400056C RID: 1388
		private List<object> showmtrl;

		// Token: 0x0400056D RID: 1389
		private Dictionary<string, TreeNode> AllNodeDic = new Dictionary<string, TreeNode>();

		// Token: 0x0400056E RID: 1390
		private Dictionary<string, IGrouping<string, TreeNode>> groupNodeDic = new Dictionary<string, IGrouping<string, TreeNode>>();

		// Token: 0x0400056F RID: 1391
		private Dictionary<string, IGrouping<string, TreeNode>> MaterialNodeDic = new Dictionary<string, IGrouping<string, TreeNode>>();
	}
}
