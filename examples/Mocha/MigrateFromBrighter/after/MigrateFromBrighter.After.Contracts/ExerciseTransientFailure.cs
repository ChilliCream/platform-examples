using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

public sealed class ExerciseTransientFailure
{
    public Guid OrderId { get; set; }
}
