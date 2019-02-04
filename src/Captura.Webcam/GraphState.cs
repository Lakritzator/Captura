namespace Captura.WebCam
{
    /// <summary> 
    /// Possible states of the interal filter graph.
    /// </summary>
    internal enum GraphState
    {
        /// <summary>
        /// No filter graph at all.
        /// </summary>
        Null,

        /// <summary>
        /// Filter graph created with device filters added.
        /// </summary>
        Created,

        /// <summary>
        /// Filter complete built, ready to run (possibly previewing).
        /// </summary>
        Rendered
    }
}