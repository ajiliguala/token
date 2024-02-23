using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Common;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000A1 RID: 161
	[Description("可禁用物料分析")]
	public class MaterialForbiddenQuery : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000B4A RID: 2890 RVA: 0x0008266C File Offset: 0x0008086C
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.InitOrgId("FUseOrgId");
			List<EnumItem> comboItems = new List<EnumItem>();
			comboItems = MaterialDeleteQueryServiceHelper.GetMtrlGroupEnumList(base.Context);
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>("FMtrlGroup", 0);
			fieldEditor.SetComboItems(comboItems);
		}

		// Token: 0x06000B4B RID: 2891 RVA: 0x000826B8 File Offset: 0x000808B8
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			FieldEditor fieldEditor = this.View.GetFieldEditor("FLabel", -1);
			fieldEditor.Visible = false;
		}

		// Token: 0x06000B4C RID: 2892 RVA: 0x000826E8 File Offset: 0x000808E8
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbRefresh"))
				{
					if (barItemKey == "tbForBidden")
					{
						this.ForbiddenMtrlObjs();
						return;
					}
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
					this.forbiddenMtrlDatas.Clear();
					Entity entity = this.View.Model.BusinessInfo.GetEntity("FEntity");
					DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
					entityDataObject.Clear();
					this.View.Model.SetValue("FLabel", string.Empty);
					this.FillEntity(entityDataObject);
					return;
				}
			}
		}

		// Token: 0x06000B4D RID: 2893 RVA: 0x000827D4 File Offset: 0x000809D4
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			base.EntryButtonCellClick(e);
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (!(a == "FUSEBILLNO"))
				{
					return;
				}
				if (e.Row == -1)
				{
					return;
				}
				Entity entryEntity = this.Model.BusinessInfo.GetEntryEntity("FEntity");
				DynamicObject entityDataObject = this.View.Model.GetEntityDataObject(entryEntity, e.Row);
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "BillId", 0L);
				if (dynamicValue <= 0L)
				{
					return;
				}
				string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(entityDataObject, "UseBillType_Id", null);
				this.OpenBill(dynamicValue, dynamicValue2);
			}
		}

		// Token: 0x06000B4E RID: 2894 RVA: 0x00082868 File Offset: 0x00080A68
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

		// Token: 0x06000B4F RID: 2895 RVA: 0x000828AC File Offset: 0x00080AAC
		public override void BeforeEntityExport(BeforeEntityExportArgs e)
		{
			base.BeforeEntityExport(e);
			Entity entity = this.View.Model.BusinessInfo.GetEntity("FEntity");
			List<DynamicObject> value = this.View.Model.GetEntityDataObject(entity).ToList<DynamicObject>();
			e.DataSource = new Dictionary<string, List<DynamicObject>>
			{
				{
					"FEntity",
					value
				}
			};
			e.ExportEntityKeyList = new List<string>
			{
				"FEntity"
			};
		}

		// Token: 0x06000B50 RID: 2896 RVA: 0x00082924 File Offset: 0x00080B24
		private void InitOrgId(string fieldkey)
		{
			string text = " FDOCUMENTSTATUS ='C'";
			if (!MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context))
			{
				text += string.Format(" {0} FFORBIDSTATUS = 'A'", ObjectUtils.IsNullOrEmptyOrWhiteSpace(text) ? "" : " AND ");
			}
			List<EnumItem> enumList = this.GetEnumList("BD_SupplyOrg", "FORGID", "FNAME", text, false);
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>(fieldkey, 0);
			fieldEditor.SetComboItems(enumList);
			foreach (EnumItem enumItem in enumList)
			{
				if (enumItem.EnumId.ToString().Trim() == base.Context.CurrentOrganizationInfo.ID.ToString().Trim())
				{
					this.View.Model.SetValue(fieldkey, enumItem.Value.ToString());
					break;
				}
			}
		}

		// Token: 0x06000B51 RID: 2897 RVA: 0x00082A2C File Offset: 0x00080C2C
		private List<EnumItem> GetEnumList(string formId, string idField, string nameField, string filter = "", bool isDataCtrl = false)
		{
			List<EnumItem> list = new List<EnumItem>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo(idField));
			list2.Add(new SelectorItemInfo(nameField));
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = formId,
				SelectItems = list2,
				FilterClauseWihtKey = filter
			};
			if (isDataCtrl)
			{
				queryBuilderParemeter.PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad";
				queryBuilderParemeter.RequiresDataPermission = true;
			}
			DynamicObjectCollection dynamicObjectCollection = MFGServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				list.Add(new EnumItem(new DynamicObject(EnumItem.EnumItemType))
				{
					EnumId = Convert.ToString(dynamicObject[idField]),
					Value = Convert.ToString(dynamicObject[idField]),
					Caption = new LocaleValue(Convert.ToString(dynamicObject[nameField]), base.Context.UserLocale.LCID)
				});
			}
			return list;
		}

		// Token: 0x06000B52 RID: 2898 RVA: 0x00082B4C File Offset: 0x00080D4C
		private bool VarifyBillHeadData(out string errorMsg)
		{
			bool result = true;
			string value = MFGBillUtil.GetValue<string>(this.View.Model, "FUseOrgId", -1, null, null);
			StringBuilder stringBuilder = new StringBuilder();
			if (string.IsNullOrWhiteSpace(value))
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("请先选择使用组织！", "015072000036593", 7, new object[0]));
				result = false;
			}
			DateTime value2 = MFGBillUtil.GetValue<DateTime>(this.View.Model, "FEndDate", -1, default(DateTime), null);
			if (value2 >= DateTime.MaxValue || value2 <= DateTime.MinValue)
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("截止时间不能为空", "0151515153499000018827", 7, new object[0]));
				result = false;
			}
			errorMsg = stringBuilder.ToString();
			return result;
		}

		// Token: 0x06000B53 RID: 2899 RVA: 0x00082F08 File Offset: 0x00081108
		private void FillEntity(DynamicObjectCollection entryDatas)
		{
			TaskProxyItem taskProxyItem = new TaskProxyItem();
			List<object> list = new List<object>();
			list.Add(base.Context);
			taskProxyItem.ProgressQueryInterval = 1;
			taskProxyItem.Title = ResManager.LoadKDString("获取数据....", "0151515151805000013364", 7, new object[0]);
			taskProxyItem.BizActionCallback = delegate(object[] o)
			{
				try
				{
					TaskProxyServiceHelper.SetTaskProgressMessage(this.Context, taskProxyItem.TaskId, ResManager.LoadKDString("正在获取物料数据", "0151515153499000018829", 7, new object[0]));
					TaskProxyServiceHelper.SetTaskProgressValue(this.Context, taskProxyItem.TaskId, 10);
					this.forbiddenMtrlDatas = this.GetForBiddenMtrlDatas();
				}
				catch (Exception ex)
				{
					Logger.Error("ENG_MtrlForBiddenQuery", ResManager.LoadKDString("获取数据异常", "0151515151805000013367", 7, new object[0]), ex);
					this.View.ShowErrMessage(ResManager.LoadKDString("获取数据异常", "0151515151805000013367", 7, new object[0]) + ex.Message.ToString(), "", 0);
				}
			};
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
			{
				if (this.View != null)
				{
					if (op.IsSuccess && ListUtils.IsEmpty<DynamicObject>(this.forbiddenMtrlDatas))
					{
						this.View.ShowMessage(ResManager.LoadKDString("未过滤出来数据，请重新选择过滤条件", "0151515151805000013368", 7, new object[0]), 0);
						this.View.UpdateView("FEntity");
						FieldEditor fieldEditor = this.View.GetFieldEditor("FLabel", -1);
						fieldEditor.Visible = false;
						this.View.Model.SetValue("FLabel", string.Empty);
					}
					if (op.IsSuccess && !ListUtils.IsEmpty<DynamicObject>(this.forbiddenMtrlDatas))
					{
						if (!ListUtils.IsEmpty<DynamicObject>(this.forbiddenMtrlDatas))
						{
							this.Model.BeginIniti();
							int num = 1;
							foreach (DynamicObject dynamicObject in this.forbiddenMtrlDatas)
							{
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", num);
								entryDatas.Add(dynamicObject);
								num++;
							}
							this.Model.EndIniti();
							string text = string.Format(ResManager.LoadKDString("共有{0}个物料可禁用!", "0151515153499000016563", 7, new object[0]), this.forbiddenMtrlDatas.Count<DynamicObject>());
							FieldEditor fieldEditor2 = this.View.GetFieldEditor("FLabel", -1);
							fieldEditor2.Visible = true;
							this.View.Model.SetValue("FLabel", text);
						}
						this.View.UpdateView("FEntity");
						return;
					}
				}
				else
				{
					Logger.Error("MFG_MaterialForbiddenQuery", ResManager.LoadKDString("获取可禁用物料数据页面View为null", "0151515153499000016564", 7, new object[0]), null);
				}
			});
		}

		// Token: 0x06000B54 RID: 2900 RVA: 0x00082FAC File Offset: 0x000811AC
		public List<DynamicObject> GetForBiddenMtrlDatas()
		{
			DateTime value = MFGBillUtil.GetValue<DateTime>(this.View.Model, "FEndDate", -1, default(DateTime), null);
			List<string> list = MFGBillUtil.GetValue<string>(this.View.Model, "FUseOrgId", -1, null, null).Split(new char[]
			{
				','
			}).Distinct<string>().ToList<string>();
			string value2 = MFGBillUtil.GetValue<string>(this.View.Model, "FMtrlGroup", -1, null, null);
			List<string> list2 = new List<string>();
			if (!string.IsNullOrWhiteSpace(value2))
			{
				list2 = value2.Split(new char[]
				{
					','
				}).Distinct<string>().ToList<string>();
			}
			return MaterialForbiddenQueryServiceHelper.GetForBiddenMtrlDatas(base.Context, list, list2, value);
		}

		// Token: 0x06000B55 RID: 2901 RVA: 0x0008306C File Offset: 0x0008126C
		private void OpenBill(long Id, string formId)
		{
			string empty = string.Empty;
			BillShowParameter billShowParameter = MFGCommonUtil.CreateBillShowParameterByPermission(base.Context, formId, Id.ToString(), ref empty);
			if (billShowParameter != null)
			{
				billShowParameter.Status = 1;
				this.View.ShowForm(billShowParameter);
				return;
			}
			this.View.ShowMessage(empty, 0);
		}

		// Token: 0x06000B56 RID: 2902 RVA: 0x00083740 File Offset: 0x00081940
		private void ForbiddenMtrlObjs()
		{
			Entity entity = this.View.Model.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entryDatas = this.View.Model.GetEntityDataObject(entity);
			List<DynamicObject> selectDatas = (from w in entryDatas
			where DataEntityExtend.GetDynamicValue<bool>(w, "IsSelect", false)
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(selectDatas))
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选中需要禁用的物料数据", "0151515153499000016565", 7, new object[0]), 0);
				return;
			}
			IOperationResult operationResult = new OperationResult();
			List<NetworkCtrlResult> networkCtrlResults = new List<NetworkCtrlResult>();
			List<object> successDMtrlIds = new List<object>();
			FormMetadata mtrlMetaData = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "BD_MATERIAL", true);
			TaskProxyItem taskProxyItem = new TaskProxyItem();
			List<object> list = new List<object>();
			list.Add(base.Context);
			taskProxyItem.ProgressQueryInterval = 1;
			taskProxyItem.Title = ResManager.LoadKDString("正在物料数据禁用....", "0151515153499000016566", 7, new object[0]);
			taskProxyItem.BizActionCallback = delegate(object[] o)
			{
				try
				{
					TaskProxyServiceHelper.SetTaskProgressMessage(this.Context, taskProxyItem.TaskId, ResManager.LoadKDString("开启物料修改网控", "0151515153499000018837", 7, new object[0]));
					TaskProxyServiceHelper.SetTaskProgressValue(this.Context, taskProxyItem.TaskId, 10);
					List<long> list2 = (from s in selectDatas
					select DataEntityExtend.GetDynamicValue<long>(s, "MtrlId_Id", 0L)).ToList<long>();
					list2 = MaterialDeleteQueryServiceHelper.StartNetworkCtrl(this.Context, "42a09bea-6faa-4855-b689-febb1938b33e", ResManager.LoadKDString("禁用", "0151515153499000016567", 7, new object[0]), list2, ref networkCtrlResults);
					if (ListUtils.IsEmpty<long>(list2))
					{
						TaskProxyServiceHelper.SetTaskProgressMessage(this.Context, taskProxyItem.TaskId, ResManager.LoadKDString("没有符合此过滤范围的物料，原因是此范围内物料正被其它用户使用...", "0151515153499000016568", 7, new object[0]));
						TaskProxyServiceHelper.SetTaskProgressValue(this.Context, taskProxyItem.TaskId, 100);
					}
					else
					{
						TaskProxyServiceHelper.SetTaskProgressMessage(this.Context, taskProxyItem.TaskId, ResManager.LoadKDString("正在物料数据禁用...", "0151515153499000016569", 7, new object[0]));
						TaskProxyServiceHelper.SetTaskProgressValue(this.Context, taskProxyItem.TaskId, 30);
						List<KeyValuePair<object, object>> list3 = new List<KeyValuePair<object, object>>();
						List<string> list4 = new List<string>();
						foreach (long num in list2)
						{
							list3.Add(new KeyValuePair<object, object>(num, ""));
							list4.Add(num.ToString());
						}
						FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(this.Context, "BD_MATERIAL");
						DynamicObject[] array = BusinessDataServiceHelper.Load(this.Context, list4.ToArray(), formMetaData.BusinessInfo.GetDynamicObjectType());
						foreach (DynamicObject dynamicObject in array)
						{
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ModifyDate", DateTime.Now);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ModifierId_Id", this.Context.UserId);
						}
						if (!ListUtils.IsEmpty<DynamicObject>(array))
						{
							BusinessDataServiceHelper.Save(this.Context, array);
						}
						operationResult = BusinessDataServiceHelper.SetBillStatus(this.Context, formMetaData.BusinessInfo, list3, null, "Forbid", null);
					}
				}
				catch (Exception ex)
				{
					Logger.Error("ENG_MtrlForBiddenQuery", ResManager.LoadKDString("物料数据禁用出现异常", "0151515153499000016570", 7, new object[0]), ex);
					this.View.ShowErrMessage(ResManager.LoadKDString("物料数据禁用出现异常", "0151515153499000016570", 7, new object[0]), "", 0);
				}
				finally
				{
					MaterialDeleteQueryServiceHelper.CommitNetworkCtrl(this.Context, networkCtrlResults);
				}
			};
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
			{
				if (this.View != null)
				{
					if (op.IsSuccess)
					{
						if (!ListUtils.IsEmpty<DynamicObject>(operationResult.SuccessDataEnity))
						{
							successDMtrlIds.AddRange((from s in operationResult.SuccessDataEnity
							select s["Id"]).ToList<object>());
						}
						if (!ObjectUtils.IsNullOrEmpty(operationResult))
						{
							FormUtils.ShowOperationResult(this.View, operationResult, null);
						}
						else
						{
							this.View.ShowMessage(ResManager.LoadKDString("无任何物料禁用成功，请检查物料在界面能否正常禁用", "0151515153499000016571", 7, new object[0]), 0);
						}
						List<DynamicObject> list2 = (from w in entryDatas
						where successDMtrlIds.Contains(DataEntityExtend.GetDynamicValue<object>(w, "MtrlId_Id", null))
						select w).ToList<DynamicObject>();
						foreach (DynamicObject item in list2)
						{
							entryDatas.Remove(item);
						}
						int num = 1;
						List<object> list3 = (from s in selectDatas
						select s["MtrlId_Id"]).ToList<object>();
						(from g in BusinessDataServiceHelper.LoadFromCache(this.Context, list3.ToArray(), mtrlMetaData.BusinessInfo.GetDynamicObjectType()).ToList<DynamicObject>()
						group g by DataEntityExtend.GetDynamicValue<long>(g, "Id", 0L)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
						foreach (DynamicObject dynamicObject in entryDatas)
						{
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", num);
							num++;
						}
						this.View.UpdateView("FEntity");
						string text = string.Format(ResManager.LoadKDString("共有{0}个物料没有被禁用!", "0151515153499000016572", 7, new object[0]), entryDatas.Count<DynamicObject>());
						FieldEditor fieldEditor = this.View.GetFieldEditor("FLabel", -1);
						fieldEditor.Visible = true;
						this.View.Model.SetValue("FLabel", text);
						return;
					}
				}
				else
				{
					Logger.Error("MFG_MaterialForbiddenQuery", ResManager.LoadKDString("可禁用物料分析禁用物料View为null", "0151515153499000016573", 7, new object[0]), null);
				}
			});
		}

		// Token: 0x06000B57 RID: 2903 RVA: 0x000838AC File Offset: 0x00081AAC
		private bool ValidatePermission(BeforeDoOperationEventArgs e)
		{
			bool flag = MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BillBusinessInfo.GetForm().Id, e.Operation.FormOperation.PermissionItemId);
			if (!flag)
			{
				this.View.ShowMessage(string.Format(ResManager.LoadKDString("没有可禁用物料分析的{0}权限！", "0151515153499000016574", 7, new object[0]), e.Operation.FormOperation.OperationName[this.View.Context.UserLocale.LCID]), 0);
				flag = false;
			}
			return flag;
		}

		// Token: 0x04000551 RID: 1361
		private List<DynamicObject> forbiddenMtrlDatas = new List<DynamicObject>();
	}
}
