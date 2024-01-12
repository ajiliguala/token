using System;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000005 RID: 5
	public class OperationMessage
	{
		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000052 RID: 82 RVA: 0x000034AA File Offset: 0x000016AA
		// (set) Token: 0x06000053 RID: 83 RVA: 0x000034B2 File Offset: 0x000016B2
		public object BillPKValue { get; set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000054 RID: 84 RVA: 0x000034BB File Offset: 0x000016BB
		// (set) Token: 0x06000055 RID: 85 RVA: 0x000034C3 File Offset: 0x000016C3
		public string BillNumber { get; set; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000056 RID: 86 RVA: 0x000034CC File Offset: 0x000016CC
		// (set) Token: 0x06000057 RID: 87 RVA: 0x000034D4 File Offset: 0x000016D4
		public string Title { get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000058 RID: 88 RVA: 0x000034DD File Offset: 0x000016DD
		// (set) Token: 0x06000059 RID: 89 RVA: 0x000034E5 File Offset: 0x000016E5
		public bool State { get; set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600005A RID: 90 RVA: 0x000034EE File Offset: 0x000016EE
		// (set) Token: 0x0600005B RID: 91 RVA: 0x000034F6 File Offset: 0x000016F6
		public string Message { get; set; }
	}
}
