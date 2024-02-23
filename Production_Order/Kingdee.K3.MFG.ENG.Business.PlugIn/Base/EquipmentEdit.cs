using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200004C RID: 76
	public class EquipmentEdit : BaseControlEdit
	{
		// Token: 0x06000535 RID: 1333 RVA: 0x0003F17E File Offset: 0x0003D37E
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.GetStatusLink();
		}

		// Token: 0x06000536 RID: 1334 RVA: 0x0003F190 File Offset: 0x0003D390
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				string text;
				if (a == "FUSEDEPTID")
				{
					text = string.Empty;
					text = StringUtils.JoinFilterString(text, string.Format(" FDEPTPROPERTY ='4866f13a3a3940b9b2fe47895a6e7cbe'  ", new object[0]), "AND");
					text = StringUtils.JoinFilterString(text, string.Format(" FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS='A'", new object[0]), "AND");
					e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
					return;
				}
				if (!(a == "FPRODUCTEVENT"))
				{
					return;
				}
				text = string.Empty;
				text = StringUtils.JoinFilterString(text, string.Format(" FUSEORGID ={0}  ", Convert.ToInt64(base.View.Model.DataObject["UseOrgId_Id"])), "AND");
				text = StringUtils.JoinFilterString(text, string.Format(" FISSYSPRESET=0 and FEFFECTSTATUS<>'F' and FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS='A' ", new object[0]), "AND");
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
			}
		}

		// Token: 0x06000537 RID: 1335 RVA: 0x0003F310 File Offset: 0x0003D510
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FPROCESSTYPE"))
				{
					return;
				}
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.Value))
				{
					Entity entity = base.View.BusinessInfo.GetEntity("FEntity");
					List<DynamicObject> source = base.View.Model.GetEntityDataObject(entity).ToList<DynamicObject>();
					List<string> processTypes = (from o in source
					select Convert.ToString(o["ProcessType"])).ToList<string>();
					processTypes.RemoveAt(e.Row);
					processTypes.Add(Convert.ToString(e.Value));
					List<string> source2 = new List<string>
					{
						"B",
						"F",
						"M"
					};
					if (source2.All((string t) => processTypes.Any((string d) => StringUtils.EqualsIgnoreCase(d, t))))
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("处理类型：批量报工、批量报工-完工、批量报工-不良品，三种类型只能任选其二！", "015072030034798", 7, new object[0]), "", 0);
						e.Cancel = true;
					}
				}
			}
		}

		// Token: 0x06000538 RID: 1336 RVA: 0x0003F450 File Offset: 0x0003D650
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			if (StringUtils.EqualsIgnoreCase("Mobile", base.Context.ClientType.ToString()))
			{
				return;
			}
			string baseDataFieldKey;
			if (e.BaseDataField is BaseDataField && (baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (!(baseDataFieldKey == "FUseDeptId"))
				{
					return;
				}
				string text = string.Empty;
				text = StringUtils.JoinFilterString(text, string.Format(" FDEPTPROPERTY ='4866f13a3a3940b9b2fe47895a6e7cbe'  ", new object[0]), "AND");
				text = StringUtils.JoinFilterString(text, string.Format("  FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS='A'", new object[0]), "AND");
				e.Filter = base.SqlAppendAnd(e.Filter, text);
			}
		}

		// Token: 0x06000539 RID: 1337 RVA: 0x0003F4FC File Offset: 0x0003D6FC
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString().ToUpper()) != null)
			{
				if (a == "STATUSCHANGE")
				{
					this.ShowForm("ENG_EquipmentStatusChange");
					return;
				}
				if (!(a == "CHANGELOG"))
				{
					return;
				}
				this.ShowList1("ENG_EqmStatusChgLogDym");
			}
		}

		// Token: 0x0600053A RID: 1338 RVA: 0x0003F560 File Offset: 0x0003D760
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbDeleteEntry"))
				{
					return;
				}
				if (e.ParentKey.Equals("FENTITY"))
				{
					this.checkStatusMonitor(e);
				}
			}
		}

		// Token: 0x0600053B RID: 1339 RVA: 0x0003F5A8 File Offset: 0x0003D7A8
		public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterEntryBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbAddLine"))
				{
					return;
				}
				base.View.Model.SetValue("FCloudStatus", "A", base.View.Model.GetEntryRowCount("FEntityLink") - 1);
				base.View.UpdateView("FCloudStatus");
			}
		}

		// Token: 0x0600053C RID: 1340 RVA: 0x0003F67C File Offset: 0x0003D87C
		private bool CanUnAudit(BeforeDoOperationEventArgs e)
		{
			this.firstDoOperation = false;
			bool canUnAudit = true;
			string text = string.Format("{0}={1}", "FRESNUMBER", Convert.ToInt32(this.Model.DataObject["Id"]).ToString());
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FID"),
				new SelectorItemInfo("FNUMBER"),
				new SelectorItemInfo("FRESNUMBER")
			};
			List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_Resource", list, text, "");
			if (baseBillInfo != null)
			{
				canUnAudit = (baseBillInfo.Count <= 0);
				if (baseBillInfo.Count > 0)
				{
					string arg = string.Join<object>(",", from p in baseBillInfo
					select p["Number"]);
					string text2 = string.Format(ResManager.LoadKDString("当前设备已被资源[{0}]引用,请确认是否反审核?", "015072000013852", 7, new object[0]), arg);
					base.View.ShowMessage(text2, 4, delegate(MessageBoxResult result)
					{
						if (result == 6)
						{
							this.View.InvokeFormOperation(e.Operation.FormOperation.Operation.ToString());
							canUnAudit = true;
							return;
						}
						this.firstDoOperation = true;
					}, "", 0);
				}
			}
			return canUnAudit;
		}

		// Token: 0x0600053D RID: 1341 RVA: 0x0003F7E8 File Offset: 0x0003D9E8
		private void ShowForm(string formId)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = formId;
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.CustomParams.Add("EquipmentId", base.View.Model.GetPKValue().ToString());
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
			{
				base.View.Refresh();
			});
		}

		// Token: 0x0600053E RID: 1342 RVA: 0x0003F85C File Offset: 0x0003DA5C
		private void ShowList(string formId)
		{
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = formId;
			listShowParameter.ParentPageId = base.View.PageId;
			listShowParameter.IsLookUp = false;
			listShowParameter.ListFilterParameter.Filter = string.Format(" FEquipmentId = {0} ", base.View.Model.GetPKValue());
			listShowParameter.IsShowUsed = true;
			listShowParameter.OpenStyle.ShowType = 6;
			listShowParameter.Height = 600;
			listShowParameter.Width = 900;
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x0600053F RID: 1343 RVA: 0x0003F8E8 File Offset: 0x0003DAE8
		private void ShowList1(string formId)
		{
			List<long> list = new List<long>();
			list.Add(Convert.ToInt64(base.View.Model.GetPKValue()));
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = formId;
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.Height = 600;
			dynamicFormShowParameter.Width = 900;
			dynamicFormShowParameter.CustomComplexParams["eqmIds"] = list.ToList<long>();
			dynamicFormShowParameter.CustomComplexParams["IsOpenByEqm"] = true;
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x06000540 RID: 1344 RVA: 0x0003F990 File Offset: 0x0003DB90
		private void GetStatusLink()
		{
			if (base.View.OpenParameter.Status.Equals(0))
			{
				Entity entity = base.View.Model.BusinessInfo.GetEntity("FEntityLink");
				Field field = base.View.Model.BusinessInfo.GetField("FCloudStatus");
				ComboField comboField = field as ComboField;
				DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)comboField.EnumObject["Items"];
				int num = 0;
				base.View.Model.DeleteEntryData("FEntityLink");
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					string text = dynamicObject["Value"].ToString();
					if (!text.Equals("B") && !text.Equals("D"))
					{
						base.View.Model.CreateNewEntryRow(entity, num);
						base.View.Model.SetValue("FCloudStatus", "B", num);
						base.View.Model.SetValue("FCloudStatus", text, num);
						num++;
					}
				}
			}
		}

		// Token: 0x06000541 RID: 1345 RVA: 0x0003FB60 File Offset: 0x0003DD60
		private void checkStatusMonitor(BarItemClickEventArgs e)
		{
			DynamicObject entryCurrentRow = this.GetEntryCurrentRow();
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(entryCurrentRow))
			{
				return;
			}
			DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(entryCurrentRow, "StatusMonitor", null);
			if (!dynamicObjectItemValue.Any((DynamicObject p) => DataEntityExtend.GetDynamicObjectItemValue<decimal>(p, "UpperLimit", 0m) != 0m))
			{
				if (!dynamicObjectItemValue.Any((DynamicObject p) => DataEntityExtend.GetDynamicObjectItemValue<decimal>(p, "LowerLimit", 0m) != 0m))
				{
					return;
				}
			}
			e.Cancel = true;
			base.View.ShowMessage(ResManager.LoadKDString("该仪表存在“状态监测”，是否删除？", "015072000012055", 7, new object[0]), 4, delegate(MessageBoxResult result)
			{
				if (result == 6)
				{
					this.Model.DeleteEntryRow("FEntity", this.Model.GetEntryCurrentRowIndex("FEntity"));
					base.View.UpdateView("FSubEntity");
				}
			}, "", 0);
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x0003FC18 File Offset: 0x0003DE18
		private DynamicObject GetEntryCurrentRow()
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			int entryCurrentRowIndex = this.Model.GetEntryCurrentRowIndex("FEntity");
			return this.Model.GetEntityDataObject(entryEntity, entryCurrentRowIndex);
		}

		// Token: 0x0400024B RID: 587
		private const string FKey_FEquipmentID = "FRESNUMBER";

		// Token: 0x0400024C RID: 588
		private const string FKey_FNumber = "FNUMBER";

		// Token: 0x0400024D RID: 589
		private const string FKey_FID = "FID";

		// Token: 0x0400024E RID: 590
		private bool firstDoOperation = true;
	}
}
