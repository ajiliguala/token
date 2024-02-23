using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.MaterialTree;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000A3 RID: 163
	[Description("动态表单-替代方案")]
	public class MaterialSubStituteList : AbstractDynamicFormPlugIn
	{
		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000B78 RID: 2936 RVA: 0x00084BED File Offset: 0x00082DED
		protected string MaterialTree_PageId
		{
			get
			{
				return string.Format("{0}_MaterialTree", this.View.PageId);
			}
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000B79 RID: 2937 RVA: 0x00084C04 File Offset: 0x00082E04
		// (set) Token: 0x06000B7A RID: 2938 RVA: 0x00084C0C File Offset: 0x00082E0C
		public DynamicObjectCollection SubStituteMaterialDatas { get; set; }

		// Token: 0x06000B7B RID: 2939 RVA: 0x00084C15 File Offset: 0x00082E15
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.FillMtrlTree();
		}

		// Token: 0x06000B7C RID: 2940 RVA: 0x00084C24 File Offset: 0x00082E24
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (a == "TBREFRESH")
				{
					this.ClearEntityAndReloadTree();
					return;
				}
				if (a == "TBCLOSE")
				{
					this.View.Close();
					return;
				}
				if (!(a == "TBNEW"))
				{
					if (!(a == "TBMODIFY"))
					{
						if (!(a == "TBDELETE"))
						{
							return;
						}
						if (this.VaildatePermission("24f64c0dbfa945f78a6be123197a63f5") && this.MainItemsSelectRowBillObject != null)
						{
							this.DeleteBillByFid();
						}
					}
					else if (this.MainItemsSelectRowBillObject != null)
					{
						if (this.VaildatePermission("f323992d896745fbaab4a2717c79ce2e"))
						{
							this.ShowSubStituteForm(2);
							return;
						}
						this.ShowSubStituteForm(1);
						return;
					}
				}
				else if (this.VaildatePermission("fce8b1aca2144beeb3c6655eaf78bc34"))
				{
					this.ShowSubStituteForm(0);
					return;
				}
			}
		}

		// Token: 0x06000B7D RID: 2941 RVA: 0x00084D94 File Offset: 0x00082F94
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (e.Key == "MFG_MaterialTree" && e.EventName == "TreeNodeClick")
			{
				List<DynamicObject> chooseMaterial = new List<DynamicObject>();
				string eventArgs = e.EventArgs;
				if (eventArgs.Contains("_"))
				{
					int num = eventArgs.IndexOf("_");
					eventArgs.Substring(0, num);
					eventArgs.Substring(num + 1, eventArgs.Length - num - 1);
				}
				else if (eventArgs.Contains("m"))
				{
					string mtrlId = eventArgs.Substring(1, eventArgs.Length - 1);
					string UseOrgId = (from data in this.SubStituteMaterialDatas
					where data["MaterialID_Id"].ToString().Equals(mtrlId)
					select data["UseOrgId_Id"]).Distinct<object>().FirstOrDefault<object>().ToString();
					List<string> listPKid = (from data in this.SubStituteMaterialDatas
					where data["MaterialID_Id"].ToString().Equals(mtrlId)
					select data["BillPKId"].ToString()).ToList<string>();
					chooseMaterial = (from data in this.SubStituteMaterialDatas
					where listPKid.Contains(data["BillPKId"].ToString()) && data["UseOrgId_Id"].ToString().Equals(UseOrgId)
					select data).Distinct<DynamicObject>().ToList<DynamicObject>();
				}
				this.FillMainDataByClick(chooseMaterial);
			}
		}

		// Token: 0x06000B7E RID: 2942 RVA: 0x00084F00 File Offset: 0x00083100
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			if (e.Key == "FEntityMainItems".ToUpper())
			{
				DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["EntityMainItems"] as DynamicObjectCollection;
				if (dynamicObjectCollection.FirstOrDefault<DynamicObject>() != null && dynamicObjectCollection.Count > 0 && e.Row >= 0)
				{
					DynamicObject dynamicObject = dynamicObjectCollection[e.Row];
					Convert.ToInt64(dynamicObject["MaterialID_Id"]);
					string text = dynamicObject["BillPKId"].ToString();
					this.MainItemsSelectRowBillObject = SubStituteServiceHelper.GetSubStituteByFid(base.Context, text);
					DynamicObjectCollection subItemMaterialListData = SubStituteServiceHelper.GetSubItemMaterialListData(base.Context, text);
					this.FillItemByClick(subItemMaterialListData, "FEntity");
				}
			}
		}

		// Token: 0x06000B7F RID: 2943 RVA: 0x00084FC2 File Offset: 0x000831C2
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
			if (this.VaildatePermission("f323992d896745fbaab4a2717c79ce2e") && this.MainItemsSelectRowBillObject != null)
			{
				this.ShowSubStituteForm(2);
			}
		}

		// Token: 0x06000B80 RID: 2944 RVA: 0x00084FE8 File Offset: 0x000831E8
		private bool VaildatePermission(string strPerItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = "ENG_Substitution",
				SubSystemId = this.View.Model.SubSytemId
			}, strPerItemId);
			if (!permissionAuthResult.Passed && strPerItemId != null)
			{
				if (!(strPerItemId == "fce8b1aca2144beeb3c6655eaf78bc34"))
				{
					if (strPerItemId == "24f64c0dbfa945f78a6be123197a63f5")
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("没有替代方案维护界面的删除权限！", "015072000002213", 7, new object[0]), "", 0);
					}
				}
				else
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("没有替代方案维护界面的新增权限！", "015072000002212", 7, new object[0]), "", 0);
				}
			}
			return permissionAuthResult.Passed;
		}

		// Token: 0x06000B81 RID: 2945 RVA: 0x000850A9 File Offset: 0x000832A9
		private void ShowSubStituteForm(OperationStatus status = 1)
		{
			if (status != null)
			{
				this.PrepareAndShow(status, this.MainItemsSelectRowBillObject);
				return;
			}
			this.PrepareAndShow(status, null);
		}

		// Token: 0x06000B82 RID: 2946 RVA: 0x00085108 File Offset: 0x00083308
		protected void FillMtrlTree()
		{
			this.MainItemsSelectRowBillObject = null;
			this.SubStituteMaterialDatas = this.GetSubStituteMaterialData();
			IDList idlist = new IDList();
			IOrderedEnumerable<DynamicObject> orderedEnumerable = from o in this.SubStituteMaterialDatas
			orderby DataEntityExtend.GetDynamicObjectItemValue<string>(o, "BillPKId", "0")
			select o;
			foreach (DynamicObject dynamicObject in orderedEnumerable)
			{
				idlist.Add(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "BillPKId", "0"), "ENG_Substitution");
			}
			this.View.Session["FormInputParam"] = idlist;
			List<object> showMaterials = new List<object>();
			List<long> userOrgIds = this.GetUserOrgIds();
			showMaterials = (from i in this.SubStituteMaterialDatas
			where userOrgIds.Contains(DataEntityExtend.GetDynamicValue<long>(i, "UseOrgId_Id", 0L))
			select DataEntityExtend.GetDynamicValue<object>(i, "MaterialID_Id", null)).Distinct<object>().ToList<object>();
			this.FillMaterialTree(showMaterials);
		}

		// Token: 0x06000B83 RID: 2947 RVA: 0x00085238 File Offset: 0x00083438
		private List<long> GetUserOrgIds()
		{
			List<long> result = new List<long>();
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.FormId = "SEC_User";
			queryBuilderParemeter.SelectItems = SelectorItemInfo.CreateItems("FOrgOrgId");
			queryBuilderParemeter.FilterClauseWihtKey = string.Format("FUserID={0}", base.Context.UserId);
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				return (from i in dynamicObjectCollection
				select DataEntityExtend.GetDynamicValue<long>(i, "FOrgOrgId", 0L)).ToList<long>();
			}
			return result;
		}

		// Token: 0x06000B84 RID: 2948 RVA: 0x000852CC File Offset: 0x000834CC
		protected void FillMaterialTree(List<object> showMaterials)
		{
			TreeParameters treeParameters = new TreeParameters();
			treeParameters.IsShowOrg = true;
			treeParameters.ShowMtrlLevel = 2;
			this.View.Session["1"] = treeParameters;
			this.View.Session["2"] = showMaterials;
			IDynamicFormView view = this.View.GetView(this.MaterialTree_PageId);
			if (view == null)
			{
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.OpenStyle.ShowType = 3;
				dynamicFormShowParameter.OpenStyle.TagetKey = "FPanelMtrlTree";
				dynamicFormShowParameter.FormId = "MFG_MaterialTree";
				dynamicFormShowParameter.PageId = this.MaterialTree_PageId;
				dynamicFormShowParameter.CustomParams.Add("ShowParam", "1");
				dynamicFormShowParameter.CustomParams.Add("ShowObject", "2");
				this.View.ShowForm(dynamicFormShowParameter);
				return;
			}
			view.OpenParameter.SetCustomParameter("ShowParam", "1");
			view.OpenParameter.SetCustomParameter("ShowObject", "2");
			view.Refresh();
			this.View.SendDynamicFormAction(view);
		}

		// Token: 0x06000B85 RID: 2949 RVA: 0x000853EC File Offset: 0x000835EC
		protected void PrepareAndShow(OperationStatus status = 1, DynamicObject SelectRowBillFIDlist = null)
		{
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.OpenStyle.ShowType = 6;
			billShowParameter.FormId = "ENG_Substitution";
			billShowParameter.ParentPageId = this.View.PageId;
			billShowParameter.MultiSelect = false;
			if (SelectRowBillFIDlist != null)
			{
				billShowParameter.PKey = SelectRowBillFIDlist["FID"].ToString();
			}
			billShowParameter.CustomParams["INVALIDTIP"] = ResManager.LoadKDString("替代方案", "015072000002214", 7, new object[0]);
			billShowParameter.CustomParams["DEFAULTEDITED"] = "true";
			if (!string.IsNullOrWhiteSpace(billShowParameter.PKey))
			{
				billShowParameter.Status = status;
			}
			this.ShowBillForm(this.View, billShowParameter, null, delegate(FormResult Result)
			{
				if (Result.ReturnData != null)
				{
					this.ClearEntityAndReloadTree();
				}
			});
		}

		// Token: 0x06000B86 RID: 2950 RVA: 0x000854B0 File Offset: 0x000836B0
		protected virtual DynamicObjectCollection GetSubStituteMaterialData()
		{
			return SubStituteServiceHelper.GetMainItemMaterialListData(base.Context);
		}

		// Token: 0x06000B87 RID: 2951 RVA: 0x000854C0 File Offset: 0x000836C0
		protected void FillMainDataByClick(List<DynamicObject> chooseMaterial)
		{
			this.Model.BeginIniti();
			this.ClearEntity("FEntityMainItems");
			this.ClearEntity("FEntity");
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FEntityMainItems");
			if (chooseMaterial != null && chooseMaterial.Count > 0)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				foreach (DynamicObject dynamicObject in chooseMaterial)
				{
					dynamicObject["Seq"] = chooseMaterial.IndexOf(dynamicObject) + 1;
					entityDataObject.Add(dynamicObject);
				}
			}
			this.View.Model.EndIniti();
			this.ChangeDataRowColor("FEntityMainItems", "#FFFFC5", -1);
			this.View.Model.EndIniti();
			this.View.UpdateView("FEntityMainItems");
		}

		// Token: 0x06000B88 RID: 2952 RVA: 0x000855B8 File Offset: 0x000837B8
		private void ChangeDataRowColor(string FEntityString, string Colorstring, int row = -1)
		{
			int i = row;
			int num = i + 1;
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			List<KeyValuePair<int, string>> list2 = new List<KeyValuePair<int, string>>();
			List<string> list3 = new List<string>
			{
				"FRePlace",
				"FIsApplicableBOM",
				"FMaterialID",
				"FMaterialName",
				"FMaterialModel",
				"FAuxPropID",
				"FMaterialProperty",
				"FUnitID",
				"FQty",
				"FIsKeyItem",
				"FMixed",
				"FDocumentStatus",
				"FForbidStatus",
				"FUseOrgId",
				"FBillPKId"
			};
			if (row == -1)
			{
				i = 0;
				num = this.Model.GetEntryRowCount(FEntityString);
			}
			while (i < num)
			{
				string text = MFGBillUtil.GetValue<string>(this.Model, "FDocumentStatus", i, null, null).ToString();
				string text2 = MFGBillUtil.GetValue<string>(this.Model, "FForbidStatus", i, null, null).ToString();
				if (!text2.Equals("B"))
				{
					if (!text.Equals("C"))
					{
						list.Add(new KeyValuePair<int, string>(i, Colorstring));
					}
				}
				else
				{
					list2.Add(new KeyValuePair<int, string>(i, "#FF0000"));
				}
				i++;
			}
			this.View.GetControl<EntryGrid>(FEntityString).SetRowBackcolor(list);
			foreach (string text3 in list3)
			{
				this.View.GetControl<EntryGrid>(FEntityString).SetCellsForecolor(text3, list2);
			}
		}

		// Token: 0x06000B89 RID: 2953 RVA: 0x0008578C File Offset: 0x0008398C
		protected void FillItemByClick(DynamicObjectCollection chooseMaterial, string EntityKey_FEntity)
		{
			this.Model.BeginIniti();
			if (this.View.BusinessInfo.GetElement(EntityKey_FEntity) != null)
			{
				this.Model.DeleteEntryData(EntityKey_FEntity);
			}
			Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity(EntityKey_FEntity);
			if (chooseMaterial != null && chooseMaterial.Count > 0)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				foreach (DynamicObject dynamicObject in chooseMaterial)
				{
					dynamicObject["Seq"] = chooseMaterial.IndexOf(dynamicObject) + 1;
					entityDataObject.Add(dynamicObject);
				}
			}
			this.Model.EndIniti();
			this.View.UpdateView(EntityKey_FEntity);
		}

		// Token: 0x06000B8A RID: 2954 RVA: 0x00085898 File Offset: 0x00083A98
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

		// Token: 0x06000B8B RID: 2955 RVA: 0x00085926 File Offset: 0x00083B26
		private void ClearEntity(string EntityKey_FEntity)
		{
			if (this.View.BusinessInfo.GetElement(EntityKey_FEntity) != null)
			{
				this.Model.DeleteEntryData(EntityKey_FEntity);
			}
		}

		// Token: 0x06000B8C RID: 2956 RVA: 0x00085948 File Offset: 0x00083B48
		private void DeleteBillByFid()
		{
			string text = this.MainItemsSelectRowBillObject["FDocumentStatus"].ToString().ToUpperInvariant();
			string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(this.MainItemsSelectRowBillObject, "FNumber", null);
			string text2 = text.Equals("B") ? string.Format(ResManager.LoadKDString("替代方案{0}为审核中状态,不能删除！", "015072000002215", 7, new object[0]), dynamicObjectItemValue) : (text.Equals("C") ? string.Format(ResManager.LoadKDString("替代方案{0}为审核状态,不能删除！", "015072000002216", 7, new object[0]), dynamicObjectItemValue) : string.Empty);
			if (string.IsNullOrEmpty(text2))
			{
				this.DeletePrompt();
				return;
			}
			this.View.ShowErrMessage(text2, "", 0);
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x000859FF File Offset: 0x00083BFF
		private void ClearEntityAndReloadTree()
		{
			this.ClearEntity("FEntityMainItems");
			this.ClearEntity("FEntity");
			this.FillMtrlTree();
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x00085BC8 File Offset: 0x00083DC8
		private void DeletePrompt()
		{
			string text = ResManager.LoadKDString("您确定要删除此替代方案吗？", "015072000002218", 7, new object[0]);
			this.View.ShowMessage(text, 4, delegate(MessageBoxResult result)
			{
				if (result == 6)
				{
					string text2 = this.MainItemsSelectRowBillObject["FID"].ToString();
					string[] array = new string[]
					{
						text2
					};
					List<string> networkCtrl = SubStituteViewServiceHelper.GetNetworkCtrl(base.Context, array.ToList<string>(), "32497d8c-efb2-4258-a0dc-db629138ad08");
					if (networkCtrl != null && networkCtrl.Count > 0 && networkCtrl[0] == text2)
					{
						this.View.ShowMessage(string.Format(ResManager.LoadKDString("当前操作与替代方案{0}业务操作-“修改”冲突，请稍后再使用。", "015072000012582", 7, new object[0]), DataEntityExtend.GetDynamicObjectItemValue<string>(this.MainItemsSelectRowBillObject, "FNumber", null)), 0);
						return;
					}
					FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_Substitution");
					IOperationResult operationResult = BusinessDataServiceHelper.Delete(base.Context, formMetaData.BusinessInfo, array, null, "");
					if (operationResult.IsSuccess)
					{
						this.View.ShowMessage(ResManager.LoadKDString("删除成功！", "015072000002219", 7, new object[0]), 0);
						this.ClearEntityAndReloadTree();
						return;
					}
					if (!ListUtils.IsEmpty<ValidationErrorInfo>(operationResult.ValidationErrors))
					{
						StringBuilder stringBuilder = new StringBuilder();
						foreach (ValidationErrorInfo validationErrorInfo in operationResult.ValidationErrors)
						{
							stringBuilder.AppendLine(validationErrorInfo.Message);
						}
						this.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
						return;
					}
				}
				else
				{
					this.View.ShowMessage(ResManager.LoadKDString("删除失败！", "015072000001858", 7, new object[0]), 0);
				}
			}, "", 0);
		}

		// Token: 0x04000563 RID: 1379
		protected const string Key_Contain = "FPanelMtrlTree";

		// Token: 0x04000564 RID: 1380
		protected const string FormKey_MaterialTree = "MFG_MaterialTree";

		// Token: 0x04000565 RID: 1381
		protected DynamicObject MainItemsSelectRowBillObject;
	}
}
