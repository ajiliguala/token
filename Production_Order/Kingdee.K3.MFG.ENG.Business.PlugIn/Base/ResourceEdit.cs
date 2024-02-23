using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000044 RID: 68
	public class ResourceEdit : BaseControlEdit
	{
		// Token: 0x060004A1 RID: 1185 RVA: 0x0003918E File Offset: 0x0003738E
		public override void OnBillInitialize(BillInitializeEventArgs e)
		{
			base.OnBillInitialize(e);
		}

		// Token: 0x060004A2 RID: 1186 RVA: 0x00039198 File Offset: 0x00037398
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			base.AfterCreateNewEntryRow(e);
			if (e.Entity.EntryName.Equals("ResourceDetailEntry") && this.src_AddNewResDetail.Equals("BarItemClick"))
			{
				string value = MFGBillUtil.GetValue<string>(base.View.Model, "FRSRCCategory", -1, null, null);
				string a;
				if ((a = value) != null)
				{
					if (a == "10")
					{
						base.View.Model.SetValue("FResourceTypeId", "ENG_Equipment", e.Row);
						return;
					}
					if (a == "20")
					{
						base.View.Model.SetValue("FResourceTypeId", "BD_Empinfo", e.Row);
						return;
					}
					if (!(a == "30"))
					{
						return;
					}
					base.View.Model.SetValue("FResourceTypeId", "ENG_Mould", e.Row);
				}
			}
		}

		// Token: 0x060004A3 RID: 1187 RVA: 0x00039288 File Offset: 0x00037488
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbSplitButton_AddNew") && !(barItemKey == "tbNewResDetail") && !(barItemKey == "tbInsResDetail"))
				{
					return;
				}
				this.src_AddNewResDetail = "BarItemClick";
				this.SetComboField(base.View.Model.GetValue("FRSRCCategory").ToString());
			}
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x000392F9 File Offset: 0x000374F9
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			this.SetResDetailName();
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x00039308 File Offset: 0x00037508
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.SetIsSetWorkCal();
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x00039318 File Offset: 0x00037518
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString().ToUpper()) != null)
			{
				if (!(a == "UNAUDIT"))
				{
					return;
				}
				if (this.firstDoOperation && !this.CanUnAudit(e))
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x060004A7 RID: 1191 RVA: 0x00039370 File Offset: 0x00037570
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			this.firstDoOperation = true;
		}

		// Token: 0x060004A8 RID: 1192 RVA: 0x000393A0 File Offset: 0x000375A0
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			if (e.Key.Equals("FbtnSetWorkCalSetup".ToUpper()) && this.VaildateWCSViewPermission())
			{
				T_WorkCalSetupFormParam t_WorkCalSetupFormParam = new T_WorkCalSetupFormParam();
				if (this.Model.DataObject["UseOrgId"] != null && this.Model.DataObject["UseOrgId"] is DynamicObject)
				{
					t_WorkCalSetupFormParam.useOrg = (long)Convert.ToInt32((this.Model.DataObject["UseOrgId"] as DynamicObject)["Id"]);
				}
				t_WorkCalSetupFormParam.treeNodeId = new DiffCalendarOption.NodeInfo(4, (base.View.Model.GetPKValue() == null) ? 0L : Convert.ToInt64(base.View.Model.GetPKValue())).FullNodeId;
				base.View.Session["FormInputParam"] = t_WorkCalSetupFormParam;
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = "ENG_WorkCalSetup";
				dynamicFormShowParameter.ParentPageId = base.View.PageId;
				base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
				{
					base.View.Session["FormInputParam"] = null;
					this.SetIsSetWorkCal();
				});
			}
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x000394D8 File Offset: 0x000376D8
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			if (e.Cancel)
			{
				return;
			}
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FIsShareRsc"))
				{
					return;
				}
				if (base.View.Model.GetPKValue() == null)
				{
					return;
				}
				int num = Convert.ToInt32(base.View.Model.GetPKValue());
				if (num == 0)
				{
					return;
				}
				if (!this.bTrigChange)
				{
					return;
				}
				bool value = MFGBillUtil.GetValue<bool>(this.Model, e.Key, e.Row, false, null);
				bool flag = (bool)e.Value;
				if (value == flag)
				{
					return;
				}
				e.Cancel = true;
				if (!this.CanShareRscChanged(value, num))
				{
					this.bTrigChange = false;
					base.View.Model.SetValue("FIsShareRsc", value, e.Row);
					base.View.UpdateView();
					this.bTrigChange = true;
					return;
				}
				this.bTrigChange = false;
				base.View.Model.SetValue("FIsShareRsc", flag, e.Row);
				this.bTrigChange = true;
			}
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x000395EC File Offset: 0x000377EC
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (key == "FCapaUnitType")
				{
					base.View.Model.SetValue("FCapacityUnitID", null);
					base.View.UpdateView("FCapacityUnitID");
					return;
				}
				if (!(key == "FIsCalcCapacity"))
				{
					if (!(key == "FResId"))
					{
						if (!(key == "FRSRCCategory"))
						{
							return;
						}
						if ((!StringUtils.EqualsIgnoreCase("20", e.OldValue.ToString()) || !StringUtils.EqualsIgnoreCase("10", e.NewValue.ToString())) && (!StringUtils.EqualsIgnoreCase("10", e.OldValue.ToString()) || !StringUtils.EqualsIgnoreCase("20", e.NewValue.ToString())))
						{
							this.ClearEntryData();
						}
						this.SetComboField(e.NewValue.ToString());
					}
					else
					{
						if (e.NewValue != null)
						{
							this.SetResDetailNameByEntry(false);
							return;
						}
						this.SetResDetailNameByEntry(true);
						return;
					}
				}
				else if ((bool)e.NewValue)
				{
					string value = MFGBillUtil.GetValue<string>(base.View.Model, "FCapaUnitType", -1, null, null);
					if (value == null || value.Length == 0)
					{
						base.View.Model.SetValue("FCapaUnitType", "20");
						base.View.UpdateView("FCapaUnitType");
						return;
					}
				}
			}
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x00039764 File Offset: 0x00037964
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FCapacityUnitID"))
				{
					if (!(fieldKey == "FResId"))
					{
						return;
					}
					string value = MFGBillUtil.GetValue<string>(this.Model, "FResourceTypeId", e.Row, null, null);
					long curResId = OtherExtend.ConvertTo<long>(base.View.Model.GetPKValue(), 0L);
					string userParam = MFGBillUtil.GetUserParam<string>(base.View, "EmployeeFilter", null);
					string text = string.Format(" EXISTS (SELECT FID FROM (select distinct se.FID from T_BD_STAFFTEMP se, T_BD_STAFF st where se.FID=st.FEMPINFOID \r\n                              and st.FFORBIDSTATUS='A' and st.FDOCUMENTSTATUS = 'C' and se.FWORKORGID={0}) AS S WHERE S.FID = fid) ", MFGBillUtil.GetMainOrgId<long>(base.View));
					if ("BD_Empinfo".Equals(value) && userParam != null && userParam.Equals("A"))
					{
						e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
					}
					if ("BD_Empinfo".Equals(value) && userParam != null && userParam.Equals("B"))
					{
						e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
						e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, this.GetResExcludeFilter(curResId));
					}
					string userParam2 = MFGBillUtil.GetUserParam<string>(base.View, "EquipmentFilter", null);
					if ("ENG_Equipment".Equals(value) && userParam2 != null && userParam2.Equals("B"))
					{
						e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, this.GetResExcludeFilter(curResId));
					}
					if ("ENG_Mould".Equals(value))
					{
						long num = Convert.ToInt64((base.View.Model.GetValue("FUseOrgId") as DynamicObject)[0]);
						e.ListFilterParameter.Filter = string.Format("FDOCUMENTSTATUS='C' AND FFORBIDSTATUS='A' AND FStatus <> 'C' AND FUSEORGID = {0}", num);
					}
				}
				else
				{
					string value2 = MFGBillUtil.GetValue<string>(this.Model, "FCapaUnitType", -1, null, null);
					if ("10".Equals(value2))
					{
						string text2 = string.Format(" {0} = {1} ", "FUnitGroupId", 10087L);
						e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text2);
						return;
					}
					string text3 = string.Format(" {0} != {1} ", "FUnitGroupId", 10087L);
					e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text3);
					return;
				}
			}
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x00039A44 File Offset: 0x00037C44
		private bool CanUnAudit(BeforeDoOperationEventArgs e)
		{
			this.firstDoOperation = false;
			bool bCheckResult = false;
			List<DynamicObject> workCenterByRscId = this.getWorkCenterByRscId(Convert.ToInt32(this.Model.DataObject["Id"]).ToString());
			if (workCenterByRscId != null)
			{
				bCheckResult = (workCenterByRscId.Count <= 0);
				if (workCenterByRscId.Count > 0)
				{
					string arg = string.Join<object>(",", from p in workCenterByRscId
					select p["Number"]);
					string text = string.Format(ResManager.LoadKDString("当前资源已被工作中心[{0}]引用,请确认是否反审核?", "015072000001795", 7, new object[0]), arg);
					base.View.ShowMessage(text, 4, delegate(MessageBoxResult result)
					{
						if (result == 6)
						{
							this.View.InvokeFormOperation(e.Operation.FormOperation.Operation.ToString());
							bCheckResult = true;
							return;
						}
						this.firstDoOperation = true;
					}, "", 0);
				}
			}
			return bCheckResult;
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x00039B40 File Offset: 0x00037D40
		private bool VaildateWCSViewPermission()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = "ENG_WorkCalSetup",
				SubSystemId = base.View.Model.SubSytemId
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("需要配置工作日历设置的查看权限", "005023000000587", 3, new object[0]), ResManager.LoadKDString("没有查看权限", "005023000000588", 3, new object[0]), 0);
			}
			return permissionAuthResult.Passed;
		}

		// Token: 0x060004AE RID: 1198 RVA: 0x00039BDC File Offset: 0x00037DDC
		private bool CanShareRscChanged(bool oldVal, int rscID)
		{
			bool result = true;
			if (oldVal)
			{
				List<DynamicObject> workCenterByRscId = this.getWorkCenterByRscId(rscID.ToString());
				if (workCenterByRscId == null)
				{
					return result;
				}
				if (workCenterByRscId.Count > 1)
				{
					string arg = string.Join<object>(",", from p in workCenterByRscId
					select p["Number"]);
					string text = string.Format(ResManager.LoadKDString("资源被多个工作中心[{0}]引用,不能取消共享", "015072000001795", 7, new object[0]), arg);
					base.View.ShowMessage(ResManager.LoadKDString(text, "015078000002332", 7, new object[0]), 0);
					result = false;
				}
			}
			return result;
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x00039C7C File Offset: 0x00037E7C
		private List<DynamicObject> getWorkCenterByRscId(string rscID)
		{
			string text = string.Format("{0}={1}", "FResourceID", rscID);
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FID"),
				new SelectorItemInfo("FNUMBER"),
				new SelectorItemInfo("FRESOURCEID")
			};
			return MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_WorkCenter", list, text, "");
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x00039CEC File Offset: 0x00037EEC
		private List<DynamicObject> getWorkCalByRscId(string rscID)
		{
			string text = string.Format("{0}={1} and {2} = {3}", new object[]
			{
				"FCalUserType",
				"'ENG_Resource'",
				"FCalUserId",
				rscID
			});
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FID")
			};
			return MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_WorkCalCustom", list, text, "");
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x00039D60 File Offset: 0x00037F60
		private void SetIsSetWorkCal()
		{
			if (base.View.Model.GetPKValue() != null)
			{
				int num = Convert.ToInt32(base.View.Model.GetPKValue());
				if (num != 0)
				{
					List<DynamicObject> workCalByRscId = this.getWorkCalByRscId(num.ToString());
					if (workCalByRscId.Count > 0 && workCalByRscId[0]["Id"] != null)
					{
						base.View.GetControl("FIsSetWorCal").SetValue(true);
						return;
					}
					base.View.GetControl("FIsSetWorCal").SetValue(false);
				}
			}
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x00039DFC File Offset: 0x00037FFC
		private void SetResDetailName()
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FResourceDetailEntry");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			if (entityDataObject.Count > 0)
			{
				foreach (DynamicObject dynamicObject in entityDataObject)
				{
					if (dynamicObject["ResId"] != null)
					{
						string text = OtherExtend.ConvertTo<string>(((DynamicObject)dynamicObject["ResId"])["Name"], null);
						base.View.Model.SetValue("FResName", text, (int)dynamicObject["seq"] - 1);
					}
				}
			}
		}

		// Token: 0x060004B3 RID: 1203 RVA: 0x00039EC4 File Offset: 0x000380C4
		private void SetResDetailNameByEntry(bool isBaseDataNull)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FResourceDetailEntry");
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FResourceDetailEntry");
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entryEntity, entryCurrentRowIndex);
			if (isBaseDataNull)
			{
				base.View.Model.SetValue("FResName", null, (int)entityDataObject["seq"] - 1);
				return;
			}
			string text = OtherExtend.ConvertTo<string>(((DynamicObject)entityDataObject["ResId"])["Name"], null);
			base.View.Model.SetValue("FResName", text, (int)entityDataObject["seq"] - 1);
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x00039F7C File Offset: 0x0003817C
		private string GetResExcludeFilter(long curResId)
		{
			string arg = " ";
			if (curResId > 0L)
			{
				arg = string.Format(" and head.fid <> {0} ", curResId);
			}
			return string.Format(" NOT EXISTS (SELECT TOP 1 1 FROM T_ENG_RESOURCEDETAIL detail LEFT OUTER JOIN  T_ENG_RESOURCE head ON  detail.FID = head.FID WHERE detail.fresid = t0.FID  and head.fforbidstatus = 'A' {0})", arg);
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x00039FB0 File Offset: 0x000381B0
		private void SetComboField(string resType)
		{
			if (StringUtils.EqualsIgnoreCase("30", resType))
			{
				List<EnumItem> list = new List<EnumItem>();
				ComboFieldEditor control = base.View.GetControl<ComboFieldEditor>("FResourceTypeId");
				list.Add(new EnumItem
				{
					Value = "ENG_Mould",
					Caption = new LocaleValue(ResManager.LoadKDString("工装", "015072000012782", 7, new object[0]), base.Context.UserLocale.LCID)
				});
				control.SetComboItems(list);
			}
			if (StringUtils.EqualsIgnoreCase("10", resType) || StringUtils.EqualsIgnoreCase("20", resType))
			{
				List<EnumItem> list2 = new List<EnumItem>();
				ComboFieldEditor control2 = base.View.GetControl<ComboFieldEditor>("FResourceTypeId");
				list2.Add(new EnumItem
				{
					Value = "ENG_Equipment",
					Caption = new LocaleValue(ResManager.LoadKDString("机器", "015072000012783", 7, new object[0]), base.Context.UserLocale.LCID)
				});
				list2.Add(new EnumItem
				{
					Value = "BD_Empinfo",
					Caption = new LocaleValue(ResManager.LoadKDString("人员", "015072000012784", 7, new object[0]), base.Context.UserLocale.LCID)
				});
				control2.SetComboItems(list2);
			}
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x0003A108 File Offset: 0x00038308
		private void ClearEntryData()
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FResourceDetailEntry");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			if (entityDataObject.Count == 0)
			{
				return;
			}
			entityDataObject.Clear();
			base.View.UpdateView("FResourceDetailEntry");
		}

		// Token: 0x04000200 RID: 512
		private const string FKey_FResourceID = "FRESOURCEID";

		// Token: 0x04000201 RID: 513
		private const string FKey_FNumber = "FNUMBER";

		// Token: 0x04000202 RID: 514
		private const string FKey_FID = "FID";

		// Token: 0x04000203 RID: 515
		private const long CONST_TIMEUNITGROUP = 10087L;

		// Token: 0x04000204 RID: 516
		private const string CONST_UNITGROUPID = "FUnitGroupId";

		// Token: 0x04000205 RID: 517
		private const string Flag_BarItemClick = "BarItemClick";

		// Token: 0x04000206 RID: 518
		private const string Flag_BasedateReturn = "BasedateReturn";

		// Token: 0x04000207 RID: 519
		private bool firstDoOperation = true;

		// Token: 0x04000208 RID: 520
		private bool bTrigChange = true;

		// Token: 0x04000209 RID: 521
		private string src_AddNewResDetail = "";

		// Token: 0x0400020A RID: 522
		private string selBasedataFormId = "";

		// Token: 0x0400020B RID: 523
		private string resourceTypeId = "";
	}
}
