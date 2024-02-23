using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.KDThread;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200007A RID: 122
	public class BomQueryIntegration : BomQueryForward
	{
		// Token: 0x0600092F RID: 2351 RVA: 0x0006D23E File Offset: 0x0006B43E
		protected override List<DynamicObject> GetBomChildData(List<DynamicObject> lstExpandSource, MemBomExpandOption_ForPSV memBomExpandOption)
		{
			memBomExpandOption.IsConvertUnitQty = true;
			return BomQueryServiceHelper.GetBomQueryIntegrationResult(base.Context, lstExpandSource, memBomExpandOption);
		}

		// Token: 0x06000930 RID: 2352 RVA: 0x0006D254 File Offset: 0x0006B454
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (fieldKey == "FBillBomId")
				{
					IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
					listFilterParameter.Filter += string.Format(" {0} FMaterialId={1}", ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? "" : "And", OtherExtend.ConvertTo<long>(this.Model.DataObject["BillMaterialId_Id"], 0L));
					return;
				}
				if (!(fieldKey == "FBomUseOrgId"))
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

		// Token: 0x06000931 RID: 2353 RVA: 0x0006D308 File Offset: 0x0006B508
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FBillBomId"))
				{
					if (!(key == "FBomVersion"))
					{
						return;
					}
					DynamicObjectCollection dynamicObjectCollection = this.Model.GetValue("FBomVersion") as DynamicObjectCollection;
					if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
					{
						this.Model.SetValue("FBillBomId", dynamicObjectCollection[0]["BomVersion_Id"]);
						return;
					}
					this.Model.SetValue("FBillBomId", 0);
				}
				else
				{
					DynamicObject dynamicObject = this.Model.GetValue("FBillBomId") as DynamicObject;
					if (dynamicObject != null)
					{
						this.Model.SetValue("FBomVersion", dynamicObject["Id"]);
						return;
					}
					this.Model.SetValue("FBomVersion", 0);
					return;
				}
			}
		}

		// Token: 0x06000932 RID: 2354 RVA: 0x0006D3EC File Offset: 0x0006B5EC
		protected override void InitializeBomQueryOption()
		{
			DateTime sysDate = MFGServiceHelper.GetSysDate(base.Context);
			MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
			this.memBomExpandOption = new MemBomExpandOption_ForPSV();
			this.memBomExpandOption.ValidDate = new DateTime?(sysDate);
			this.memBomExpandOption.IsExpandForbidden = false;
			this.memBomExpandOption.BomExpandId = SequentialGuid.NewGuid().ToString();
			this.memBomExpandOption.ParentCsdYieldRate = true;
			this.memBomExpandOption.ChildCsdYieldRate = true;
			this.memBomExpandOption.ExpandSkipRow = true;
			this.memBomExpandOption.DeleteSkipRow = false;
			this.memBomExpandOption.Mode = 0;
		}

		// Token: 0x06000933 RID: 2355 RVA: 0x0006D498 File Offset: 0x0006B698
		protected override void UpdateBomQueryOption()
		{
			this.memBomExpandOption.ExpandLevelTo = this.memBomExpandOption.BomMaxLevel;
			this.memBomExpandOption.IsExpandForbidden = false;
			this.memBomExpandOption.Mode = 0;
			this.memBomExpandOption.MaterialNum = MFGBillUtil.GetValue<int>(this.Model, "FQty", -1, 0, null);
			this.memBomExpandOption.IsShowOutSource = DataEntityExtend.GetDynamicObjectItemValue<bool>(this.Model.DataObject, "ErpClsID", false);
			this.memBomExpandOption.BomExpandCalType = 0;
			DateTime? dateTime = new DateTime?(MFGBillUtil.GetValue<DateTime>(this.Model, "FValidDate", -1, MFGServiceHelper.GetSysDate(base.Context), null));
			if (dateTime >= KDTimeZone.MinSystemDateTime)
			{
				this.memBomExpandOption.ValidDate = new DateTime?(dateTime.Value);
			}
			if (DataEntityExtend.GetDynamicValue<bool>(this.Model.DataObject, "IsIntShowSubMtrl", false))
			{
				this.memBomExpandOption.CsdSubstitution = true;
				this.memBomExpandOption.IsExpandSubMtrl = false;
			}
			else
			{
				this.memBomExpandOption.CsdSubstitution = false;
				this.memBomExpandOption.IsExpandSubMtrl = false;
			}
			bool value = MFGBillUtil.GetValue<bool>(this.View.Model, "FIsHideOutSourceBOM", -1, false, null);
			this.memBomExpandOption.IsHideOutSourceBOM = value;
		}

		// Token: 0x06000934 RID: 2356 RVA: 0x0006D5EB File Offset: 0x0006B7EB
		protected override string GetBillName()
		{
			return "ENG_BomQueryIntegration";
		}

		// Token: 0x06000935 RID: 2357 RVA: 0x0006D984 File Offset: 0x0006BB84
		protected override void FillBomChildData()
		{
			ViewUtils.ShowProcessForm(this.View, delegate(FormResult t)
			{
			}, true, ResManager.LoadKDString("正在查询数据，请稍候...", "015072000039150", 7, new object[0]));
			MainWorker.QuequeTask(delegate()
			{
				CultureInfoUtils.SetCurrentLanguage(base.Context);
				try
				{
					this.View.Session.Clear();
					this.View.Session["ProcessRateValue"] = 10;
					this.UpdateBomQueryOption();
					this.Model.DeleteEntryData("FBottomEntity");
					if (StringUtils.EqualsIgnoreCase(this.View.BillBusinessInfo.GetForm().Id, "ENG_BomQueryForward2"))
					{
						this.Model.DeleteEntryData("FCobyEntity");
						this.View.Model.SetValue("FSelectMaterialId", 0, -1);
					}
					int iForceRow = 0;
					List<DynamicObject> list = this.BuildBomExpandSourceData(iForceRow);
					this.View.Session["ProcessRateValue"] = 30;
					if (ListUtils.IsEmpty<DynamicObject>(list))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("当前物料对应的BOM不存在，请确认！", "015072000003341", 7, new object[0]), ResManager.LoadKDString("BOM不存在！", "015072000002208", 7, new object[0]), 0);
					}
					else
					{
						this.bomQueryChildItems = this.GetBomChildData(list, this.memBomExpandOption);
						this.View.Session["ProcessRateValue"] = 60;
						this.bomPrintChildItems = this.bomQueryChildItems;
						if (StringUtils.EqualsIgnoreCase(this.View.BillBusinessInfo.GetForm().Id, "ENG_BomQueryForward2"))
						{
							base.IsShowSubstituteMaterials();
						}
						base.BindChildEntitys();
						Entity entity = this.Model.BusinessInfo.GetEntity("FBottomEntity");
						DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
						if (entityDataObject.Count != 0)
						{
							List<KeyValuePair<int, string>> list2 = new List<KeyValuePair<int, string>>();
							foreach (DynamicObject dynamicObject in entityDataObject)
							{
								foreach (Field field in entity.Fields)
								{
									if (!(field.Key.ToUpper() != "FERPCLSID") && dynamicObject["ErpClsId"] != null && (Convert.ToString(dynamicObject["ErpClsId"]) == "2" || Convert.ToString(dynamicObject["ErpClsId"]) == "3" || Convert.ToString(dynamicObject["ErpClsId"]) == "5") && (string.IsNullOrEmpty(Convert.ToString(dynamicObject["BomId_Id"])) || Convert.ToString(dynamicObject["BomId_Id"]) == "0"))
									{
										list2.Add(new KeyValuePair<int, string>(entityDataObject.IndexOf(dynamicObject), "#ff0000"));
									}
								}
							}
							EntryGrid control = this.View.GetControl<EntryGrid>("FBottomEntity");
							control.SetRowBackcolor(list2);
							this.View.Session["ProcessRateValue"] = 100;
						}
					}
				}
				finally
				{
					this.View.Session["ProcessRateValue"] = 100;
				}
			}, delegate(AsynResult t)
			{
			});
		}

		// Token: 0x06000936 RID: 2358 RVA: 0x0006D9FF File Offset: 0x0006BBFF
		protected override string GetFilterName()
		{
			return "ENG_BomQueryIntegration_Filter";
		}

		// Token: 0x06000937 RID: 2359 RVA: 0x0006DA14 File Offset: 0x0006BC14
		public override void BeforeEntityExport(BeforeEntityExportArgs e)
		{
			if (!e.ExportEntityKeyList.Any((string w) => StringUtils.EqualsIgnoreCase(w, "FBillHead")))
			{
				e.ExportEntityKeyList.Add("FBillHead");
				e.Headers.Add("FBillHead", null);
				e.DataSource.Add("FBillHead", new List<DynamicObject>
				{
					this.View.Model.DataObject
				});
			}
		}

		// Token: 0x06000938 RID: 2360 RVA: 0x0006DA9C File Offset: 0x0006BC9C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			if ((e.Operation.FormOperation.IsEqualOperation("Print") || e.Operation.FormOperation.IsEqualOperation("EntityExport")) && !MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BillBusinessInfo.GetForm().Id, e.Operation.FormOperation.PermissionItemId))
			{
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("没有物料清单汇总查询的{0}权限", "015072000019426", 7, new object[0]), e.Operation.FormOperation.OperationName[base.Context.UserLocale.LCID]), "", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x06000939 RID: 2361 RVA: 0x0006DB68 File Offset: 0x0006BD68
		public override void OnPrepareNotePrintData(PreparePrintDataEventArgs e)
		{
			BusinessInfo businessInfo = this.View.BusinessInfo;
			List<DynamicObject> list = null;
			if (e.DataSourceId.Equals("FBillHead", StringComparison.OrdinalIgnoreCase))
			{
				list = new DynamicObject[]
				{
					this.View.Model.DataObject
				}.ToList<DynamicObject>();
			}
			if (e.DataSourceId.Equals("FBottomEntity", StringComparison.OrdinalIgnoreCase))
			{
				list = this.bomQueryChildItems;
			}
			e.DataObjects = MFGCommonUtil.ReflushDynamicObjectTypeSource(this.View, businessInfo, e.DataSourceId, e.DynamicObjectType, list);
		}

		// Token: 0x04000465 RID: 1125
		private const string ENTITYKEY_TOPENTITY = "FTopEntity";

		// Token: 0x04000466 RID: 1126
		private const string ENTITYKEY_BOTTOMENTITY = "FBottomEntity";

		// Token: 0x04000467 RID: 1127
		private const string FIELDKEY_FQTY = "FQty";

		// Token: 0x04000468 RID: 1128
		private const string FIELDKEY_FVALIDDATE = "FValidDate";

		// Token: 0x04000469 RID: 1129
		private const string FIELDKEY_ISOUTSOURCE = "ErpClsID";
	}
}
