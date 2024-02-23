using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Util;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000038 RID: 56
	[Description("技术文档列表插件")]
	public class TechDocList : BaseControlList
	{
		// Token: 0x06000428 RID: 1064 RVA: 0x00034E04 File Offset: 0x00033004
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.CustomerParams = e.Paramter.GetCustomParameters();
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x00034E20 File Offset: 0x00033020
		public override void CreateNewData(BizDataEventArgs e)
		{
			base.CreateNewData(e);
			object obj;
			if (this.CustomerParams.TryGetValue("MaterialId", out obj))
			{
				this.materialIds = (List<long>)obj;
				this.CustomerParams.Remove("MaterialId");
			}
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x00034E68 File Offset: 0x00033068
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			if (ListUtils.IsEmpty<long>(this.materialIds))
			{
				return;
			}
			if (this.materialIds.Count == 1)
			{
				e.AppendQueryFilter(string.Format(" (FAPPLYSCOPE='C' AND FMPMATERIALID ={0}) OR (FAPPLYSCOPE='B' AND FMMATERIALID ={0}) OR (FAPPLYSCOPE='D' AND FRMATERIALID ={0})", this.materialIds.FirstOrDefault<long>()));
				return;
			}
			string text = string.Format("(FAPPLYSCOPE='C' AND FMPMATERIALID in ({0})) OR (FAPPLYSCOPE='B' AND FMMATERIALID in ({0})) OR (FAPPLYSCOPE='D' AND FRMATERIALID in ({0}))", string.Join<long>(",", this.materialIds));
			e.AppendQueryFilter(text);
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x00034EDC File Offset: 0x000330DC
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString()) != null)
			{
				if (!(a == "New"))
				{
					if (!(a == "Copy"))
					{
						if (!(a == "Edit"))
						{
							return;
						}
						if (this.CustomerParams.ContainsKey("MoObjects"))
						{
							this.View.Session["Edit"] = true;
						}
					}
					else if (this.CustomerParams.ContainsKey("MoObjects"))
					{
						this.View.Session["tbCopy"] = true;
						return;
					}
				}
				else if (this.CustomerParams.ContainsKey("MoObjects"))
				{
					this.View.Session["MoObjects"] = this.CustomerParams["MoObjects"];
					return;
				}
			}
		}

		// Token: 0x040001CB RID: 459
		private Dictionary<string, object> CustomerParams = new Dictionary<string, object>();

		// Token: 0x040001CC RID: 460
		private List<long> materialIds = new List<long>();
	}
}
