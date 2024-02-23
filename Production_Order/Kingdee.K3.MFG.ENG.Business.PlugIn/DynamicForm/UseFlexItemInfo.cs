using System;
using System.Collections.Generic;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000067 RID: 103
	public class UseFlexItemInfo
	{
		// Token: 0x17000040 RID: 64
		// (get) Token: 0x0600078E RID: 1934 RVA: 0x0005A989 File Offset: 0x00058B89
		// (set) Token: 0x0600078F RID: 1935 RVA: 0x0005A991 File Offset: 0x00058B91
		public string FlexGroupFieldKey { get; set; }

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000790 RID: 1936 RVA: 0x0005A99A File Offset: 0x00058B9A
		// (set) Token: 0x06000791 RID: 1937 RVA: 0x0005A9A2 File Offset: 0x00058BA2
		public string ControlBaseDataId { get; set; }

		// Token: 0x06000792 RID: 1938 RVA: 0x0005A9AC File Offset: 0x00058BAC
		public bool IsThisFlexGroup(string flexGroupFieldKey, string controlBaseDataId)
		{
			string a = this.JoinKeyAndId(flexGroupFieldKey, controlBaseDataId);
			string b = this.JoinKeyAndId(this.FlexGroupFieldKey, this.ControlBaseDataId);
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06000793 RID: 1939 RVA: 0x0005A9E0 File Offset: 0x00058BE0
		private string JoinKeyAndId(string flexGroupFieldKey, string controlBaseDataId)
		{
			return string.Format("{0},{1}", flexGroupFieldKey, controlBaseDataId);
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000794 RID: 1940 RVA: 0x0005A9FB File Offset: 0x00058BFB
		// (set) Token: 0x06000795 RID: 1941 RVA: 0x0005AA03 File Offset: 0x00058C03
		public List<DynamicObject> FlexList { get; set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000796 RID: 1942 RVA: 0x0005AA0C File Offset: 0x00058C0C
		// (set) Token: 0x06000797 RID: 1943 RVA: 0x0005AA14 File Offset: 0x00058C14
		public int FlexItemId { get; set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000798 RID: 1944 RVA: 0x0005AA1D File Offset: 0x00058C1D
		// (set) Token: 0x06000799 RID: 1945 RVA: 0x0005AA25 File Offset: 0x00058C25
		public int FlexItemMasterId { get; set; }
	}
}
