using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.Formula;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000023 RID: 35
	[Description("车间公式表单插件")]
	public class FormulaEdit : BaseControlEdit
	{
		// Token: 0x060002B8 RID: 696 RVA: 0x0001FE7C File Offset: 0x0001E07C
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			object value = base.View.Model.GetValue("FMulLangExpress");
			if (!StringUtils.IsEmpty(value.ToString()))
			{
				base.View.Model.SetValue("FExpressDesc", value.ToString());
			}
		}

		// Token: 0x060002B9 RID: 697 RVA: 0x0001FED0 File Offset: 0x0001E0D0
		public override void AfterSetStatus(AfterSetStatusEventArgs e)
		{
			base.AfterSetStatus(e);
			Control control = base.View.GetControl("FExpress");
			if (!control.Enabled)
			{
				base.View.GetControl("FExpressDesc").Enabled = false;
			}
		}

		// Token: 0x060002BA RID: 698 RVA: 0x0001FF14 File Offset: 0x0001E114
		public override List<TreeNode> GetTreeViewData(TreeNodeArgs treeNodeArgs)
		{
			base.GetTreeViewData(treeNodeArgs);
			List<TreeNode> list = new List<TreeNode>();
			TreeView control = base.View.GetControl<TreeView>("FTreeView");
			this.typeNodeIdList.Add("0");
			this.BindLeftTreeActivity(ref list);
			this.BindLeftTreeCustom(ref list);
			this.ExpandTreeNode(control, list);
			return list;
		}

		// Token: 0x060002BB RID: 699 RVA: 0x0001FF6C File Offset: 0x0001E16C
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			object value = base.View.Model.GetValue("FExpressDesc");
			string text = (value == null) ? string.Empty : value.ToString();
			string empty = string.Empty;
			string empty2 = string.Empty;
			if (!this.IsValidateExpress(text, out empty, out empty2))
			{
				base.View.ShowErrMessage(empty, "", 0);
				e.Cancel = true;
				return;
			}
			base.View.Model.SetValue("FExpress", empty2);
			base.View.Model.SetValue("FMulLangExpress", new LocaleValue(text, base.Context.UserLocale.LCID));
		}

		// Token: 0x060002BC RID: 700 RVA: 0x0002001B File Offset: 0x0001E21B
		public override void TreeNodeDoubleClick(TreeNodeArgs e)
		{
			if (!this.CheckEditable())
			{
				return;
			}
			base.TreeNodeDoubleClick(e);
			if (this.typeNodeIdList.Contains(e.NodeId))
			{
				return;
			}
			this.SetExpressValueByNode(e);
		}

		// Token: 0x060002BD RID: 701 RVA: 0x00020048 File Offset: 0x0001E248
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			if (!this.CheckEditable())
			{
				return;
			}
			base.ButtonClick(e);
			if (e.Key.Equals("FbtnClear", StringComparison.OrdinalIgnoreCase))
			{
				base.View.Model.SetValue("FExpressDesc", null);
				base.View.Model.SetValue("FExpress", null);
				return;
			}
			this.SetExpressValueByBtn(e.Key);
		}

		// Token: 0x060002BE RID: 702 RVA: 0x00020100 File Offset: 0x0001E300
		private bool IsValidateExpress(object objExpress, out string strMsg, out string strExpress)
		{
			strExpress = "";
			strMsg = "";
			if (objExpress == null || string.IsNullOrWhiteSpace(objExpress.ToString()))
			{
				return true;
			}
			strExpress = objExpress.ToString();
			string text = objExpress.ToString();
			string strTempValue = string.Empty;
			List<string> list = (from o in text.Split(this.operChar)
			where !string.IsNullOrWhiteSpace(o)
			select o).ToList<string>();
			foreach (string text2 in list)
			{
				strTempValue = text2.Trim();
				if (!this.IsNumberData(strTempValue))
				{
					FormulaElement formulaElement = this.nodeElementList.Find((FormulaElement o) => (o.IsFunction && o.Express == strTempValue) || (!o.IsFunction && o.Display == strTempValue));
					if (formulaElement != null && !formulaElement.IsFunction)
					{
						strExpress = strExpress.Replace(strTempValue, formulaElement.Express);
					}
				}
			}
			return true;
		}

		// Token: 0x060002BF RID: 703 RVA: 0x00020224 File Offset: 0x0001E424
		private void BindLeftTreeActivity(ref List<TreeNode> nodeLst)
		{
			TreeNode treeNode = new TreeNode();
			treeNode.parentid = "0";
			treeNode.id = "10";
			treeNode.text = this.activityTypeName;
			treeNode.children = this.GetActivityChildren();
			nodeLst.Add(treeNode);
			this.typeNodeIdList.Add(treeNode.id);
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x00020280 File Offset: 0x0001E480
		private List<TreeNode> GetActivityChildren()
		{
			List<TreeNode> list = new List<TreeNode>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FNumber"));
			list2.Add(new SelectorItemInfo("FName"));
			string text = string.Format("{0} = 'C' and {1} = 'A'", "FDocumentStatus", "FFORBIDSTATUS");
			OQLFilter oqlfilter = OQLFilter.CreateHeadEntityFilter(text);
			DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, "ENG_BaseActivity", list2, oqlfilter);
			foreach (DynamicObject dynamicObject in array)
			{
				list.Add(new TreeNode
				{
					id = dynamicObject["Number"].ToString(),
					text = dynamicObject["Name"].ToString(),
					parentid = "10"
				});
				JSONObject jsonobject = new JSONObject();
				jsonobject.Put("Id", dynamicObject["Id"]);
				jsonobject.Put("Number", dynamicObject["Number"]);
				jsonobject.Put("TypeNumber", "10");
				jsonobject.Put("TypeName", this.activityTypeName);
				jsonobject.Put("Name", dynamicObject["Name"].ToString());
				jsonobject.Put("IsFunction", false);
				jsonobject.Put("FuncExpress", null);
				jsonobject.Put("FuncMapping", null);
				this.nodeElementList.Add(new FormulaElement(jsonobject));
			}
			return list;
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x00020428 File Offset: 0x0001E628
		private void BindLeftTreeCustom(ref List<TreeNode> nodeLst)
		{
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FId"));
			list.Add(new SelectorItemInfo("FNumber"));
			list.Add(new SelectorItemInfo("FName"));
			list.Add(new SelectorItemInfo("FTypeNumber"));
			list.Add(new SelectorItemInfo("FTypeName"));
			list.Add(new SelectorItemInfo("FIsFunction"));
			list.Add(new SelectorItemInfo("FFunctionExpress"));
			list.Add(new SelectorItemInfo("FFunctionMapping"));
			string text = string.Format("{0} = 'C' and {1} = 'A'", "FDocumentStatus", "FFORBIDSTATUS");
			OQLFilter oqlfilter = OQLFilter.CreateHeadEntityFilter(text);
			DynamicObject[] source = BusinessDataServiceHelper.Load(base.Context, "ENG_FORMULAELEMENT", list, oqlfilter);
			IOrderedEnumerable<DynamicObject> source2 = from e in source
			orderby e["TypeNumber"], e["Number"]
			select e;
			this.BindCustomChildren(source2.ToArray<DynamicObject>(), ref nodeLst);
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x0002056C File Offset: 0x0001E76C
		private void BindCustomChildren(DynamicObject[] elementList, ref List<TreeNode> nodeLst)
		{
			List<TreeNode> list = new List<TreeNode>();
			for (int i = 0; i < elementList.Length; i++)
			{
				DynamicObject element = elementList[i];
				TreeNode treeNode = list.Find((TreeNode n) => n.id == element["TypeNumber"].ToString());
				if (treeNode == null)
				{
					treeNode = new TreeNode();
					treeNode.parentid = "0";
					treeNode.id = element["TypeNumber"].ToString();
					treeNode.text = element["TypeName"].ToString();
					list.Add(treeNode);
					nodeLst.Add(treeNode);
					this.typeNodeIdList.Add(treeNode.id);
				}
				TreeNode treeNode2 = new TreeNode();
				treeNode2.parentid = treeNode.id;
				treeNode2.id = element["Number"].ToString();
				treeNode2.text = element["Name"].ToString();
				treeNode.children.Add(treeNode2);
				JSONObject jsonobject = new JSONObject();
				jsonobject.Put("Id", element["Id"]);
				jsonobject.Put("TypeNumber", element["TypeNumber"]);
				jsonobject.Put("TypeName", element["TypeName"].ToString());
				jsonobject.Put("Number", element["Number"]);
				jsonobject.Put("Name", element["Name"].ToString());
				jsonobject.Put("IsFunction", element["IsFunction"]);
				jsonobject.Put("FuncExpress", element["FunctionExpress"]);
				jsonobject.Put("FuncMapping", element["FunctionMapping"]);
				this.nodeElementList.Add(new FormulaElement(jsonobject));
			}
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x00020794 File Offset: 0x0001E994
		private void ExpandTreeNode(TreeView treeView, List<TreeNode> nodeLst)
		{
			if (nodeLst.Count == 0)
			{
				return;
			}
			foreach (TreeNode treeNode in nodeLst)
			{
				treeView.InvokeControlMethod("ExpandNode", new object[]
				{
					treeNode.id
				});
			}
			treeView.SetExpanded(true);
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x00020824 File Offset: 0x0001EA24
		private void SetExpressValueByNode(TreeNodeArgs nodeArgs)
		{
			Control control = base.View.GetControl("FExpress");
			if (!control.Enabled)
			{
				return;
			}
			string strNodeValue = string.Empty;
			string arg = string.Empty;
			object value = base.View.Model.GetValue("FExpressDesc");
			string text = (value == null) ? string.Empty : value.ToString();
			strNodeValue = nodeArgs.NodeId;
			if (text.LastIndexOfAny(this.operChar) < 0)
			{
				text = string.Empty;
			}
			FormulaElement formulaElement = this.nodeElementList.Find((FormulaElement n) => n.Number == strNodeValue);
			arg = formulaElement.Display;
			base.View.Model.SetValue("FExpressDesc", string.Format("{0} {1}", text, arg));
			base.View.GetControl("FExpressDesc").SetFocus();
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x00020908 File Offset: 0x0001EB08
		private void SetExpressValueByBtn(string btnKey)
		{
			Control control = base.View.GetControl("FExpress");
			if (!control.Enabled)
			{
				return;
			}
			object value = base.View.Model.GetValue("FExpressDesc");
			string arg = (value == null) ? string.Empty : value.ToString();
			string text = base.View.GetControl(btnKey).Text;
			base.View.Model.SetValue("FExpressDesc", string.Format("{0} {1}", arg, text));
			base.View.GetControl("FExpressDesc").SetFocus();
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x000209A0 File Offset: 0x0001EBA0
		private bool CheckEditable()
		{
			object value = base.View.Model.GetValue("FDocumentStatus");
			return !"B".Equals(value.ToString()) && !"C".Equals(value.ToString());
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x000209EC File Offset: 0x0001EBEC
		private bool IsNumberData(string strValue)
		{
			string pattern = "^(-?\\d+)(\\.\\d+)?$";
			if (Regex.IsMatch(strValue, pattern))
			{
				double num = 1.0;
				return double.TryParse(strValue, out num);
			}
			return false;
		}

		// Token: 0x04000151 RID: 337
		private List<FormulaElement> nodeElementList = new List<FormulaElement>();

		// Token: 0x04000152 RID: 338
		private List<string> typeNodeIdList = new List<string>();

		// Token: 0x04000153 RID: 339
		private string activityTypeName = ResManager.LoadKDString("基本活动", "015072000013785", 7, new object[0]);

		// Token: 0x04000154 RID: 340
		private char[] operChar = new char[]
		{
			'+',
			'-',
			'*',
			'/',
			'(',
			')',
			','
		};
	}
}
