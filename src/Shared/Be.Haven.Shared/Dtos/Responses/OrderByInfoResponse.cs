namespace Be.Haven.Shared.Dtos.Responses
{
    /// <summary>
    /// Defines ordering details for sorting query results.
    /// Used to indicate which property of a dataset should be sorted
    /// and in what direction.
    /// </summary>
    public class OrderByInfoResponse
    {
        /// <summary>
        /// Gets or sets the name of the property used for sorting.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the sort direction applied to the selected property.
        /// </summary>
        public SortDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this sorting rule
        /// is the default sorting applied initially.
        /// </summary>
        public bool Initial { get; set; }
    }
}
