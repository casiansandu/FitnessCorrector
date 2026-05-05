namespace FitnessCorrector.Domain.Entities;

public class WorkoutSessionRepMetric
{
    private WorkoutSessionRepMetric() { }

    public static WorkoutSessionRepMetric Create(
        Guid workoutSessionId,
        int repIndex,
        double depth,
        double tempoTotalSeconds,
        double tempoEccentricSeconds,
        double tempoConcentricSeconds,
        double symmetry)
    {
        if (workoutSessionId == Guid.Empty) throw new ArgumentException("Invalid workout session ID");
        if (repIndex < 0) throw new ArgumentOutOfRangeException(nameof(repIndex));

        return new WorkoutSessionRepMetric
        {
            Id = Guid.NewGuid(),
            WorkoutSessionId = workoutSessionId,
            RepIndex = repIndex,
            Depth = depth,
            TempoTotalSeconds = tempoTotalSeconds,
            TempoEccentricSeconds = tempoEccentricSeconds,
            TempoConcentricSeconds = tempoConcentricSeconds,
            Symmetry = symmetry
        };
    }

    public Guid Id { get; private set; }
    public Guid WorkoutSessionId { get; private set; }
    public int RepIndex { get; private set; }
    public double Depth { get; private set; }
    public double TempoTotalSeconds { get; private set; }
    public double TempoEccentricSeconds { get; private set; }
    public double TempoConcentricSeconds { get; private set; }
    public double Symmetry { get; private set; }
}
