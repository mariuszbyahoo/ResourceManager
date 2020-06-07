using ResourceManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models
{
    public interface IResourceData
    {
		/// <summary>
		/// Status zasobu; dostępny i zajęty.
		/// </summary>
		ResourceStatus Availability { get; set; }

		/// <summary>
		/// Identyfikator dzierżawcy 
		/// </summary>
		Guid LeasedTo { get; set; }

		/// <summary>
		/// Data, do której dany zasób jest niedostępny
		/// </summary>
		public DateTime OccupiedTill { get; set; }
	}
}
