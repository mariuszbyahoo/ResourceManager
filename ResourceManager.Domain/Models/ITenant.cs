using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models.Interfaces
{
	/// <summary>
	/// Dzierżawca
	/// </summary>
	public interface ITenant
	{
		/// <summary>
		/// Identyfikator dzierżawcy
		/// </summary>
		Guid Id { get;}

		/// <summary>
		/// Priorytet dzierżawcy
		/// </summary>
		byte Priority { get; set; }
	}
}
