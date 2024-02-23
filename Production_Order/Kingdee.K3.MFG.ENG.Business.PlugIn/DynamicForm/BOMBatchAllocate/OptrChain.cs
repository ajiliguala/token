using System;
using System.Collections.Generic;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.BOMBatchAllocate
{
	// Token: 0x0200005E RID: 94
	public class OptrChain
	{
		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000703 RID: 1795 RVA: 0x00053127 File Offset: 0x00051327
		// (set) Token: 0x06000704 RID: 1796 RVA: 0x0005312F File Offset: 0x0005132F
		public Queue<AbstractClainLink> ChianLinks
		{
			get
			{
				return this.chianLinks;
			}
			set
			{
				this.chianLinks = value;
			}
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000705 RID: 1797 RVA: 0x00053138 File Offset: 0x00051338
		// (set) Token: 0x06000706 RID: 1798 RVA: 0x00053140 File Offset: 0x00051340
		public BusinessInfo BizInfo
		{
			get
			{
				return this.bizInfo;
			}
			set
			{
				this.bizInfo = value;
			}
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000707 RID: 1799 RVA: 0x00053149 File Offset: 0x00051349
		// (set) Token: 0x06000708 RID: 1800 RVA: 0x00053151 File Offset: 0x00051351
		public List<DynamicObject> Datas
		{
			get
			{
				return this.datas;
			}
			set
			{
				this.datas = value;
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x06000709 RID: 1801 RVA: 0x0005315A File Offset: 0x0005135A
		// (set) Token: 0x0600070A RID: 1802 RVA: 0x00053162 File Offset: 0x00051362
		public IOperationResult Result
		{
			get
			{
				return this.result;
			}
			set
			{
				this.result = value;
			}
		}

		// Token: 0x0600070B RID: 1803 RVA: 0x0005316C File Offset: 0x0005136C
		public void Init()
		{
			this.chianLinks = new Queue<AbstractClainLink>();
			this.chianLinks.Enqueue(new SaveClainLink
			{
				BizInfo = this.bizInfo,
				Datas = this.datas,
				Result = this.result
			});
			this.chianLinks.Enqueue(new SubmitClainLink
			{
				BizInfo = this.bizInfo,
				Datas = this.datas,
				Result = this.result
			});
			this.chianLinks.Enqueue(new AuditClainLink
			{
				BizInfo = this.bizInfo,
				Datas = this.datas,
				Result = this.result
			});
		}

		// Token: 0x0600070C RID: 1804 RVA: 0x00053226 File Offset: 0x00051426
		public void DoOperations(Context ctx)
		{
			while (!ListUtils.IsEmpty<AbstractClainLink>(this.chianLinks))
			{
				this.chianLinks.Dequeue().DoOperation(ctx);
			}
		}

		// Token: 0x04000324 RID: 804
		private Queue<AbstractClainLink> chianLinks;

		// Token: 0x04000325 RID: 805
		private BusinessInfo bizInfo;

		// Token: 0x04000326 RID: 806
		private List<DynamicObject> datas;

		// Token: 0x04000327 RID: 807
		private IOperationResult result;
	}
}
