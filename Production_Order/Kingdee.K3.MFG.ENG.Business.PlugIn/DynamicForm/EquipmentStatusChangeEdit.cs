using System;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000099 RID: 153
	[Description("设备状态变更（动态表单）处理类")]
	public class EquipmentStatusChangeEdit : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000AFB RID: 2811 RVA: 0x0007DD91 File Offset: 0x0007BF91
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.EquipmentId = e.Paramter.GetCustomParameter("EquipmentId").ToString();
		}

		// Token: 0x06000AFC RID: 2812 RVA: 0x0007DDB5 File Offset: 0x0007BFB5
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.LoadData();
		}

		// Token: 0x06000AFD RID: 2813 RVA: 0x0007DDD4 File Offset: 0x0007BFD4
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbSave"))
				{
					return;
				}
				if (this.ValidateSave())
				{
					this.SaveEquipment();
					this.SaveLog();
					this.View.ShowMessage(ResManager.LoadKDString("状态变更成功", "015072000012091", 7, new object[0]), 0, delegate(MessageBoxResult result)
					{
						this.View.Close();
					}, "", 0);
					return;
				}
				this.View.ShowErrMessage(ResManager.LoadKDString("变更后状态不能与原始状态相同！", "015072000012092", 7, new object[0]), "", 0);
			}
		}

		// Token: 0x06000AFE RID: 2814 RVA: 0x0007DE7C File Offset: 0x0007C07C
		private void LoadData()
		{
			string text = string.Format(" FID = {0} ", this.EquipmentId);
			this.dyoEquipment = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_Equipment", null, text, "").FirstOrDefault<DynamicObject>();
			string text2 = this.dyoEquipment["CurrentStatus"].ToString();
			this.Model.SetValue("FCurrentStatus", text2);
			this.View.UpdateView("FCurrentStatus");
		}

		// Token: 0x06000AFF RID: 2815 RVA: 0x0007DEF4 File Offset: 0x0007C0F4
		private bool ValidateSave()
		{
			string text = this.Model.GetValue("FCurrentStatus").ToString();
			string value = this.Model.GetValue("FChangeStatus").ToString();
			return !text.Equals(value);
		}

		// Token: 0x06000B00 RID: 2816 RVA: 0x0007DF40 File Offset: 0x0007C140
		private void SaveEquipment()
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_Equipment", true) as FormMetadata;
			string text = string.Format(" FID = {0} ", this.EquipmentId);
			this.dyoEquipment = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_Equipment", null, text, "").FirstOrDefault<DynamicObject>();
			OperateOption operateOption = OperateOption.Create();
			operateOption.SetVariableValue("IgnoreWarning", true);
			string text2 = this.Model.GetValue("FChangeStatus").ToString();
			this.dyoEquipment["CurrentStatus"] = text2;
			BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, this.dyoEquipment, operateOption, "Save");
		}

		// Token: 0x06000B01 RID: 2817 RVA: 0x0007DFF4 File Offset: 0x0007C1F4
		private void SaveLog()
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_EqmStatusChgLogBill", true) as FormMetadata;
			OperateOption operateOption = OperateOption.Create();
			operateOption.SetVariableValue("IgnoreWarning", true);
			DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(this.View.Context);
			string text = this.Model.GetValue("FCurrentStatus").ToString();
			string text2 = this.Model.GetValue("FChangeStatus").ToString();
			DynamicObject dynamicObject = new DynamicObject(formMetadata.BusinessInfo.GetDynamicObjectType());
			dynamicObject["EquipmentId_Id"] = this.dyoEquipment["Id"];
			dynamicObject["OriginalStatus"] = text;
			dynamicObject["ChangedStatus"] = text2;
			dynamicObject["ChangerId_Id"] = base.Context.UserId;
			dynamicObject["ChangeTime"] = systemDateTime;
			dynamicObject["ChangeType"] = "A";
			dynamicObject["ModifyDate"] = systemDateTime;
			BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, dynamicObject, operateOption, "Save");
		}

		// Token: 0x04000526 RID: 1318
		private string EquipmentId = string.Empty;

		// Token: 0x04000527 RID: 1319
		private DynamicObject dyoEquipment;
	}
}
