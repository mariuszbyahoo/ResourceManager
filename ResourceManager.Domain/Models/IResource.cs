using ResourceManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models
{
	/// <summary>
	/// Zasób
	/// </summary>
	public interface IResource
	{
		/// <summary>
		/// Identyfikator zasobu
		/// </summary>
		Guid Id { get;}

		/// <summary>
		/// Wariant zasobu
		/// </summary>
		string Variant { get; set; }

		/// <summary>
		/// Status zasobu; dostępny i zajęty.
		/// </summary>
		ResourceStatus Availability { get; set; }

		/// <summary>
		/// Identyfikator dzierżawcy 
		/// </summary>
		public Guid LeasedTo { get; set; }
	}
}
