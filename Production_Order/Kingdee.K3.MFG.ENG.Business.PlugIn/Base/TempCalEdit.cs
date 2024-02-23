using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000019 RID: 25
	[Description("临时日历_表单插件")]
	public class TempCalEdit : BaseControlEdit
	{
		// Token: 0x06000272 RID: 626 RVA: 0x0001D30C File Offset: 0x0001B50C
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			bool flag = false;
			if (base.View.OpenParameter.GetCustomParameters().ContainsKey("FEquipmentId"))
			{
				long num = (long)base.View.OpenParameter.GetCustomParameter("FEquipmentId");
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_Equipment", true) as FormMetadata;
				DynamicObject dynamicObject = BusinessDataServiceHelper.LoadSingle(base.Context, num, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
				base.View.Model.DataObject["EquipmentId_Id"] = num;
				base.View.Model.DataObject["EquipmentId"] = dynamicObject;
				flag = true;
			}
			if (base.View.OpenParameter.GetCustomParameters().ContainsKey("FDate"))
			{
				DateTime dateTime = (DateTime)base.View.OpenParameter.GetCustomParameter("FDate");
				base.View.Model.DataObject["Date"] = dateTime;
				flag = true;
			}
			if (flag)
			{
				this.UpdateViewByEqmAndDate();
			}
			base.View.GetControl<TabControl>("FTabHead").SelectedTabItemKey = "FTabHeadGeneral";
			base.View.GetControl<TabControl>("FTabHead").SelectedIndex = 0;
		}

		// Token: 0x06000273 RID: 627 RVA: 0x0001D464 File Offset: 0x0001B664
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBCREATETEMPCALENDAR"))
				{
					return;
				}
				if (base.View.Model.DataObject["DocumentStatus"].ToString() == "Z")
				{
					base.View.ShowMessage(ResManager.LoadKDString("暂存状态的临时日历不允许套用！", "0151515151774000013708", 7, new object[0]), 0);
					return;
				}
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = "ENG_CreateTempCalendar";
				dynamicFormShowParameter.OpenStyle.ShowType = 4;
				dynamicFormShowParameter.CustomComplexParams.Add("Source", base.View.Model.DataObject);
				base.View.ShowForm(dynamicFormShowParameter);
			}
		}

		// Token: 0x06000274 RID: 628 RVA: 0x0001D52F File Offset: 0x0001B72F
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
		}

		// Token: 0x06000275 RID: 629 RVA: 0x0001D538 File Offset: 0x0001B738
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (a == "FEQUIPMENTID")
				{
					long eqmId = Convert.ToInt64((e.Value as DynamicObject)["Id"]);
					e.Cancel = (!this.IsEqmAndDateUnique(eqmId, Convert.ToDateTime(base.View.Model.GetValue("FDate"))) && this.IsEqmMultiWorkCenter(eqmId));
					return;
				}
				if (!(a == "FDATE"))
				{
					return;
				}
				e.Cancel = !this.IsEqmAndDateUnique(Convert.ToInt64(base.View.Model.DataObject["EquipmentId_Id"]), Convert.ToDateTime(e.Value));
			}
		}

		// Token: 0x06000276 RID: 630 RVA: 0x0001D604 File Offset: 0x0001B804
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpper()) != null)
			{
				if (a == "FSHIFTID")
				{
					this.ShiftChanged();
					return;
				}
				if (a == "FSHIFTSLICEID")
				{
					this.ShiftSliceChanged(e.Row);
					return;
				}
				if (a == "FEQUIPMENTID")
				{
					this.UpdateViewByEqmAndDate();
					return;
				}
				if (a == "FDATE")
				{
					this.UpdateViewByEqmAndDate();
					return;
				}
				if (!(a == "FPERIODSTARTTIME") && !(a == "FPERIODENDTIME"))
				{
					return;
				}
				this.DetailTimeChanged(e.Row);
			}
		}

		// Token: 0x06000277 RID: 631 RVA: 0x0001D6A4 File Offset: 0x0001B8A4
		public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
		{
			base.AfterDeleteRow(e);
			string a;
			if ((a = e.EntityKey.ToUpper()) != null)
			{
				if (!(a == "FSUBENTITY"))
				{
					return;
				}
				this.UpdateEntryDataByDetail();
			}
		}

		// Token: 0x06000278 RID: 632 RVA: 0x0001D6DC File Offset: 0x0001B8DC
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string operation;
			if ((operation = e.Operation.Operation) != null)
			{
				if (!(operation == "Save"))
				{
					return;
				}
				base.View.UpdateView("FEntity");
				base.View.UpdateView("FSubEntity");
			}
		}

		// Token: 0x06000279 RID: 633 RVA: 0x0001D730 File Offset: 0x0001B930
		private void DetailTimeChanged(int rowIndex)
		{
			if (base.View.Model.GetValue("FPeriodStartTime", rowIndex) != null && base.View.Model.GetValue("FPeriodEndTime", rowIndex) != null)
			{
				DateTime dateTime = Convert.ToDateTime(base.View.Model.GetValue("FPeriodStartTime", rowIndex).ToString());
				DateTime dateTime2 = Convert.ToDateTime(base.View.Model.GetValue("FPeriodEndTime", rowIndex).ToString());
				if (dateTime2 <= dateTime)
				{
					dateTime2 = dateTime2.AddDays(1.0);
				}
				TimeSpan timeSpan = dateTime2 - dateTime;
				base.View.Model.SetValue("FWorkHours", Convert.ToDecimal(timeSpan.TotalHours), rowIndex);
				this.UpdateEntryDataByDetail();
			}
		}

		// Token: 0x0600027A RID: 634 RVA: 0x0001D804 File Offset: 0x0001BA04
		private void UpdateEntryDataByDetail()
		{
			DynamicObject entryData = null;
			int entryRowIndex = 0;
			base.View.Model.TryGetEntryCurrentRow("FEntity", ref entryData, ref entryRowIndex);
			this.UpdateEntryDataByDetail(entryData, entryRowIndex);
		}

		// Token: 0x0600027B RID: 635 RVA: 0x0001D84C File Offset: 0x0001BA4C
		private void UpdateEntryDataByDetail(DynamicObject entryData, int entryRowIndex)
		{
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)entryData["TempCalDetail"];
			if (dynamicObjectCollection.Count > 0)
			{
				decimal num = (from s in dynamicObjectCollection
				select Convert.ToDecimal(s["WorkHours"])).Sum();
				this.Model.SetValue("FStartTime", dynamicObjectCollection.First<DynamicObject>()["PeriodStartTime"], entryRowIndex);
				this.Model.SetValue("FEndTime", dynamicObjectCollection.Last<DynamicObject>()["PeriodEndTime"], entryRowIndex);
				this.Model.SetValue("FHours", num, entryRowIndex);
				return;
			}
			this.Model.SetValue("FStartTime", null, entryRowIndex);
			this.Model.SetValue("FEndTime", null, entryRowIndex);
			this.Model.SetValue("FHours", null, entryRowIndex);
		}

		// Token: 0x0600027C RID: 636 RVA: 0x0001D930 File Offset: 0x0001BB30
		private void UpdateViewByEqmAndDate()
		{
			long num = Convert.ToInt64(base.View.Model.DataObject["EquipmentId_Id"]);
			DateTime dateTime = Convert.ToDateTime(base.View.Model.GetValue("FDate"));
			if (num <= 0L || !DateTimeFormatUtils.IsValidDate(dateTime))
			{
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["TempCalData"] as DynamicObjectCollection;
			base.View.Model.SetValue("FShiftId", 0);
			dynamicObjectCollection.Clear();
			base.View.UpdateView("FEntity");
			base.View.UpdateView("FSubEntity");
			base.View.UpdateView("FEquipmentId");
			base.View.UpdateView("FEquipmentName");
			base.View.UpdateView("FDate");
			if (this.IsEqmMultiWorkCenter(num))
			{
				return;
			}
			DynamicObject workDateEntityRowByEqmAndDate = CalendarServiceHelper.GetWorkDateEntityRowByEqmAndDate(base.Context, num, this.wcId, dateTime);
			if (workDateEntityRowByEqmAndDate != null)
			{
				base.View.Model.SetValue("FShiftId", workDateEntityRowByEqmAndDate["ShiftId_Id"]);
				bool flag = Convert.ToBoolean(workDateEntityRowByEqmAndDate["IsWorkTime"]);
				if (flag)
				{
					base.View.Model.SetValue("FDateStyle", "1");
				}
				else
				{
					base.View.Model.SetValue("FDateStyle", "2");
				}
				base.View.Model.SetValue("FIsWorkTime", flag);
			}
		}

		// Token: 0x0600027D RID: 637 RVA: 0x0001DAC4 File Offset: 0x0001BCC4
		private bool IsEqmMultiWorkCenter(long eqmId)
		{
			string text = "select q.fid from T_ENG_WORKCENTER q\r\njoin T_ENG_WORKCENTERDATA q1 on q.FID=q1.FID\r\njoin T_ENG_RESOURCE RS on q1.FRESOURCEID=RS.FID\r\nINNER JOIN T_ENG_RESOURCEDETAIL RS1 ON RS.FID = RS1.FID \r\nAND RS.FDOCUMENTSTATUS = 'C' AND RS.FFORBIDSTATUS = 'A' AND RS1.FRESOURCETYPEID = 'ENG_Equipment' AND RS1.FISFORBIDDEN = '0'  and q1.FJoinScheduling='1'\r\nwhere FRESID=@FEquipmentId";
			List<long> list = new List<long>();
			List<SqlParam> list2 = new List<SqlParam>();
			list2.Add(new SqlParam("@FEquipmentId", 12, eqmId));
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, text, list2))
			{
				while (dataReader.Read())
				{
					long item = Convert.ToInt64(dataReader["fid"]);
					list.Add(item);
				}
			}
			if (list.Count > 1)
			{
				base.View.ShowMessage(ResManager.LoadKDString("该设备所属资源在多个工作中心下，请调整设置。", "0151515151774000014134", 7, new object[0]), 0);
				return true;
			}
			if (list.Count == 1)
			{
				this.wcId = list.ElementAt(0);
			}
			return false;
		}

		// Token: 0x0600027E RID: 638 RVA: 0x0001DB90 File Offset: 0x0001BD90
		private void ShiftChanged()
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["TempCalData"] as DynamicObjectCollection;
			dynamicObjectCollection.Clear();
			DynamicObject dynamicObject = (DynamicObject)base.View.Model.GetValue("FShiftId");
			if (dynamicObject != null)
			{
				DynamicObjectCollection dynamicObjectCollection2 = dynamicObject["ShiftData"] as DynamicObjectCollection;
				for (int i = 0; i < dynamicObjectCollection2.Count; i++)
				{
					base.View.Model.CreateNewEntryRow("FEntity");
					DynamicObject dynamicObject2 = dynamicObjectCollection2[i];
					this.Model.SetValue("FSeq", dynamicObject2["Seq"], i);
					this.Model.SetValue("FShiftSliceId", dynamicObject2["ShiftSliceId_Id"], i);
					this.Model.SetValue("FStartTime", dynamicObject2["StartTime"], i);
					this.Model.SetValue("FEndTime", dynamicObject2["EndTime"], i);
					this.Model.SetValue("FHours", dynamicObject2["Hours"], i);
				}
			}
			base.View.UpdateView("FEntity");
			base.View.UpdateView("FSubEntity");
		}

		// Token: 0x0600027F RID: 639 RVA: 0x0001DCF0 File Offset: 0x0001BEF0
		private void ShiftSliceChanged(int entryRowIndex)
		{
			DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntryEntity("FEntity"), entryRowIndex);
			DynamicObjectCollection dynamicObjectCollection = entityDataObject["TempCalDetail"] as DynamicObjectCollection;
			dynamicObjectCollection.Clear();
			DynamicObject dynamicObject = (DynamicObject)base.View.Model.GetValue("FShiftSliceId", entryRowIndex);
			if (dynamicObject != null)
			{
				DynamicObjectCollection dynamicObjectCollection2 = (DynamicObjectCollection)dynamicObject["ShiftSliceData"];
				SubEntryEntity subEntryEntity = base.View.BusinessInfo.GetEntryEntity("FSubEntity") as SubEntryEntity;
				for (int i = 0; i < dynamicObjectCollection2.Count; i++)
				{
					DynamicObject dynamicObject2 = dynamicObjectCollection2[i];
					base.View.Model.CreateNewEntryRow(entityDataObject, subEntryEntity, i);
					DynamicObject dynamicObject3 = dynamicObjectCollection[i];
					dynamicObject3["Seq"] = dynamicObject2["Seq"];
					dynamicObject3["Remarks"] = dynamicObject2["Remarks"];
					dynamicObject3["PeriodStartTime"] = dynamicObject2["StartTime"];
					dynamicObject3["PeriodEndTime"] = dynamicObject2["EndTime"];
					dynamicObject3["WorkHours"] = dynamicObject2["Hours"];
				}
				base.View.UpdateView("FSubEntity");
				this.Model.SetValue("FStartTime", dynamicObjectCollection2.First<DynamicObject>()["StartTime"], entryRowIndex);
				this.Model.SetValue("FEndTime", dynamicObjectCollection2.Last<DynamicObject>()["EndTime"], entryRowIndex);
				decimal num = (from s in dynamicObjectCollection2
				select Convert.ToDecimal(s["Hours"])).Sum();
				this.Model.SetValue("FHours", num, entryRowIndex);
			}
		}

		// Token: 0x06000280 RID: 640 RVA: 0x0001DEDC File Offset: 0x0001C0DC
		private bool IsEqmAndDateUnique(long eqmId, DateTime date)
		{
			if (eqmId <= 0L || !DateTimeFormatUtils.IsValidDate(date))
			{
				return true;
			}
			string text = "SELECT 1 FROM T_ENG_TEMPCAL WHERE FEQUIPMENTID=@EqmId AND FDATE = @FDate AND FUSEORGID = @OrgId";
			List<SqlParam> list = new List<SqlParam>();
			list.Add(new SqlParam("@EqmId", 12, eqmId));
			list.Add(new SqlParam("@FDate", 6, date));
			list.Add(new SqlParam("@OrgId", 12, Convert.ToInt64(base.View.Model.DataObject["UseOrgId_Id"])));
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, text, list))
			{
				if (dataReader.Read())
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("该设备在当日已存在临时日历，不允许新增!", "0151515151774000013707", 7, new object[0]), "", 0);
					return false;
				}
			}
			return true;
		}

		// Token: 0x0400013A RID: 314
		private long wcId;
	}
}
