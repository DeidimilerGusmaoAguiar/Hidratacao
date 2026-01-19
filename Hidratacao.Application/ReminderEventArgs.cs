namespace Hidratacao.Application;

public sealed class ReminderEventArgs : EventArgs
{
    public ReminderEventArgs(DateTime occurredAtLocal, int remainingMl, int suggestedMl)
    {
        OccurredAtLocal = occurredAtLocal;
        RemainingMl = remainingMl;
        SuggestedMl = suggestedMl;
    }

    public DateTime OccurredAtLocal { get; }
    public int RemainingMl { get; }
    public int SuggestedMl { get; }
}
