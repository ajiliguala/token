using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000070 RID: 112
	public class BomBatchEditParentEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000820 RID: 2080 RVA: 0x00060CA8 File Offset: 0x0005EEA8
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this._EditType = Convert.ToString(e.Paramter.GetCustomParameter("EditType"));
			this._UseOrgId = Convert.ToInt64(e.Paramter.GetCustomParameter("UseOrgId"));
			object obj = null;
			if (this.View.ParentFormView != null)
			{
				Dictionary<string, object> session = this.View.ParentFormView.Session;
				session.TryGetValue("CanEditFieldKeys", out obj);
				if (obj != null)
				{
					this._CanEditFieldKeys = (List<string>)obj;
				}
			}
		}

		// Token: 0x06000821 RID: 2081 RVA: 0x00060D2F File Offset: 0x0005EF2F
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.View.Model.SetValue("FOrgId", this._UseOrgId);
		}

		// Token: 0x06000822 RID: 2082 RVA: 0x00060D58 File Offset: 0x0005EF58
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.LockFields();
		}

		// Token: 0x06000823 RID: 2083 RVA: 0x00060D8C File Offset: 0x0005EF8C
		private void LockFields()
		{
			List<string> list = new List<string>();
			list.AddRange((from c in this._CanEditFieldKeys
			select c.Split(new char[]
			{
				'*'
			})[1]).ToList<string>());
			if (!list.Contains("FBOMUSE"))
			{
				this.View.LockField("FBOMUSE", false);
			}
			if (!list.Contains("FGroup"))
			{
				this.View.LockField("FGroup", false);
			}
		}

		// Token: 0x040003B6 RID: 950
		private string _EditType = string.Empty;

		// Token: 0x040003B7 RID: 951
		private long _UseOrgId;

		// Token: 0x040003B8 RID: 952
		private List<string> _CanEditFieldKeys = new List<string>();
	}
}
