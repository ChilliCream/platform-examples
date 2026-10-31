using Paramore.Brighter;

namespace Shop.Contracts;

public sealed class ExerciseTransientFailure : Command
{
    public ExerciseTransientFailure() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
}
