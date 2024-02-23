using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200005A RID: 90
	[Description("表单元素选择器 - 插件")]
	public class BillFieldSelectorEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000033 RID: 51
		// (get) Token: 0x0600069E RID: 1694 RVA: 0x0004E218 File Offset: 0x0004C418
		private string FormId
		{
			get
			{
				string result;
				try
				{
					result = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FormId"));
				}
				catch
				{
					result = "";
				}
				return result;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x0600069F RID: 1695 RVA: 0x0004E25C File Offset: 0x0004C45C
		private int SelMode
		{
			get
			{
				int result;
				try
				{
					result = (int)Convert.ToInt16(this.View.OpenParameter.GetCustomParameter("SelMode"));
				}
				catch
				{
					result = 0;
				}
				return result;
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060006A0 RID: 1696 RVA: 0x0004E29C File Offset: 0x0004C49C
		private bool isFlexiblePL
		{
			get
			{
				bool result;
				try
				{
					result = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("IsFlexiblePL"));
				}
				catch (Exception)
				{
					result = true;
				}
				return result;
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060006A1 RID: 1697 RVA: 0x0004E2DC File Offset: 0x0004C4DC
		private FormMetadata FormMetadata
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(this.FormId))
				{
					return MetaDataServiceHelper.Load(base.Context, this.FormId, true) as FormMetadata;
				}
				return null;
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060006A2 RID: 1698 RVA: 0x0004E304 File Offset: 0x0004C504
		private bool isExpandBaseData
		{
			get
			{
				bool result;
				try
				{
					if (!this.View.OpenParameter.GetCustomParameters().ContainsKey("isExpandBaseData"))
					{
						result = true;
					}
					else
					{
						result = Convert.ToBoolean(this.View.OpenParameter.GetCustomParameter("isExpandBaseData"));
					}
				}
				catch (Exception)
				{
					result = true;
				}
				return result;
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060006A3 RID: 1699 RVA: 0x0004E364 File Offset: 0x0004C564
		// (set) Token: 0x060006A4 RID: 1700 RVA: 0x0004E36C File Offset: 0x0004C56C
		private TreeNode SelectedNode { get; set; }

		// Token: 0x060006A5 RID: 1701 RVA: 0x0004E378 File Offset: 0x0004C578
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			TreeNode treeNode = this.View.OpenParameter.GetCustomParameter("TopNode") as TreeNode;
			if (treeNode != null)
			{
				this.topNode = treeNode;
			}
			Dictionary<string, string[]> dictionary = this.View.OpenParameter.GetCustomParameter("MapOrmName") as Dictionary<string, string[]>;
			if (dictionary != null)
			{
				this.mapOrmName = dictionary;
			}
		}

		// Token: 0x060006A6 RID: 1702 RVA: 0x0004E3D6 File Offset: 0x0004C5D6
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			this.InitFieldTree(string.Empty);
		}

		// Token: 0x060006A7 RID: 1703 RVA: 0x0004E3EC File Offset: 0x0004C5EC
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbOK")
				{
					this.ReturnSelectElement();
					return;
				}
				if (!(barItemKey == "tbCancel"))
				{
					return;
				}
				this.View.Close();
			}
		}

		// Token: 0x060006A8 RID: 1704 RVA: 0x0004E438 File Offset: 0x0004C638
		public override void TreeNodeClick(TreeNodeArgs e)
		{
			base.TreeNodeClick(e);
			this.SelectedNode = new TreeNode
			{
				id = e.NodeId
			};
		}

		// Token: 0x060006A9 RID: 1705 RVA: 0x0004E468 File Offset: 0x0004C668
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			if (StringUtils.EqualsIgnoreCase(e.Key, "FSearch"))
			{
				string value = MFGBillUtil.GetValue<string>(this.View.Model, "FNodeName", -1, null, null);
				this.InitFieldTree(value);
			}
		}

		// Token: 0x060006AA RID: 1706 RVA: 0x0004E4B0 File Offset: 0x0004C6B0
		private void InitFieldTree(string filter)
		{
			if (this.FormMetadata == null)
			{
				return;
			}
			this.mapOrmName.Clear();
			this.topNode = this.CreateFieldTree(this.FormMetadata, filter);
			TreeView control = this.View.GetControl<TreeView>("FTreeView");
			control.SetRootNode(this.topNode);
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(filter))
			{
				control.InvokeControlMethod("ExpandTree", new object[0]);
				control.SetExpanded(true);
				return;
			}
			control.SetExpanded(false);
		}

		// Token: 0x060006AB RID: 1707 RVA: 0x0004E534 File Offset: 0x0004C734
		private TreeNode CreateFieldTree(FormMetadata formMetadata, string filter)
		{
			List<string> list = new List<string>
			{
				"FTotalRptQty",
				"FBoxNumber",
				"FMaterialId",
				"FMoNumber",
				"FMoRowNumber",
				"FLot",
				"FBillNo"
			};
			TreeNode treeNode = new TreeNode
			{
				id = formMetadata.BusinessInfo.GetForm().Id,
				text = formMetadata.BusinessInfo.GetForm().Name
			};
			string[] array = new string[]
			{
				formMetadata.BusinessInfo.GetForm().Id,
				formMetadata.BusinessInfo.GetForm().Name
			};
			this.mapOrmName.ContainsKey(formMetadata.BusinessInfo.GetForm().Id);
			foreach (Entity entity in formMetadata.BusinessInfo.Entrys)
			{
				if (!entity.EntryName.Contains("_Link"))
				{
					string text = string.Empty;
					string[] array2 = new string[2];
					TreeNode treeNode2;
					if (entity is SubEntryEntity)
					{
						SubEntryEntity subEntryEntity = entity as SubEntryEntity;
						text = subEntryEntity.ParentEntity.Key + "." + entity.Key;
						array2[0] = subEntryEntity.ParentEntity.EntryName + "." + entity.EntryName;
						array2[1] = entity.Name.ToString();
						treeNode2 = new TreeNode
						{
							id = subEntryEntity.ParentEntity.Key + "." + entity.Key,
							text = string.Format("{0}.{1}", entity.Key, entity.Name)
						};
					}
					else
					{
						text = entity.Key;
						array2[0] = entity.EntryName;
						array2[1] = entity.Name.ToString();
						treeNode2 = new TreeNode
						{
							id = entity.Key,
							text = string.Format("{0}.{1}", entity.Key, entity.Name)
						};
					}
					treeNode.children.Add(treeNode2);
					this.mapOrmName.ContainsKey(text);
					foreach (Field field in from o in entity.Fields
					orderby o.FieldName
					select o)
					{
						if ((!this.FormId.Equals("SFC_OperationReport") || list.Contains(field.Key) || !this.isFlexiblePL) && !(field is ProxyField) && !(field is BaseDataPropertyField) && !(field is BasePropertyField))
						{
							string text2 = string.Empty;
							string[] array3 = new string[2];
							if (entity is HeadEntity)
							{
								text2 = field.Key;
								array3[0] = field.PropertyName;
								array3[1] = field.Name.ToString();
							}
							else
							{
								text2 = text + "." + field.Key;
								array3[0] = array2[0] + "." + field.PropertyName;
								array3[1] = array2[1] + "." + field.Name.ToString();
							}
							TreeNode treeNode3 = new TreeNode
							{
								id = text2,
								text = string.Format("{0}.{1}", field.Key, field.Name)
							};
							if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(filter))
							{
								treeNode2.children.Add(treeNode3);
								if (!this.mapOrmName.ContainsKey(text2))
								{
									this.mapOrmName.Add(text2, array3);
								}
								if (field is BaseDataField && this.isExpandBaseData)
								{
									this.ExpandBaseData(treeNode3, field as BaseDataField, text2, array3);
								}
							}
							else if (field.Key.Contains(filter) || field.Name.ToString().Contains(filter) || StringUtils.EqualsIgnoreCase(field.Key, filter) || StringUtils.EqualsIgnoreCase(field.Name.ToString(), filter))
							{
								treeNode2.children.Add(treeNode3);
								if (!this.mapOrmName.ContainsKey(text2))
								{
									this.mapOrmName.Add(text2, array3);
								}
								if (field is BaseDataField && this.isExpandBaseData)
								{
									this.ExpandBaseData(treeNode3, field as BaseDataField, text2, array3);
								}
							}
						}
					}
				}
			}
			return treeNode;
		}

		// Token: 0x060006AC RID: 1708 RVA: 0x0004EA44 File Offset: 0x0004CC44
		private void ExpandBaseData(TreeNode node, BaseDataField field, string key, string[] value)
		{
			List<string> list = new List<string>
			{
				"FNumber",
				"FName",
				"FSpecification"
			};
			if (field.LookUpObject != null && !string.IsNullOrEmpty(field.LookUpObject.FormId))
			{
				string formId = field.LookUpObject.FormId;
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, formId, true) as FormMetadata;
				if (node.children.Count == 0)
				{
					foreach (Entity entity in formMetadata.BusinessInfo.Entrys)
					{
						if (entity is HeadEntity || entity is SubHeadEntity)
						{
							string text = key + "." + entity.Key;
							string[] array = new string[]
							{
								value[0] + "." + entity.EntryName,
								value[1] + "." + entity.Name.ToString()
							};
							TreeNode treeNode = new TreeNode
							{
								id = text,
								text = string.Format("{0}.{1}", entity.Key, entity.Name)
							};
							node.children.Add(treeNode);
							foreach (Field field2 in from o in entity.Fields
							orderby o.FieldName
							select o)
							{
								if ((!this.FormId.Equals("SFC_OperationReport") || !formMetadata.Id.Equals("BD_MATERIAL") || list.Contains(field2.Key) || !this.isFlexiblePL) && (!this.FormId.Equals("SFC_OperationReport") || !formMetadata.Id.Equals("BD_BatchMainFile") || field2.Key.Equals("FNumber") || !this.isFlexiblePL) && !(field2 is ProxyField) && !(field2 is BaseDataPropertyField) && !(field2 is BasePropertyField))
								{
									string text2 = string.Empty;
									string[] array2 = new string[2];
									if (entity is HeadEntity)
									{
										text2 = key + "." + field2.Key;
										array2[0] = value[0] + "." + field2.PropertyName;
										array2[1] = value[1] + "." + field2.Name.ToString();
									}
									else
									{
										text2 = text + "." + field2.Key;
										array2[0] = array[0] + "." + field2.PropertyName;
										array2[1] = array[1] + "." + field2.Name.ToString();
									}
									treeNode.children.Add(new TreeNode
									{
										id = text2,
										text = string.Format("{0}.{1}", field2.Key, field2.Name)
									});
									if (!this.mapOrmName.ContainsKey(text2))
									{
										this.mapOrmName.Add(text2, array2);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060006AD RID: 1709 RVA: 0x0004EDE4 File Offset: 0x0004CFE4
		private void ReturnSelectElement()
		{
			if (this.SelectedNode == null)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选择一个元素!", "015065000002528", 7, new object[0]), 0);
				return;
			}
			if (!this.mapOrmName.ContainsKey(this.SelectedNode.id))
			{
				this.View.ShowMessage(ResManager.LoadKDString("此元素不能选择，请选择具体的属性！", "015072000021494", 7, new object[0]), 0);
				return;
			}
			string[] array = new string[2];
			this.mapOrmName.TryGetValue(this.SelectedNode.id, out array);
			object[] array2 = new object[]
			{
				array[0],
				array[1],
				this.topNode,
				this.mapOrmName,
				this.SelectedNode.id
			};
			this.View.ReturnToParentWindow(array2);
			this.View.Close();
		}

		// Token: 0x040002F1 RID: 753
		private const string CST_tbOK = "tbOK";

		// Token: 0x040002F2 RID: 754
		private const string CST_tbCancel = "tbCancel";

		// Token: 0x040002F3 RID: 755
		private Dictionary<string, string[]> mapOrmName = new Dictionary<string, string[]>();

		// Token: 0x040002F4 RID: 756
		private TreeNode topNode;
	}
}
