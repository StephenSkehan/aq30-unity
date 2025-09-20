namespace AQ.App.Leads
{
    /// <summary>Runtime progress for a visible lead (persisted in save).</summary>
    [System.Serializable]
    public struct ActiveLeadState
    {
        public string LeadCardId;
        /// <summary>Bitmask of requirement completion (max 3 tracked for FTUE, scalable).</summary>
        public byte RequirementMask;
        public bool Locked;

        public bool IsRequirementMet(int index) => (RequirementMask & (1 << index)) != 0;
        public void SetRequirementMet(int index, bool value)
        {
            if (value) RequirementMask |= (byte)(1 << index);
            else RequirementMask &= (byte)~(1 << index);
        }
    }
}
