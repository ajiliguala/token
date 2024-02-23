using System;
using System.Linq;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000046 RID: 70
	public class ShiftEdit : BaseControlEdit
	{
		// Token: 0x060004D3 RID: 1235 RVA: 0x0003AF28 File Offset: 0x00039128
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FUseOrgId"))
				{
					return;
				}
				if (this.workCalSetupParam.SourceCaller == 1)
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x0003AF6C File Offset: 0x0003916C
		public override void DataChanged(DataChangedEventArgs e)
		{
			string a;
			if ((a = e.Field.Key.ToUpper()) != null)
			{
				if (!(a == "FSHIFTSLICEID"))
				{
					return;
				}
				if (base.View.Model.GetValue("FShiftSliceId", e.Row) != null)
				{
					DynamicObject shiftSliceInfo = (DynamicObject)base.View.Model.GetValue("FShiftSliceId", e.Row);
					this.setShiftData(shiftSliceInfo, e.Row);
					this.SetTotalHours();
				}
			}
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x0003AFEC File Offset: 0x000391EC
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			if (e.OperationResult != null && e.OperationResult.IsSuccess && (e.Operation.Operation.Equals("Save") || e.Operation.Operation.Equals("Submit")) && this.workCalSetupParam.SourceCaller == 1 && !MFGBillUtil.GetValue<string>(base.View.Model, "FDocumentStatus", -1, null, null).Equals("C"))
			{
				base.View.Model.SetValue("FDocumentStatus", 'C');
				base.View.Model.SetValue("FApproverId", base.Context.UserId);
				base.View.Model.SetValue("FApproveDate", MFGServiceHelper.GetSysDate(base.Context));
				base.View.InvokeFormOperation(5);
			}
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x0003B0F4 File Offset: 0x000392F4
		public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
		{
			this.SetTotalHours();
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x0003B0FC File Offset: 0x000392FC
		public override void AfterCopyRow(AfterCopyRowEventArgs e)
		{
			base.AfterCopyRow(e);
			this.SetTotalHours();
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x0003B10C File Offset: 0x0003930C
		public override void OnBillInitialize(BillInitializeEventArgs e)
		{
			base.OnBillInitialize(e);
			if (base.View.ParentFormView == null || base.View.ParentFormView.ParentFormView == null)
			{
				return;
			}
			if (base.View.ParentFormView.ParentFormView.OpenParameter.FormId.Equals("ENG_WorkCalSetup"))
			{
				this.workCalSetupParam = MFGBillUtil.GetParentFormSession<WorkCalCustomEdit.T_EditShiftParam>(base.View, "FormInputParam");
			}
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x0003B17C File Offset: 0x0003937C
		public override void AfterCreateNewData(EventArgs e)
		{
			base.AfterCreateNewData(e);
			if (this.workCalSetupParam.SourceCaller == 1)
			{
				base.View.Model.SetValue("FIsPrivate", true);
				this.Model.SetValue("FUseOrgId", this.workCalSetupParam.UserOrgId);
				if (this.workCalSetupParam.ShiftId != 0L)
				{
					this.BindShiftData();
				}
			}
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x0003B1EE File Offset: 0x000393EE
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FEntity"))
			{
				this.SetTotalHours();
			}
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x0003B210 File Offset: 0x00039410
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			if (this.workCalSetupParam.SourceCaller == 1)
			{
				long num = 0L;
				long.TryParse(Convert.ToString(base.View.Model.GetPKValue()), out num);
				if (num > 0L)
				{
					this.workCalSetupParam.ShiftId = num;
					base.View.ReturnToParentWindow(this.workCalSetupParam);
				}
			}
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x0003B279 File Offset: 0x00039479
		public override void AfterBindData(EventArgs e)
		{
			this.LockAllocateMenu();
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x0003B281 File Offset: 0x00039481
		public override void AfterUpdateViewState(EventArgs e)
		{
			this.LockAllocateMenu();
		}

		// Token: 0x060004DE RID: 1246 RVA: 0x0003B28C File Offset: 0x0003948C
		private void LockAllocateMenu()
		{
			if (this.workCalSetupParam.SourceCaller == 1)
			{
				base.View.GetMainBarItem("tbAllocate").Visible = false;
				base.View.GetMainBarItem("tbCancelAllocate").Visible = false;
				base.View.GetMainBarItem("tbAllocateInquire").Visible = false;
			}
		}

		// Token: 0x060004DF RID: 1247 RVA: 0x0003B2EC File Offset: 0x000394EC
		private void AutoSetTimePeriod(CreateNewEntryEventArgs e)
		{
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(e.Entity, e.Row);
			if (entityDataObject == null)
			{
				return;
			}
			DynamicObject dynamicObject = null;
			if (e.Row > 0)
			{
				dynamicObject = this.Model.GetEntityDataObject(e.Entity, e.Row - 1);
			}
			TimeField timeField = base.View.BillBusinessInfo.GetField("FStartTime") as TimeField;
			TimeField timeField2 = base.View.BillBusinessInfo.GetField("FEndTime") as TimeField;
			DecimalField decimalField = base.View.BillBusinessInfo.GetField("FHours") as DecimalField;
			DateTime? dateTime = null;
			if (dynamicObject != null)
			{
				dateTime = timeField2.DynamicProperty.GetValue<DateTime?>(dynamicObject);
			}
			dateTime = new DateTime?((dateTime == null) ? new DateTime(MFGServiceHelper.GetSysDate(base.Context).Year, MFGServiceHelper.GetSysDate(base.Context).Month, MFGServiceHelper.GetSysDate(base.Context).Day, 8, 0, 0) : dateTime.Value.AddMinutes(10.0));
			DateTime dateTime2 = dateTime.Value.AddHours(4.0);
			this.Model.SetValue(timeField, entityDataObject, dateTime);
			this.Model.SetValue(timeField2, entityDataObject, dateTime2);
			TimeSpan timeSpan = dateTime2 - dateTime.Value;
			this.Model.SetValue(decimalField, entityDataObject, Convert.ToDecimal(timeSpan.TotalHours));
		}

		// Token: 0x060004E0 RID: 1248 RVA: 0x0003B486 File Offset: 0x00039686
		private DateTime SetToday(DateTime dt)
		{
			dt = Convert.ToDateTime(dt.ToString("HH:mm:ss"));
			return dt;
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x0003B49C File Offset: 0x0003969C
		private void BindShiftData()
		{
			DynamicObject shiftInformation = ENGServiceHelper.GetShiftInformation(base.Context, this.workCalSetupParam.ShiftId.ToString());
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)this.Model.DataObject["ShiftData"];
			dynamicObjectCollection.Clear();
			if (shiftInformation != null)
			{
				this.Model.SetValue("FAllHours", DataEntityExtend.GetDynamicObjectItemValue<decimal>(shiftInformation, "AllHours", 0m));
				DynamicObjectCollection dynamicObjectCollection2 = (DynamicObjectCollection)shiftInformation["ShiftData"];
				int num = 0;
				foreach (DynamicObject dynamicObject in dynamicObjectCollection2)
				{
					this.Model.CreateNewEntryRow("FEntity");
					num++;
				}
			}
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x0003B574 File Offset: 0x00039774
		public void SetTotalHours()
		{
			int entryRowCount = this.Model.GetEntryRowCount("FEntity");
			decimal d = 0m;
			for (int i = 0; i < entryRowCount; i++)
			{
				d += Convert.ToDecimal(MFGBillUtil.GetValue<decimal>(base.View.Model, "FHours", i, 0m, null));
			}
			base.View.Model.SetValue("FAllHours", d.ToString());
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x0003B5FC File Offset: 0x000397FC
		private void setShiftData(DynamicObject ShiftSliceInfo, int iRow)
		{
			if (ShiftSliceInfo == null)
			{
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)ShiftSliceInfo["ShiftSliceData"];
			if (dynamicObjectCollection != null)
			{
				DateTime dateTime = default(DateTime);
				DateTime dateTime2 = default(DateTime);
				decimal num = 0m;
				int num2 = 0;
				dynamicObjectCollection.Sort<int>((DynamicObject p) => Convert.ToInt32(p["seq"]), null);
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					DateTime dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DateTime>(dynamicObject, "StartTime", MFGServiceHelper.GetSysDate(base.Context));
					DateTime dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DateTime>(dynamicObject, "EndTime", MFGServiceHelper.GetSysDate(base.Context));
					decimal dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObject, "Hours", 0m);
					if (num2 == 0)
					{
						dateTime = dynamicObjectItemValue;
						dateTime2 = dynamicObjectItemValue2;
						num2++;
					}
					if (num2 == dynamicObjectCollection.Count<DynamicObject>() - 1)
					{
						dateTime2 = dynamicObjectItemValue2;
					}
					num += dynamicObjectItemValue3;
				}
				this.Model.SetValue("FStartTime", dateTime, iRow);
				this.Model.SetValue("FEndTime", dateTime2, iRow);
				this.Model.SetValue("FHours", num, iRow);
			}
		}

		// Token: 0x0400021E RID: 542
		private WorkCalCustomEdit.T_EditShiftParam workCalSetupParam;
	}
}
