// ------------------------------------------------------------------------------
// <auto-generated>
//    Generated by avrogen, version 1.12.0+8c27801dc8d42ccc00997f25c0b8f45f8d4a233e
//    Changes to this file may cause incorrect behavior and will be lost if code
//    is regenerated
// </auto-generated>
// ------------------------------------------------------------------------------
namespace Generated.Entity
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using global::Avro;
	using global::Avro.Specific;
	
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("avrogen", "1.12.0+8c27801dc8d42ccc00997f25c0b8f45f8d4a233e")]
	public partial class CartItem : global::Avro.Specific.ISpecificRecord
	{
		public static global::Avro.Schema _SCHEMA = global::Avro.Schema.Parse(@"{""type"":""record"",""name"":""CartItem"",""namespace"":""Generated.Entity"",""fields"":[{""name"":""Product"",""type"":{""type"":""record"",""name"":""Product"",""namespace"":""Generated.Entity"",""fields"":[{""name"":""ProductId"",""type"":""int""},{""name"":""Name"",""type"":""string""},{""name"":""Price"",""type"":""double""}]}},{""name"":""Quantity"",""type"":""int""},{""name"":""TotalPrice"",""type"":""double""}]}");
		private Generated.Entity.Product _Product;
		private int _Quantity;
		private double _TotalPrice;
		public virtual global::Avro.Schema Schema
		{
			get
			{
				return CartItem._SCHEMA;
			}
		}
		public Generated.Entity.Product Product
		{
			get
			{
				return this._Product;
			}
			set
			{
				this._Product = value;
			}
		}
		public int Quantity
		{
			get
			{
				return this._Quantity;
			}
			set
			{
				this._Quantity = value;
			}
		}
		public double TotalPrice
		{
			get
			{
				return this._TotalPrice;
			}
			set
			{
				this._TotalPrice = value;
			}
		}
		public virtual object Get(int fieldPos)
		{
			switch (fieldPos)
			{
			case 0: return this.Product;
			case 1: return this.Quantity;
			case 2: return this.TotalPrice;
			default: throw new global::Avro.AvroRuntimeException("Bad index " + fieldPos + " in Get()");
			};
		}
		public virtual void Put(int fieldPos, object fieldValue)
		{
			switch (fieldPos)
			{
			case 0: this.Product = (Generated.Entity.Product)fieldValue; break;
			case 1: this.Quantity = (System.Int32)fieldValue; break;
			case 2: this.TotalPrice = (System.Double)fieldValue; break;
			default: throw new global::Avro.AvroRuntimeException("Bad index " + fieldPos + " in Put()");
			};
		}
	}
}
