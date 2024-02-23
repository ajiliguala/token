using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
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
	// Token: 0x0200009F RID: 159
	[Description("可删除物料分析")]
	public class MaterialDeleteQuery : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000B2C RID: 2860 RVA: 0x00080434 File Offset: 0x0007E634
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.InitOrgId("FUseOrgId");
			List<EnumItem> comboItems = new List<EnumItem>();
			comboItems = MaterialDeleteQueryServiceHelper.GetMtrlGroupEnumList(base.Context);
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>("FMtrlGroup", 0);
			fieldEditor.SetComboItems(comboItems);
		}

		// Token: 0x06000B2D RID: 2861 RVA: 0x00080480 File Offset: 0x0007E680
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbRefresh"))
				{
					if (barItemKey == "tbDelete")
					{
						this.ClearMtrlObjs();
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
					this.mtrlObjs.Clear();
					this.refMtrlObjs.Clear();
					Entity entity = this.View.Model.BusinessInfo.GetEntity("FEntity");
					DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
					entityDataObject.Clear();
					this.View.Model.SetValue("FLabel", string.Empty);
					this.FillEntry();
					return;
				}
			}
		}

		// Token: 0x06000B2E RID: 2862 RVA: 0x00080578 File Offset: 0x0007E778
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			FieldEditor fieldEditor = this.View.GetFieldEditor("FLabel", -1);
			fieldEditor.Visible = false;
		}

		// Token: 0x06000B2F RID: 2863 RVA: 0x000805A8 File Offset: 0x0007E7A8
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

		// Token: 0x06000B30 RID: 2864 RVA: 0x000805EC File Offset: 0x0007E7EC
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

		// Token: 0x06000B31 RID: 2865 RVA: 0x00080664 File Offset: 0x0007E864
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

		// Token: 0x06000B32 RID: 2866 RVA: 0x0008076C File Offset: 0x0007E96C
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
				FilterClauseWihtKey = filter,
				OrderByClauseWihtKey = "FNUMBER ASC"
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

		// Token: 0x06000B33 RID: 2867 RVA: 0x00080898 File Offset: 0x0007EA98
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

		// Token: 0x06000B34 RID: 2868 RVA: 0x00080E04 File Offset: 0x0007F004
		private void FillEntry()
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
					TaskProxyServiceHelper.SetTaskProgressValue(this.Context, taskProxyItem.TaskId, 30);
					this.mtrlObjs = this.GetMtrlObjsByFilter();
					TaskProxyServiceHelper.SetTaskProgressMessage(this.Context, taskProxyItem.TaskId, ResManager.LoadKDString("正在判断物料是否被业务单据引用", "0151515153499000018830", 7, new object[0]));
					TaskProxyServiceHelper.SetTaskProgressValue(this.Context, taskProxyItem.TaskId, 60);
					if (!ListUtils.IsEmpty<DynamicObject>(this.mtrlObjs))
					{
						this.refMtrlObjs = MaterialDeleteQueryServiceHelper.GetNoRefMtrlObjs(this.Context, this.mtrlObjs);
						Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
						DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
						if (!ListUtils.IsEmpty<DynamicObject>(this.refMtrlObjs))
						{
							List<DynamicObject> list2 = (from d in this.refMtrlObjs
							orderby DataEntityExtend.GetDynamicValue<string>(d, "UseOrgNumber", null)
							select d).ThenBy((DynamicObject t) => DataEntityExtend.GetDynamicValue<DateTime>(t, "CreateDate", default(DateTime))).ToList<DynamicObject>();
							this.Model.BeginIniti();
							int num = 1;
							foreach (DynamicObject dynamicObject in list2)
							{
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", num);
								entityDataObject.Add(dynamicObject);
								num++;
							}
							this.Model.EndIniti();
						}
						double num2 = Convert.ToDouble(this.refMtrlObjs.Count<DynamicObject>());
						double num3 = Convert.ToDouble(this.mtrlObjs.Count<DynamicObject>());
						double num4 = Convert.ToDouble(num2 / num3);
						string text = string.Format(ResManager.LoadKDString("共有{0}个物料没有被引用，占比{1}，建议删除", "0151515153499000018831", 7, new object[0]), this.refMtrlObjs.Count<DynamicObject>(), num4.ToString("P"));
						FieldEditor fieldEditor = this.View.GetFieldEditor("FLabel", -1);
						fieldEditor.Visible = true;
						this.View.Model.SetValue("FLabel", text);
					}
				}
				catch (Exception ex)
				{
					Logger.Error("ENG_MtrlDeleteQuery", ResManager.LoadKDString("获取数据异常", "0151515151805000013367", 7, new object[0]), ex);
					this.View.ShowErrMessage(ResManager.LoadKDString("获取数据异常", "0151515151805000013367", 7, new object[0]) + ex.Message.ToString(), "", 0);
				}
			};
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
			{
				if (op.IsSuccess && ListUtils.IsEmpty<DynamicObject>(this.mtrlObjs))
				{
					this.View.ShowMessage(ResManager.LoadKDString("未过滤出来数据，请重新选择过滤条件", "0151515151805000013368", 7, new object[0]), 0);
					this.View.UpdateView("FEntity");
					FieldEditor fieldEditor = this.View.GetFieldEditor("FLabel", -1);
					fieldEditor.Visible = false;
					this.View.Model.SetValue("FLabel", string.Empty);
				}
				if (op.IsSuccess && ListUtils.IsEmpty<DynamicObject>(this.refMtrlObjs))
				{
					this.View.ShowMessage(ResManager.LoadKDString("根据表头过滤条件过滤出的数据被业务单据引用,无数据展示", "0151515153499000018834", 7, new object[0]), 0);
					this.View.UpdateView("FEntity");
					FieldEditor fieldEditor2 = this.View.GetFieldEditor("FLabel", -1);
					fieldEditor2.Visible = false;
					this.View.Model.SetValue("FLabel", string.Empty);
				}
				if (op.IsSuccess && !ListUtils.IsEmpty<DynamicObject>(this.refMtrlObjs))
				{
					this.View.UpdateView("FEntity");
				}
			});
		}

		// Token: 0x06000B35 RID: 2869 RVA: 0x00080F00 File Offset: 0x0007F100
		private List<DynamicObject> GetMtrlObjsByFilter()
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
			List<DynamicObject> list3 = MaterialDeleteQueryServiceHelper.GetMtrlObjs(base.Context, list, list2, value);
			if (ListUtils.IsEmpty<DynamicObject>(list3))
			{
				return list3.ToList<DynamicObject>();
			}
			List<DynamicObject> list4 = (from w in list3
			where DataEntityExtend.GetDynamicValue<long>(w, "CreateOrgId_Id", 0L) == DataEntityExtend.GetDynamicValue<long>(w, "MUseOrgId_Id", 0L)
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list4))
			{
				return list3.ToList<DynamicObject>();
			}
			List<long> list5 = (from s in list4
			select DataEntityExtend.GetDynamicValue<long>(s, "MtrlId_Id", 0L)).ToList<long>();
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "BD_MATERIAL",
				SelectItems = SelectorItemInfo.CreateItems("FMASTERID"),
				FilterClauseWihtKey = "FCreateOrgId<>FUseOrgId"
			};
			string sqlWithCardinality = StringUtils.GetSqlWithCardinality(list5.Distinct<long>().Count<long>(), "@MTRLIDS", 1, true);
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				TableName = sqlWithCardinality,
				TableNameAs = "TMS",
				FieldName = "FID",
				ScourceKey = "FMASTERID"
			});
			queryBuilderParemeter.SqlParams.Add(new SqlParam("@MTRLIDS", 161, list5.Distinct<long>().ToArray<long>()));
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				return list3.ToList<DynamicObject>();
			}
			List<long> second = (from s in dynamicObjectCollection
			select DataEntityExtend.GetDynamicValue<long>(s, "FMASTERID", 0L)).ToList<long>();
			List<long> oMtrlIds = list5.Intersect(second).ToList<long>();
			return (from w in list3
			where !oMtrlIds.Contains(DataEntityExtend.GetDynamicValue<long>(w, "MtrlId_Id", 0L))
			select w).ToList<DynamicObject>();
		}

		// Token: 0x06000B36 RID: 2870 RVA: 0x00081AEC File Offset: 0x0007FCEC
		private void ClearMtrlObjs()
		{
			Entity entity = this.View.Model.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entryDatas = this.View.Model.GetEntityDataObject(entity);
			List<DynamicObject> wEntryDatas = (from w in entryDatas
			where DataEntityExtend.GetDynamicValue<bool>(w, "IsSelect", false)
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(wEntryDatas))
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选中需要删除的物料数据", "0151515153499000018835", 7, new object[0]), 0);
				return;
			}
			List<NetworkCtrlResult> networkCtrlResults = new List<NetworkCtrlResult>();
			List<object> successDMtrlIds = new List<object>();
			FormMetadata mtrlMetaData = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "BD_MATERIAL", true);
			TaskProxyItem taskProxyItem = new TaskProxyItem();
			List<object> list = new List<object>();
			list.Add(base.Context);
			taskProxyItem.ProgressQueryInterval = 1;
			taskProxyItem.Title = ResManager.LoadKDString("正在物料数据删除....", "0151515153499000018836", 7, new object[0]);
			taskProxyItem.BizActionCallback = delegate(object[] o)
			{
				try
				{
					TaskProxyServiceHelper.SetTaskProgressMessage(this.Context, taskProxyItem.TaskId, ResManager.LoadKDString("开启物料修改网控", "0151515153499000018837", 7, new object[0]));
					TaskProxyServiceHelper.SetTaskProgressValue(this.Context, taskProxyItem.TaskId, 10);
					List<long> mtrlIds = (from s in wEntryDatas
					select DataEntityExtend.GetDynamicValue<long>(s, "MtrlId_Id", 0L)).ToList<long>();
					mtrlIds = MaterialDeleteQueryServiceHelper.StartNetworkCtrl(this.Context, "fae2446c-66e3-4cfe-83ab-5c7c1409d177", ResManager.LoadKDString("修改", "005023000000547", 3, new object[0]), mtrlIds, ref networkCtrlResults);
					if (ListUtils.IsEmpty<long>(mtrlIds))
					{
						TaskProxyServiceHelper.SetTaskProgressMessage(this.Context, taskProxyItem.TaskId, ResManager.LoadKDString("没有符合此过滤范围的物料清单，原因是此范围内物料清单正被其它用户使用...", "0151515153499000018838", 7, new object[0]));
						TaskProxyServiceHelper.SetTaskProgressValue(this.Context, taskProxyItem.TaskId, 100);
					}
					else
					{
						TaskProxyServiceHelper.SetTaskProgressMessage(this.Context, taskProxyItem.TaskId, ResManager.LoadKDString("正在物料数据删除", "0151515153499000018839", 7, new object[0]));
						TaskProxyServiceHelper.SetTaskProgressValue(this.Context, taskProxyItem.TaskId, 30);
						List<DynamicObject> list2 = new List<DynamicObject>();
						list2 = (from w in wEntryDatas
						where mtrlIds.Contains(DataEntityExtend.GetDynamicValue<long>(w, "MtrlId_Id", 0L)) && (DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(w, "MtrlId", null), "DocumentStatus", null) == "C" || DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(w, "MtrlId", null), "DocumentStatus", null) == "B")
						select w).ToList<DynamicObject>();
						IOperationResult operationResult = new OperationResult();
						List<object> list3 = new List<object>();
						List<object> list4 = new List<object>();
						if (!ListUtils.IsEmpty<DynamicObject>(list2))
						{
							IOperationResult operationResult2 = BusinessDataServiceHelper.UnAudit(this.Context, mtrlMetaData.BusinessInfo, (from s in list2
							select s["MtrlId_Id"]).ToArray<object>(), OperateOption.Create());
							if (!ListUtils.IsEmpty<DynamicObject>(operationResult2.SuccessDataEnity))
							{
								List<object> collection = (from s in operationResult2.SuccessDataEnity
								select s["Id"]).ToList<object>();
								list3.AddRange(collection);
								list4.AddRange(collection);
							}
						}
						List<DynamicObject> list5 = new List<DynamicObject>();
						list5 = (from w in wEntryDatas
						where mtrlIds.Contains(DataEntityExtend.GetDynamicValue<long>(w, "MtrlId_Id", 0L)) && (DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(w, "MtrlId", null), "DocumentStatus", null) == "Z" || DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(w, "MtrlId", null), "DocumentStatus", null) == "D" || DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(w, "MtrlId", null), "DocumentStatus", null) == "A")
						select w).ToList<DynamicObject>();
						if (!ListUtils.IsEmpty<DynamicObject>(list5))
						{
							List<object> collection2 = new List<object>();
							collection2 = (from s in list5
							select s["MtrlId_Id"]).ToList<object>();
							list3.AddRange(collection2);
						}
						if (!ListUtils.IsEmpty<object>(list3))
						{
							IOperationResult operationResult3 = BusinessDataServiceHelper.Delete(this.Context, mtrlMetaData.BusinessInfo, list3.ToArray(), OperateOption.Create(), "");
							OperationResultExt.MergeResult(operationResult, operationResult3);
							if (!ListUtils.IsEmpty<DynamicObject>(operationResult3.SuccessDataEnity))
							{
								successDMtrlIds.AddRange((from s in operationResult3.SuccessDataEnity
								select s["Id"]).ToList<object>());
								List<object> list6 = list4.Except(successDMtrlIds).ToList<object>();
								if (!ListUtils.IsEmpty<object>(list6))
								{
									BusinessDataServiceHelper.Submit(this.Context, mtrlMetaData.BusinessInfo, list6.ToArray(), "Submit", OperateOption.Create());
									BusinessDataServiceHelper.Audit(this.Context, mtrlMetaData.BusinessInfo, list6.ToArray(), OperateOption.Create());
								}
							}
							else if (!ListUtils.IsEmpty<object>(list4))
							{
								BusinessDataServiceHelper.Submit(this.Context, mtrlMetaData.BusinessInfo, list4.ToArray(), "Submit", OperateOption.Create());
								BusinessDataServiceHelper.Audit(this.Context, mtrlMetaData.BusinessInfo, list4.ToArray(), OperateOption.Create());
							}
						}
						if (!ObjectUtils.IsNullOrEmpty(operationResult))
						{
							FormUtils.ShowOperationResult(this.View, operationResult, null);
						}
						else
						{
							this.View.ShowMessage(ResManager.LoadKDString("无任何物料删除成功，请检查物料在界面能否正常反审核或正常删除", "0151515153499000018840", 7, new object[0]), 0);
						}
						MaterialDeleteQueryServiceHelper.CommitNetworkCtrl(this.Context, networkCtrlResults);
					}
				}
				catch (Exception ex)
				{
					Logger.Error("ENG_MtrlDeleteQuery", ResManager.LoadKDString("物料数据删除出现异常", "0151515153499000018841", 7, new object[0]), ex);
					this.View.ShowErrMessage(ResManager.LoadKDString("物料数据删除出现异常", "0151515153499000018841", 7, new object[0]), "", 0);
					MaterialDeleteQueryServiceHelper.CommitNetworkCtrl(this.Context, networkCtrlResults);
				}
			};
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
			{
				if (op.IsSuccess)
				{
					List<DynamicObject> list2 = (from w in entryDatas
					where successDMtrlIds.Contains(DataEntityExtend.GetDynamicValue<object>(w, "MtrlId_Id", null))
					select w).ToList<DynamicObject>();
					foreach (DynamicObject item in list2)
					{
						entryDatas.Remove(item);
					}
					int num = 1;
					List<object> list3 = (from s in wEntryDatas
					select s["MtrlId_Id"]).ToList<object>();
					Dictionary<long, IGrouping<long, DynamicObject>> dictionary = (from g in BusinessDataServiceHelper.LoadFromCache(this.Context, list3.ToArray(), mtrlMetaData.BusinessInfo.GetDynamicObjectType()).ToList<DynamicObject>()
					group g by DataEntityExtend.GetDynamicValue<long>(g, "Id", 0L)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
					foreach (DynamicObject dynamicObject in entryDatas)
					{
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", num);
						long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MtrlId_Id", 0L);
						IGrouping<long, DynamicObject> source;
						if (dictionary.TryGetValue(dynamicValue, out source))
						{
							DateTime dynamicValue2 = DataEntityExtend.GetDynamicValue<DateTime>(source.FirstOrDefault<DynamicObject>(), "ApproveDate", default(DateTime));
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "MtrlId", source.FirstOrDefault<DynamicObject>());
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "AuditDate", dynamicValue2);
						}
						num++;
					}
					this.View.UpdateView("FEntity");
					this.View.UpdateView("FMtrlId");
					this.View.UpdateView("FAuditDate");
					double num2 = Convert.ToDouble(entryDatas.Count<DynamicObject>());
					double num3 = Convert.ToDouble(this.mtrlObjs.Count<DynamicObject>());
					double num4 = Convert.ToDouble(num2 / num3);
					string text = string.Format(ResManager.LoadKDString("共有{0}个物料没有被引用，占比{1}，建议删除", "0151515153499000018831", 7, new object[0]), entryDatas.Count<DynamicObject>(), num4.ToString("P"));
					FieldEditor fieldEditor = this.View.GetFieldEditor("FLabel", -1);
					fieldEditor.Visible = true;
					this.View.Model.SetValue("FLabel", text);
				}
			});
		}

		// Token: 0x06000B37 RID: 2871 RVA: 0x00081C50 File Offset: 0x0007FE50
		private bool ValidatePermission(BeforeDoOperationEventArgs e)
		{
			bool flag = MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BillBusinessInfo.GetForm().Id, e.Operation.FormOperation.PermissionItemId);
			if (!flag)
			{
				this.View.ShowMessage(string.Format(ResManager.LoadKDString("没有可删除物料分析的{0}权限！", "0151515153499000018842", 7, new object[0]), e.Operation.FormOperation.OperationName[this.View.Context.UserLocale.LCID]), 0);
				flag = false;
			}
			return flag;
		}

		// Token: 0x04000547 RID: 1351
		private List<DynamicObject> mtrlObjs = new List<DynamicObject>();

		// Token: 0x04000548 RID: 1352
		private List<DynamicObject> refMtrlObjs = new List<DynamicObject>();
	}
}
