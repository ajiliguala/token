using System;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000042 RID: 66
	public class WorkCalEdit : BaseControlEdit
	{
		// Token: 0x06000495 RID: 1173 RVA: 0x000388FC File Offset: 0x00036AFC
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			Entity entity = base.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			if (!ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				DateTime sysDate = MFGServiceHelper.GetSysDate(base.Context);
				int num = Convert.ToInt32(sysDate.DayOfWeek);
				DateTime dateTime = sysDate.Date.AddDays((double)(-1 * ((num == 0) ? 6 : (num - 1))));
				int num2 = 0;
				foreach (DynamicObject dynamicObject in entityDataObject)
				{
					if (dateTime.CompareTo(DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject, "Day", default(DateTime))) <= 0)
					{
						base.View.SetEntityFocusRow("FEntity", num2);
						break;
					}
					num2++;
				}
			}
		}
	}
}
