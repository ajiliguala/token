using System;
using System.ComponentModel;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.JSON;
using Kingdee.BOS.VerificationHelper.Verifiers;
using Kingdee.K3.Core.MFG.ENG.ProductModel;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000011 RID: 17
	[Description("配置清单客户端插件")]
	public class CfgBillEdit : AbstractBillPlugIn
	{
		// Token: 0x06000213 RID: 531 RVA: 0x000190F8 File Offset: 0x000172F8
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			base.PreOpenForm(e);
			FeatureVerifier.CheckFeature(e.Context, "PDB");
		}

		// Token: 0x06000214 RID: 532 RVA: 0x00019114 File Offset: 0x00017314
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string operation;
			if ((operation = e.Operation.FormOperation.Operation) != null)
			{
				if (!(operation == "Config"))
				{
					return;
				}
				this.Config();
			}
		}

		// Token: 0x06000215 RID: 533 RVA: 0x00019158 File Offset: 0x00017358
		public void Config()
		{
			JSONObject jsonobject = MdlCfgServiceHelper.BuildDynFieldMdlFromPrdModeling(base.Context, DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "MdlId_Id", 0L));
			JSONObject jsonobject2 = MdlCfgServiceHelper.FormatCfgBillObj(base.Context, this.Model.DataObject);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
			{
				FormId = "ENG_MDLCFGEXEC",
				PageId = Guid.NewGuid().ToString(),
				ParentPageId = base.View.ParentFormView.PageId
			};
			MdlCfgOption mdlCfgOption = new MdlCfgOption();
			mdlCfgOption.CfgBillObj = KDObjectConverter.DeserializeObject<JSONObject>(jsonobject2.Get("cfgBillObj").ToString());
			mdlCfgOption.Context = base.Context;
			dynamicFormShowParameter.CustomComplexParams.Add("option", mdlCfgOption);
			dynamicFormShowParameter.CustomParams.Add("MdlCfgParameter", KDObjectConverter.SerializeObject(jsonobject));
			dynamicFormShowParameter.CustomParams.Add("isExistCfgBill", "1");
			dynamicFormShowParameter.CustomParams.Add("cfgBillId", this.Model.DataObject["Id"].ToString());
			dynamicFormShowParameter.CustomParams.Add("cfgBillNo", this.Model.DataObject["Number"].ToString());
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult x)
			{
				base.View.Refresh();
			});
		}
	}
}
