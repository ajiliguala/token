using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.Base.Object;
using Kingdee.BOS.Core.Base.Validation;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Metadata.FormValidationElement;
using Kingdee.BOS.Core.Metadata.PreInsertData.DataType;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000A2 RID: 162
	[Description("物料禁用引用单据查询")]
	public class MaterialForbidQuery : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000B5A RID: 2906 RVA: 0x00083958 File Offset: 0x00081B58
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.View.GetControl("FMaterialID").SetCustomPropertyValue("ToolTip", ResManager.LoadKDString("为空时查询所有已禁用的物料", "015072000036592", 7, new object[0]));
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(this.View.Model.DataObject, "MulUseOrg", null);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicValue))
			{
				this.View.Model.SetValue("FMulUseOrg", this.View.Context.CurrentOrganizationInfo.ID);
			}
		}

		// Token: 0x06000B5B RID: 2907 RVA: 0x000839F0 File Offset: 0x00081BF0
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbCheck"))
				{
					if (!(barItemKey == "tbExit"))
					{
						return;
					}
					this.View.Close();
				}
				else
				{
					string empty = string.Empty;
					if (!this.VarifyBillHeadData(out empty))
					{
						this.View.ShowErrMessage(empty, "", 0);
						e.Cancel = true;
						return;
					}
					this.FillEntitys();
					return;
				}
			}
		}

		// Token: 0x06000B5C RID: 2908 RVA: 0x00083A68 File Offset: 0x00081C68
		public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
		{
			base.AuthPermissionBeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FMaterialID"))
				{
					return;
				}
				e.IsIsolationOrg = false;
			}
		}

		// Token: 0x06000B5D RID: 2909 RVA: 0x00083AAC File Offset: 0x00081CAC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (fieldKey == "FMaterialID")
				{
					string text = e.ListFilterParameter.Filter;
					DynamicObjectCollection value = MFGBillUtil.GetValue<DynamicObjectCollection>(this.View.Model, "FMulUseOrg", -1, null, null);
					if (!ListUtils.IsEmpty<DynamicObject>(value))
					{
						string arg = string.Join<long>(",", (from s in value
						select DataEntityExtend.GetDynamicValue<long>(s, "MulUseOrg_Id", 0L)).Distinct<long>().ToList<long>());
						if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
						{
							text += string.Format("AND FUSEORGID IN ({0}) ", arg);
						}
						else
						{
							text += string.Format("FUSEORGID IN ({0}) ", arg);
						}
					}
					else
					{
						text += "FUSEORGID = 0 ";
					}
					e.ListFilterParameter.Filter = text;
					e.IsShowUsed = false;
					return;
				}
				if (!(fieldKey == "FMulUseOrg"))
				{
					return;
				}
				bool flag = MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context);
				if (flag)
				{
					e.IsShowUsed = false;
				}
			}
		}

		// Token: 0x06000B5E RID: 2910 RVA: 0x00083BBC File Offset: 0x00081DBC
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (baseDataFieldKey == "FMaterialID")
				{
					e.IsShowUsed = false;
					return;
				}
				if (!(baseDataFieldKey == "FMulUseOrg"))
				{
					return;
				}
				bool flag = MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context);
				if (flag)
				{
					e.IsShowUsed = false;
				}
			}
		}

		// Token: 0x06000B5F RID: 2911 RVA: 0x00083C14 File Offset: 0x00081E14
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			base.EntryButtonCellClick(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FBillNumber") && StringUtils.EqualsIgnoreCase(e.FieldKey, "FBillNumberOther"))
			{
				return;
			}
			if (e.Row != -1)
			{
				string empty = string.Empty;
				string text = string.Empty;
				long value;
				if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FBillNumber"))
				{
					text = MFGBillUtil.GetValue<string>(this.View.Model, "FFormId", e.Row, null, null);
					value = MFGBillUtil.GetValue<long>(this.View.Model, "FBILLID", e.Row, 0L, null);
				}
				else
				{
					text = MFGBillUtil.GetValue<string>(this.View.Model, "FFormIdOther", e.Row, null, null);
					value = MFGBillUtil.GetValue<long>(this.View.Model, "FBillIdOther", e.Row, 0L, null);
				}
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
				{
					MFGBillUtil.ShowBillForm(this.View, text, value, 1);
				}
			}
		}

		// Token: 0x06000B60 RID: 2912 RVA: 0x00083D0C File Offset: 0x00081F0C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Id.ToUpperInvariant()) != null)
			{
				if (!(a == "ENTITYEXPORT"))
				{
					return;
				}
				if (!this.ValidatePermission(e))
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000B61 RID: 2913 RVA: 0x00083D50 File Offset: 0x00081F50
		private bool VarifyBillHeadData(out string errorMsg)
		{
			bool result = true;
			StringBuilder stringBuilder = new StringBuilder();
			DynamicObjectCollection value = MFGBillUtil.GetValue<DynamicObjectCollection>(this.Model, "FMulUseOrg", -1, null, null);
			if (ListUtils.IsEmpty<DynamicObject>(value))
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("请先选择使用组织！", "015072000036593", 7, new object[0]));
				result = false;
			}
			errorMsg = stringBuilder.ToString();
			return result;
		}

		// Token: 0x06000B62 RID: 2914 RVA: 0x00083DAC File Offset: 0x00081FAC
		private bool ValidatePermission(BeforeDoOperationEventArgs e)
		{
			bool flag = MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BillBusinessInfo.GetForm().Id, e.Operation.FormOperation.PermissionItemId);
			if (!flag)
			{
				this.View.ShowMessage(string.Format(ResManager.LoadKDString("没有禁用子项查询的{0}权限！", "015072000036594", 7, new object[0]), e.Operation.FormOperation.OperationName[this.View.Context.UserLocale.LCID]), 0);
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000B63 RID: 2915 RVA: 0x00083E44 File Offset: 0x00082044
		private void FillEntitys1()
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntityBom");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			Entity entity2 = this.View.BusinessInfo.GetEntity("FEntityOther");
			DynamicObjectCollection entityDataObject2 = this.View.Model.GetEntityDataObject(entity2);
			MaterialForbidQueryOption materialForbidQueryOption = new MaterialForbidQueryOption();
			materialForbidQueryOption.UseOrgId = MFGBillUtil.GetValue<long>(this.View.Model, "FUseOrgId", -1, 0L, null);
			materialForbidQueryOption.MaterialIdLst = this.SplitBaseFilter("FMaterialID");
			materialForbidQueryOption.QueryBusinessInfo = this.View.BusinessInfo;
			this.View.Model.BeginIniti();
			entityDataObject.Clear();
			entityDataObject2.Clear();
			string text = string.Empty;
			DynamicObjectCollection materialInfoByOrgAndMaterialId = MaterialForbidQueryServiceHelper.GetMaterialInfoByOrgAndMaterialId(base.Context, materialForbidQueryOption);
			if (ListUtils.IsEmpty<DynamicObject>(materialInfoByOrgAndMaterialId))
			{
				text = ResManager.LoadKDString("未查到相关物料信息", "015072000036595", 7, new object[0]);
			}
			List<object> list = new List<object>();
			foreach (DynamicObject dynamicObject in materialInfoByOrgAndMaterialId)
			{
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject))
				{
					list.Add(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FMATERIALID", 0L));
				}
			}
			if (ListUtils.IsEmpty<object>(list))
			{
				text = ResManager.LoadKDString("未查到相关物料信息", "015072000036595", 7, new object[0]);
			}
			else
			{
				List<BaseDataRefResult> list2 = new List<BaseDataRefResult>();
				list2 = this.CheckIsRefByOtherForm(list.ToArray());
				if (ListUtils.IsEmpty<BaseDataRefResult>(list2))
				{
					text = ResManager.LoadKDString("所查询物料未找到引用信息", "015072000036590", 7, new object[0]);
				}
				else
				{
					Dictionary<string, List<DynamicObject>> dictionary = new Dictionary<string, List<DynamicObject>>();
					try
					{
						dictionary = MaterialForbidQueryServiceHelper.BuildEntity(base.Context, list2, materialForbidQueryOption);
					}
					catch (Exception ex)
					{
						text = ex.Message;
						if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
						{
							this.View.ShowMessage(text, 0);
						}
						this.View.Model.EndIniti();
						this.View.UpdateView("FEntityBom");
						this.View.UpdateView("FEntityOther");
						return;
					}
					finally
					{
						if (ListUtils.IsEmpty<KeyValuePair<string, List<DynamicObject>>>(dictionary))
						{
							text = ResManager.LoadKDString("未查到相关数据", "015072000036596", 7, new object[0]);
						}
						else
						{
							foreach (DynamicObject item in dictionary["B"])
							{
								entityDataObject.Add(item);
							}
							foreach (DynamicObject item2 in dictionary["O"])
							{
								entityDataObject2.Add(item2);
							}
						}
					}
				}
			}
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
			{
				this.View.ShowMessage(text, 0);
			}
			this.View.Model.EndIniti();
			this.View.UpdateView("FEntityBom");
			this.View.UpdateView("FEntityOther");
		}

		// Token: 0x06000B64 RID: 2916 RVA: 0x00084310 File Offset: 0x00082510
		private void FillEntitys()
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntityBom");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			Entity entity2 = this.View.BusinessInfo.GetEntity("FEntityOther");
			DynamicObjectCollection entityDataObject2 = this.View.Model.GetEntityDataObject(entity2);
			Entity entity3 = this.View.BusinessInfo.GetEntity("FEntityInventory");
			DynamicObjectCollection entityDataObject3 = this.View.Model.GetEntityDataObject(entity3);
			MaterialForbidQueryOption materialForbidQueryOption = new MaterialForbidQueryOption();
			DynamicObjectCollection value = MFGBillUtil.GetValue<DynamicObjectCollection>(this.View.Model, "FMulUseOrg", -1, null, null);
			materialForbidQueryOption.UseOrgIdList = (from s in value
			select DataEntityExtend.GetDynamicValue<long>(s, "MulUseOrg_Id", 0L)).Distinct<long>().ToList<long>();
			materialForbidQueryOption.MaterialIdLst = this.SplitBaseFilter("FMaterialID");
			materialForbidQueryOption.QueryBusinessInfo = this.View.BusinessInfo;
			string text = string.Empty;
			List<DynamicObject> list = new List<DynamicObject>();
			List<DynamicObject> list2 = new List<DynamicObject>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			entityDataObject.Clear();
			entityDataObject2.Clear();
			entityDataObject3.Clear();
			DynamicObjectCollection materialInfoByOrgAndMaterialId = MaterialForbidQueryServiceHelper.GetMaterialInfoByOrgAndMaterialId(base.Context, materialForbidQueryOption);
			if (ListUtils.IsEmpty<DynamicObject>(materialInfoByOrgAndMaterialId))
			{
				text = ResManager.LoadKDString("未查到相关物料信息", "015072000036595", 7, new object[0]);
			}
			else
			{
				List<object> list4 = new List<object>();
				foreach (DynamicObject dynamicObject in materialInfoByOrgAndMaterialId)
				{
					if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject))
					{
						list4.Add(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FMATERIALID", 0L));
					}
				}
				List<BaseDataRefResult> list5 = new List<BaseDataRefResult>();
				List<BaseDataRefResult> list6 = new List<BaseDataRefResult>();
				List<BaseDataRefResult> list7 = new List<BaseDataRefResult>();
				List<BaseDataRefResult> list8 = new List<BaseDataRefResult>();
				list5 = this.CheckIsRefByOtherForm(list4.ToArray());
				if (ListUtils.IsEmpty<BaseDataRefResult>(list5))
				{
					text = ResManager.LoadKDString("所查询物料未找到引用信息", "015072000036590", 7, new object[0]);
				}
				else
				{
					foreach (BaseDataRefResult baseDataRefResult in list5)
					{
						if (StringUtils.EqualsIgnoreCase(baseDataRefResult.BillNo, "T_PRD_PPBOMENTRY") || StringUtils.EqualsIgnoreCase(baseDataRefResult.BillNo, "T_PLN_PLBOMENTRY") || StringUtils.EqualsIgnoreCase(baseDataRefResult.BillNo, "T_SUB_PPBOMENTRY") || StringUtils.EqualsIgnoreCase(baseDataRefResult.FormId, "ENG_BOM"))
						{
							list6.Add(baseDataRefResult);
						}
						else if (StringUtils.EqualsIgnoreCase(baseDataRefResult.BillNo, "T_STK_INVENTORY"))
						{
							list7.Add(baseDataRefResult);
						}
						else
						{
							list8.Add(baseDataRefResult);
						}
					}
					LinqUtil.DistinctBy(list6, (BaseDataRefResult m) => new
					{
						Id = m.Id.ToString()
					}).ToList<BaseDataRefResult>();
					LinqUtil.DistinctBy(list8, (BaseDataRefResult m) => new
					{
						Id = m.Id.ToString()
					}).ToList<BaseDataRefResult>();
					IOperationResult operationResult = MaterialForbidQueryServiceHelper.BuildTabsEntity(base.Context, list6, materialForbidQueryOption, 1);
					IOperationResult operationResult2 = MaterialForbidQueryServiceHelper.BuildTabsEntity(base.Context, list8, materialForbidQueryOption, 2);
					IOperationResult operationResult3 = MaterialForbidQueryServiceHelper.BuildTabsEntity(base.Context, list7, materialForbidQueryOption, 3);
					list = (List<DynamicObject>)operationResult.FuncResult;
					list2 = (List<DynamicObject>)operationResult2.FuncResult;
					list3 = (List<DynamicObject>)operationResult3.FuncResult;
					if ((!operationResult.IsSuccess || ListUtils.IsEmpty<DynamicObject>(list)) && (!operationResult2.IsSuccess || ListUtils.IsEmpty<DynamicObject>(list2)) && (!operationResult3.IsSuccess || ListUtils.IsEmpty<DynamicObject>(list3)))
					{
						text = ResManager.LoadKDString("未查询到数据", "015072000036591", 7, new object[0]);
					}
				}
			}
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
			{
				this.View.ShowMessage(text, 0);
				this.View.UpdateView("FEntityBom");
				this.View.UpdateView("FEntityOther");
				this.View.UpdateView("FEntityInventory");
				return;
			}
			this.View.Model.BeginIniti();
			if (!ListUtils.IsEmpty<DynamicObject>(list))
			{
				List<DynamicObject> list9 = (from o in list
				orderby DataEntityExtend.GetDynamicObjectItemValue<long>(o, "MaterialNumber_Id", 0L), Convert.ToString(o["BillType"]), DataEntityExtend.GetDynamicObjectItemValue<string>(o, "BillNumber", null), DataEntityExtend.GetDynamicObjectItemValue<int>(o, "RowSeq", 0)
				select o).ToList<DynamicObject>();
				foreach (DynamicObject item in list9)
				{
					entityDataObject.Add(item);
				}
			}
			if (!ListUtils.IsEmpty<DynamicObject>(list2))
			{
				List<DynamicObject> list10 = (from o in list2
				orderby DataEntityExtend.GetDynamicObjectItemValue<long>(o, "MaterialNumberOther_Id", 0L), Convert.ToString(o["BillTypeOther"]), DataEntityExtend.GetDynamicObjectItemValue<string>(o, "BillNumberOther", null), DataEntityExtend.GetDynamicObjectItemValue<int>(o, "RowSeqOther", 0)
				select o).ToList<DynamicObject>();
				foreach (DynamicObject item2 in list10)
				{
					entityDataObject2.Add(item2);
				}
			}
			if (!ListUtils.IsEmpty<DynamicObject>(list3))
			{
				List<DynamicObject> list11 = (from o in list3
				orderby DataEntityExtend.GetDynamicObjectItemValue<long>(o, "MaterialNumberInventory_Id", 0L)
				select o).ToList<DynamicObject>();
				foreach (DynamicObject item3 in list11)
				{
					entityDataObject3.Add(item3);
				}
			}
			this.View.Model.EndIniti();
			this.View.UpdateView("FEntityBom");
			this.View.UpdateView("FEntityOther");
			this.View.UpdateView("FEntityInventory");
		}

		// Token: 0x06000B65 RID: 2917 RVA: 0x00084A04 File Offset: 0x00082C04
		private List<BaseDataRefResult> CheckIsRefByOtherForm(object[] materialId)
		{
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "BD_MATERIAL");
			List<BaseDataRefResult> list = new List<BaseDataRefResult>();
			List<BaseDataRefItem> list2 = new List<BaseDataRefItem>();
			List<ObjectTypeRef> list3 = null;
			List<BaseDataRefItem> list4 = new List<BaseDataRefItem>();
			FormOperation formOperation = (from v in formMetaData.BusinessInfo.GetForm().FormOperations
			where StringUtils.EqualsIgnoreCase(v.Operation, "Forbid")
			select v).FirstOrDefault<FormOperation>();
			AbstractValidation abstractValidation = formOperation.Validations.FirstOrDefault((AbstractValidation v) => v.GetType() == typeof(BaseDataRefValidator));
			if (abstractValidation != null)
			{
				list3 = new List<ObjectTypeRef>();
				list3 = (abstractValidation as BaseDataRefValidator).ExceptRefItem;
			}
			if (list3 == null)
			{
				return null;
			}
			list4 = MFGServiceHelperForBD.GetBaseDataRefItem(base.Context, "BD_MATERIAL", list3);
			if (list4 == null)
			{
				return null;
			}
			using (List<ObjectTypeRef>.Enumerator enumerator = list3.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ObjectTypeRef item = enumerator.Current;
					BaseDataRefItem baseDataRefItem = (from w in list4
					where StringUtils.EqualsIgnoreCase(w.FTABLENAME, item.TableName)
					select w).FirstOrDefault<BaseDataRefItem>();
					if (baseDataRefItem != null)
					{
						list2.Add(baseDataRefItem);
					}
				}
			}
			if (ListUtils.IsEmpty<BaseDataRefItem>(list2))
			{
				return null;
			}
			return MaterialForbidQueryServiceHelper.GetExitsDataRefResults(base.Context, "BD_MATERIAL", list2, materialId);
		}

		// Token: 0x06000B66 RID: 2918 RVA: 0x00084B84 File Offset: 0x00082D84
		private List<long> SplitBaseFilter(string filedKey)
		{
			List<long> list = new List<long>();
			DynamicObjectCollection value = MFGBillUtil.GetValue<DynamicObjectCollection>(this.View.Model, filedKey, -1, null, null);
			if (ListUtils.IsEmpty<DynamicObject>(value))
			{
				return null;
			}
			list.AddRange((from s in value
			select DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(s, "MaterialID", null), "MsterId", 0L)).Distinct<long>());
			return list;
		}
	}
}
