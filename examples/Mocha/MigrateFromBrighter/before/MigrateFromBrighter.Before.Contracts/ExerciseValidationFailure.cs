using Paramore.Brighter;

namespace Shop.Contracts;

public sealed class ExerciseValidationFailure : Command
{
    public ExerciseValidationFailure() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
}
