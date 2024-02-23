using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.K3.Core.MFG.Expression;
using Kingdee.K3.Core.MFG.Expression.Functions;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000A9 RID: 169
	[Description("函数库表单插件")]
	public class MdlFunctionLabEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000BEA RID: 3050 RVA: 0x00088B74 File Offset: 0x00086D74
		public override void BeforeBindData(EventArgs e)
		{
			List<IMdlCfgFunction> functions = FunctionLab.GetFunctions(base.Context);
			int num = 0;
			foreach (IMdlCfgFunction mdlCfgFunction in functions)
			{
				this.Model.CreateNewEntryRow("FEntity");
				this.Model.SetValue("FFuncName", mdlCfgFunction.FunctionName, num);
				this.Model.SetValue("FFuncIntro", mdlCfgFunction.Name, num);
				this.Model.SetValue("FAssemblyPath", mdlCfgFunction.GetType().FullName, num);
				this.Model.SetValue("FFuncRemark", mdlCfgFunction.Remark, num);
				num++;
			}
		}
	}
}
