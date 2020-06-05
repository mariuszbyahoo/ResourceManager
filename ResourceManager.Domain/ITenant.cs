using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain
{
	/// <summary>
	/// Dzierżawca
	/// </summary>
	public interface ITenant
	{
		/// <summary>
		/// Identyfikator dzierżawcy
		/// </summary>
		Guid Id { get; set; }

		/// <summary>
		/// Priorytet dzierżawcy
		/// </summary>
		byte Priority { get; set; }
	}
}
