using System;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000017 RID: 23
	public class ResponseInfo
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000A5 RID: 165 RVA: 0x00008689 File Offset: 0x00006889
		// (set) Token: 0x060000A6 RID: 166 RVA: 0x00008691 File Offset: 0x00006891
		public string status { get; set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000A7 RID: 167 RVA: 0x0000869A File Offset: 0x0000689A
		// (set) Token: 0x060000A8 RID: 168 RVA: 0x000086A2 File Offset: 0x000068A2
		public string message { get; set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x000086AB File Offset: 0x000068AB
		// (set) Token: 0x060000AA RID: 170 RVA: 0x000086B3 File Offset: 0x000068B3
		public string data { get; set; }
	}
}
