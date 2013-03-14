using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapster.Example.DomainModels
{
    /// <summary>
    /// The complete domain model of an order.
    /// </summary>
    public class Order
    {
        #region Database Backed Properties

        [DbField("OrderID")]
        public int Id { get; set; }

        public DateTime Created { get; set; }
        public Guid CreatorUserGuid { get; set; }
        public int CurrencyId { get; set; }
        public string OrderNumber { get; set; }
        public int OrderStatusID { get; set; }
        public Char OrderType { get; set; }
        public string PONumber { get; set; }
        public string ShippingMethodCode { get; set; }

        #endregion

        #region Collections

        [DbTable("OrderLineItems")]
        public List<OrderLineItem> LineItems { get; set; }

        #endregion

        #region Calculated Properties

        [NoDb]
        public bool IsBillOnly
        {
            get
            {
                return this.OrderType == 'B';
            }
        }

        #endregion
    }
}
