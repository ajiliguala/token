using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.BomExpand.PlugIn;
using Kingdee.K3.Core.MFG.ENG.BomExpand.PlugIn.Args;
using Kingdee.K3.MFG.Common.BusinessEntity.ENG.BomExpand;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200000D RID: 13
	[Description("产品配置展开插件")]
	public class BomConfigExpandMemServicePlugIn : AbstractBomExpandMemServicePlugIn
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000205 RID: 517 RVA: 0x000188E8 File Offset: 0x00016AE8
		// (set) Token: 0x06000206 RID: 518 RVA: 0x000188F0 File Offset: 0x00016AF0
		public Dictionary<long, long> DicBomMaps { private get; set; }

		// Token: 0x06000207 RID: 519 RVA: 0x000188FC File Offset: 0x00016AFC
		public override void OnGetChildBomInfo(ChildBomInfoEventArgs e)
		{
			base.OnGetChildBomInfo(e);
			if (!ListUtils.IsEmpty<KeyValuePair<long, long>>(this.DicBomMaps))
			{
				BomExpandView.BomExpandResult bomExpandResult = new BomExpandView.BomExpandResult(e.ExpandResultRow);
				if (this.DicBomMaps.Keys.Contains(bomExpandResult.BomEntryId))
				{
					e.NewBomId = this.DicBomMaps[bomExpandResult.BomEntryId];
				}
			}
		}
	}
}
