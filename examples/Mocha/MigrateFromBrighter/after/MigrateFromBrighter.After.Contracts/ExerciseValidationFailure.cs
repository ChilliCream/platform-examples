using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

public sealed class ExerciseValidationFailure
{
    public Guid OrderId { get; set; }
}
