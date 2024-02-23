using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Common;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200007E RID: 126
	[Description("BOM同步检查表单插件")]
	public class BOMSynCheckEdit : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x0600094C RID: 2380 RVA: 0x0006E590 File Offset: 0x0006C790
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string barItemKey;
			switch (barItemKey = e.BarItemKey)
			{
			case "FBtnCheck":
				this.FillEntity();
				return;
			case "tbBatchSave":
				this.BatchSaveBomTask("FEntity");
				return;
			case "tbMaterialAllocate":
				this.MaterialAllocate();
				return;
			case "tbScheduleCheck":
				this.SetSyncCheckSchedule();
				return;
			case "tbSylLableCheck":
				this.FillBomEntity();
				return;
			case "tbBomSave":
				this.BatchSaveBomTask("FBomEntity");
				break;

				return;
			}
		}

		// Token: 0x0600094D RID: 2381 RVA: 0x0006E674 File Offset: 0x0006C874
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			if (e.BaseDataField.Key.Equals("FBOMVersion"))
			{
				if (string.IsNullOrWhiteSpace(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = " FCREATEORGID=FUSEORGID ";
				}
				else
				{
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter += " AND FCREATEORGID=FUSEORGID ";
				}
			}
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FCREATEORGID") && !(a == "FUSEORGID"))
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

		// Token: 0x0600094E RID: 2382 RVA: 0x0006E738 File Offset: 0x0006C938
		private void FillEntity()
		{
			long value = MFGBillUtil.GetValue<long>(this.View.Model, "FCreateOrgId", -1, 0L, null);
			if (value == 0L)
			{
				this.View.ShowMessage(ResManager.LoadKDString("创建组织不能为空", "015072000025079", 7, new object[0]), 0);
				return;
			}
			long value2 = MFGBillUtil.GetValue<long>(this.View.Model, "FUseOrgId", -1, 0L, null);
			if (value2 > 0L && value == value2)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("创建组织和使用组织不能一致", "015072000038206", 7, new object[0]), "", 0);
				return;
			}
			List<long> bomMasterIds = new List<long>();
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["BOMVersion"] as DynamicObjectCollection;
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				bomMasterIds = (from s in dynamicObjectCollection
				select DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(s, "BOMVersion", null), "MsterId", 0L)).ToList<long>();
			}
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			BomSynOption bomSynOption = new BomSynOption();
			bomSynOption.BSCBusinessInfo = this.View.BusinessInfo;
			bomSynOption.SrcOrgId = value;
			bomSynOption.UseOrgId = value2;
			bomSynOption.BomMasterIds = bomMasterIds;
			this.View.Model.BeginIniti();
			entityDataObject.Clear();
			IEnumerable<DynamicObject> diffBomInfos = BOMSynCheckServiceHelper.GetDiffBomInfos(base.Context, bomSynOption);
			if (!ListUtils.IsEmpty<DynamicObject>(diffBomInfos))
			{
				using (IEnumerator<DynamicObject> enumerator = diffBomInfos.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DynamicObject item = enumerator.Current;
						entityDataObject.Add(item);
					}
					goto IL_1C5;
				}
			}
			this.View.ShowMessage(ResManager.LoadKDString("没有物料清单未同步信息", "015072000013257", 7, new object[0]), 0);
			IL_1C5:
			this.View.Model.EndIniti();
			this.View.UpdateView("FEntity");
		}

		// Token: 0x0600094F RID: 2383 RVA: 0x0006EA34 File Offset: 0x0006CC34
		private void FillEntityTask()
		{
			long value = MFGBillUtil.GetValue<long>(this.View.Model, "FCreateOrgId", -1, 0L, null);
			if (value == 0L)
			{
				this.View.ShowMessage(ResManager.LoadKDString("创建组织不能为空", "015072000025079", 7, new object[0]), 0);
				return;
			}
			long value2 = MFGBillUtil.GetValue<long>(this.View.Model, "FUseOrgId", -1, 0L, null);
			if (value2 > 0L && value == value2)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("创建组织和使用组织不能一致", "015072000038206", 7, new object[0]), "", 0);
				return;
			}
			List<long> bomMasterIds = new List<long>();
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["BOMVersion"] as DynamicObjectCollection;
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				bomMasterIds = (from s in dynamicObjectCollection
				select DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(s, "BOMVersion", null), "MsterId", 0L)).ToList<long>();
			}
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entrys = this.View.Model.GetEntityDataObject(entity);
			BomSynOption bomSynOption = new BomSynOption();
			bomSynOption.BSCBusinessInfo = this.View.BusinessInfo;
			bomSynOption.SrcOrgId = value;
			bomSynOption.UseOrgId = value2;
			bomSynOption.BomMasterIds = bomMasterIds;
			this.View.Model.BeginIniti();
			entrys.Clear();
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ENG_BOM", true);
			TaskProxyItem taskProxyItem = new TaskProxyItem();
			taskProxyItem.Parameters = new List<object>
			{
				base.Context,
				bomSynOption,
				taskProxyItem.TaskId
			}.ToArray();
			taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.BomSynUpdatePPBom.BOMSynCheckService,Kingdee.K3.MFG.ENG.App.Core";
			taskProxyItem.MethodName = "GetDiffBomInfos";
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, false, delegate(IOperationResult action)
			{
				if (action.IsSuccess)
				{
					this.View.Model.BeginIniti();
					IEnumerable<DynamicObject> enumerable = action.FuncResult as IEnumerable<DynamicObject>;
					if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
					{
						using (IEnumerator<DynamicObject> enumerator = enumerable.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								DynamicObject item = enumerator.Current;
								entrys.Add(item);
							}
							goto IL_8D;
						}
					}
					this.View.ShowMessage(ResManager.LoadKDString("没有物料清单未同步信息", "015072000013257", 7, new object[0]), 0);
					IL_8D:
					this.View.Model.EndIniti();
					this.View.UpdateView("FEntity");
				}
			});
			this.View.UpdateView("FEntity");
		}

		// Token: 0x06000950 RID: 2384 RVA: 0x0006EC68 File Offset: 0x0006CE68
		private void FillBomEntity()
		{
			long value = MFGBillUtil.GetValue<long>(this.View.Model, "FCreateOrgId", -1, 0L, null);
			if (value == 0L)
			{
				this.View.ShowMessage(ResManager.LoadKDString("创建组织不能为空", "015072000025079", 7, new object[0]), 0);
				return;
			}
			long value2 = MFGBillUtil.GetValue<long>(this.View.Model, "FUseOrgId", -1, 0L, null);
			if (value2 == 0L)
			{
				this.View.ShowMessage(ResManager.LoadKDString("使用组织不能为空", "015072000025080", 7, new object[0]), 0);
				return;
			}
			if (value == value2)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("创建组织和使用组织不能一致，一致无法进行子项字段检查操作", "015072000025081", 7, new object[0]), "", 0);
				return;
			}
			List<DynamicObject> source = MFGBillUtil.GetValue<DynamicObjectCollection>(this.Model, "FBOMVersion", -1, null, null).ToList<DynamicObject>();
			List<long> bomIds = (from bomObj in source
			select OtherExtend.ConvertTo<long>(bomObj["BomVersion_Id"], 0L)).ToList<long>();
			Entity entryEntity = this.View.BusinessInfo.GetEntryEntity("FBomEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			BomSynOption bomSynOption = new BomSynOption();
			bomSynOption.SrcOrgId = value;
			bomSynOption.useOrgId = value2;
			bomSynOption.bomIds = bomIds;
			bomSynOption.BSCBusinessInfo = this.View.BusinessInfo;
			this.View.Model.BeginIniti();
			entityDataObject.Clear();
			IEnumerable<DynamicObject> diffBomSylLableInfos = BOMSynCheckServiceHelper.GetDiffBomSylLableInfos(base.Context, bomSynOption);
			if (!ListUtils.IsEmpty<DynamicObject>(diffBomSylLableInfos))
			{
				using (IEnumerator<DynamicObject> enumerator = diffBomSylLableInfos.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DynamicObject item = enumerator.Current;
						entityDataObject.Add(item);
					}
					goto IL_1D3;
				}
			}
			this.View.ShowMessage(ResManager.LoadKDString("物料清单子项字段没有差异", "015072000025083", 7, new object[0]), 0);
			IL_1D3:
			this.View.Model.EndIniti();
			this.View.UpdateView("FBomEntity");
		}

		// Token: 0x06000951 RID: 2385 RVA: 0x0006EE7C File Offset: 0x0006D07C
		private void BatchSaveBomTask(string entryKey)
		{
			Entity entity = this.View.BusinessInfo.GetEntity(entryKey);
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			Dictionary<long, string> dictionary = new Dictionary<long, string>();
			string text = (entryKey == "FEntity") ? "Checked" : "IsSelect";
			string text2 = (entryKey == "FEntity") ? "BOMID_Id" : "SrcBomId_Id";
			string text3 = (entryKey == "FEntity") ? "BOMID" : "SrcBomId";
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, text, false);
				if (dynamicValue)
				{
					long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, text2, 0L);
					if (!dictionary.Keys.Contains(dynamicValue2))
					{
						DynamicObject dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, text3, null);
						string value = (dynamicValue3 == null) ? string.Empty : DataEntityExtend.GetDynamicValue<string>(dynamicValue3, "Number", null);
						dictionary.Add(dynamicValue2, value);
					}
				}
			}
			if (ListUtils.IsEmpty<KeyValuePair<long, string>>(dictionary))
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选择需保存的BOM", "015072000013112", 7, new object[0]), 0);
				return;
			}
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ENG_BOM", true);
			TaskProxyItem taskProxyItem = new TaskProxyItem();
			taskProxyItem.Parameters = new List<object>
			{
				base.Context,
				formMetadata.BusinessInfo,
				dictionary,
				taskProxyItem.TaskId
			}.ToArray();
			taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.BomSynUpdatePPBom.BOMSynCheckService,Kingdee.K3.MFG.ENG.App.Core";
			taskProxyItem.MethodName = "BatchSaveBomTask";
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult action)
			{
			});
		}

		// Token: 0x06000952 RID: 2386 RVA: 0x0006F074 File Offset: 0x0006D274
		private void BatchSaveBom()
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			Dictionary<long, string> dictionary = new Dictionary<long, string>();
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "Checked", false);
				if (dynamicValue)
				{
					long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BOMID_Id", 0L);
					if (!dictionary.Keys.Contains(dynamicValue2))
					{
						DynamicObject dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BOMID", null);
						string value = (dynamicValue3 == null) ? string.Empty : DataEntityExtend.GetDynamicValue<string>(dynamicValue3, "Number", null);
						dictionary.Add(dynamicValue2, value);
					}
				}
			}
			if (ListUtils.IsEmpty<KeyValuePair<long, string>>(dictionary))
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选择需保存的BOM", "015072000013112", 7, new object[0]), 0);
				return;
			}
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ENG_BOM", true);
			IOperationResult operationResult = BOMSynCheckServiceHelper.BatchSaveBom(base.Context, formMetadata.BusinessInfo, dictionary);
			if (operationResult != null && operationResult.OperateResult.Count > 0)
			{
				FormUtils.ShowOperationResult(this.View, operationResult, null);
			}
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x0006F1D0 File Offset: 0x0006D3D0
		private void MaterialAllocate()
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "DiffType", null);
				DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "SrcChdMaterialId", null);
				if (dynamicValue2 != null)
				{
					long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicValue2, "msterID", 0L);
					long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "TgtOrgId_Id", 0L);
					string item = string.Format("{0}_{1}", dynamicValue3, dynamicValue4);
					string a;
					if ((a = dynamicValue) != null)
					{
						if (!(a == "1"))
						{
							if (a == "4")
							{
								list2.Add(item);
							}
						}
						else
						{
							list.Add(item);
						}
					}
				}
			}
			if (!ListUtils.IsEmpty<string>(list2))
			{
				this.View.ShowErrMessage("", ResManager.LoadKDString("源组织存在有未审核的子项物料，请检查处理！", "015072000033738", 7, new object[0]), 0);
				return;
			}
			if (ListUtils.IsEmpty<string>(list))
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有需要分配的物料", "015072000012289", 7, new object[0]), 0);
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
			{
				FormId = "ENG_MtrlBatchAlloc",
				PageId = SequentialGuid.NewGuid().ToString(),
				ParentPageId = this.View.PageId
			};
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.CustomComplexParams.Add("IsAutoAudit", OtherExtend.ConvertTo<bool>(this.Model.GetValue("FIsAutoAudit"), false));
			dynamicFormShowParameter.CustomComplexParams.Add("NeedAllocMtrls", list);
			this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult action)
			{
				this.FillEntity();
			});
		}

		// Token: 0x06000954 RID: 2388 RVA: 0x0006F3E8 File Offset: 0x0006D5E8
		private void SetSyncCheckSchedule()
		{
			long value = MFGBillUtil.GetValue<long>(this.View.Model, "FCreateOrgId", -1, 0L, null);
			long value2 = MFGBillUtil.GetValue<long>(this.View.Model, "FUseOrgId", -1, 0L, null);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "ENG_BomSyncCheckScheParam";
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.CustomParams.Add("ENG_BomSyncCheckScheduleParam_CreateOrgId", value.ToString());
			dynamicFormShowParameter.CustomParams.Add("ENG_BomSyncCheckScheduleParam_UseOrgId", value2.ToString());
			dynamicFormShowParameter.CustomParams.Add("ENG_BomSyncCheckScheduleParam_Name", ResManager.LoadKDString("物料清单同步检查调度任务", "015072000016559", 7, new object[0]));
			dynamicFormShowParameter.CustomParams.Add("ENG_BomSyncCheckScheduleParam_PluginClass", "Kingdee.K3.MFG.ENG.App.Core.BacksageSchedule.BomSyncCheckSchedule,Kingdee.K3.MFG.ENG.App.Core");
			this.View.ShowForm(dynamicFormShowParameter);
		}
	}
}
