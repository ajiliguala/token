using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000034 RID: 52
	[Description("班次_表单插件")]
	public class ShiftSliceEdit : BaseControlEdit
	{
		// Token: 0x060003BA RID: 954 RVA: 0x0002E77C File Offset: 0x0002C97C
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FStartTime") && !(key == "FEndTime"))
				{
					if (!(key == "FHours"))
					{
						return;
					}
					this.SetTotalHours();
				}
				else if (base.View.Model.GetValue("FStartTime", e.Row) != null && base.View.Model.GetValue("FEndTime", e.Row) != null && base.View.Model.GetValue("FStartTime", e.Row).ToString() != "" && base.View.Model.GetValue("FEndTime", e.Row).ToString() != "")
				{
					DateTime dateTime = Convert.ToDateTime(base.View.Model.GetValue("FStartTime", e.Row).ToString());
					DateTime dateTime2 = Convert.ToDateTime(base.View.Model.GetValue("FEndTime", e.Row).ToString());
					dateTime = this.SetToday(dateTime);
					dateTime2 = this.SetToday(dateTime2);
					if (dateTime2 <= dateTime)
					{
						dateTime2 = dateTime2.AddDays(1.0);
					}
					TimeSpan timeSpan = dateTime2 - dateTime;
					base.View.Model.SetValue("FHours", Convert.ToDecimal(timeSpan.TotalHours), e.Row);
					return;
				}
			}
		}

		// Token: 0x060003BB RID: 955 RVA: 0x0002E915 File Offset: 0x0002CB15
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FEntity"))
			{
				this.AutoSetTimePeriod(e);
				this.SetTotalHours();
			}
		}

		// Token: 0x060003BC RID: 956 RVA: 0x0002E93B File Offset: 0x0002CB3B
		public override void AfterCopyRow(AfterCopyRowEventArgs e)
		{
			base.AfterCopyRow(e);
			this.SetTotalHours();
		}

		// Token: 0x060003BD RID: 957 RVA: 0x0002E94A File Offset: 0x0002CB4A
		public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
		{
			this.SetTotalHours();
		}

		// Token: 0x060003BE RID: 958 RVA: 0x0002E954 File Offset: 0x0002CB54
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

		// Token: 0x060003BF RID: 959 RVA: 0x0002EAEE File Offset: 0x0002CCEE
		private DateTime SetToday(DateTime dt)
		{
			dt = Convert.ToDateTime(dt.ToString("HH:mm:ss"));
			return dt;
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x0002EB04 File Offset: 0x0002CD04
		private void SetTotalHours()
		{
			int entryRowCount = this.Model.GetEntryRowCount("FEntity");
			decimal d = 0m;
			for (int i = 0; i < entryRowCount; i++)
			{
				d += Convert.ToDecimal(MFGBillUtil.GetValue<decimal>(base.View.Model, "FHours", i, 0m, null));
			}
			base.View.Model.SetValue("FWorkHours", d.ToString());
		}
	}
}
