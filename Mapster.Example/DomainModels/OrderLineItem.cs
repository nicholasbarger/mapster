using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapster.Example.DomainModels
{
    /// <summary>
    /// The complete domain model of a line item for an order.
    /// </summary>
    public class OrderLineItem
    {
        #region Database Backed Properties

        [DbField("OrderLineItemID")]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public string PartDescription { get; set; }
        public string PartNumber { get; set; }

        #endregion
    }
}
