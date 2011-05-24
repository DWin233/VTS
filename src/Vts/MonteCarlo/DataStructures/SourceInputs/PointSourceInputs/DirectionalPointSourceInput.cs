using Vts.Common;

namespace Vts.MonteCarlo.Sources
{   
    // todo: re-do this file for new sources

    /// <summary>
    /// Implements ISourceInput.  Defines input data for Directional PointSource 
    /// implementation including position, and direction.
    /// </summary>
    public class DirectionalPointSourceInput : ISourceInput
    {
        // this handles point
        public DirectionalPointSourceInput(
            Position pointLocation,
            Direction direction) 
        {
            SourceType = SourceType.DirectionalPoint;
            PointLocation = pointLocation;
            EmittingDirection = direction;
        }

        public DirectionalPointSourceInput()
            : this(
                new Position (0, 0, 0),
                new Direction(0, 0, 1)) { }

        public Position PointLocation { get; set; }
        public Direction EmittingDirection { get; set; }
        public SourceType SourceType { get; set; }
    }
}
