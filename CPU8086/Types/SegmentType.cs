namespace Final.CPU8086.Types
{
    public enum SegmentType
    {
        /// <summary>
        /// No segment
        /// </summary>
        None = 0,
        /// <summary>
        /// Code segment (CS)
        /// </summary>
        CS,
        /// <summary>
        /// Data segment (DS)
        /// </summary>
        DS,
        /// <summary>
        /// Stack segment (SS)
        /// </summary>
        SS,
        /// <summary>
        /// Extra segment (ES)
        /// </summary>
        ES,
    }
}
