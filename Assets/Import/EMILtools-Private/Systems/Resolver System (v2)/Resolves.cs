using System;

namespace EMILtools.Systems
{
    [Serializable]
    public readonly struct Resolves
    {
        public readonly IResolvable[] beforeExecution;
        public readonly IResolvable[] afterExecution;
        public readonly IResolvable[] failedExecution;
        public readonly bool resetWhenAllResolved;
        
        /// <summary>
        /// Returns true if all arrays are null, used when defaulted
        /// </summary>
        public bool allNull => beforeExecution == null && afterExecution == null && failedExecution == null;

        public Resolves(bool resetWhenAllResolved)
        {
            this.resetWhenAllResolved = resetWhenAllResolved;
            beforeExecution = Array.Empty<IResolvable>();
            afterExecution = Array.Empty<IResolvable>();
            failedExecution = Array.Empty<IResolvable>();
        }

        public Resolves(bool resetWhenAllResolved, IResolvable[] beforeExe = null,
            IResolvable[] afterExe = null, IResolvable[] failExe = null)
        {
            this.resetWhenAllResolved = resetWhenAllResolved;
            this.beforeExecution = beforeExe ?? Array.Empty<IResolvable>();
            this.afterExecution = afterExe ?? Array.Empty<IResolvable>();
            this.failedExecution = failExe ?? Array.Empty<IResolvable>();
        }

        public void ResetAllOnceResolves()
        {
            for (int i = 0; i < beforeExecution.Length; i++) beforeExecution[i].ResetWait();
            for (int i = 0; i < afterExecution.Length; i++) afterExecution[i].ResetWait();
            for (int i = 0; i < failedExecution.Length; i++) failedExecution[i].ResetWait();
        }
    }
}