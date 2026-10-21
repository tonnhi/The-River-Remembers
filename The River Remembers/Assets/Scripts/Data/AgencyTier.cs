namespace ConNuoc.Data
{
    /// <summary>
    /// The three rungs of the Agency Ladder (scoped prototype version — Household
    /// is folded into Personal-tier scenarios narratively rather than being its own tier).
    /// Capability, not XP: a tier is either unlocked or it isn't.
    /// </summary>
    public enum AgencyTier
    {
        Personal = 0,
        Community = 1,
        Institutional = 2
    }
}
