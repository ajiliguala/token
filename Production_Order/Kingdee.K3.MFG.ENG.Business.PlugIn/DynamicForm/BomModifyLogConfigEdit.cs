using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000073 RID: 115
	[Description("BOM修改日志配置插件")]
	public class BomModifyLogConfigEdit : BaseMFGSysParamEditPlugIn
	{
		// Token: 0x06000830 RID: 2096 RVA: 0x00061774 File Offset: 0x0005F974
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this._orgId = Convert.ToInt64(e.Paramter.GetCustomParameter("OrgId"));
		}

		// Token: 0x06000831 RID: 2097 RVA: 0x00061798 File Offset: 0x0005F998
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.FillEntitys();
		}

		// Token: 0x06000832 RID: 2098 RVA: 0x000617A8 File Offset: 0x0005F9A8
		private void FillEntitys()
		{
			IOperationResult fields = BomModifyLogConfigServiceHelper.GetFields(base.Context, this._orgId);
			List<DynamicObject> list = (List<DynamicObject>)fields.FuncResult;
			Entity entity = this.View.BusinessInfo.GetEntity("FBomModifyLogConfig");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			entityDataObject.Clear();
			if (!fields.IsSuccess || ListUtils.IsEmpty<DynamicObject>(list))
			{
				this.View.UpdateView("FBomModifyLogConfig");
				return;
			}
			this.Model.BeginIniti();
			foreach (DynamicObject item in list)
			{
				entityDataObject.Add(item);
			}
			this.Model.EndIniti();
			this.View.UpdateView("FBomModifyLogConfig");
		}

		// Token: 0x06000833 RID: 2099 RVA: 0x0006188C File Offset: 0x0005FA8C
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (this._orgId == 0L)
			{
				this.View.LockField("FIsUseBomModifyLog", false);
			}
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(this.View.Model.DataObject, "BomModifyLogConfig", null);
			foreach (DynamicObject dynamicObject in dynamicValue)
			{
				bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IsSystemSet", false);
				if (dynamicValue2)
				{
					this.View.GetFieldEditor("FIsShow", dynamicValue.IndexOf(dynamicObject)).Enabled = false;
				}
			}
		}

		// Token: 0x040003BC RID: 956
		private long _orgId;
	}
}
